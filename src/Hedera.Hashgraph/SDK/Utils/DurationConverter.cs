// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using System;

namespace Hedera.Hashgraph.SDK.Utils
{
	/// <summary>
	/// Utility class used internally by the sdk.
	/// </summary>
	internal static class DurationConverter
    {
		/// <summary>
		/// Create a duration object from a protobuf.
		/// </summary>
		/// <param name="duration">the duration protobuf</param>
		/// <returns>                         the duration object</returns>
		internal static TimeSpan FromProtobuf(Proto.Duration duration)
        {
            return TimeSpan.FromSeconds(duration.Seconds);
        }

        /// <summary>
        /// Convert the duration object into a protobuf.
        /// </summary>
        /// <param name="duration">the duration object</param>
        /// <returns>                         the protobuf</returns>
        internal static Proto.Duration ToProtobuf(TimeSpan duration)
        {
            return new Proto.Duration 
            {
                Seconds = duration.Seconds 
            };
        }
    }
}