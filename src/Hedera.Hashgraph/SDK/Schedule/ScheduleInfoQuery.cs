// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Schedule
{
    /// <summary>
    /// A query that returns information about the current state of a schedule
    /// transaction on a Hedera network.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/get-schedule-info">Hedera Documentation</a>
    /// </summary>
    public class ScheduleInfoQuery : Query<ScheduleInfo, ScheduleInfoQuery>
    {
        /// <summary>
        /// Assign the schedule id.
        /// </summary>
        /// <param name="ScheduleId">the schedule id</param>
        /// <returns>{@code this}</returns>
        public virtual ScheduleId? ScheduleId { get; set; }

		public override void ValidateChecksums(Client client)
        {
            if (ScheduleId != null)
            {
                ScheduleId.ValidateChecksum(client);
            }
        }

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ScheduleGetInfoQuery()
            {
                Header = header
            };

            if (ScheduleId != null)
            {
                builder.ScheduleID = ScheduleId.ToProtobuf();
            }

            queryBuilder.ScheduleGetInfo = builder;
        }

        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.ScheduleGetInfo.Header;
        }

        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.ScheduleGetInfo.Header;
        }

        public override ScheduleInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return ScheduleInfo.FromProtobuf(response.ScheduleGetInfo.ScheduleInfo);
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.ScheduleService.ScheduleServiceClient.getScheduleInfo);

			return Proto.ScheduleService.Descriptor.FindMethodByName(methodname);
		}

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));

		}
	}
}