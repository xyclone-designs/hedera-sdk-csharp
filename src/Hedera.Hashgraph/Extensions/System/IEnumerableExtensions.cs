
namespace System
{
    public static class ArrayExtensions
	{
        public static T[] CopyArray<T>(this T[] ts, int start = 0)
        {
			T[] _out = [];
			ts.CopyTo(_out, start);
			return _out;
        }
	}
}
