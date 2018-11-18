using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UnexpectedTypeException : BaseValidationException
    {
        public UnexpectedTypeException(ParserRuleContext ctx, ObjectType expectedType, ObjectType apparentType, ParserRuleContext invalidTypeExpression, string additionalMessage = null) : 
            base(ctx, $"Expected for '{invalidTypeExpression.GetText()}' to resolve to type '{expectedType}', but instead it has resolved to type '{apparentType}'. {additionalMessage ?? string.Empty}")
        {
        }
    }
}
