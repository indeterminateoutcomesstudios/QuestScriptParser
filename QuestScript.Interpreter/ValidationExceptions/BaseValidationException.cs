using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class BaseValidationException : Exception
    {
        public ParserRuleContext Context { get; }

        public BaseValidationException(ParserRuleContext ctx, string msg = null)
            :base($"At line {ctx.start.Line} and column {ctx.start.Column}, {msg}")
        {
            Context = ctx;
        }
    }
}
