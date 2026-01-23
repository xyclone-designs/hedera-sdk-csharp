// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;

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
            Objects.RequireNonNull(scheduleId);
            scheduleId = scheduleId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (scheduleId != null)
            {
                scheduleId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = ScheduleGetInfoQuery.NewBuilder();
            if (scheduleId != null)
            {
                builder.SetScheduleID(scheduleId.ToProtobuf());
            }

            queryBuilder.SetScheduleGetInfo(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetScheduleGetInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetScheduleGetInfo().GetHeader();
        }

        override ScheduleInfo MapResponse(Response response, AccountId nodeId, Query request)
        {
            return ScheduleInfo.FromProtobuf(response.GetScheduleGetInfo().GetScheduleInfo());
        }

        override MethodDescriptor<Query, Response> GetMethodDescriptor()
        {
            return ScheduleServiceGrpc.GetGetScheduleInfoMethod();
        }

        public override CompletableFuture<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `Token_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}