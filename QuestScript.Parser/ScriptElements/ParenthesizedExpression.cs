using System;
using System.Collections.Generic;

namespace QuestScript.Parser.ScriptElements
{
    public class ParenthesizedExpression : Expression
    {
        public override IEnumerable<Expression> Children
        {
            get { yield return Parenthesized; }
        }

        public Expression Parenthesized { get; }

        public ParenthesizedExpression(Expression parenthesized)
        {
            if (parenthesized == null)  //precaution!
                throw new ArgumentNullException(nameof(parenthesized));
            parenthesized.Parent = this;
            Parenthesized = parenthesized;
        }

        public override string ToString() => $" ({Parenthesized?.ToString() ?? string.Empty}) ";
    }
}
