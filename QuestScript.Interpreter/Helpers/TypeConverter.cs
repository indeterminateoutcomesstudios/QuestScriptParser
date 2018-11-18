using System.Collections.Generic;
using System.Linq;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Helpers
{
    public static class TypeConverter
    {
        private static Dictionary<ObjectType, ObjectType[]> _allowedImplicitCasting = new Dictionary<ObjectType, ObjectType[]>
        {
            { ObjectType.Integer, new []{ ObjectType.Double, ObjectType.String, ObjectType.Object } },
            { ObjectType.Double, new []{ ObjectType.Integer, ObjectType.String, ObjectType.Object } },
            { ObjectType.Boolean, new []{ ObjectType.String, ObjectType.Object } },
            { ObjectType.Object, new []{ ObjectType.String } },
            { ObjectType.String, new []{ ObjectType.Object } },
        };

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
