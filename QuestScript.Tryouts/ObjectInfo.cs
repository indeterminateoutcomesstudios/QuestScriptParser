using System;
using System.Collections.Generic;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Tryouts
{
    public class ObjectInfo : IObject
    {
        private readonly List<ObjectInfo> _attributes;

        public string Name { get; set; }
        public ObjectType Type { get; set; }
        public string TypeName { get; set; }

        public IReadOnlyList<IObject> Attributes => _attributes;

        public IReadOnlyList<IDelegate> Delegates => throw new NotSupportedException("TODO : implement testing IDelegate object");

        public ObjectInfo(string name, ObjectType type, string typeName, List<ObjectInfo> attributes = null)
        {
            _attributes = attributes ?? new List<ObjectInfo>();
            Name = name;
            Type = type;
            TypeName = typeName;
        }
    }
}
