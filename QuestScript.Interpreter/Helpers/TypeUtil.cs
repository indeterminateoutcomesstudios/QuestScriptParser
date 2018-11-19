using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Helpers
{
    public static class TypeUtil
    {
        private static IReadOnlyList<ObjectType> _comparableTypes = new List<ObjectType>{ ObjectType.Integer, ObjectType.Double };
        private static Dictionary<ObjectType, ObjectType[]> _allowedImplicitCasting = new Dictionary<ObjectType, ObjectType[]>
        {
            { ObjectType.Integer, new []{ ObjectType.Double, ObjectType.String, ObjectType.Object } },
            { ObjectType.Double, new []{ ObjectType.Integer, ObjectType.String, ObjectType.Object } },
            { ObjectType.Boolean, new []{ ObjectType.String, ObjectType.Object } },
            { ObjectType.Object, new []{ ObjectType.String } },
            { ObjectType.String, new []{ ObjectType.Object } },
        };

        private static Dictionary<ObjectType, Type> _conversionToType = new Dictionary<ObjectType, Type>
        {
            { ObjectType.Object, typeof(object) },
            { ObjectType.Double, typeof(double) },
            { ObjectType.Boolean, typeof(bool) },
            { ObjectType.Integer, typeof(int) },
            { ObjectType.String, typeof(string) },
            { ObjectType.List, typeof(ArrayList) },
            { ObjectType.Void, typeof(void) }
        };

        public static bool TryConvertToType(ObjectType type, out Type result) =>
            _conversionToType.TryGetValue(type, out result);

        public static bool IsComparable(ObjectType type) => _comparableTypes.Contains(type);        


        public static bool CanConvert(ObjectType from, ObjectType to) => 
            _allowedImplicitCasting.TryGetValue(@from, out var conversions) && conversions.Contains(to);

        public static object Convert(object from,ObjectType fromType, ObjectType toType)
        {
            if (!CanConvert(fromType, toType))
                return null;

            switch (toType)
            {
                case ObjectType.Integer:
                    return (int) from;
                case ObjectType.Double:
                    return (double) from;
                case ObjectType.String:
                    return from.ToString();
                case ObjectType.Object:
                    return from;
            }
            
            //precaution, should never arrive here...
            return null;
        }
    }
}
