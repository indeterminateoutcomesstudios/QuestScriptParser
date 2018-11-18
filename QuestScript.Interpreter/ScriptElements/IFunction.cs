using System.Collections.Generic;

namespace QuestScript.Interpreter.ScriptElements
{
    public interface IFunction : IInstance
    {
        ObjectType ReturnType { get; }    
        IReadOnlyList<string> ParameterTypes { get; }
    }
}
