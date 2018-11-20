using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class InvalidArrayLiteralException : BaseInterpreterException
    {
        public InvalidArrayLiteralException(ParserRuleContext ctx, string additionalMessage) : 
            base(ctx, $"Found invalid declaration of an array. {additionalMessage}")
        {
        }
    }
}
