using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Variable
    {
        public string Name;
        public ObjectType Type;
        public Func<object> ValueResolver;

        //if true, prevent changes to the variable, handle resolving of value differently
        public bool IsEnumerationVariable;

        //if true, prevent changes to the variable
        public bool IsIterationVariable;
        public ParserRuleContext Context;
    }
}
