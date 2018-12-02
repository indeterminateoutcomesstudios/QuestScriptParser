using System;
using System.Collections.Generic;
using System.Dynamic;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Tryouts
{
    public class ObjectInstanceInfo : IObjectInstance
    {
        private readonly List<ObjectInstanceInfo> _attributes;

        public ObjectInstanceInfo(string name, ObjectType type, string typeName,
            List<ObjectInstanceInfo> attributes = null)
        {
            _attributes = attributes ?? new List<ObjectInstanceInfo>();
            Name = name;
            Type = type;
            TypeName = typeName;
        }

        public string Name { get; set; }

        public Func<object> ValueResolver => () => new ExpandoObject();

        public ObjectType Type { get; set; }
        public string TypeName { get; set; }

        public IReadOnlyList<IObjectInstance> Attributes => _attributes;

        public IReadOnlyList<IDelegate> Delegates =>
            throw new NotSupportedException("TODO : implement testing IDelegate object");
    }
}