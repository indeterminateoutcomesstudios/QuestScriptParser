using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class UnresolvedVariableException : BaseInterpreterException
    {
        public UnresolvedVariableException(string name, ParserRuleContext variableContext, string description = null) :
            base(variableContext,
                $"Undefined {description ?? "variable"} found. Make sure '{name}' is defined before it is used. Or perhaps it is a typo?")
        {
            Name = name;
        }

        public string Name { get; }
    }
}