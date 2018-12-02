﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Extensions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;
// ReSharper disable UnusedMember.Global

namespace QuestScript.Interpreter
{
    public class GameObjectResolverVisitor : QuestGameParserBaseVisitor<bool>
    {
        private static readonly QuestScriptLexer ScriptLexer = new QuestScriptLexer(null);
        private static readonly QuestScriptParser ScriptParser = new QuestScriptParser(null);

        private static readonly QuestGameLexer QuestGameLexer = new QuestGameLexer(null);
        private static readonly QuestGameParser QuestGameParser = new QuestGameParser(null);

        private static readonly string QuestCoreLibrariesPath;
        private static readonly string QuestLanguageLibrariesPath;

        private readonly List<FunctionDefinition> _functionDefinitions = new List<FunctionDefinition>();
        private readonly Dictionary<string, HashSet<string>> _functionReferences = new Dictionary<string, HashSet<string>>();
        private readonly FunctionReferencesBuilder _referenceBuilder = new FunctionReferencesBuilder();

        private readonly Stack<string> _currentFile = new Stack<string>();

        public HashSet<IFunction> Functions { get; } = new HashSet<IFunction>();
        public HashSet<string> References { get; } = new HashSet<string>();

        public HashSet<BaseInterpreterException> InterpreterErrors { get; } = new HashSet<BaseInterpreterException>();
        public Dictionary<string, HashSet<BaseParserErrorException>> ParserErrors { get; } = new Dictionary<string, HashSet<BaseParserErrorException>>();

        public GameFileType Type { get; private set; }

        static GameObjectResolverVisitor()
        {
            QuestCoreLibrariesPath = Path.Combine(EnvironmentUtil.GetQuestInstallationPath(), "Core");
            QuestLanguageLibrariesPath = Path.Combine(QuestCoreLibrariesPath, "Languages");
        }

        public override bool VisitElement(QuestGameParser.ElementContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ElementName?.Text)) //precaution
                return false;
            switch (context.ElementName.Text)
            {
                case "include":
                    VisitInclude(context);
                    break;
                case "library":
                    Type = GameFileType.Library;
                    break;
                case "function":
                    VisitFunction(context);
                    break;
                case "asl":
                    Type = GameFileType.Game;
                    break;
            }

            return base.VisitElement(context);
        }

        private void ProcessIncludedLibraryAndMerge(string library)
        {
            var libraryPath = Path.Combine(QuestCoreLibrariesPath, library);
            if (File.Exists(libraryPath) == false)
            {
                //try languages folder
                //try current folder
                var alternativeLibraryPath = Path.Combine(QuestLanguageLibrariesPath, library);
                if (File.Exists(alternativeLibraryPath))
                {
                    return; //do not process language files - not relevant
                }

                alternativeLibraryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, library);
                if (File.Exists(alternativeLibraryPath) == false)
                {
                    throw new FileNotFoundException(
                        $"Couldn't find referenced libary '{library}'. Tried looking in paths '{libraryPath}' and '{alternativeLibraryPath}'.");
                }

                libraryPath = alternativeLibraryPath;
            }

            _currentFile.Push(libraryPath);
            var inputStream = new AntlrFileStream(libraryPath);
            QuestGameLexer.SetInputStream(inputStream);
            QuestGameParser.SetInputStream(new CommonTokenStream(QuestGameLexer));

            var includeLibraryVisitor = new GameObjectResolverVisitor();
            var parsedTree = QuestGameParser.game();
            includeLibraryVisitor.Visit(parsedTree);

            foreach (var reference in includeLibraryVisitor.References)
            {
                ProcessIncludedLibraryAndMerge(reference);
            }

            MergeWith(includeLibraryVisitor);
            _currentFile.Pop();
        }

        private void MergeWith(GameObjectResolverVisitor otherVisitor)
        {
            _functionDefinitions.AddRange(otherVisitor._functionDefinitions);
            //TODO: add here other stuff that the visitor parses
        }



        private void VisitFunction(QuestGameParser.ElementContext context)
        {
            if (!context.TryGetAttributeByName("name", out var nameAttribute))
            {
                ThrowMissingAttribute(context, "name");
            }

            var functionName = nameAttribute.Value;
            var functionImplementation = context.content().GetText();
            var parameters = new List<string>();
            if (context.TryGetAttributeByName("parameters", out var parameterList))
            {
                parameters.AddRange(parameterList.Value.Split(','));
            }

            var returnType = ObjectType.Void;

            if (context.TryGetAttributeByName("type", out var returnTypeAttribute))
            {
                returnType = TypeUtil.TryParse(returnTypeAttribute.Value, out returnType)
                    ? returnType
                    : ObjectType.Unknown;
            }

            var newDefinition = new FunctionDefinition
            {
                Name = functionName,
                Parameters = parameters,
                Implementation = Regex.Unescape(functionImplementation),
                ReturnType = returnType
            };
            _functionDefinitions.Add(newDefinition);

            ScriptLexer.SetInputStream(new AntlrInputStream(newDefinition.Implementation));
            ScriptParser.SetInputStream(new CommonTokenStream(ScriptLexer));

            ScriptParser.RemoveErrorListeners();

            var key = _currentFile.Count > 0 ? _currentFile.Peek() : "root";
            if (!ParserErrors.TryGetValue(key, out var errors))
            {
                errors = new HashSet<BaseParserErrorException>();
                ParserErrors.Add(key, errors);
            }

            ScriptParser.AddErrorListener(new ParseErrorGatherer(errors));

            var parseTree = ScriptParser.script();
            _referenceBuilder.Reset();
            _functionReferences.Add(newDefinition.Name, _referenceBuilder.Visit(parseTree));
        }

        private void VisitInclude(QuestGameParser.ElementContext context)
        {
            if (!context.TryGetAttributeByName("ref", out var attr))
            {
                ThrowMissingAttribute(context, "ref");
            }

            References.Add(attr.Value);

            if (attr.Value == "CoreFunctions.aslx")
                Debugger.Break();

            ProcessIncludedLibraryAndMerge(attr.Value);
        }

        private static void ThrowMissingAttribute(QuestGameParser.ElementContext context, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{context.ElementName.Text}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted. The element I found is : " +
                context.GetText());
        }

        public class FunctionDefinition
        {
            public string Name;
            public List<string> Parameters;
            public string Implementation;
            public ObjectType ReturnType;

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(Parameters)}: {string.Join(",", Parameters)}, {nameof(ReturnType)}: {ReturnType}";
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
