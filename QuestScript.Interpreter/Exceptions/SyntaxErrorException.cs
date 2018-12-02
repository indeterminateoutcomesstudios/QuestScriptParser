using System;

namespace QuestScript.Interpreter.Exceptions
{
    public class SyntaxErrorException : BaseParserErrorException
    {
        public SyntaxErrorException(string message) : base(message)
        {
        }

        public SyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}