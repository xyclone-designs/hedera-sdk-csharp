// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
{
    /// <summary>
    /// A query that returns information about the current state of a schedule
    /// transaction on a Hedera network.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/get-schedule-info">Hedera Documentation</a>
    /// </summary>
    public class ScheduleInfoQuery : Query<ScheduleInfo, ScheduleInfoQuery>
    {
        private ScheduleId scheduleId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleInfoQuery()
        {
        }

        /// <summary>
        /// Extract the schedule id.
        /// </summary>
        /// <returns>                         the schedule id</returns>
        public virtual ScheduleId GetScheduleId()
        {
            return scheduleId;
        }

        /// <summary>
        /// Assign the schedule id.
        /// </summary>
        /// <param name="scheduleId">the schedule id</param>
        /// <returns>{@code this}</returns>
        public virtual ScheduleInfoQuery SetScheduleId(ScheduleId scheduleId)
        {
            ArgumentNullException.ThrowIfNull(scheduleId);
            scheduleId = scheduleId;
            return this;
        }

        public override void ValidateChecksums(Client client)
        {
            if (scheduleId != null)
            {
                scheduleId.ValidateChecksum(client);
            }
        }

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.ScheduleGetInfoQuery()
            {
                Header = header
            };

            if (scheduleId != null)
            {
                builder.ScheduleID = scheduleId.ToProtobuf();
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

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return ScheduleServiceGrpc.GetGetScheduleInfoMethod();
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `Token_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}