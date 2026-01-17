using Google.Protobuf.WellKnownTypes;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Utility class used internally by the sdk.
     */
	public sealed class DurationConverter 
    {
        private DurationConverter() { }

        /**
         * Create a duration object from a protobuf.
         *
         * @param duration                  the duration protobuf
         * @return                          the duration object
         */
        public static Duration FromProtobuf(Proto.Duration duration) 
        {
            return Duration.FromTimeSpan(TimeSpan.FromSeconds(duration.Seconds));
        }
        /**
         * Convert the duration object into a protobuf.
         *
         * @param duration                  the duration object
         * @return                          the protobuf
         */
        public static Proto.Duration ToProtobuf(Duration duration) 
        {
			return new Proto.Duration 
            { 
                Seconds = duration.Seconds 
            };
        }
    }
}