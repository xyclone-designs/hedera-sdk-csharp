// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ExchangeRate.cs.xml" path='docs/member[@name="T:ExchangeRate"]/*' />
    public sealed class ExchangeRate
    {
        public ExchangeRate(int hbars, int cents, DateTimeOffset expirationTime)
        {
            Hbars = hbars;
            Cents = cents;
            ExpirationTime = expirationTime;
            ExchangeRateInCents = (double)cents / (double)hbars;
        }

        /// <include file="ExchangeRate.cs.xml" path='docs/member[@name="M:ExchangeRate.FromProtobuf(Proto.Services.ExchangeRate)"]/*' />
        public static ExchangeRate FromProtobuf(Proto.Services.ExchangeRate pb)
        {
            return new ExchangeRate(pb.HbarEquiv, pb.CentEquiv, DateTimeOffset.FromUnixTimeSeconds(pb.ExpirationTime.Seconds));
        }

		/// <include file="ExchangeRate.cs.xml" path='docs/member[@name="P:ExchangeRate.Hbars"]/*' />
		public int Hbars { get; }
		/// <include file="ExchangeRate.cs.xml" path='docs/member[@name="P:ExchangeRate.Cents"]/*' />
		public int Cents { get; }
		/// <include file="ExchangeRate.cs.xml" path='docs/member[@name="P:ExchangeRate.ExpirationTime"]/*' />
		public DateTimeOffset ExpirationTime { get; }
		/// <include file="ExchangeRate.cs.xml" path='docs/member[@name="P:ExchangeRate.ExchangeRateInCents"]/*' />
		public double ExchangeRateInCents { get; }
	}
}
