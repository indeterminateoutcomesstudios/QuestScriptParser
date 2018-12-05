using System;

namespace QuestScript.Interpreter.ScriptElements
{
    public class FunctionDefinition : IEquatable<FunctionDefinition>
    {
        public readonly string Implementation;
        public readonly string Name;
        public readonly string[] Parameters;
        public readonly ObjectType ReturnType;

        public FunctionDefinition(string name, string[] parameters, ObjectType returnType, string implementation)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Implementation = implementation;
        }

        public bool Equals(FunctionDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(Parameters, other.Parameters);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FunctionDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }

        public static bool operator ==(FunctionDefinition left, FunctionDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FunctionDefinition left, FunctionDefinition right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(Parameters)}: {String.Join(",", Parameters)}, {nameof(ReturnType)}: {ReturnType}";
        }
    }
}