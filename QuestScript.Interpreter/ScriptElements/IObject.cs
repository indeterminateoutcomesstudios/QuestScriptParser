using System.Collections.Generic;

namespace QuestScript.Interpreter.ScriptElements
{
    public interface IObject : INode
    {
        ObjectType Type { get; }
        string TypeName { get; }

        IReadOnlyList<IObject> Attributes { get; }
        IReadOnlyList<IDelegate> Delegates { get; }
    }
}
