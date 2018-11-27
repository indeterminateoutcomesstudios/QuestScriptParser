using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuestScript.Parser.Helpers
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type,Types.Type> TypeInstanceCache = new Dictionary<Type, Types.Type>();

        public static Types.Type GetScriptTypeInstance(this Type type)
        {
            if (!typeof(Types.Type).IsAssignableFrom(type))
                return null;

            if (TypeInstanceCache.TryGetValue(type, out var value))
            {
                var typeInfo = type.GetTypeInfo();
                var prop = typeInfo.GetProperty("Instance");
                value = (Types.Type)prop?.GetValue(null);
                TypeInstanceCache.Add(type,value);
            }

            return value;
        }
    }
}
