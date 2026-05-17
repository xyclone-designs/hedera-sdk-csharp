using System.Collections.Generic;

namespace System.Linq
{
    public static class ICollectionsExtensions
    {
        public static void AddRange<T>(this ICollection<T> ts, params T[] values)
        {
            foreach (T value in values) ts.Add(value);
        }
        public static void AddRange<T>(this ICollection<T> ts, IEnumerable<T> values)
        {
            foreach (T value in values) ts.Add(value);
        }
	}
}
