// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
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

namespace Hedera.Hashgraph.SDK.Transactions.Schedule
{
    /// <summary>
    /// A transaction that appends signatures to a schedule transaction.
    /// You will need to know the schedule ID to reference the schedule
    /// transaction to submit signatures to. A record will be generated
    /// for each ScheduleSign transaction that is successful and the schedule
    /// entity will subsequently update with the public keys that have signed
    /// the schedule transaction. To view the keys that have signed the
    /// schedule transaction, you can query the network for the schedule info.
    /// Once a schedule transaction receives the last required signature, the
    /// schedule transaction executes.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/schedule-transaction/sign-a-schedule-transaction">Hedera Documentation</a>
    /// </summary>
    public sealed class ScheduleSignTransaction : Transaction<ScheduleSignTransaction>
    {
        private ScheduleId scheduleId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleSignTransaction()
        {
            defaultMaxTransactionFee = new Hbar(5);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ScheduleSignTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the schedule id.
        /// </summary>
        /// <returns>                         the schedule id</returns>
        public ScheduleId GetScheduleId()
        {
            return scheduleId;
        }

        /// <summary>
        /// A schedule identifier.
        /// <p>
        /// This MUST identify the schedule which SHALL be deleted.
        /// </summary>
        /// <param name="scheduleId">the schedule id</param>
        /// <returns>{@code this}</returns>
        public ScheduleSignTransaction SetScheduleId(ScheduleId scheduleId)
        {
            Objects.RequireNonNull(scheduleId);
            RequireNotFrozen();
            scheduleId = scheduleId;
            return this;
        }

        /// <summary>
        /// Clears the schedule id
        /// </summary>
        /// <returns>{@code this}</returns>
        public ScheduleSignTransaction ClearScheduleId()
        {
            RequireNotFrozen();
            scheduleId = null;
            return this;
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ScheduleSignTransactionBody
        ///         builder }</returns>
        ScheduleSignTransactionBody.Builder Build()
        {
            var builder = ScheduleSignTransactionBody.NewBuilder();
            if (scheduleId != null)
            {
                builder.SetScheduleID(scheduleId.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetScheduleSign();
            if (body.HasScheduleID())
            {
                scheduleId = ScheduleId.FromProtobuf(body.GetScheduleID());
            }
        }

        override void ValidateChecksums(Client client)
        {
            if (scheduleId != null)
            {
                scheduleId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return ScheduleServiceGrpc.GetSignScheduleMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetScheduleSign(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            throw new NotSupportedException("cannot schedule ScheduleSignTransaction");
        }
    }
}