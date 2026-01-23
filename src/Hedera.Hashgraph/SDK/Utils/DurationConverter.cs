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
		internal static Duration FromProtobuf(Proto.Duration duration)
        {
            return Duration.FromTimeSpan(TimeSpan.FromSeconds(duration.Seconds));
        }

        /// <summary>
        /// Convert the duration object into a protobuf.
        /// </summary>
        /// <param name="duration">the duration object</param>
        /// <returns>                         the protobuf</returns>
        internal static Proto.Duration ToProtobuf(Duration duration)
        {
            return new Proto.Duration 
            {
                Seconds = duration.Seconds 
            };
        }
    }
}