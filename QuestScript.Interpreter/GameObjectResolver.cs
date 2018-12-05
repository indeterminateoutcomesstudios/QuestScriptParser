using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Antlr4.Runtime;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Extensions;
using QuestScript.Interpreter.GameObjectParsers;
using QuestScript.Interpreter.Helpers;
using QuestScript.Parser;
using QuestScript.Parser.ScriptElements;

// ReSharper disable UnusedMember.Global

namespace QuestScript.Interpreter
{
    public class GameObjectResolver
    {
        internal static readonly QuestScriptLexer ScriptLexer = new QuestScriptLexer(null);
        internal static readonly QuestScriptParser ScriptParser = new QuestScriptParser(null);

        private static readonly string QuestCoreLibrariesPath;
        private static readonly string QuestLanguageLibrariesPath;
        private readonly Stack<string> _currentFile = new Stack<string>();

        private readonly Dictionary<string,FunctionDefinition> _functionDefinitions = new Dictionary<string, FunctionDefinition>();
        private readonly Graph<string> _functionReferenceGraph = new Graph<string>();

        private readonly Dictionary<string, ObjectDefinition> _objectDefinitions = new Dictionary<string, ObjectDefinition>();

        private readonly Graph<ObjectTypeDefinition> _typeInheritanceGraph = new Graph<ObjectTypeDefinition>();
        private readonly Dictionary<string, ObjectTypeDefinition> _objectTypeDefinitions = new Dictionary<string, ObjectTypeDefinition>();

        private readonly Dictionary<string, FunctionDefinition> _delegateDefinitions = new Dictionary<string, FunctionDefinition>();
        
        private readonly HashSet<string> _undefinedFunctions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        private readonly XDocument _gameFile;
        

        internal readonly HashSet<string> AlreadyLoadedFiles;
        protected internal static ParseErrorGatherer ErrorListener;

        private static int _recursionCount;
        private readonly string _filePath;

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
            _filePath = new FileInfo(questFile).Directory?.FullName;
            using (var file = File.Open(questFile, FileMode.Open))
                _gameFile = XDocument.Load(file);

            if (_gameFile.Root == null)
                throw new ArgumentException("ASLX game file should not be empty...", nameof(questFile));

            InferFileType();

            ProcessIncludeReferences();

            var functionParser = new FunctionsParser(this);
            var parsedFunctions = functionParser.Parse(_gameFile);
            _functionReferenceGraph.MergeWith(parsedFunctions.ReferenceGraph);
            _functionDefinitions.MergeWith(parsedFunctions.Definitions);

            var delegateParser = new DelegatesParser();
            _delegateDefinitions.MergeWith(delegateParser.Parse(_gameFile));

            var objectTypeParser = new ObjectTypeParser(_delegateDefinitions);
            _typeInheritanceGraph.MergeWith(objectTypeParser.Parse(_gameFile));
            _objectTypeDefinitions.MergeWith(_typeInheritanceGraph.ToDictionary(x => x.Name, x => x));

            BuildTypeInheritanceGraph();

            var objectParser = new ObjectParser(_delegateDefinitions, _typeInheritanceGraph, _objectTypeDefinitions);
            _objectDefinitions.MergeWith(objectParser.Parse(_gameFile));

            PostLoadInitialization();
        }

        private void PostLoadInitialization()
        {
            //note: this function will run only once, after all include libraries were parsed
            if (_recursionCount > 0)
                return;

            //due to ambiguous Quest scripting syntax, it is possible to have false positives on functions. 
            //for example, in the statement "x = foo", the "foo" may be a variable, object or a function
            foreach (var def in _objectDefinitions)
            {
                if (!_functionDefinitions.ContainsKey(def.Key)) 
                    continue;
                _functionDefinitions.Remove(def.Key);
                _functionReferenceGraph.RemoveVertex(def.Key);
            }

            //as stuff is parsed, those will get updated and will help determining whether identifier is method, field, function or an object
            ScriptParser.FunctionDefinitions = _functionDefinitions;
            ScriptParser.ObjectDefinitions = _objectDefinitions;

            //build function dependency list for each function
            foreach (var kvp in _functionDefinitions)
            {
                kvp.Value.DependsOn = _functionReferenceGraph.Traverse(kvp.Key).ToArray();

                //just in case, track undefined functions
                foreach(var functionName in kvp.Value.DependsOn)
                    if (_functionDefinitions.ContainsKey(functionName))
                        _undefinedFunctions.Add(functionName);
            }
        }

        private void BuildTypeInheritanceGraph()
        {
            foreach (var vertex in _typeInheritanceGraph)
            {
#if DEBUG
                foreach (var field in vertex.Fields)
                {
                    //precaution, just in case, should never throw
                    if (field.Type == ObjectType.Unknown)
                        throw new InvalidDataException(
                            $"Failed to recognize type of field with name = '{field.Name}', in object '{vertex.Name}'");
                }
#endif

                var inheritsFromTypeDefinitions =
                    _typeInheritanceGraph.Where(x => vertex.InheritsFrom.Contains(x.Name));
                _typeInheritanceGraph.Connect(vertex, inheritsFromTypeDefinitions);
            }
        }

        public HashSet<string> IncludeReferences { get; } = new HashSet<string>();

        public HashSet<BaseInterpreterException> InterpreterErrors { get; } = new HashSet<BaseInterpreterException>();

        public Dictionary<string, HashSet<BaseParserErrorException>> FunctionParserErrors { get; } =
            new Dictionary<string, HashSet<BaseParserErrorException>>();

        public GameFileType Type { get; private set; }

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

                alternativeLibraryPath = Path.Combine(_filePath, library);
                if (File.Exists(alternativeLibraryPath) == false)
                    throw new FileNotFoundException(
                        $"Couldn't find referenced library '{library}'. Tried looking in paths '{libraryPath}' and '{alternativeLibraryPath}'.");

                libraryPath = alternativeLibraryPath;
            }

            _recursionCount++;        
            _currentFile.Push(libraryPath);
            
            var includeLibraryResolver = new GameObjectResolver(libraryPath, AlreadyLoadedFiles);
            foreach (var reference in includeLibraryResolver.IncludeReferences)
            {
                //since we are single threaded, its ok to mutate static int this way
                ProcessIncludedLibraryAndMerge(reference);
            }

            MergeWith(includeLibraryResolver);
            _currentFile.Pop();
            _recursionCount--;
        }

        private void MergeWith(GameObjectResolver otherVisitor)
        {
            _functionDefinitions.MergeWith(otherVisitor._functionDefinitions);
            foreach (var filePath in otherVisitor.AlreadyLoadedFiles) //do not load the same files twice...
                AlreadyLoadedFiles.Add(filePath);

            _functionReferenceGraph.MergeWith(otherVisitor._functionReferenceGraph);
            _typeInheritanceGraph.MergeWith(otherVisitor._typeInheritanceGraph);           

            _delegateDefinitions.MergeWith(otherVisitor._delegateDefinitions);
            
            _objectDefinitions.MergeWith(otherVisitor._objectDefinitions);
            _objectTypeDefinitions.MergeWith(otherVisitor._objectTypeDefinitions);

            //TODO: add here other stuff that the visitor parses
        }      

    }

    public enum GameFileType
    {
        Unknown,
        Game,
        Library
    }
}