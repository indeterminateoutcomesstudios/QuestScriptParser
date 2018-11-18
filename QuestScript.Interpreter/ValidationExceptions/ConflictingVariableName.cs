using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class ConflictingVariableName : BaseValidationException
    {
        public ConflictingVariableName(ParserRuleContext ctx, string variableName, string additionalMessage = null) : 
            base(ctx, $"found variable '{variableName}' with conflicting name. {additionalMessage ?? string.Empty}")
        {
        }
    }
}
