using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace QuestScript.Parser.ScriptElements
{
    public class LiteralExpression : Expression
    {
        public object Value { get; set; }
        
        //literal expression may have no children...
        public override IEnumerable<Expression> Children => Enumerable.Empty<Expression>();

        public override string ToString() => Value.ToString();
    }
}
