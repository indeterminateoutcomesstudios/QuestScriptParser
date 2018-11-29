using System.Collections.Generic;
using QuestScript.Parser.Types;

namespace QuestScript.Parser.ScriptElements
{
    public abstract class Expression
    {
        public Type Type { get; set; } = DynamicType.Instance;

        public Expression Parent { get; set; }

        public abstract IEnumerable<Expression> Children { get; }

        public new abstract string ToString();
    }
}
