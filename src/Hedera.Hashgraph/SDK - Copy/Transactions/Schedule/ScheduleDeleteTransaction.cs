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
    /// Mark a schedule in the network state as deleted.
    /// 
    /// This transaction MUST be signed by the `adminKey` for the
    /// identified schedule.<br/>
    /// If a schedule does not have `adminKey` set or if `adminKey` is an empty
    /// `KeyList`, that schedule SHALL be immutable and MUST NOT be deleted.<br/>
    /// A deleted schedule SHALL not be executed.<br/>
    /// A deleted schedule MUST NOT be the subject of a subsequent
    /// `scheduleSign` transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class ScheduleDeleteTransaction : Transaction<ScheduleDeleteTransaction>
    {
        private ScheduleId scheduleId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleDeleteTransaction()
        {
            defaultMaxTransactionFee = new Hbar(5);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ScheduleDeleteTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ScheduleDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        public ScheduleDeleteTransaction SetScheduleId(ScheduleId scheduleId)
        {
            Objects.RequireNonNull(scheduleId);
            RequireNotFrozen();
            scheduleId = scheduleId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetScheduleDelete();
            if (body.HasScheduleID())
            {
                scheduleId = ScheduleId.FromProtobuf(body.GetScheduleID());
            }
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.ScheduleDeleteTransactionBody builder }</returns>
        ScheduleDeleteTransactionBody.Builder Build()
        {
            var builder = ScheduleDeleteTransactionBody.NewBuilder();
            if (scheduleId != null)
            {
                builder.SetScheduleID(scheduleId.ToProtobuf());
            }

            return builder;
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
            return ScheduleServiceGrpc.GetDeleteScheduleMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetScheduleDelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetScheduleDelete(Build());
        }
    }
}