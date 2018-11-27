﻿namespace QuestScript.Parser.Types
{
    public class StringType : Type
    {
        public override System.Type UnderlyingType => typeof(string);
        public override string ToString() => "string";
    }
}
