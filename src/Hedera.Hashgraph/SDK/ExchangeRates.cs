// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Contains a set of Exchange Rates (current and next).
    /// </summary>
    public sealed class ExchangeRates
    {
        private ExchangeRates(ExchangeRate currentRate, ExchangeRate nextRate)
        {
            CurrentRate = currentRate;
            NextRate = nextRate;
        }

		/// <summary>
		/// Current Exchange Rate
		/// </summary>
		public ExchangeRate CurrentRate { get; }
		/// <summary>
		/// Next Exchange Rate
		/// </summary>
		public ExchangeRate NextRate { get; }

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
		/// <summary>
		/// Create an Exchange Rates from a protobuf.
		/// </summary>
		/// <param name="pb">the protobuf</param>
		/// <returns>                         the new exchange rates</returns>
		public static ExchangeRates FromProtobuf(Proto.ExchangeRateSet pb)
        {
            return new ExchangeRates(ExchangeRate.FromProtobuf(pb.CurrentRate), ExchangeRate.FromProtobuf(pb.NextRate));
        }
    }
}