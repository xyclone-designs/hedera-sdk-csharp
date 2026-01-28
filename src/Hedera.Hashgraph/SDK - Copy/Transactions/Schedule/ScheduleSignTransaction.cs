// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

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
        ScheduleSignTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
            ArgumentNullException.ThrowIfNull(scheduleId);
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
        public Proto.ScheduleSignTransactionBody Build()
        {
            var builder = new Proto.ScheduleSignTransactionBody();
            if (scheduleId != null)
            {
                builder.ScheduleID = scheduleId.ToProtobuf();
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.ScheduleSign;
            if (body.ScheduleID is not null)
            {
                scheduleId = ScheduleId.FromProtobuf(body.ScheduleID);
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
            return ScheduleServiceGrpc.SignScheduleMethod;
        }
        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleSign = Build();
        }
        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule ScheduleSignTransaction");
        }
    }
}