﻿using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UnresolvedVariableException : BaseValidationException
    {
        public string Name { get; }

        public UnresolvedVariableException(string name, ParserRuleContext variableContext, string description = null) : 
            base(variableContext,$"I found {description ?? "variable"} I couldn't recognize. Can you make sure '{name}' is defined before it is used?")
        {                       
            Name = name;
        }
    }
}
