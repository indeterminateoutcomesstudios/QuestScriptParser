using System;
using System.Collections.Generic;
using QuestScript.Parser.Tokens;

namespace QuestScript.Parser.ScriptElements
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }

        public Expression Right { get; }

        public ScriptToken Op { get; }

        public bool IsArithmetic { get; internal set; }

        public bool IsLogic { get; internal set; }        

        public override IEnumerable<Expression> Children
        {
            get 
            { 
                yield return Left;
                yield return Right;
            }
        }

        public override string ToString()
        {
            return $"{Left} {Op} {Right}";
        }

        public BinaryExpression(Expression left, Expression right, ScriptToken op)
        {
            left.Parent = this;
            right.Parent = this;

            Left = left;
            Right = right;
            Op = op;
        }
    }
}
