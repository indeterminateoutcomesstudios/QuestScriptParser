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
        private readonly Graph<ObjectTypeDefinition> _typeInheritanceGraph = new Graph<ObjectTypeDefinition>();

        private readonly XDocument _gameFile;
        
        private readonly FunctionReferencesBuilder _referenceBuilder = new FunctionReferencesBuilder();

        internal readonly HashSet<string> AlreadyLoadedFiles;
        protected internal static ParseErrorGatherer ErrorListener;

        static GameObjectResolver()
        {
            QuestCoreLibrariesPath = Path.Combine(EnvironmentUtil.GetQuestInstallationPath(), "Core");
            QuestLanguageLibrariesPath = Path.Combine(QuestCoreLibrariesPath, "Languages");
            ErrorListener = new ParseErrorGatherer();
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
            ProcessTypeDefinitions();

            ProcessObjectDefinitions();

            DisambiguateFunctionDefinitions();
        }

        private void ProcessTypeDefinitions()
        {
            foreach (var objectElement in _gameFile.Root?.Elements("type") ?? Enumerable.Empty<XElement>())
            {
                var objectTypeDefinition = ParseType(objectElement);
                _typeInheritanceGraph.AddVertex(objectTypeDefinition);
            }

            foreach (var vertex in _typeInheritanceGraph)
            {
                #if DEBUG
                foreach (var field in vertex.Fields)
                {
                    //precaution, just in case, should never throw
                    if(field.Type == ObjectType.Unknown)
                        throw new InvalidDataException($"Failed to recognize type of field with name = '{field.Name}', in object '{vertex.Name}'");
                }
                #endif

                var inheritsFromTypeDefinitions =
                    _typeInheritanceGraph.Where(x => vertex.InheritsFrom.Contains(x.Name));
                _typeInheritanceGraph.Connect(vertex, inheritsFromTypeDefinitions);
            }
        }

        public ObjectTypeDefinition ParseType(XElement typeElement)
        {
            var typeName = typeElement.Attribute("name")?.Value ?? throw new InvalidDataException($"I see type definition without a name, this is invalid ASLX markup. ({typeElement})");
            var inheritsFrom = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName == "inherit")
                .Select(x => x.Attribute("name")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var fieldsWithInvalidXmlName = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName == "attr" && x.Parent == typeElement)
                .Select(x =>
                {
                    var defaultType = !string.IsNullOrWhiteSpace(x.Value) ? "string" : "boolean";
                    return TypeUtil.TryParse(x.Attribute("type")?.Value ?? defaultType, out var convertedType)
                            ? new Field(x.Attribute("name")?.Value, convertedType)
                            : new Field(x.Attribute("name")?.Value, ObjectType.Unknown);
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name));

            var fields = typeElement.DescendantsAndSelf()
                .Where(x => x.Name.LocalName != "inherit" && x.Name.LocalName != "attr" && x.Name.LocalName != "elementType" && x.Parent == typeElement)                
                .Select(x =>
                {
                    var name = x.Name.LocalName;
                    var typeAsString = x.Attribute("type")?.Value ?? "string";
                    return TypeUtil.TryParse(typeAsString, out var type) ? 
                        new Field(name, type) : 
                        new Field(name, ObjectType.Unknown);
                });

            return new ObjectTypeDefinition(typeName,
                new HashSet<Field>(fields.Union(fieldsWithInvalidXmlName)), 
                new HashSet<string>(inheritsFrom));
        }

        private void ProcessObjectDefinitions()
        {
            foreach (var objectElement in _gameFile.Root?.Elements("object") ?? Enumerable.Empty<XElement>())
            {
                ParseObject(objectElement);
            }
        }

        private void ParseObject(XElement @object)
        {
            var name = @object.Attribute("name")?.Value ?? throw new InvalidDataException($"I see object definition without a name, this is invalid ASLX markup. ({@object})");
            var inheritsFrom = @object.DescendantsAndSelf()
                .Where(x => x.Name == "inherit")
                .Select(x => x.Attribute("name")?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            
        }

        //due to ambiguous Quest scripting syntax, it is possible to have false positives on functions. 
        //for example, in the statement "x = foo", the "foo" may be a variable, object or a function
        private void DisambiguateFunctionDefinitions()
        {
        }

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
            _typeInheritanceGraph.MergeWith(otherVisitor._typeInheritanceGraph);

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