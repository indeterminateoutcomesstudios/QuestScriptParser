using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Exceptions
{
    public class InvalidOperandsException : BaseInterpreterException
    {
        private static string CreateMessage(string op, ObjectType left, ObjectType right)
        {
            if (left != ObjectType.Unknown && right != ObjectType.Unknown)
            {
                return $"The operator '{op}' cannot be applied to '{left}' and '{right}' ";
            }

            return $"The operator '{op}' cannot be applied when the type of {(left == ObjectType.Unknown ? "right" : "left")} part of the expression cannot be inferred.";
        }    

        public InvalidOperandsException(ParserRuleContext ctx, string op, ObjectType left, ObjectType right) : 
            base(ctx,CreateMessage(op,left,right))
        {
        }
    }
}
