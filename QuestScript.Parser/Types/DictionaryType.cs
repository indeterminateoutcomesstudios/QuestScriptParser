using System.Collections.Generic;

namespace QuestScript.Parser.Types
{
    public class DictionaryType : Type
    {
        public override System.Type UnderlyingType => typeof(Dictionary<object,object>);
        public override string ToString()
        {
            return "dictionary";
        }
    }
}
