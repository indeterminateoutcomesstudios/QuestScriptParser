using System.Collections.Generic;
using QuestScript.Interpreter.Exceptions;

namespace QuestScript.Interpreter.ScriptElements
{
    public interface IFunction
    {
        ObjectType ReturnType { get; }
        IReadOnlyCollection<string> Parameters { get; }
        HashSet<BaseInterpreterException> Errors { get; }
        object Call();
    }
}