
using System.Linq;

namespace System
{
    public static class ArrayExtensions
	{
        public static T[] CopyArray<T>(this T[] ts)
        {
			T[] _out = [];
			ts.CopyTo(_out);
			return _out;
        }
		public static T[] CopyArray<T>(this T[] ts, int start = 0, int length = -1)
		{
			length = length == -1 ? ts.Length : length;

			return [.. ts.Skip(start).Take(length)];
		}
	}
}
