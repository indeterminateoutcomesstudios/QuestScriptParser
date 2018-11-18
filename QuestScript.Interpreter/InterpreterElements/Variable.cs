using System;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Variable
    {
        public string Name;
        public ObjectType Type;
        public Func<object> ValueResolver;
        public bool IsEnumerationVariable;
    }
}
