using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class FailedToInferTypeException : BaseInterpreterException
    {
        public FailedToInferTypeException(ParserRuleContext ctx, ParserRuleContext invalidExpressionContext, Exception inner) : 
            base(ctx, $"Failed to infer the type of expression '{invalidExpressionContext.GetText()}'. This should not happen and is likely a bug.", inner)
        {
        }
    }
}
