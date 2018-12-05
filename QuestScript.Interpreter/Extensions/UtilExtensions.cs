using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FastMember;
using QuestScript.Interpreter.ScriptElements;

namespace QuestScript.Interpreter.Extensions
{
    public static class UtilExtensions
    {
        private static readonly TypeAccessor LazyTypeAccessor;

        static UtilExtensions()
        {
            LazyTypeAccessor = TypeAccessor.Create(typeof(Lazy<object>));
        }

        public static MethodDefinition ToMethodDefinition(this FunctionDefinition functionDefinition,string delegateFieldName) => 
            new MethodDefinition(functionDefinition.Name,delegateFieldName,functionDefinition.Parameters,functionDefinition.ReturnType,functionDefinition.Implementation);


        public static void MergeWith<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            IDictionary<TKey, TValue> otherDict)
        {
            foreach (var kvp in otherDict)
            {
                if (!dict.TryGetValue(kvp.Key, out _))
                    dict.Add(kvp.Key, kvp.Value);
            }
        }


        public static void MergeWith<TKey, TData>(this IDictionary<TKey, ICollection<TData>> dict, IDictionary<TKey, ICollection<TData>> otherDict)
        {
            foreach (var kvp in otherDict)
            {
                if (dict.TryGetValue(kvp.Key, out var collection))
                {
                    foreach(var otherItem in kvp.Value)
                        collection.Add(otherItem);
                }
                else
                {
                    dict.Add(kvp.Key,kvp.Value);
                }
            }
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