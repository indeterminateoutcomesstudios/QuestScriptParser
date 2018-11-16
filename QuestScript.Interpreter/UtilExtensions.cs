using System;
using System.Collections.Generic;

namespace QuestScript.Interpreter
{
    public static class UtilExtensions
    {
        public static void EnsureKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> newValueFactory)
        {
            if (dict.ContainsKey(key) == false)
            {
                dict.Add(key, newValueFactory());
            }
        }
    }
}
