// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Denotes a conversion between Hbars and cents (USD).
    /// </summary>
    public sealed class ExchangeRate
    {
        public ExchangeRate(int hbars, int cents, DateTimeOffset expirationTime)
        {
            Hbars = hbars;
            Cents = cents;
            ExpirationTime = expirationTime;
            ExchangeRateInCents = (double)cents / (double)hbars;
        }

        /// <summary>
        /// Create an Exchange Rate from a protobuf.
        /// </summary>
        /// <param name="pb">the protobuf</param>
        /// <returns>                         the new exchange rate</returns>
        public static ExchangeRate FromProtobuf(Proto.ExchangeRate pb)
        {
            return new ExchangeRate(pb.HbarEquiv, pb.CentEquiv, DateTimeOffset.FromUnixTimeSeconds(pb.ExpirationTime.Seconds));
        }

		/// <summary>
		/// Denotes Hbar equivalent to cents (USD)
		/// </summary>
		public int Hbars { get; }
		/// <summary>
		/// Denotes cents (USD) equivalent to Hbar
		/// </summary>
		public int Cents { get; }
		/// <summary>
		/// Expiration time of this exchange rate
		/// </summary>
		public DateTimeOffset ExpirationTime { get; }
		/// <summary>
		/// Calculated exchange rate
		/// </summary>
		public double ExchangeRateInCents { get; }
	}
}