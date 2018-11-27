namespace QuestScript.Parser.Types
{
    public class ObjectType : Type
    {
        public override System.Type UnderlyingType => typeof(object);
        public override string ToString() => "object";
    }
}
