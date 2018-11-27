namespace QuestScript.Parser.Types
{
    public class IntegerType : Type
    {
        public override System.Type UnderlyingType => typeof(int);       
        public override string ToString() => "int";

        public static IntegerType Instance { get; } = new IntegerType();

    }
}
