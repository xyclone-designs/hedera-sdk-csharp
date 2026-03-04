// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.SDK.Fees
{
	public sealed class FeeEstimate
    {
        internal FeeEstimate(long @base, IEnumerable<FeeExtra> extras)
        {
            Base = @base;
            Extras = extras.ToList().AsReadOnly();
        }
        internal static FeeEstimate FromJson(JsonNode feeEstimate)
        {
            long @base = GetLong(feeEstimate, "base", "base_fee");

            return new FeeEstimate(@base, feeEstimate["extras"]?.GetValue<JsonArray?>()?.OfType<JsonNode>().Select(_ => FeeExtra.FromJson(_)) ?? []);
        }

		private static long GetLong(JsonNode jsonnode, string primaryKey, string alternateKey)
		{
			return
				jsonnode[primaryKey]?.GetValue<long>() ??
				jsonnode[alternateKey]?.GetValue<long>() ??
				throw new ArgumentException("Missing expected fee estimate field: " + primaryKey);
		}

		/// <include file="FeeEstimate.cs.xml" path='docs/member[@name="P:.Base"]/*' />
		public long Base { get; }
		/// <include file="FeeEstimate.cs.xml" path='docs/member[@name="P:.Extras"]/*' />
		public IList<FeeExtra> Extras { get; }

		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is not FeeEstimate that)
            {
                return false;
            }

            return Base == that.Base && Equals(Extras, that.Extras);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Base, Extras);
        }
    }
}