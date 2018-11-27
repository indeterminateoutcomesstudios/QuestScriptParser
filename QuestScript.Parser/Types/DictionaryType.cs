using System.Collections.Generic;

namespace QuestScript.Parser.Types
{
    public class DictionaryType : Type
    {
        public override System.Type UnderlyingType => typeof(Dictionary<string,object>);
        public override string ToString() => "dictionary";

        public static DictionaryType Instance { get; } = new DictionaryType();
    }
}
