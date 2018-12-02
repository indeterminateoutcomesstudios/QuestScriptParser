namespace QuestScript.Interpreter.ScriptElements
{
    //delegate in Quest is essentially an instance method
    public interface IDelegate : IFunction
    {
        string DefinedObjectType { get; }
    }
}