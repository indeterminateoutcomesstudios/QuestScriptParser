using System;
using Antlr4.Runtime;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.InterpreterElements
{
    public class Variable : IEquatable<Variable>
    {
        public string Name;
        public ObjectType Type;
        public Lazy<object> Value;

        //if true, prevent changes to the variable, handle resolving of value differently
        public bool IsEnumerationVariable;

        //if true, prevent changes to the variable
        public bool IsIterationVariable;
        public ParserRuleContext Context;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(Value)}: {Value?.Value}";
        }

        public bool Equals(Variable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Variable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (int) Type;
            }
        }

        public static bool operator ==(Variable left, Variable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Variable left, Variable right)
        {
            return !Equals(left, right);
        }
    }
}
