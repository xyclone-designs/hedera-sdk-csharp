using Google.Protobuf.WellKnownTypes;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Instance in time utilities.
     */
    public sealed class DateTimeOffsetConverter {
        /**
         * Constructor.
         */
        private DateTimeOffsetConverter() {}

		/**
         * Create an instance from a timestamp protobuf.
         *
         * @param timestamp                 the protobuf
         * @return                          the instance
         */
		public static DateTimeOffset FromProtobuf(Proto.Timestamp timestamp) 
        {
		    return DateTimeOffset.UnixEpoch
                .AddSeconds(timestamp.Seconds)
                .AddTicks(timestamp.Nanos / 100);
        }
		/**
         * Create an instance from a timestamp in seconds protobuf.
         *
         * @param timestampSeconds          the protobuf
         * @return                          the instance
         */
		public static DateTimeOffset FromProtobuf(Proto.TimestampSeconds timestampSeconds)
		{
			return DateTimeOffset.UnixEpoch
				.AddSeconds(timestampSeconds.Seconds);
		}

		/**
         * Convert an instance into a timestamp.
         *
         * @param instant                   the instance
         * @return                          the timestamp
         */
		public static Proto.Timestamp ToProtobuf(DateTimeOffset instant) 
        {
            return new Proto.Timestamp
            {
                Seconds = instant.Second,
                Nanos = instant.Nanosecond,
            };
        }
        public static Proto.Timestamp ToProtobuf(Duration duration)
		{
			return new Proto.Timestamp
			{
				Seconds = duration.Seconds,
				Nanos = duration.Nanos,
			};
		}
		/**
         * Convert an instance into a timestamp in seconds.
         *
         * @param instant                   the instance
         * @return                          the timestamp in seconds
         */
		public static Proto.TimestampSeconds ToSecondsProtobuf(DateTimeOffset instant)
		{
			return new TimestampSeconds
			{
				Seconds = instant.Second,
			};
		}
    }
}