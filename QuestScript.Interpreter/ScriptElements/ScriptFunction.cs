using System;
using System.Collections.Generic;
using QuestScript.Interpreter.Exceptions;

namespace QuestScript.Interpreter.ScriptElements
{
    //runtime representation of a global function defined in Quest Script
    public class ScriptFunction : IFunction
    {
        public ScriptFunction(string name, IReadOnlyCollection<string> parameterTypes, ObjectType returnType,
            ScriptEnvironment implementation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));
            ReturnType = returnType;
            Implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
        }

        public string Name { get; }

        public ScriptEnvironment Implementation { get; }

        public ObjectType ReturnType { get; }

        public IReadOnlyCollection<string> Parameters { get; }

        public HashSet<BaseInterpreterException> Errors { get; } = new HashSet<BaseInterpreterException>();

        public object Call()
        {
            throw new NotImplementedException();
        }

        protected bool Equals(ScriptFunction other)
        {
            return string.Equals(Name, other.Name) && Equals(Parameters.Count, other.Parameters.Count);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ScriptFunction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (Parameters != null ? Parameters.Count.GetHashCode() : 0);
            }
        }
    }
}