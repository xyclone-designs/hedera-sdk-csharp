// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="T:TopicCreateTransaction"]/*' />
    public sealed class TopicCreateTransaction : Transaction<TopicCreateTransaction>
    {
        /// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.#ctor"]/*' />
        public TopicCreateTransaction()
        {
            AutoRenewPeriod = Transaction.DEFAULT_AUTO_RENEW_PERIOD;
            DefaultMaxTransactionFee = new Hbar(25);
        }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TopicCreateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TopicCreateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        private List<CustomFixedFee> _CustomFees = [];
        private List<Key> _FeeExemptKeys = [];

		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen"]/*' />
		public string TopicMemo { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen_2"]/*' />
		public Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen_3"]/*' />
		public Key? SubmitKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen_4"]/*' />
		public TimeSpan? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen_5"]/*' />
		public AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.RequireNotFrozen_6"]/*' />
		public Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="T:TopicCreateTransaction_2"]/*' />
		public ListGuarded<Key> FeeExemptKeys
		{
			init; get => field ??= new ListGuarded<Key>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.InitFromTransactionBody"]/*' />
		public ListGuarded<CustomFixedFee> CustomFees
		{
			init; get => field ??= new ListGuarded<CustomFixedFee>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.InitFromTransactionBody_2"]/*' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusCreateTopic;

            if (body.AutoRenewAccount is not null)
            {
                AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);
            }

            if (body.AdminKey is not null)
            {
                AdminKey = Key.FromProtobufKey(body.AdminKey);
            }

            if (body.SubmitKey is not null)
            {
                SubmitKey = Key.FromProtobufKey(body.SubmitKey);
            }

            if (body.AutoRenewPeriod is not null)
            {
                AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();
            }

            if (body.FeeScheduleKey is not null)
            {
                FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);
            }

            if (body.FeeExemptKeyList is not null)
            {
                FeeExemptKeys.ClearAndSet(body.FeeExemptKeyList.Select(_ => Key.FromProtobufKey(_)).OfType<Key>());
            }

            if (body.CustomFees is not null)
            {
                CustomFees.ClearAndSet(body.CustomFees.Select((x) => CustomFixedFee.FromProtobuf(x.FixedFee)));
            }

            TopicMemo = body.Memo;
        }

		/// <include file="TopicCreateTransaction.cs.xml" path='docs/member[@name="M:TopicCreateTransaction.ToProtobuf"]/*' />
		public Proto.Services.ConsensusCreateTopicTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ConsensusCreateTopicTransactionBody
			{
				Memo = TopicMemo
			};
            
            if (AutoRenewAccountId != null)
				builder.AutoRenewAccount = AutoRenewAccountId.ToProtobuf();

            if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

            if (SubmitKey != null)
				builder.SubmitKey = SubmitKey.ToProtobufKey();

            if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

            if (FeeScheduleKey != null)
				builder.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();

			foreach (var feeExemptKey in FeeExemptKeys)
				builder.FeeExemptKeyList.Add(feeExemptKey.ToProtobufKey());

			foreach (CustomFixedFee customFee in CustomFees)
				builder.CustomFees.Add(customFee.ToTopicFeeProtobuf());

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			AutoRenewAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusCreateTopic = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusCreateTopic = ToProtobuf();
        }
        public override TopicCreateTransaction FreezeWith(Client? client)
        {
            if (client.OperatorAccountId != null && AutoRenewAccountId == null)
            {
				AutoRenewAccountId = TransactionIds != null && TransactionIds.Count != 0 && TransactionIds.Current != null ? TransactionIds.Current.AccountId : client.OperatorAccountId;
            }

            return base.FreezeWith(client);
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ConsensusService.ConsensusServiceClient.createTopic);

			return Proto.Services.ConsensusService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
