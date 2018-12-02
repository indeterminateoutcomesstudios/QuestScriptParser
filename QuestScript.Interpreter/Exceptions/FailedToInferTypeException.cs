using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class FailedToInferTypeException : BaseInterpreterException
    {
        public FailedToInferTypeException(ParserRuleContext ctx, ParserRuleContext invalidExpressionContext,
            string additionalMessage) :
            base(ctx,
                $"Failed to infer the type of expression '{invalidExpressionContext.GetText()}'. {additionalMessage}")
        {
        }

        public FailedToInferTypeException(ParserRuleContext ctx, ParserRuleContext invalidExpressionContext,
            Exception inner) :
            base(ctx, $"Failed to infer the type of expression '{invalidExpressionContext.GetText()}'.", inner)
        {
        }
    }
}