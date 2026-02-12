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
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScheduleSignTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public ScheduleSignTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// A schedule identifier.
		/// <p>
		/// This MUST identify the schedule which SHALL be deleted.
		/// </summary>
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
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ScheduleSignTransactionBody
        ///         builder }</returns>
        public Proto.ScheduleSignTransactionBody ToProtobuf()
        {
            var builder = new Proto.ScheduleSignTransactionBody();
            
            if (ScheduleId != null)
            {
                builder.ScheduleID = ScheduleId.ToProtobuf();
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ScheduleSign;

            if (body.ScheduleID is not null)
            {
                ScheduleId = ScheduleId.FromProtobuf(body.ScheduleID);
            }
        }

        public override void ValidateChecksums(Client client)
        {
			ScheduleId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ScheduleSign = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule ScheduleSignTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.ScheduleService.ScheduleServiceClient.signSchedule);

			return Proto.ScheduleService.Descriptor.FindMethodByName(methodname);
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