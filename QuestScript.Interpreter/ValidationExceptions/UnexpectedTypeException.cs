using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UnexpectedTypeException : BaseValidationException
    {
        public UnexpectedTypeException(ParserRuleContext ctx, ObjectType expectedType, ObjectType apparentType, ParserRuleContext invalidTypeExpression, string additionalMessage = null) : 
            base(ctx, $"Expected '{invalidTypeExpression.GetText()}' to be of type '{expectedType}', but instead it is of type '{apparentType}'. {additionalMessage ?? String.Empty}")
        {
        }
    }
}
