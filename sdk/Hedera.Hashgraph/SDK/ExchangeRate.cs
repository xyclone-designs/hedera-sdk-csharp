using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Denotes a conversion between Hbars and cents (USD).
	 */
	public sealed class ExchangeRate
	{
		/**
		 * Denotes Hbar equivalent to cents (USD)
		 */
		public int Hbars { get; }
		/**
		 * Denotes cents (USD) equivalent to Hbar
		 */
		public int Cents { get; }
		/**
		 * Expiration time of this exchange rate
		 */
		public DateTimeOffset ExpirationTime { get; }
		/**
		 * Calculated exchange rate
		 */
		public double ExchangeRateInCents { get; }

		ExchangeRate(int hbars, int cents, DateTimeOffset expirationTime)
		{
			this.Hbars = hbars;
			this.Cents = cents;
			this.ExpirationTime = expirationTime;
			this.ExchangeRateInCents = (double)cents / (double)hbars;
		}

		/**
		 * Create an Exchange Rate from a protobuf.
		 *
		 * @param pb                        the protobuf
		 * @return                          the new exchange rate
		 */
		public static ExchangeRate FromProtobuf(Proto.ExchangeRate pb)
		{
			return new ExchangeRate(pb.HbarEquiv, pb.CentEquiv, DateTimeOffsetConverter.FromProtobuf(pb.ExpirationTime));
		}
	}

}