// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Java.Time;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Denotes a conversion between Hbars and cents (USD).
    /// </summary>
    public sealed class ExchangeRate
    {
        /// <summary>
        /// Denotes Hbar equivalent to cents (USD)
        /// </summary>
        public readonly int hbars;
        /// <summary>
        /// Denotes cents (USD) equivalent to Hbar
        /// </summary>
        public readonly int cents;
        /// <summary>
        /// Expiration time of this exchange rate
        /// </summary>
        public readonly Instant expirationTime;
        /// <summary>
        /// Calculated exchange rate
        /// </summary>
        public readonly double exchangeRateInCents;
        ExchangeRate(int hbars, int cents, Instant expirationTime)
        {
            hbars = hbars;
            cents = cents;
            expirationTime = expirationTime;
            exchangeRateInCents = (double)cents / (double)hbars;
        }

        /// <summary>
        /// Create an Exchange Rate from a protobuf.
        /// </summary>
        /// <param name="pb">the protobuf</param>
        /// <returns>                         the new exchange rate</returns>
        static ExchangeRate FromProtobuf(Proto.ExchangeRate pb)
        {
            return new ExchangeRate(pb.GetHbarEquiv(), pb.GetCentEquiv(), InstantConverter.FromProtobuf(pb.GetExpirationTime()));
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("hbars", hbars).Add("cents", cents).Add("expirationTime", expirationTime).Add("exchangeRateInCents", exchangeRateInCents).ToString();
        }
    }
}