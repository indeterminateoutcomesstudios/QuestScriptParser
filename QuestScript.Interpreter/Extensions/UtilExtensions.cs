using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FastMember;

namespace QuestScript.Interpreter.Extensions
{
    public static class UtilExtensions
    {
        private static readonly TypeAccessor LazyTypeAccessor;

        static UtilExtensions()
        {
            LazyTypeAccessor = TypeAccessor.Create(typeof(Lazy<object>));
        }     

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }

        public static string CleanTokenArtifacts(this string str)
        {
            return str.Replace("\"", string.Empty);
        }

        public static void EnsureKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            Func<TValue> newValueFactory)
        {
            if (dict.ContainsKey(key) == false) dict.Add(key, newValueFactory());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetValueOrLazyValue(this object valueOrLazy)
        {
            while (true)
            {
                if (!(valueOrLazy is Lazy<object>))
                    return valueOrLazy;
                valueOrLazy = LazyTypeAccessor[valueOrLazy, "Value"];
            }
        }
    }
}