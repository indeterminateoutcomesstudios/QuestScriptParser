using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class Test : IEquatable<Test>
    {
        public int x { get;set; }
        public int y { get;set; }
        public int zx { get;set; }

        public bool Equals(Test other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return x == other.x && y == other.y && zx == other.zx;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Test) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ zx;
                return hashCode;
            }
        }

        public static bool operator ==(Test left, Test right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Test left, Test right)
        {
            return !Equals(left, right);
        }
    }

    public class BaseInterpreterException : Exception, IEquatable<BaseInterpreterException>
    {
        public ParserRuleContext Context { get; }
        public int Line { get; }
        public int Column { get; }


        public BaseInterpreterException(ParserRuleContext ctx, string msg)
            :base(msg ?? string.Empty)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

        public BaseInterpreterException(ParserRuleContext ctx, string msg, Exception inner)
            :base(msg ?? string.Empty,inner)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

        public bool Equals(BaseInterpreterException other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Line == other.Line && Column == other.Column && Message.Equals(other.Message,StringComparison.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseInterpreterException) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Message.GetHashCode();
                hash = (hash * 397) ^ Line;
                hash = (hash * 397) ^ Column;
                return hash;
            }
        }

        public static bool operator ==(BaseInterpreterException left, BaseInterpreterException right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseInterpreterException left, BaseInterpreterException right)
        {
            return !Equals(left, right);
        }
    }
}
