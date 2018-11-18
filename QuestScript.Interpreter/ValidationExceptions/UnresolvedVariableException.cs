using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UnresolvedVariableException : BaseValidationException
    {
        public string Name { get; }

        public UnresolvedVariableException(string name, ParserRuleContext variableContext, string description = null) : 
            base(variableContext,$"Undefined {description ?? "variable"} found. Make sure '{name}' is defined before it is used. Or perhaps it is a typo?")
        {                       
            Name = name;
        }
    }
}
