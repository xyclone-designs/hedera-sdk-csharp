// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Java.Util.Stream;
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
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Transactions.Topic
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
        private AccountId autoRenewAccountId = null;
        private Duration autoRenewPeriod = null;
        private string topicMemo = "";
        private Key adminKey = null;
        private Key submitKey = null;
        private Key feeScheduleKey = null;
        private IList<Key> feeExemptKeys = new ();
        private IList<CustomFixedFee> customFees = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicCreateTransaction()
        {
            SetAutoRenewPeriod(DEFAULT_AUTO_RENEW_PERIOD);
            defaultMaxTransactionFee = new Hbar(25);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TopicCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TopicCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the topic memo.
        /// </summary>
        /// <returns>                         the topic memo</returns>
        public string GetTopicMemo()
        {
            return topicMemo;
        }

        /// <summary>
        /// Set a short publicly visible memo on the new topic.
        /// </summary>
        /// <param name="memo">The memo to be set</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction SetTopicMemo(string memo)
        {
            Objects.RequireNonNull(memo);
            RequireNotFrozen();
            topicMemo = memo;
            return this;
        }

        /// <summary>
        /// Extract the admin key.
        /// </summary>
        /// <returns>                         the admin key</returns>
        public Key GetAdminKey()
        {
            return adminKey;
        }

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
        public TopicCreateTransaction SetAdminKey(Key adminKey)
        {
            Objects.RequireNonNull(adminKey);
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Extract the submit key.
        /// </summary>
        /// <returns>                         the submit key</returns>
        public Key GetSubmitKey()
        {
            return submitKey;
        }

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
        public TopicCreateTransaction SetSubmitKey(Key submitKey)
        {
            Objects.RequireNonNull(submitKey);
            RequireNotFrozen();
            submitKey = submitKey;
            return this;
        }

        /// <summary>
        /// Extract the auto renew period.
        /// </summary>
        /// <returns>                         the auto renew period</returns>
        public Duration GetAutoRenewPeriod()
        {
            return autoRenewPeriod;
        }

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
        public TopicCreateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            Objects.RequireNonNull(autoRenewPeriod);
            RequireNotFrozen();
            autoRenewPeriod = autoRenewPeriod;
            return this;
        }

        /// <summary>
        /// Extract the auto renew account id.
        /// </summary>
        /// <returns>                         the auto renew account id</returns>
        public AccountId GetAutoRenewAccountId()
        {
            return autoRenewAccountId;
        }

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
        public TopicCreateTransaction SetAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            Objects.RequireNonNull(autoRenewAccountId);
            RequireNotFrozen();
            autoRenewAccountId = autoRenewAccountId;
            return this;
        }

        /// <summary>
        /// Returns the key which allows updates to the new topic’s fees.
        /// </summary>
        /// <returns>the feeScheduleKey</returns>
        public Key GetFeeScheduleKey()
        {
            return feeScheduleKey;
        }

        /// <summary>
        /// Sets the key which allows updates to the new topic’s fees.
        /// </summary>
        /// <param name="feeScheduleKey">the feeScheduleKey to be set</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction SetFeeScheduleKey(Key feeScheduleKey)
        {
            RequireNotFrozen();
            feeScheduleKey = feeScheduleKey;
            return this;
        }

        /// <summary>
        /// Returns the keys that will be exempt from paying fees.
        /// </summary>
        /// <returns>the feeExemptKeys</returns>
        public IList<Key> GetFeeExemptKeys()
        {
            return feeExemptKeys;
        }

        /// <summary>
        /// Sets the keys that will be exempt from paying fees.
        /// </summary>
        /// <param name="feeExemptKeys">the keys to be set</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction SetFeeExemptKeys(IList<Key> feeExemptKeys)
        {
            Objects.RequireNonNull(feeExemptKeys);
            RequireNotFrozen();
            feeExemptKeys = feeExemptKeys;
            return this;
        }

        /// <summary>
        /// Clears all keys that will be exempt from paying fees.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction ClearFeeExemptKeys()
        {
            RequireNotFrozen();
            feeExemptKeys.Clear();
            return this;
        }

        /// <summary>
        /// Adds a key that will be exempt from paying fees.
        /// </summary>
        /// <param name="feeExemptKey">feeExemptKey</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction AddFeeExemptKey(Key feeExemptKey)
        {
            Objects.RequireNonNull(feeExemptKey);
            RequireNotFrozen();
            if (feeExemptKeys != null)
            {
                feeExemptKeys.Add(feeExemptKey);
            }

            return this;
        }

        /// <summary>
        /// Returns the fixed fees to assess when a message is submitted to the new topic.
        /// </summary>
        /// <returns>the List<CustomFixedFee></returns>
        public IList<CustomFixedFee> GetCustomFees()
        {
            return customFees;
        }

        /// <summary>
        /// Sets the fixed fees to assess when a message is submitted to the new topic.
        /// </summary>
        /// <param name="">customFees List of CustomFixedFee</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction SetCustomFees(IList<CustomFixedFee> customFees)
        {
            Objects.RequireNonNull(customFees);
            RequireNotFrozen();
            customFees = customFees;
            return this;
        }

        /// <summary>
        /// Clears fixed fees.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction ClearCustomFees()
        {
            RequireNotFrozen();
            customFees = new ();
            return this;
        }

        /// <summary>
        /// Adds fixed fee to assess when a message is submitted to the new topic.
        /// </summary>
        /// <param name="">customFixedFee CustomFixedFee</param>
        /// <returns>{@code this}</returns>
        public TopicCreateTransaction AddCustomFee(CustomFixedFee customFixedFee)
        {
            Objects.RequireNonNull(customFees);
            customFees.Add(customFixedFee);
            RequireNotFrozen();
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetConsensusCreateTopic();
            if (body.HasAutoRenewAccount())
            {
                autoRenewAccountId = AccountId.FromProtobuf(body.GetAutoRenewAccount());
            }

            if (body.HasAdminKey())
            {
                adminKey = Key.FromProtobufKey(body.GetAdminKey());
            }

            if (body.HasSubmitKey())
            {
                submitKey = Key.FromProtobufKey(body.GetSubmitKey());
            }

            if (body.HasAutoRenewPeriod())
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.GetAutoRenewPeriod());
            }

            if (body.HasFeeScheduleKey())
            {
                feeScheduleKey = Key.FromProtobufKey(body.GetFeeScheduleKey());
            }

            if (body.GetFeeExemptKeyListList() != null)
            {
                feeExemptKeys = body.GetFeeExemptKeyListList().Stream().Map(Key.FromProtobufKey()).Collect(Collectors.ToList());
            }

            if (body.GetCustomFeesList() != null)
            {
                customFees = body.GetCustomFeesList().Stream().Map((x) => CustomFixedFee.FromProtobuf(x.GetFixedFee())).Collect(Collectors.ToList());
            }

            topicMemo = body.GetMemo();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusCreateTopicTransactionBody}</returns>
        ConsensusCreateTopicTransactionBody.Builder Build()
        {
            var builder = ConsensusCreateTopicTransactionBody.NewBuilder();
            if (autoRenewAccountId != null)
            {
                builder.SetAutoRenewAccount(autoRenewAccountId.ToProtobuf());
            }

            if (adminKey != null)
            {
                builder.SetAdminKey(adminKey.ToProtobufKey());
            }

            if (submitKey != null)
            {
                builder.SetSubmitKey(submitKey.ToProtobufKey());
            }

            if (autoRenewPeriod != null)
            {
                builder.SetAutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod));
            }

            if (feeScheduleKey != null)
            {
                builder.SetFeeScheduleKey(feeScheduleKey.ToProtobufKey());
            }

            if (feeExemptKeys != null)
            {
                foreach (var feeExemptKey in feeExemptKeys)
                {
                    builder.AddFeeExemptKeyList(feeExemptKey.ToProtobufKey());
                }
            }

            if (customFees != null)
            {
                foreach (CustomFixedFee customFee in customFees)
                {
                    builder.AddCustomFees(customFee.ToTopicFeeProtobuf());
                }
            }

            builder.SetMemo(topicMemo);
            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (autoRenewAccountId != null)
            {
                autoRenewAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return ConsensusServiceGrpc.GetCreateTopicMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetConsensusCreateTopic(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetConsensusCreateTopic(Build());
        }

        public override TopicCreateTransaction FreezeWith(Client client)
        {
            if (client != null && client.GetOperatorAccountId() != null && autoRenewAccountId == null)
            {
                autoRenewAccountId = transactionIds != null && !transactionIds.IsEmpty() && transactionIds.GetCurrent() != null ? transactionIds.GetCurrent().accountId : client.GetOperatorAccountId();
            }

            return base.FreezeWith(client);
        }
    }
}