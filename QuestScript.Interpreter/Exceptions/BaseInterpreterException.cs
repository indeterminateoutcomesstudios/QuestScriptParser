using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.Exceptions
{
    public class BaseInterpreterException : Exception, IEquatable<BaseInterpreterException>
    {
        public BaseInterpreterException(ParserRuleContext ctx, string msg)
            : base(msg ?? string.Empty)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

        public BaseInterpreterException(ParserRuleContext ctx, string msg, Exception inner)
            : base(msg ?? string.Empty, inner)
        {
            Context = ctx;
            Line = ctx.start.Line;
            Column = ctx.start.Column;
        }

        public ParserRuleContext Context { get; }
        public int Line { get; }
        public int Column { get; }

        public bool Equals(BaseInterpreterException other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Line == other.Line && Column == other.Column &&
                   Message.Equals(other.Message, StringComparison.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
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