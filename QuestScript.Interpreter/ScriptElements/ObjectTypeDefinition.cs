using System;
using System.Collections.Generic;

namespace QuestScript.Interpreter.ScriptElements
{
    public class ObjectTypeDefinition : IEquatable<ObjectTypeDefinition>
    {
        public readonly string Name;
        public readonly HashSet<string> InheritsFrom;
        public readonly HashSet<Field> Fields;

        public override string ToString()
        {
            return InheritsFrom.Count > 0 ? 
                $"{nameof(Name)}: {Name}, Fields: {Fields.Count}, Inherits From: {string.Join(",", InheritsFrom)}" : 
                $"{nameof(Name)}: {Name}, Fields: {Fields.Count}";
        }

        public ObjectTypeDefinition(string name, HashSet<Field> fields, HashSet<string> inheritsFrom)
        {
            Name = name;
            Fields = fields;
            InheritsFrom = inheritsFrom;
        }

        public bool Equals(ObjectTypeDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(Name, other.Name,StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ObjectTypeDefinition) obj);
        }

        public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;
        public static bool operator ==(ObjectTypeDefinition left, ObjectTypeDefinition right) => Equals(left, right);
        public static bool operator !=(ObjectTypeDefinition left, ObjectTypeDefinition right) => !Equals(left, right);
    }
}
