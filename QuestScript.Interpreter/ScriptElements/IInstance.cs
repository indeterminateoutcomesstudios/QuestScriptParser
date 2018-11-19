using System;

namespace QuestScript.Interpreter.ScriptElements
{
    public interface IInstance
    {
        string Name { get; }
        Func<object> ValueResolver { get; }
    }
}
