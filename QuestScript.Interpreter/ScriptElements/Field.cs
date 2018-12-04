using System;

namespace QuestScript.Interpreter.ScriptElements
{
    public class Field : IEquatable<Field>
    {
        public readonly string Name;
        public readonly ObjectType Type;

        public Field(string name, ObjectType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}";
        }

        public bool Equals(Field other)
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
            return Equals((Field) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (int) Type;
            }
        }

        public static bool operator ==(Field left, Field right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Field left, Field right)
        {
            return !Equals(left, right);
        }
    }
}
