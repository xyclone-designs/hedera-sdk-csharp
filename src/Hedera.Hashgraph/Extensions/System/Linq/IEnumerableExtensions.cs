using System.Collections.Generic;

namespace System.Linq
{
    public static class IEnumerableExtensions
	{
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> ts) where T : ICloneable
        {
            return ts.Select(_ => (T)_.Clone());
        }
		public static List<T> CloneList<T>(this IEnumerable<T> ts) where T : ICloneable
		{
			return [.. ts.Clone()];
		}
	}
}
