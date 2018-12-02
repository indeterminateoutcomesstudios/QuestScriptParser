using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Helpers
{
    public static class TypeUtil
    {
        private static readonly IReadOnlyList<ObjectType> ComparableTypes =
            new List<ObjectType> {ObjectType.Integer, ObjectType.Double};

        private static readonly Dictionary<ObjectType, ObjectType[]> AllowedImplicitCasting =
            new Dictionary<ObjectType, ObjectType[]>
            {
                {ObjectType.Integer, new[] {ObjectType.Double, ObjectType.String, ObjectType.Object}},
                {ObjectType.Double, new[] {ObjectType.Integer, ObjectType.String, ObjectType.Object}},
                {ObjectType.Boolean, new[] {ObjectType.String, ObjectType.Object}},
                {ObjectType.Object, new[] {ObjectType.String}},
                {ObjectType.String, new[] {ObjectType.Object}}
            };

        private static readonly Dictionary<ObjectType, Type> ConversionToType = new Dictionary<ObjectType, Type>
        {
            {ObjectType.Object, typeof(object)},
            {ObjectType.Double, typeof(double)},
            {ObjectType.Boolean, typeof(bool)},
            {ObjectType.Integer, typeof(int)},
            {ObjectType.String, typeof(string)},
            {ObjectType.List, typeof(ArrayList)},
            {ObjectType.Void, typeof(void)}
        };

        private static readonly Dictionary<string, ObjectType> AlternativeObjectTypeNames;

        //since the mapping is 1:1 - there is no issue here
        private static readonly Dictionary<Type, ObjectType> ConversionFromType =
            ConversionToType.ToDictionary(x => x.Value, x => x.Key);

        static TypeUtil()
        {
            AlternativeObjectTypeNames =
                typeof(ObjectType).GetFields().Where(f => Enum.TryParse<ObjectType>(f.Name, true, out _)).Select(f =>
                    new
                    {
                        Attribute = f.GetCustomAttribute<AlternativeNameAttribute>(),
                        Enum = Enum.Parse(typeof(ObjectType), f.Name, true)
                    }).Where(x => x.Attribute != null).ToDictionary(x => x.Attribute.Name, x => (ObjectType) x.Enum);
        }

        public static bool TryParse(string value, out ObjectType result)
        {
            result = ObjectType.Unknown;

            if (Enum.TryParse(value, true, out result))
                return true;

            //now, perhaps we have alternative names that fit?
            if (AlternativeObjectTypeNames.TryGetValue(value, out result)) return true;


            return false;
        }

        public static bool TryConvertType(ObjectType type, out Type result)
        {
            return ConversionToType.TryGetValue(type, out result);
        }

        public static bool TryConvertType(Type type, out ObjectType result)
        {
            return ConversionFromType.TryGetValue(type, out result);
        }

        public static bool IsComparable(ObjectType type)
        {
            return ComparableTypes.Contains(type);
        }

        public static bool IsNumeric(ObjectType type)
        {
            return type == ObjectType.Double || type == ObjectType.Integer;
        }

        public static bool CanConvert(ObjectType from, ObjectType to)
        {
            return AllowedImplicitCasting.TryGetValue(from, out var conversions) && conversions.Contains(to);
        }

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
                    result = (int) (double) from;
                    return true;
                case ObjectType.Double:
                    if (!from.IsNumeric()) //precaution
                        return false;
                    result = (double) from;
                    return true;
                case ObjectType.String:
                    result = from.ToString();
                    return true;
                case ObjectType.Object:
                    result = from;
                    return true;
                case ObjectType.Boolean:
                    if (!(from is bool))
                        return false;
                    result = (bool) from;
                    return true;
            }

            //precaution, should never arrive here...
            return false;
        }
    }
}