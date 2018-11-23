using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
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
        private static readonly QuestScriptLexer Lexer = new QuestScriptLexer(null);
        private static readonly QuestScriptParser Parser = new QuestScriptParser(null);

        public HashSet<IFunction> Functions { get; } = new HashSet<IFunction>();
        public HashSet<string> References { get; } = new HashSet<string>();
        public GameFileType Type { get; private set; }

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

            var sourceCodeStream = new AntlrInputStream(functionImplementation);
            Lexer.SetInputStream(sourceCodeStream);
            Parser.SetInputStream(new UnbufferedTokenStream(Lexer));

            var parsedFunctionImplementation = Parser.script();
            var builder = new EnvironmentTreeBuilder();
            builder.Visit(parsedFunctionImplementation);

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
        }

        private static void ThrowMissingAttribute(QuestGameParser.ElementContext context, string attribute)
        {
            throw new InvalidDataException(
                $"Found <{context.ElementName.Text}> element, but didn't find '{attribute}' attribute. This should not happen, and it is likely this ASLX game file is corrupted. The element I found is : " +
                context.GetText());
        }
    }
}
