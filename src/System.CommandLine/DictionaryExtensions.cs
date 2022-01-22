using System.Collections.Generic;

namespace System.CommandLine
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            Func<TKey, TValue> create)
        {
            if (source.TryGetValue(key, out TValue? value))
            {
                return value;
            }
            else
            {
                value = create(key);

                source.Add(key, value);

                return value;
            }
        }

        public static bool TryAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            TValue value)
        {
            if (source.ContainsKey(key))
            {
                return false;
            }
            else
            {
                source.Add(key, value);
                return true;
            }
        }
    }
}