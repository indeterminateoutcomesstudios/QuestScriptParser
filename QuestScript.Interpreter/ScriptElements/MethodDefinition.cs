namespace QuestScript.Interpreter.ScriptElements
{
    public class MethodDefinition : FunctionDefinition
    {
        public readonly string DelegateFieldName;

        public MethodDefinition(string name, string delegateFieldName, string[] parameters, ObjectType returnType, string implementation) : 
            base(name, parameters, returnType, implementation)
        {
            DelegateFieldName = delegateFieldName;
        }
    }
}
