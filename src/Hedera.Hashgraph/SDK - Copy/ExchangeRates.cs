// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Contains a set of Exchange Rates (current and next).
    /// </summary>
    public sealed class ExchangeRates
    {
        /// <summary>
        /// Current Exchange Rate
        /// </summary>
        public readonly ExchangeRate currentRate;
        /// <summary>
        /// Next Exchange Rate
        /// </summary>
        public readonly ExchangeRate nextRate;
        private ExchangeRates(ExchangeRate currentRate, ExchangeRate nextRate)
        {
            currentRate = currentRate;
            nextRate = nextRate;
        }

        /// <summary>
        /// Create an Exchange Rates from a protobuf.
        /// </summary>
        /// <param name="pb">the protobuf</param>
        /// <returns>                         the new exchange rates</returns>
        static ExchangeRates FromProtobuf(Proto.ExchangeRateSet pb)
        {
            return new ExchangeRates(ExchangeRate.FromProtobuf(pb.GetCurrentRate()), ExchangeRate.FromProtobuf(pb.GetNextRate()));
        }

        /// <summary>
        /// Create an Exchange Rates from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new exchange rates</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static ExchangeRates FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.ExchangeRateSet.Parser.ParseFrom(bytes));
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("currentRate", currentRate.ToString()).Add("nextRate", nextRate.ToString()).ToString();
        }
    }
}