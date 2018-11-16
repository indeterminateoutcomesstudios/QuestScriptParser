using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UndefinedFunctionException : Exception
    {
        public IFunction UndefinedFunction { get; }

        public UndefinedFunctionException(ParserRuleContext ctx, IFunction undefinedFunction) 
            : base($"At line {ctx.start.Line} and column {ctx.start.Column} I found an undefined function named {undefinedFunction.Name}")
        {
            UndefinedFunction = undefinedFunction;
        }
    }
}
