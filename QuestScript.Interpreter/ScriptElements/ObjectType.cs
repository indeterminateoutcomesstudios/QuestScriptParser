using System;

namespace QuestScript.Interpreter.ScriptElements
{
    [AttributeUsage(AttributeTargets.All)]
    public class AlternativeNameAttribute : Attribute
    {
        public string Name { get; set; }

        public AlternativeNameAttribute(string name)
        {
            Name = name;
        }
    }

    public enum ObjectType
    {
        Unknown,

        [AlternativeName("int")]
        Integer,

        Double,
        String,
        Object,
        Script,
        Boolean,
        List,
        Dictionary,
        Void,
        Null
    }
}
