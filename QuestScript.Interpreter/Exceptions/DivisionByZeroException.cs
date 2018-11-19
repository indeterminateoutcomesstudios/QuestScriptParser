using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class DivisionByZeroException : BaseInterpreterException
    {
        public DivisionByZeroException(ParserRuleContext ctx) : 
            base(ctx, $"Tried to divide in the following expression '{ctx.GetText()}'. This is obviously forbidden :)")
        {
        }
    }
}
