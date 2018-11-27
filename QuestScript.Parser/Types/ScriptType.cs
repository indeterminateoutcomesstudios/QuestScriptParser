using System;

namespace QuestScript.Parser.Types
{
    public class ScriptType : Type
    {
        //TODO : do not forget to implement
        public override System.Type UnderlyingType => throw new NotImplementedException("Script as an object is not yet implemented");
        public override string ToString() => "script";
    }
}
