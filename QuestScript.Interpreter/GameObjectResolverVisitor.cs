using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using QuestScript.Interpreter.Exceptions;
using QuestScript.Interpreter.Extensions;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Parser;

namespace QuestScript.Interpreter
{
    public enum GameFileType
    {
        Game,
        Library
    }

    public class GameObjectResolverVisitor : QuestGameParserBaseVisitor<bool>
    {
        private static readonly QuestScriptLexer ScriptLexer = new QuestScriptLexer(null);
        private static readonly QuestScriptParser ScriptParser = new QuestScriptParser(null);

        private static readonly QuestGameLexer QuestGameLexer = new QuestGameLexer(null);
        private static readonly QuestGameParser QuestGameParser = new QuestGameParser(null);

        private static readonly string QuestCoreLibrariesPath;
        private static readonly string QuestLanguageLibrariesPath;
        public HashSet<IFunction> Functions { get; } = new HashSet<IFunction>();
        public HashSet<string> References { get; } = new HashSet<string>();

        public HashSet<BaseInterpreterException> Errors { get; } = new HashSet<BaseInterpreterException>();    

        public GameFileType Type { get; private set; }

        static GameObjectResolverVisitor()
        {
            QuestCoreLibrariesPath = Path.Combine(EnvironmentUtil.GetQuestPath(), "Core");
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
                var alternativeLibraryPath = Path.Combine(QuestLanguageLibrariesPath,library);
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

            var inputStream = new AntlrFileStream(libraryPath);
            QuestGameLexer.SetInputStream(inputStream);
            QuestGameParser.SetInputStream(new CommonTokenStream(QuestGameLexer));

            var includeLibraryVisitor = new GameObjectResolverVisitor();
            var parsedTree = QuestGameParser.document();
            includeLibraryVisitor.Visit(parsedTree);

            foreach (var reference in includeLibraryVisitor.References)
            {
                ProcessIncludedLibraryAndMerge(reference);
            }

            MergeWith(includeLibraryVisitor);
        }

        private void MergeWith(GameObjectResolverVisitor otherVisitor)
        {
            foreach (var func in otherVisitor.Functions)
                Functions.Add(func);

            //TODO: add here other stuff that the visitor parses
        }

        private void VisitFunction(QuestGameParser.ElementContext context)
        {
            if (!context.TryGetAttributeByName("name", out var nameAttribute))
            {
                ThrowMissingAttribute(context,"name");
            }

            var functionName = nameAttribute.Value;
            var functionImplementation = context.content().GetText();
            var parameters = new List<string>();
            if (context.TryGetAttributeByName("parameters", out var parameterList))
            {
                parameters.AddRange(parameterList.Value.Split(','));
            }

            var returnType = ObjectType.Unknown;

            if (context.TryGetAttributeByName("type", out var returnTypeAttribute))
            {
                returnType =  TypeUtil.TryParse(returnTypeAttribute.Value,out returnType)
                    ? returnType
                    : ObjectType.Unknown;
            }

            var sourceCodeStream = new AntlrInputStream(Regex.Unescape(functionImplementation).Replace("\r\n",string.Empty));
            ScriptLexer.SetInputStream(sourceCodeStream);
            ScriptParser.SetInputStream(new CommonTokenStream(ScriptLexer));

            var parsedFunctionImplementation = ScriptParser.script();
            var builder = new EnvironmentTreeBuilder();
            builder.Visit(parsedFunctionImplementation);
            if (builder.Errors.Count > 0)
            {
                Debugger.Break();
            }
            var func = new ScriptFunction(functionName, parameters, returnType, builder.Output);
            foreach (var error in builder.Errors)
            {
                func.Errors.Add(error);
            }
            Functions.Add(func);
        }

        private void VisitInclude(QuestGameParser.ElementContext context)
        {
            if(!context.TryGetAttributeByName("ref", out var attr))
            {
                ThrowMissingAttribute(context,"ref");
            }

            References.Add(attr.Value);
            ProcessIncludedLibraryAndMerge(attr.Value);
        }

        private static void ThrowMissingAttribute(QuestGameParser.ElementContext context, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{context.ElementName.Text}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted. The element I found is : " +
                context.GetText());
        }
    }
}
