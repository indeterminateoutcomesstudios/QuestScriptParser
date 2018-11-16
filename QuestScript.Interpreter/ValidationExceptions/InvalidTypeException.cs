using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class InvalidTypeException :  Exception
    {
        public ObjectType[] Expected { get; set; }
        public ObjectType Found { get; set; }

        public InvalidTypeException(ParserRuleContext ctx,ObjectType expected, ObjectType found) 
            : base($"At line {ctx.start.Line} and column {ctx.start.Column} I found unexpected type: Expected to find {expected}, but I found {found}.")
        {
            Expected = new []{ expected };
            Found = found;
        }

        public InvalidTypeException(ParserRuleContext ctx,ObjectType[] expected, ObjectType found) 
            : base($"At line {ctx.start.Line} and column {ctx.start.Column} I found unexpected type: Expected to find one of the following types: {string.Join(",",expected)}. But suprisingly, I found {found}.")
        {
            Expected = expected;
            Found = found;
        }
    }
}
