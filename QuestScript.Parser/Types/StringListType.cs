using System;
using System.Collections.Generic;
using System.Text;

namespace QuestScript.Parser.Types
{
    public class StringListType : Type
    {
        public override System.Type UnderlyingType => typeof(List<string>);
        public override string ToString() => "stringlist";

        public static StringListType Instance { get; } = new StringListType();
    }
}
