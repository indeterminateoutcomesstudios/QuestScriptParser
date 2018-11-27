namespace QuestScript.Parser.Types
{
    public class DynamicType : Type
    {
        public override System.Type UnderlyingType => typeof(object);

        public DynamicType()
        {
            TypeResolution = Resolution.Runtime;
        }

        public override string ToString() => "object";

        public static DynamicType Instance { get; } = new DynamicType();
    }
}
