using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;

// ReSharper disable UnusedMember.Global

namespace QuestScript.Interpreter
{
    public class GameObjectResolver
    {
        private static readonly QuestScriptLexer ScriptLexer = new QuestScriptLexer(null);
        private static readonly QuestScriptParser ScriptParser = new QuestScriptParser(null);

        private static readonly string QuestCoreLibrariesPath;
        private static readonly string QuestLanguageLibrariesPath;
        private readonly Stack<string> _currentFile = new Stack<string>();

        private readonly List<FunctionDefinition> _functionDefinitions = new List<FunctionDefinition>();

        private readonly Graph<string> _functionReferenceGraph = new Graph<string>(StringComparer.InvariantCultureIgnoreCase);

        private readonly XDocument _gameFile;
        private readonly FunctionReferencesBuilder _referenceBuilder = new FunctionReferencesBuilder();

        internal readonly HashSet<string> AlreadyLoadedFiles;
        protected internal static ParseErrorGatherer ErrorListener;

        static GameObjectResolver()
        {
            QuestCoreLibrariesPath = Path.Combine(EnvironmentUtil.GetQuestInstallationPath(), "Core");
            QuestLanguageLibrariesPath = Path.Combine(QuestCoreLibrariesPath, "Languages");
            ScriptParser.AddErrorListener(ErrorListener);
        }

        public GameObjectResolver(string questFile, HashSet<string> alreadyLoadedFiles = null)
        {
            AlreadyLoadedFiles = alreadyLoadedFiles ?? new HashSet<string>();

            using (var file = File.Open(questFile, FileMode.Open))
                _gameFile = XDocument.Load(file);

            if (_gameFile.Root == null)
                throw new ArgumentException("ASLX game file should not be empty...", nameof(questFile));

            InferFileType();
            ProcessIncludeReferences();
            ProcessFunctionDefinitions();
        }

        public HashSet<IFunction> Functions { get; } = new HashSet<IFunction>();
        public HashSet<string> IncludeReferences { get; } = new HashSet<string>();

        public HashSet<BaseInterpreterException> InterpreterErrors { get; } = new HashSet<BaseInterpreterException>();

        public Dictionary<string, HashSet<BaseParserErrorException>> ParserErrors { get; } =
            new Dictionary<string, HashSet<BaseParserErrorException>>();

        public GameFileType Type { get; private set; }

        private void ProcessFunctionDefinitions()
        {
            foreach (var functionElement in _gameFile.Root?.Elements("function") ?? Enumerable.Empty<XElement>())
                ProcessFunction(functionElement);
        }

        private void ProcessIncludeReferences()
        {
            foreach (var element in (_gameFile.Root?.Elements("include") ?? Enumerable.Empty<XElement>())
                .Where(el => el.HasAttributes && el.FirstAttribute.Name == "ref"))
            {
                var library = element.Attribute("ref")?.Value;
                IncludeReferences.Add(library);

                if (!AlreadyLoadedFiles.Contains(library))
                {
                    AlreadyLoadedFiles.Add(library);
                    ProcessIncludedLibraryAndMerge(library);
                }
            }
        }

        private void InferFileType()
        {
            switch (_gameFile?.Root?.Name.LocalName)
            {
                case "asl":
                    Type = GameFileType.Game;
                    break;
                case "library":
                    Type = GameFileType.Library;
                    break;
            }
        }

        private void ProcessIncludedLibraryAndMerge(string library)
        {
            if (library == null)
                return;
            var libraryPath = Path.Combine(QuestCoreLibrariesPath, library);
            if (File.Exists(libraryPath) == false)
            {
                //try languages folder
                //try current folder
                var alternativeLibraryPath = Path.Combine(QuestLanguageLibrariesPath, library);
                if (File.Exists(alternativeLibraryPath)) return; //do not process language files - not relevant

                alternativeLibraryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, library);
                if (File.Exists(alternativeLibraryPath) == false)
                    throw new FileNotFoundException(
                        $"Couldn't find referenced library '{library}'. Tried looking in paths '{libraryPath}' and '{alternativeLibraryPath}'.");

                libraryPath = alternativeLibraryPath;
            }

            _currentFile.Push(libraryPath);

            var includeLibraryResolver = new GameObjectResolver(libraryPath, AlreadyLoadedFiles);           

            foreach (var reference in includeLibraryResolver.IncludeReferences) 
                ProcessIncludedLibraryAndMerge(reference);

            MergeWith(includeLibraryResolver);
            _currentFile.Pop();
        }

        private void MergeWith(GameObjectResolver otherVisitor)
        {
            _functionDefinitions.AddRange(otherVisitor._functionDefinitions);
            foreach (var filePath in otherVisitor.AlreadyLoadedFiles) //do not load the same files twice...
                AlreadyLoadedFiles.Add(filePath);

            _functionReferenceGraph.MergeWith(otherVisitor._functionReferenceGraph);

            //TODO: add here other stuff that the visitor parses
        }


        private void ProcessFunction(XElement function)
        {
            var functionName = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "name")?.Value;
            if (string.IsNullOrWhiteSpace(functionName)) ThrowMissingAttribute(function, "name");

            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException(
                    $"Failed to process function definition, because function name is null. (definition: ${function})");

            var functionImplementation = function.Value;

            if (string.IsNullOrWhiteSpace(functionImplementation))
                return; //nothing to do, precaution, should never happen. Perhaps throw here?

            var parameters = new List<string>();
            var functionParameters = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "parameters")?.Value;
            if (!string.IsNullOrWhiteSpace(functionParameters)) parameters.AddRange(functionParameters.Split(','));

            var returnType = ObjectType.Void;

            var returnTypeAttribute = function.Attributes().FirstOrDefault(x => x.Name.LocalName == "type")?.Value;
            if (!string.IsNullOrWhiteSpace(returnTypeAttribute))
                returnType = TypeUtil.TryParse(returnTypeAttribute, out returnType)
                    ? returnType
                    : ObjectType.Unknown;

            var newDefinition = new FunctionDefinition
            {
                Name = functionName,
                Parameters = parameters,
                Implementation = functionImplementation,
                ReturnType = returnType
            };
            _functionDefinitions.Add(newDefinition);

            ScriptLexer.SetInputStream(new AntlrInputStream(newDefinition.Implementation));
            ScriptParser.SetInputStream(new CommonTokenStream(ScriptLexer));
            
            var key = _currentFile.Count > 0 ? _currentFile.Peek() : "root";
            if (!ParserErrors.TryGetValue(key, out var errors))
            {
                errors = new HashSet<BaseParserErrorException>();
                ParserErrors.Add(key, errors);
            }

            ErrorListener.Errors = errors;
            var parseTree = ScriptParser.script();
         
            ScriptParser.RemoveErrorListener(ErrorListener);

            _functionReferenceGraph.Connect(newDefinition.Name, _referenceBuilder.Visit(parseTree));
            _referenceBuilder.Reset();
        }

        private static void ThrowMissingAttribute(XElement element, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{element}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted.");
        }

        public class FunctionDefinition
        {
            public string Implementation;
            public string Name;
            public List<string> Parameters;
            public ObjectType ReturnType;

            public override string ToString()
            {
                return
                    $"{nameof(Name)}: {Name}, {nameof(Parameters)}: {string.Join(",", Parameters)}, {nameof(ReturnType)}: {ReturnType}";
            }
        }
    }

    public enum GameFileType
    {
        Unknown,
        Game,
        Library
    }
}