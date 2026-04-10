// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="ExchangeRates.cs.xml" path='docs/member[@name="T:ExchangeRates"]/*' />
    public sealed class ExchangeRates
    {
        private ExchangeRates(ExchangeRate currentRate, ExchangeRate nextRate)
        {
            CurrentRate = currentRate;
            NextRate = nextRate;
        }

		/// <include file="ExchangeRates.cs.xml" path='docs/member[@name="P:ExchangeRates.CurrentRate"]/*' />
		public ExchangeRate CurrentRate { get; }
		/// <include file="ExchangeRates.cs.xml" path='docs/member[@name="P:ExchangeRates.NextRate"]/*' />
		public ExchangeRate NextRate { get; }

		/// <include file="ExchangeRates.cs.xml" path='docs/member[@name="M:ExchangeRates.FromBytes(System.Byte[])"]/*' />
		public static ExchangeRates FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.ExchangeRateSet.Parser.ParseFrom(bytes));
		}
		/// <include file="ExchangeRates.cs.xml" path='docs/member[@name="M:ExchangeRates.FromProtobuf(Proto.ExchangeRateSet)"]/*' />
		public static ExchangeRates FromProtobuf(Proto.ExchangeRateSet pb)
        {
            return new ExchangeRates(ExchangeRate.FromProtobuf(pb.CurrentRate), ExchangeRate.FromProtobuf(pb.NextRate));
        }
    }
}