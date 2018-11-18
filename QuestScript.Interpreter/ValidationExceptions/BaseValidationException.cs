using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class BaseValidationException : Exception
    {
        public ParserRuleContext Context { get; }
        public int Line { get; }
        public int Column { get; }


        public BaseValidationException(ParserRuleContext ctx, string msg = null)
            :base(msg ?? string.Empty)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }
    }
}
