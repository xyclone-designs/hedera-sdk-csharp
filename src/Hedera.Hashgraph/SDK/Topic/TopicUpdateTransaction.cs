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

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Update a topic.
    /// <p>
    /// If there is no adminKey, the only authorized update (available to anyone) is to extend the expirationTime.
    /// Otherwise transaction must be signed by the adminKey.
    /// <p>
    /// If an adminKey is updated, the transaction must be signed by the pre-update adminKey and post-update adminKey.
    /// <p>
    /// If a new autoRenewAccount is specified (not just being removed), that account must also sign the transaction.
    /// </summary>
    public sealed class TopicUpdateTransaction : Transaction<TopicUpdateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TopicUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TopicUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private IList<Key>? _FeeExemptKeys = null;
		private IList<CustomFixedFee>? _CustomFees = null;

		/// <summary>
		/// The topic ID specifying the topic to update.
		/// <p>
		/// A topic with this ID MUST exist and MUST NOT be deleted.<br/>
		/// This value is REQUIRED.
		/// </summary>
		/// <param name="topicId">The TopicId to be set</param>
		/// <returns>{@code this}</returns>
		public TopicId? TopicId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// An updated memo to be associated with this topic.
		/// <p>
		/// If this value is set, the current `adminKey` for the topic MUST sign
		/// this transaction.<br/>
		/// This value, if set, SHALL be encoded UTF-8 and SHALL NOT exceed
		/// 100 bytes when so encoded.
		/// </summary>
		/// <param name="memo">The memo to be set</param>
		/// <returns>{@code this}</returns>
		public string? TopicMemo { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Updated access control for modification of the topic.
		/// <p>
		/// If this field is set, that key and the previously set key MUST both
		/// sign this transaction.<br/>
		/// If this value is an empty `KeyList`, the prior key MUST sign this
		/// transaction, and the topic SHALL be immutable after this transaction
		/// completes, except for expiration and renewal.
		/// </summary>
		/// <param name="adminKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
		public Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Updated access control for message submission to the topic.
		/// <p>
		/// If this value is set, the current `adminKey` for the topic MUST sign
		/// this transaction.<br/>
		/// If this value is set to an empty `KeyList`, the `submitKey` for the
		/// topic will be unset after this transaction completes. When the
		/// `submitKey` is unset, any account may submit a message on the topic,
		/// without restriction.
		/// </summary>
		/// <param name="submitKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
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
         * @param autoRenewPeriod The Duration to be set for auto renewal
         * @return {@code this}
         */
		public Duration? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// An updated ID for the account to be charged renewal fees at the topic's
		/// `expirationTime` to extend the lifetime of the topic.
		/// <p>
		/// If this value is set and not the "sentinel account", the referenced
		/// account MUST sign this transaction.<br/>
		/// If this value is set, the current `adminKey` for the topic MUST sign
		/// this transaction.<br/>
		/// If this value is set to the "sentinel account", which is `0.0.0`, the
		/// `autoRenewAccount` SHALL be removed from the topic.
		/// </summary>
		/// <param name="autoRenewAccountId">The AccountId to be set for auto renewal</param>
		/// <returns>{@code this}</returns>
		public AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Sets the effective consensus timestamp at (and after) which all consensus transactions and queries will fail.
		/// The expirationTime may be no longer than MAX_AUTORENEW_PERIOD (8000001 seconds) from the consensus timestamp of
		/// this transaction.
		/// On topics with no adminKey, extending the expirationTime is the only updateTopic option allowed on the topic.
		/// </summary>
		/// <param name="expirationTime">the new expiration time</param>
		/// <returns>{@code this}</returns>
		public Timestamp? ExpirationTime { get; set { RequireNotFrozen(); field = value; ExpirationTimeDuration = null; } }
		public Duration? ExpirationTimeDuration { get; set { RequireNotFrozen(); field = value; ExpirationTime = null; } }
		/// <summary>
		/// Sets the key which allows updates to the new topic’s fees.
		/// </summary>
		/// <param name="feeScheduleKey">the feeScheduleKey</param>
		/// <returns>{@code this}</returns>
		public Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Sets the keys that will be exempt from paying fees.
		/// </summary>
		/// <param name="feeExemptKeys">List of feeExemptKeys</param>
		/// <returns>{@code this}</returns>
		public IList<Key>? FeeExemptKeys 
		{
			get { RequireNotFrozen(); return _FeeExemptKeys; } 
			set { RequireNotFrozen(); _FeeExemptKeys = value is null ? null : [.. value]; }
		}
		public IReadOnlyList<Key>? FeeExemptKeys_Read { get => _FeeExemptKeys?.AsReadOnly(); }
		/// <summary>
		/// Sets the fixed fees to assess when a message is submitted to the new topic.
		/// </summary>
		/// <param name="customFees">List of CustomFixedFee customFees</param>
		/// <returns>{@code this}</returns>
		public IList<CustomFixedFee>? CustomFees
		{
			get { RequireNotFrozen(); return _CustomFees; }
			set { RequireNotFrozen(); _CustomFees = value is null ? null : [.. value]; }
		}
		public IReadOnlyList<CustomFixedFee>? CustomFees_Read { get => _CustomFees?.AsReadOnly(); }


		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
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
                AutoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);

            if (body.AutoRenewAccount is not null)
                AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);

            if (body.Memo is not null)
                TopicMemo = body.Memo;

            if (body.ExpirationTime is not null)
                ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);

            if (body.FeeScheduleKey is not null)
                FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);

            if (body.FeeExemptKeyList is not null)
                FeeExemptKeys = [.. body.FeeExemptKeyList.Keys.Select(_ => Key.FromProtobufKey(_)).OfType<Key>()];

            if (body.CustomFees is not null)
                CustomFees = [..body.CustomFees.Fees.Select((x) => CustomFixedFee.FromProtobuf(x.FixedFee))];
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusUpdateTopicTransactionBody}</returns>
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
                builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod);

            if (TopicMemo != null)
				builder.Memo = TopicMemo;

			if (ExpirationTime != null)
                builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);

            if (ExpirationTimeDuration != null)
                builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);

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
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}