// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Java.Time;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class ExchangeRatesTest
    {
        private static readonly string exchangeRateSetHex = "0a1008b0ea0110b6b4231a0608f0bade9006121008b0ea01108cef231a060880d7de9006";
        public virtual void FromProtobuf()
        {
            byte[] exchangeRatesBytes = Hex.Decode(exchangeRateSetHex);
            ExchangeRates exchangeRates = ExchangeRates.FromBytes(exchangeRatesBytes);
            Assert.Equal(exchangeRates.currentRate.cents, 580150);
            Assert.Equal(exchangeRates.currentRate.hbars, 30000);
            DateTimeOffset currentExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1645714800);
            Assert.Equal(exchangeRates.currentRate.expirationTime, currentExpirationTime);
            Assert.Equal(exchangeRates.currentRate.exchangeRateInCents, 19.338333333333335);
            Assert.Equal(exchangeRates.nextRate.cents, 587660);
            Assert.Equal(exchangeRates.nextRate.hbars, 30000);
            DateTimeOffset nextExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1645718400);
            Assert.Equal(exchangeRates.nextRate.expirationTime, nextExpirationTime);
            Assert.Equal(exchangeRates.nextRate.exchangeRateInCents, 19.588666666666665);
        }
    }
}