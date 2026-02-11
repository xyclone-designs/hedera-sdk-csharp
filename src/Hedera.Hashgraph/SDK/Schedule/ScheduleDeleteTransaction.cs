// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Schedule
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleDeleteTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal ScheduleDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal ScheduleDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A schedule identifier.
        /// <p>
        /// This MUST identify the schedule which SHALL be deleted.
        /// </summary>
        /// <param name="scheduleId">the schedule id</param>
        /// <returns>{@code this}</returns>
        public ScheduleId? ScheduleId
		{
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            if (SourceTransactionBody.ScheduleDelete.ScheduleID is not null)
				ScheduleId = ScheduleId.FromProtobuf(SourceTransactionBody.ScheduleDelete.ScheduleID);
		}

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.ScheduleDeleteTransactionBody builder }</returns>
        public Proto.ScheduleDeleteTransactionBody Build()
        {
			Proto.ScheduleDeleteTransactionBody proto =  new ();
            
            if (ScheduleId != null)
				proto.ScheduleID = ScheduleId.ToProtobuf();

			return proto;
        }

        public override void ValidateChecksums(Client client)
        {
			ScheduleId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleDelete = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ScheduleDelete = Build();
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return ScheduleServiceGrpc.DeleteScheduleMethod;
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
        }
        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}