using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UndefinedMemberException : Exception
    {
        public string InstanceSymbol { get; set; }
        public string MemberSymbol { get; set; }

        public UndefinedMemberException(string instanceSymbol, string memberSymbol, ParserRuleContext symbolContext) : base($"At line {symbolContext.start.Line} and column {symbolContext.start.Column} I found object attribute I couldn't find in the definition of '{instanceSymbol}'. Can you make sure attribute with name '{memberSymbol}' is defined?")
        {
            InstanceSymbol = instanceSymbol;
            MemberSymbol = memberSymbol;
        }
    }
}
