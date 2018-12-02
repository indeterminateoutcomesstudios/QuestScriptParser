using System;

namespace QuestScript.Interpreter.Exceptions
{
    public class AmbiguityException : BaseParserErrorException
    {
        public AmbiguityException(string message) : base(message)
        {
        }

        public AmbiguityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}