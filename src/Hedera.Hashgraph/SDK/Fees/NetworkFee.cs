// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.SDK.Fees
{
    public sealed class NetworkFee
    {
        /// <summary>
        /// Multiplied by the node fee to determine the total network fee.
        /// </summary>
        public int Multiplier { get; }
        public long Subtotal { get; }

		internal NetworkFee(int multiplier, long subtotal)
        {
            this.Multiplier = multiplier;
            this.Subtotal = subtotal;
        }

        internal static NetworkFee FromJson(string json)
        {
            return FromJson(JsonNode.Parse(json) ?? throw new ArgumentException(null, nameof(json)));
        }
        internal static NetworkFee FromJson(JsonNode jsonnode)
        {
            return new NetworkFee(
				jsonnode["multiplier"]?.GetValue<int>() ?? throw new ArgumentException(null, "multiplier"),
				jsonnode["subtotal"]?.GetValue<long>() ?? throw new ArgumentException(null, "subtotal"));
        }
    }
}