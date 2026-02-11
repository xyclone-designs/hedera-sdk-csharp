// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Topic
{
    /// <summary>
    /// Create a topic to accept and group consensus messages.
    /// 
    /// If `autoRenewAccount` is specified, that account Key MUST also sign this
    /// transaction.<br/>
    /// If `adminKey` is set, that Key MUST sign the transaction.<br/>
    /// On success, the resulting `TransactionReceipt` SHALL contain the newly
    /// created `TopicId`.
    /// 
    /// The `autoRenewPeriod` on a topic MUST be set to a value between
    /// `autoRenewPeriod.minDuration` and `autoRenewPeriod.maxDuration`. These
    /// values are configurable, typically 30 and 92 days.<br/>
    /// This also sets the initial expirationTime of the topic.
    /// 
    /// If no `adminKey` is set on a topic
    ///   -`autoRenewAccount` SHALL NOT be set on the topic.
    ///   - A `deleteTopic` transaction SHALL fail.
    ///   - An `updateTopic` transaction that only extends the expirationTime MAY
    ///     succeed.
    ///   - Any other `updateTopic` transaction SHALL fail.
    /// 
    /// If the topic expires and is not automatically renewed, the topic SHALL enter
    /// the `EXPIRED` state.
    ///   - All transactions on the topic SHALL fail with TOPIC_EXPIRED
    ///      - Except an updateTopic() call that only extends the expirationTime.
    ///   - getTopicInfo() SHALL succeed, and show the topic is expired.
    /// The topic SHALL remain in the `EXPIRED` state for a time determined by the
    /// `autorenew.gracePeriod` (configurable, originally 7 days).<br/>
    /// After the grace period, if the topic's expirationTime is not extended, the
    /// topic SHALL be automatically deleted from state entirely, and cannot be
    /// recovered or recreated.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class TopicCreateTransaction : Transaction<TopicCreateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicCreateTransaction()
        {
            AutoRenewPeriod = DEFAULT_AUTO_RENEW_PERIOD;
            DefaultMaxTransactionFee = new Hbar(25);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TopicCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TopicCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        private IList<CustomFixedFee> _CustomFees = [];
        private IList<Key> _FeeExemptKeys = [];

		/// <summary>
		/// Set a short publicly visible memo on the new topic.
		/// </summary>
		/// <param name="memo">The memo to be set</param>
		/// <returns>{@code this}</returns>
		public string TopicMemo { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <summary>
		/// Access control for modification of the topic after it is created.
		/// <p>
		/// If this field is set, that key MUST sign this transaction.<br/>
		/// If this field is set, that key MUST sign each future transaction to
		/// update or delete the topic.<br/>
		/// An updateTopic transaction that _only_ extends the topic expirationTime
		/// (a "manual renewal" transaction) SHALL NOT require admin key
		/// signature.<br/>
		/// A topic without an admin key SHALL be immutable, except for expiration
		/// and renewal.<br/>
		/// If adminKey is not set, then `autoRenewAccount` SHALL NOT be set.
		/// </summary>
		/// <param name="adminKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
		public Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Access control for message submission to the topic.
		/// <p>
		/// If this field is set, that key MUST sign each consensus submit message
		/// for this topic.<br/>
		/// If this field is not set then any account may submit a message on the
		/// topic, without restriction.
		/// </summary>
		/// <param name="submitKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
		public Key? SubmitKey { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The initial lifetime, in seconds, for the topic.<br/>
		/// This is also the number of seconds for which the topic SHALL be
		/// automatically renewed upon expiring, if it has a valid auto-renew
		/// account.
		/// <p>
		/// This value MUST be set.<br/>
		/// This value MUST be greater than the configured
		/// MIN_AUTORENEW_PERIOD.<br/>
		/// This value MUST be less than the configured MAX_AUTORENEW_PERIOD.
		/// </summary>
		/// <param name="autoRenewPeriod">The Duration to be set for auto renewal</param>
		/// <returns>{@code this}</returns>
		public Duration? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The ID of the account to be charged renewal fees at the topic's
		/// expirationTime to extend the lifetime of the topic.
		/// <p>
		/// The topic lifetime SHALL be extended by the smallest of the following:
		/// <ul>
		///   <li>The current `autoRenewPeriod` duration.</li>
		///   <li>The maximum duration that this account has funds to purchase.</li>
		///   <li>The configured MAX_AUTORENEW_PERIOD at the time of automatic
		///       renewal.</li>
		/// </ul>
		/// If this value is set, the referenced account MUST sign this
		/// transaction.<br/>
		/// If this value is set, the `adminKey` field MUST also be set (though that
		/// key MAY not have any correlation to this account).
		/// </summary>
		/// <param name="autoRenewAccountId">The AccountId to be set for auto renewal</param>
		/// <returns>{@code this}</returns>
		public AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Sets the key which allows updates to the new topic’s fees.
		/// </summary>
		/// <param name="feeScheduleKey">the feeScheduleKey to be set</param>
		/// <returns>{@code this}</returns>
		public Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Sets the keys that will be exempt from paying fees.
		/// </summary>
		/// <param name="feeExemptKeys">the keys to be set</param>
		/// <returns>{@code this}</returns>
		public IList<Key> FeeExemptKeys { get { RequireNotFrozen(); return _FeeExemptKeys; } set { RequireNotFrozen(); _FeeExemptKeys = value; } }
		public IReadOnlyList<Key> FeeExemptKeys_Read { get => _FeeExemptKeys.AsReadOnly(); }

		/// <summary>
		/// Sets the fixed fees to assess when a message is submitted to the new topic.
		/// </summary>
		/// <param name="">customFees List of CustomFixedFee</param>
		/// <returns>{@code this}</returns>
		public IList<CustomFixedFee> CustomFees { get { RequireNotFrozen(); return _CustomFees; } set { RequireNotFrozen(); _CustomFees = value; } } 
		public IReadOnlyList<CustomFixedFee> CustomFees_Read { get => _CustomFees.AsReadOnly(); }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
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
                AutoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            if (body.FeeScheduleKey is not null)
            {
                FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);
            }

            if (body.FeeExemptKeyList is not null)
            {
                FeeExemptKeys = [.. body.FeeExemptKeyList.Select(_ => Key.FromProtobufKey(_))];
            }

            if (body.CustomFees is not null)
            {
                CustomFees = [.. body.CustomFees.Select((x) => CustomFixedFee.FromProtobuf(x.FixedFee))];
            }

            TopicMemo = body.Memo;
        }

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link
		///         Proto.ConsensusCreateTopicTransactionBody}</returns>
		public Proto.ConsensusCreateTopicTransactionBody Build()
        {
            var builder = new Proto.ConsensusCreateTopicTransactionBody
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
				builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod);

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
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ConsensusCreateTopic = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ConsensusCreateTopic = Build();
        }
        public override TopicCreateTransaction FreezeWith(Client client)
        {
            if (client.OperatorAccountId != null && AutoRenewAccountId == null)
            {
				AutoRenewAccountId = TransactionIds != null && TransactionIds.Count != 0 && TransactionIds.GetCurrent() != null ? TransactionIds.GetCurrent().AccountId : client.OperatorAccountId;
            }

            return base.FreezeWith(client);
        }

        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return ConsensusServiceGrpc.GetCreateTopicMethod();
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