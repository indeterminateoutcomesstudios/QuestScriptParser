﻿using System;

namespace QuestScript.Interpreter.ScriptElements
{
    [AttributeUsage(AttributeTargets.All)]
    public class AlternativeNameAttribute : Attribute
    {
        public AlternativeNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public enum ObjectType
    {
        Unknown,

        [AlternativeName("int")] Integer,
        Double,
        String,
        Object,
        Script,
        Boolean,
        ObjectList,
        StringList,
        List,
        Dictionary,
        StringDictionary,
        ObjectDictionary,
        ScriptDictionary,
        Void,
        Null
    }
}