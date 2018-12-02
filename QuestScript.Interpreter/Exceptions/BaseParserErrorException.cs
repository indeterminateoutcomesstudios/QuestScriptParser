using System;

namespace QuestScript.Interpreter.Exceptions
{
    public abstract class BaseParserErrorException : Exception
    {
        protected BaseParserErrorException()
        {
        }

        protected BaseParserErrorException(string message) : base(message)
        {
        }

        protected BaseParserErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string OffendingExpression { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int OffendingToken { get; set; }
        public string Filename { get; set; }

        protected bool Equals(BaseParserErrorException other)
        {
            return string.Equals(OffendingExpression, other.OffendingExpression) && Line == other.Line &&
                   Column == other.Column && OffendingToken == other.OffendingToken &&
                   string.Equals(Filename, other.Filename);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseParserErrorException) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = OffendingExpression != null ? OffendingExpression.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Column;
                hashCode = (hashCode * 397) ^ OffendingToken;
                hashCode = (hashCode * 397) ^ (Filename != null ? Filename.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}