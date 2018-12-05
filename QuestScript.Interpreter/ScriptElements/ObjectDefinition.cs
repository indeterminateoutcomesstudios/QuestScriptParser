using System;
using System.Collections.Generic;

namespace QuestScript.Interpreter.ScriptElements
{
    public class ObjectDefinition : IEquatable<ObjectDefinition>
    {
        public readonly string Name;
        public string[] InheritsFrom;

        public Dictionary<string, Field> Fields;
        public Dictionary<string, MethodDefinition> Methods;
        public ObjectDefinition Parent;

        public ObjectDefinition(string name)
        {
            Name = name;
        }

        public bool Equals(ObjectDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectDefinition) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(ObjectDefinition left, ObjectDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectDefinition left, ObjectDefinition right)
        {
            return !Equals(left, right);
        }
    }
}
