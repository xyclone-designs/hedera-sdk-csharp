// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;

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
        ScheduleDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
            ArgumentNullException.ThrowIfNull(scheduleId);
            RequireNotFrozen();
            scheduleId = scheduleId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.ScheduleDelete;
            if (body.ScheduleID is not null)
            {
                scheduleId = ScheduleId.FromProtobuf(body.ScheduleID);
            }
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.ScheduleDeleteTransactionBody builder }</returns>
        Proto.ScheduleDeleteTransactionBody Build()
        {
            var builder =  new Proto.ScheduleDeleteTransactionBody();
            if (scheduleId != null)
            {
                builder.ScheduleID = scheduleId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (scheduleId != null)
            {
                scheduleId.ValidateChecksum(client);
            }
        }
        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return ScheduleServiceGrpc.GetDeleteScheduleMethod();
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleDelete = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ScheduleDelete = Build();
        }
    }
}