using System;
using System.Collections.Generic;
using System.Text;

namespace JackFruit
{
    public static class Extensions
    {
        public static TValue ValueOr<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
            { return value; }
            return defaultValue;
        }
    }
}
