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

		/// <summary>
		/// The charged count of items as calculated by max(0, count - included).
		/// </summary>
		public int Charged { get; }
        /// <summary>
        /// The actual count of items received.
        /// </summary>
        public int Count { get; }
        /// <summary>
        /// The fee price per unit in tinycents.
        /// </summary>
        public long FeePerUnit { get; }
        /// <summary>
        /// The count of this "extra" that is included for free.
        /// </summary>
        public int Included { get; }
        /// <summary>
        /// The unique name of this extra fee as defined in the fee schedule.
        /// </summary>
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