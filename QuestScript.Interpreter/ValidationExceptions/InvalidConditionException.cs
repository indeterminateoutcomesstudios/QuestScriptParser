﻿using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class InvalidConditionException : BaseValidationException
    {
        public InvalidConditionException(ParserRuleContext ctx ,string statement, ParserRuleContext condition) : 
            base(ctx, $"{statement}'s condition expression ('{condition}') must evaluate to boolean, but it didn't.")
        {
        }
    }
}
