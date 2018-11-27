using System.Collections;

namespace QuestScript.Parser.Types
{
    public class ListType : Type
    {
        public override System.Type UnderlyingType => typeof(ArrayList);
        public override string ToString() => "list";
    }
}
