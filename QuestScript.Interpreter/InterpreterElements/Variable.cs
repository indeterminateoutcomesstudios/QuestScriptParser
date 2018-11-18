using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Variable
    {
        public string Name;
        public ObjectType Type;
        public Func<ParserRuleContext> Value;
    }
}
