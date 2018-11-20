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

        public static bool TryConvertType(ObjectType type, out Type result) =>
            _conversionToType.TryGetValue(type, out result);

        public static bool IsComparable(ObjectType type) => _comparableTypes.Contains(type);

        public static bool IsNumeric(ObjectType type) => type == ObjectType.Double || type == ObjectType.Integer;

        public static bool CanConvert(ObjectType from, ObjectType to) => 
            _allowedImplicitCasting.TryGetValue(@from, out var conversions) && conversions.Contains(to);

        //enough for our purposes
        private static bool IsNumeric(this object obj)
        {   
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryConvert(object from, ObjectType toType, out object result)
        {
            result = null;
            switch (toType)
            {
                case ObjectType.Integer:
                    if (!from.IsNumeric()) //precaution
                        return false;
                    //numeric values in Quest can be only int and double
                    result = (int)(double)from;
                    return true;
                case ObjectType.Double:
                    if (!from.IsNumeric()) //precaution
                        return false;
                    result = (double)from;
                    return true;
                case ObjectType.String:
                    result = from.ToString();
                    return true;
                case ObjectType.Object:
                    result = from;
                    return true;
            }
            
            //precaution, should never arrive here...
            return false;
        }
    }
}
