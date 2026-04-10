// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.SDK
{
	public sealed class FeeExtra
    {
        internal FeeExtra(int charged, int count, long feePerUnit, int included, string? name, long subtotal)
		{
			Charged = charged;
			Count = count;
			FeePerUnit = feePerUnit;
			Included = included;
			Name = name;
			Subtotal = subtotal;
		}

        internal static FeeExtra FromJson(string json)
        {
            return FromJson(JsonNode.Parse(json) ?? throw new ArgumentException(null, nameof(json)));
        }
		internal static FeeExtra FromJson(JsonNode jsonnode)
		{
			return new FeeExtra(
                charged: jsonnode["charged"]?.GetValue<int>() ?? throw new ArgumentException(null, "charged"), 
                count: jsonnode["count"]?.GetValue<int>() ?? throw new ArgumentException(null, "count"), 
                feePerUnit: jsonnode["fee_per_unit"]?.GetValue<long>() ?? throw new ArgumentException(null, "fee_per_unit"), 
                included: jsonnode["included"]?.GetValue<int>() ?? throw new ArgumentException(null, "included"), 
                name: jsonnode["name"]?.GetValue<string>(), 
                subtotal: jsonnode["subtotal"]?.GetValue<long>() ?? throw new ArgumentException(null, "subtotal"));
		}

		/// <include file="FeeExtra.cs.xml" path='docs/member[@name="P:.Charged"]/*' />
		public int Charged { get; }
        /// <include file="FeeExtra.cs.xml" path='docs/member[@name="P:.Count"]/*' />
        public int Count { get; }
        /// <include file="FeeExtra.cs.xml" path='docs/member[@name="P:.FeePerUnit"]/*' />
        public long FeePerUnit { get; }
        /// <include file="FeeExtra.cs.xml" path='docs/member[@name="P:.Included"]/*' />
        public int Included { get; }
        /// <include file="FeeExtra.cs.xml" path='docs/member[@name="P:.Name"]/*' />
        public string? Name { get; }
        public long Subtotal { get; }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is not FeeExtra that)
            {
                return false;
            }

            return 
                Charged == that.Charged &&
                Count == that.Count && 
                FeePerUnit == that.FeePerUnit && 
                Included == that.Included && 
                Subtotal == that.Subtotal && 
                Equals(Name, that.Name);
        }        
        public override int GetHashCode()
        {
            return HashCode.Combine(Charged, Count, FeePerUnit, Included, Name, Subtotal);
        }
    }
}