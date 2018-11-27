using QuestScript.Parser.Types;

namespace QuestScript.Parser.Expressions
{
    public abstract class ScriptExpression
    {
        public Type Type { get; set; } = DynamicType.Instance;
    }
}
