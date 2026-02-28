using Hedera.Hashgraph.Proto;

namespace System
{
    public static class ProtoExtensions
	{
        public static TimeSpan ToTimeSpan(this Duration duration) 
        {
			return TimeSpan.FromSeconds(duration.Seconds);
		}
		public static Duration ToProtoDuration(this TimeSpan timeSpan)
		{
			return new Duration { Seconds = (long)timeSpan.TotalSeconds };
		}
		public static Timestamp ToProtoTimestamp(this TimeSpan timeSpan)
		{
			return DateTimeOffset.UtcNow.Add(timeSpan).ToProtoTimestamp();
		}
		public static TimestampSeconds ToProtoTimestampSeconds(this TimeSpan timeSpan)
		{
			return DateTimeOffset.UtcNow.Add(timeSpan).ToProtoTimestampSeconds();
		}

		public static DateTimeOffset ToDateTimeOffset(this Timestamp timestamp)
		{
			return DateTimeOffset.UnixEpoch
				.AddSeconds(timestamp.Seconds)
				.AddNanoseconds(timestamp.Nanos);
		}

		public static DateTimeOffset ToDateTimeOffset(this TimestampSeconds timestampSeconds)
		{
			return DateTimeOffset.UnixEpoch
				.AddSeconds(timestampSeconds.Seconds);
		}
		public static Timestamp ToProtoTimestamp(this DateTimeOffset dateTimeOffset)
		{
			return new Timestamp { Seconds = dateTimeOffset.ToUnixTimeSeconds(), Nanos = dateTimeOffset.Nanosecond };
		}
		public static TimestampSeconds ToProtoTimestampSeconds(this DateTimeOffset dateTimeOffset)
		{
			return new TimestampSeconds { Seconds = dateTimeOffset.ToUnixTimeSeconds() };
		}
	}
}
