using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Exceptions
{
    public class FailedValueInterpretation : BaseInterpreterException
    {
        public FailedValueInterpretation(ParserRuleContext ctx, ObjectType type, object obj) : 
            base(ctx, $"Thought that '{obj}' is of type {type}, but something went wrong. This is likely a bug and should be reported.")
        {
        }

        public FailedValueInterpretation(ParserRuleContext ctx, ObjectType type, object obj, Exception inner) : 
            base(ctx, $"Thought that '{obj}' is of type {type}, but something went wrong. This is likely a bug and should be reported.", inner)
        {
        }
    }
}
