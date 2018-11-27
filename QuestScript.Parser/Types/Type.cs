using System;

namespace QuestScript.Parser.Types
{
    public abstract class Type : IEquatable<Type>
    {
        public abstract System.Type UnderlyingType { get; }

        public Resolution TypeResolution { get; set; }

        public enum Resolution
        {
            Static,
            Runtime
        }

        public abstract override string ToString();

        #region Equality Members

        public bool Equals(Type other)
        {
            return UnderlyingType == other?.UnderlyingType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Type;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return UnderlyingType.GetHashCode();
        }

        public static bool operator ==(Type left, Type right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Type left, Type right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
