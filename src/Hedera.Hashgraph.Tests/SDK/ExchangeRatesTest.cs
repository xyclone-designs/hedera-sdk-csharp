// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class ExchangeRatesTest
    {
        private static readonly string exchangeRateSetHex = "0a1008b0ea0110b6b4231a0608f0bade9006121008b0ea01108cef231a060880d7de9006";
        public virtual void FromProtobuf()
        {
            byte[] exchangeRatesBytes = Hex.Decode(exchangeRateSetHex);
            ExchangeRates exchangeRates = ExchangeRates.FromBytes(exchangeRatesBytes);
            Assert.Equal(exchangeRates.CurrentRate.Cents, 580150);
            Assert.Equal(exchangeRates.CurrentRate.Hbars, 30000);
            DateTimeOffset currentExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1645714800);
            Assert.Equal(exchangeRates.CurrentRate.ExpirationTime, currentExpirationTime);
            Assert.Equal(exchangeRates.CurrentRate.ExchangeRateInCents, 19.338333333333335);
            Assert.Equal(exchangeRates.NextRate.Cents, 587660);
            Assert.Equal(exchangeRates.NextRate.Hbars, 30000);
            DateTimeOffset nextExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1645718400);
            Assert.Equal(exchangeRates.NextRate.ExpirationTime, nextExpirationTime);
            Assert.Equal(exchangeRates.NextRate.ExchangeRateInCents, 19.588666666666665);
        }
    }
}