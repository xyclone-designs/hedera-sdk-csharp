// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.SDK.Fees
{
    public sealed class FeeEstimateResponse
    {
		internal FeeEstimateResponse(FeeEstimateMode mode, NetworkFee? networkFee, FeeEstimate? nodeFee, IEnumerable<string> notes, FeeEstimate? serviceFee, long total)
        {
            Mode = mode;
            NetworkFee = networkFee;
            NodeFee = nodeFee;
            Notes = notes.ToList().AsReadOnly();
            ServiceFee = serviceFee;
            Total = total;
        }

        internal static FeeEstimateResponse FromJson(string json, FeeEstimateMode defaultMode)
        {
            JsonNode root = JsonNode.Parse(json) ?? throw new ArgumentException(null, nameof(json));

            return new FeeEstimateResponse(
                ParseModeFromJson(root, defaultMode), 
                ParseNetworkFeeFromJson(root),
                ParseFeeEstimateFromJson(root, "node"),
                ParseNotesFromJson(root), 
                ParseFeeEstimateFromJson(root, "service"),
                ParseTotalFromJson(root));
        }

		/// <summary>
		/// The mode that was used to calculate the fees.
		/// </summary>
		public FeeEstimateMode Mode { get; }
		public NetworkFee? NetworkFee { get; }
		public FeeEstimate? NodeFee { get; }
		public FeeEstimate? ServiceFee { get; }
		public IReadOnlyList<string> Notes { get; }
		/// <summary>
		/// The sum of the network, node, and service subtotals in tinycents.
		/// </summary>
		public long Total { get; }

        private static NetworkFee? ParseNetworkFeeFromJson(JsonNode root)
        {
			if (root["network"] is JsonNode jsonnode)
				return NetworkFee.FromJson(jsonnode);

			return null;
        }
        private static FeeEstimate? ParseFeeEstimateFromJson(JsonNode root, string fieldName)
        {
            if (root[fieldName] is JsonNode jsonnode)
				return FeeEstimate.FromJson(jsonnode);

			return null;
        }
		private static long ParseTotalFromJson(JsonNode root)
		{
			return root["total"]?.GetValue<long>() ?? 0;
		}
		private static IEnumerable<string> ParseNotesFromJson(JsonNode root)
        {
            return root
                .GetValue<JsonArray>()
                .OfType<JsonNode>()
                .Select(_ => _.ToString()) ?? [];
        }
		private static FeeEstimateMode ParseModeFromJson(JsonNode root, FeeEstimateMode defaultMode)
		{
			return 
                Enum.TryParse(root["mode"]?.GetValue<string>(), true, out FeeEstimateMode mode) ? mode :
                Enum.TryParse(root["mode_used"]?.GetValue<string>(), true, out FeeEstimateMode mode_used) ? mode_used : defaultMode;
		}

		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o is not FeeEstimateResponse that)
            {
                return false;
            }

            return 
                Total == that.Total && 
                Mode == that.Mode && 
                Equals(NetworkFee, that.NetworkFee) && 
                Equals(NodeFee, that.NodeFee) && 
                Equals(Notes, that.Notes) && 
                Equals(ServiceFee, that.ServiceFee);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Mode, NetworkFee, NodeFee, Notes, ServiceFee, Total);
        }
    }
}