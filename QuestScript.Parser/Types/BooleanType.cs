namespace QuestScript.Parser.Types
{
    public class BooleanType : Type
    {
        public override System.Type UnderlyingType => typeof(bool);
        public override string ToString() => "boolean";

        public static BooleanType Instance { get; } = new BooleanType();
    }
}
