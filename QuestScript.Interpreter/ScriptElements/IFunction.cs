using System.Collections.Generic;

namespace QuestScript.Interpreter.ScriptElements
{
    public interface IFunction : INode
    {
        ObjectType ReturnType { get; }    
        IReadOnlyList<string> ParameterTypes { get; }
    }
}
