
namespace System
{
    public static class DateTimeOffsetExtensions
	{
		private const long NanosecondsPerTick = 100; // 1 tick = 100ns

		public static DateTimeOffset AddNanoseconds(this DateTimeOffset value, long nanoseconds)
		{
			long ticksToAdd = nanoseconds / NanosecondsPerTick;

			return value.AddTicks(ticksToAdd);
		}
	}
}
