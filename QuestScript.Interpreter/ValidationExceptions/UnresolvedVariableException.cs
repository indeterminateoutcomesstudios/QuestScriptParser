using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UnresolvedVariableException : Exception
    {
        public string Name { get; }
        public ParserRuleContext VariableContext { get; }

        public UnresolvedVariableException(string name, ParserRuleContext variableContext, string description = null) : base($"At line {variableContext.start.Line} and column {variableContext.start.Column} I found {description ?? "variable"} I couldn't recognize. Can you make sure '{name}' is defined before it is used?")
        {                       
            Name = name;
            VariableContext = variableContext;
        }
    }
}
