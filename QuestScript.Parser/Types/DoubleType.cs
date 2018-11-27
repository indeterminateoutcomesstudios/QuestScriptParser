namespace QuestScript.Parser.Types
{
    public class DoubleType : Type
    {
        public override System.Type UnderlyingType => typeof(double);
        public override string ToString() => "double";
    }
}
