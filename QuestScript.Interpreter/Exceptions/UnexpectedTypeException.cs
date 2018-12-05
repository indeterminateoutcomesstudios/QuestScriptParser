using Antlr4.Runtime;
using QuestScript.Parser.ScriptElements;

namespace QuestScript.Interpreter.Exceptions
{
    public class UnexpectedTypeException : BaseInterpreterException
    {
        public UnexpectedTypeException(ParserRuleContext ctx, ObjectType expectedType, ObjectType apparentType,
            ParserRuleContext invalidTypeExpression, string additionalMessage = null) :
            base(ctx,
                $"Expected for '{invalidTypeExpression.GetText()}' to resolve to type '{expectedType}', but instead it has resolved to type '{apparentType}'. {additionalMessage ?? string.Empty}")
        {
        }
    }
}