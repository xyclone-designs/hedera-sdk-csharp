using System.Collections.Generic;

namespace System.Linq
{
    public static class IDictionaryExtensions
    {
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> ts, TKey key, TValue value)
        {
            if (ts.ContainsKey(key))
                ts[key] = value;
            else ts.Add(key, value);
        }
	}
}
