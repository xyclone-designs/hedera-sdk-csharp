// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Collections.Generic;
using System.Linq;
using Hedera.Hashgraph.SDK.Core;

namespace Hedera.Hashgraph.SDK.Consensus
{
    /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="T:TopicUpdateTransaction"]' />
    public sealed class TopicUpdateTransaction : Transaction<TopicUpdateTransaction>
    {
        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor"]' />
        public TopicUpdateTransaction() { }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]' />
		internal TopicUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]' />
		internal TopicUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen"]' />
		public TopicId? TopicId 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_2"]' />
		public string? TopicMemo 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_3"]' />
		public Key? AdminKey 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_4"]' />
		public Key? SubmitKey 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.AutoRenewPeriod"]' />
        public TimeSpan? AutoRenewPeriod 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_5"]' />
		public AccountId? AutoRenewAccountId 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_6"]' />
        public DateTimeOffset? ExpirationTime
        {
            get;
            set
            {
                RequireNotFrozen();
                field = value;

                if (field == null && ExpirationTimeDuration is not null)
                    ExpirationTimeDuration = null;
            }
        }
        public TimeSpan? ExpirationTimeDuration
        {
            get;
            set
            {
                RequireNotFrozen();
                field = value;

                if (field == null && ExpirationTime is not null)
                    ExpirationTime = null;
            }
        }
        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.RequireNotFrozen_7"]' />
        public Key? FeeScheduleKey 
        { 
            get; 
            set { RequireNotFrozen(); field = value; } 
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="T:TopicUpdateTransaction_2"]' />
		public ListGuarded<Key> FeeExemptKeys
		{
            init => field = GenerateListGuarded(value);
            get => field ??= GenerateListGuarded<Key>();
        }
		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.InitFromTransactionBody"]' />
		public ListGuarded<CustomFixedFee> CustomFees
		{
            init => field = GenerateListGuarded(value);
            get => field ??= GenerateListGuarded<CustomFixedFee>();
        }

		/// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.InitFromTransactionBody_2"]' />
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ConsensusUpdateTopic;
            if (body.TopicId is not null)
                TopicId = TopicId.FromProtobuf(body.TopicId);

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
				FeeExemptKeys.Operate(_ => body.FeeExemptKeyList.Keys.Select(_ => Key.FromProtobufKey(_)).OfType<Key>());

			if (body.CustomFees is not null)
				CustomFees.Operate(_ => body.CustomFees.Fees.Select((x) => CustomFixedFee.FromProtobuf(x.FixedFee)));
		}

        /// <include file="TopicUpdateTransaction.cs.xml" path='docs/member[@name="M:TopicUpdateTransaction.ToProtobuf"]' />
        public Proto.Services.ConsensusUpdateTopicTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.ConsensusUpdateTopicTransactionBody();

            if (TopicId != null)
                builder.TopicId = TopicId.ToProtobuf();

            if (AutoRenewAccountId != null)
                builder.AutoRenewAccount = AutoRenewAccountId.ToProtobuf();

            if (AdminKey != null)
                builder.AdminKey = AdminKey.ToProtobufKey();

            if (SubmitKey != null)
                builder.SubmitKey = SubmitKey.ToProtobufKey();

            if (AutoRenewPeriod != null)
                builder.AutoRenewPeriod = AutoRenewPeriod.Value.ToProtoDuration();

            if (TopicMemo != null)
				builder.Memo = TopicMemo;

			if (ExpirationTime != null)
                builder.ExpirationTime = ExpirationTime.Value.ToProtoTimestamp();

            if (ExpirationTimeDuration != null)
                builder.ExpirationTime = ExpirationTimeDuration.Value.ToProtoTimestamp();

            if (FeeScheduleKey != null)
                builder.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();

            if (FeeExemptKeys != null)
            {
                var feeExemptKeyList = new Proto.Services.FeeExemptKeyList();
                
                foreach (var feeExemptKey in FeeExemptKeys)
                {
                    feeExemptKeyList.Keys.Add(feeExemptKey.ToProtobufKey());
                }

                builder.FeeExemptKeyList = feeExemptKeyList;
            }

            if (CustomFees != null)
            {
                var protoCustomFeeList = new Proto.Services.FixedCustomFeeList();
                
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
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
		{
			bodyBuilder.ConsensusUpdateTopic = ToProtobuf();
		}
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
		{
			scheduled.ConsensusUpdateTopic = ToProtobuf();
		}
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.ConsensusService.ConsensusServiceClient.updateTopic);

			return Proto.Services.ConsensusService.Descriptor.FindMethodByName(methodname);
		}
    }
}
