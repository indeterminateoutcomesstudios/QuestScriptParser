using System;

namespace QuestScript.Parser.ScriptElements
{
    public class Field : IEquatable<Field>
    {
        public readonly string Name;
        public readonly ObjectType Type; //note: if this is ObjectType.Object, the Name is the "id" of the object instance
        public readonly string OriginalType;

        public Field(string name, ObjectType type, string originalType)
        {
            Name = name;
            Type = type;
            OriginalType = originalType;
        }

        public override string ToString() => $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}";

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
