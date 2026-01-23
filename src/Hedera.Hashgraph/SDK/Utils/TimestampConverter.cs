// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.SDK.Utils
{
	/// <summary>
	/// Instance in time utilities.
	/// </summary>
	internal sealed class TimestampConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        private TimestampConverter()
        {
        }

		/// <summary>
		/// Create an instance from a timestamp protobuf.
		/// </summary>
		/// <param name="timestamp">the protobuf</param>
		/// <returns>                         the instance</returns>
		internal static Timestamp FromProtobuf(Proto.Timestamp timestamp)
        {
            return new Timestamp
            {
				Seconds = timestamp.Seconds,
				Nanos = timestamp.Nanos,
			};
        }

		/// <summary>
		/// Create an instance from a timestamp in seconds protobuf.
		/// </summary>
		/// <param name="timestampSeconds">the protobuf</param>
		/// <returns>                         the instance</returns>
		internal static Timestamp FromProtobuf(Proto.TimestampSeconds timestampSeconds)
        {
			return new Timestamp
			{
				Seconds = timestampSeconds.Seconds,
			};
		}

		/// <summary>
		/// Convert an instance into a timestamp.
		/// </summary>
		/// <param name="instant">the instance</param>
		/// <returns>                         the timestamp</returns>
		internal static Proto.Timestamp ToProtobuf(Timestamp timestamp)
        {
			return new Proto.Timestamp
			{
				Seconds = timestamp.Seconds,
				Nanos = timestamp.Nanos,
			};
		}

		internal static Proto.Timestamp ToProtobuf(Duration duration)
        {
			return new Proto.Timestamp
			{
				Seconds = duration.Seconds,
				Nanos = duration.Nanos,
			};
		}

        /// <summary>
        /// Convert an instance into a timestamp in seconds.
        /// </summary>
        /// <param name="instant">the instance</param>
        /// <returns>                         the timestamp in seconds</returns>
        internal static Proto.TimestampSeconds ToSecondsProtobuf(Timestamp timestamp)
        {
			return new Proto.TimestampSeconds
			{
				Seconds = timestamp.Seconds,
			};
		}
    }
}