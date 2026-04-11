// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="T:TopicUpdateTransaction"]/*' />
    public sealed class TopicUpdateTransaction : Transaction<TopicUpdateTransaction>
    {
        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor"]/*' />
        public TopicUpdateTransaction() { }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TopicUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TopicUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen"]/*' />
		public TopicId? TopicId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_2"]/*' />
		public string? TopicMemo { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_3"]/*' />
		public Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_4"]/*' />
		public Key? SubmitKey { get; set { RequireNotFrozen(); field = value; } }
		/*
         * An updated value for the number of seconds by which the topic expiration
         * will be automatically extended upon expiration, if it has a valid
         * auto-renew account.
         * <p>
         * If this value is set, the current `adminKey` for the topic MUST sign
         * this transaction.<br/>
         * This value, if set, MUST be greater than the
         * configured MIN_AUTORENEW_PERIOD.<br/>
         * This value, if set, MUST be less than the
         * configured MAX_AUTORENEW_PERIOD.
         *
         * @param autoRenewPeriod The TimeSpan to be set for auto renewal
         * @return {@code this}
         */
		public TimeSpan? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_5"]/*' />
		public AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_6"]/*' />
		public DateTimeOffset? ExpirationTime { get; set { RequireNotFrozen(); field = value; ExpirationTimeDuration = null; } }
		public TimeSpan? ExpirationTimeDuration { get; set { RequireNotFrozen(); field = value; ExpirationTime = null; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_7"]/*' />
		public Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="T:TopicUpdateTransaction_2"]/*' />
		public ListGuarded<Key> FeeExemptKeys
		{
			init; get => field ??= new ListGuarded<Key>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.InitFromTransactionBody"]/*' />
		public ListGuarded<CustomFixedFee> CustomFees
		{
			init; get => field ??= new ListGuarded<CustomFixedFee>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}


		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.InitFromTransactionBody_2"]/*' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusUpdateTopic;
            if (body.TopicID is not null)
                TopicId = TopicId.FromProtobuf(body.TopicID);

            if (body.AdminKey is not null)
                AdminKey = Key.FromProtobufKey(body.AdminKey);

            if (body.SubmitKey is not null)
                SubmitKey = Key.FromProtobufKey(body.SubmitKey);

            if (body.AutoRenewPeriod is not null)
                AutoRenewPeriod = body.AutoRenewPeriod.ToTimeSpan();

            if (body.AutoRenewAccount is not null)
                AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);

            if (body.Memo is not null)
                TopicMemo = body.Memo;

            if (body.ExpirationTime is not null)
                ExpirationTime = body.ExpirationTime.ToDateTimeOffset();

            if (body.FeeScheduleKey is not null)
                FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);

			if (body.FeeExemptKeyList is not null)
				FeeExemptKeys.ClearAndSet(body.FeeExemptKeyList.Keys.Select(_ => Key.FromProtobufKey(_)).OfType<Key>());

			if (body.CustomFees is not null)
				CustomFees.ClearAndSet(body.CustomFees.Fees.Select((x) => CustomFixedFee.FromProtobuf(x.FixedFee)));
		}

        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.ToProtobuf"]/*' />
        public Proto.ConsensusUpdateTopicTransactionBody ToProtobuf()
        {
            var builder = new Proto.ConsensusUpdateTopicTransactionBody();

            if (TopicId != null)
                builder.TopicID = TopicId?.ToProtobuf();

            if (AutoRenewAccountId != null)
                builder.AutoRenewAccount = AutoRenewAccountId?.ToProtobuf();

            if (AdminKey != null)
                builder.AdminKey = AdminKey?.ToProtobufKey();

            if (SubmitKey != null)
                builder.SubmitKey = SubmitKey?.ToProtobufKey();

            if (AutoRenewPeriod != null)
                builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

            if (TopicMemo != null)
				builder.Memo = TopicMemo;

			if (ExpirationTime != null)
                builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();

            if (ExpirationTimeDuration != null)
                builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();

            if (FeeScheduleKey != null)
                builder.FeeScheduleKey = FeeScheduleKey?.ToProtobufKey();

            if (FeeExemptKeys != null)
            {
                var feeExemptKeyList = new Proto.FeeExemptKeyList();
                
                foreach (var feeExemptKey in FeeExemptKeys)
                {
                    feeExemptKeyList.Keys.Add(feeExemptKey.ToProtobufKey());
                }

                builder.FeeExemptKeyList = feeExemptKeyList;
            }

            if (CustomFees != null)
            {
                var protoCustomFeeList = new Proto.FixedCustomFeeList();
                
                foreach (CustomFixedFee customFee in CustomFees)
                {
                    protoCustomFeeList.Fees.Add(customFee.ToTopicFeeProtobuf());
                }

                builder.CustomFees = protoCustomFeeList;
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TopicId?.ValidateChecksum(client);

			if ((AutoRenewAccountId != null) && !AutoRenewAccountId.Equals(new AccountId(0, 0, 0)))
				AutoRenewAccountId.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.ConsensusUpdateTopic = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.ConsensusUpdateTopic = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.ConsensusService.ConsensusServiceClient.updateTopic);

			return Proto.ConsensusService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}