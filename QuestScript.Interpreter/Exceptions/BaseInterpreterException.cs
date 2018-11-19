using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class BaseInterpreterException : Exception
    {
        public ParserRuleContext Context { get; }
        public int Line { get; }
        public int Column { get; }


        public BaseInterpreterException(ParserRuleContext ctx, string msg)
            :base(msg ?? string.Empty)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

        public BaseInterpreterException(ParserRuleContext ctx, string msg, Exception inner)
            :base(msg ?? string.Empty,inner)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

    }
}
