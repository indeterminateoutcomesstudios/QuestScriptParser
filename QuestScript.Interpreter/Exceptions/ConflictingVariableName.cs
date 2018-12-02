using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class ConflictingVariableName : BaseInterpreterException
    {
        public ConflictingVariableName(ParserRuleContext ctx, string variableName, string additionalMessage = null) :
            base(ctx, $"Found variable '{variableName}' with conflicting name. {additionalMessage ?? string.Empty}")
        {
        }
    }
}