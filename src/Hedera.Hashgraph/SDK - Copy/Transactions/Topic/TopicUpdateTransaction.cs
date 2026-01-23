// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK;
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
        private TopicId topicId = null;
        private AccountId autoRenewAccountId = null;
        private string topicMemo = null;
        private Key adminKey = null;
        private Key submitKey = null;
        private Duration autoRenewPeriod = null;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private Key feeScheduleKey = null;
        private IList<Key> feeExemptKeys = null;
        private IList<CustomFixedFee> customFees = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopicUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TopicUpdateTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TopicUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the topic id.
        /// </summary>
        /// <returns>                         the topic id</returns>
        public TopicId GetTopicId()
        {
            return topicId;
        }

        /// <summary>
        /// The topic ID specifying the topic to update.
        /// <p>
        /// A topic with this ID MUST exist and MUST NOT be deleted.<br/>
        /// This value is REQUIRED.
        /// </summary>
        /// <param name="topicId">The TopicId to be set</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetTopicId(TopicId topicId)
        {
            Objects.RequireNonNull(topicId);
            RequireNotFrozen();
            topicId = topicId;
            return this;
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
        /// An updated memo to be associated with this topic.
        /// <p>
        /// If this value is set, the current `adminKey` for the topic MUST sign
        /// this transaction.<br/>
        /// This value, if set, SHALL be encoded UTF-8 and SHALL NOT exceed
        /// 100 bytes when so encoded.
        /// </summary>
        /// <param name="memo">The memo to be set</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetTopicMemo(string memo)
        {
            Objects.RequireNonNull(memo);
            RequireNotFrozen();
            topicMemo = memo;
            return this;
        }

        /// <summary>
        /// Clear the memo for this topic.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearTopicMemo()
        {
            RequireNotFrozen();
            topicMemo = "";
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
        public TopicUpdateTransaction SetAdminKey(Key adminKey)
        {
            Objects.RequireNonNull(adminKey);
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Clear the admin key for this topic.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearAdminKey()
        {
            RequireNotFrozen();
            adminKey = new KeyList();
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
        public TopicUpdateTransaction SetSubmitKey(Key submitKey)
        {
            Objects.RequireNonNull(submitKey);
            RequireNotFrozen();
            submitKey = submitKey;
            return this;
        }

        /// <summary>
        /// Clear the submit key for this topic.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearSubmitKey()
        {
            RequireNotFrozen();
            submitKey = new KeyList();
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
        public TopicUpdateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
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
        public TopicUpdateTransaction SetAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            Objects.RequireNonNull(autoRenewAccountId);
            RequireNotFrozen();
            autoRenewAccountId = autoRenewAccountId;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="autoRenewAccountId">The AccountId to be cleared for auto renewal</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedUse {@link #clearAutoRenewAccountId()}
        /// <p>
        /// Clear the auto renew account ID for this topic.
        /// </remarks>
        public TopicUpdateTransaction ClearAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            return ClearAutoRenewAccountId();
        }

        /// <summary>
        /// Clear the auto renew account ID for this topic.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearAutoRenewAccountId()
        {
            RequireNotFrozen();
            autoRenewAccountId = new AccountId(0, 0, 0);
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>Expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
        }

        /// <summary>
        /// Sets the effective consensus timestamp at (and after) which all consensus transactions and queries will fail.
        /// The expirationTime may be no longer than MAX_AUTORENEW_PERIOD (8000001 seconds) from the consensus timestamp of
        /// this transaction.
        /// On topics with no adminKey, extending the expirationTime is the only updateTopic option allowed on the topic.
        /// </summary>
        /// <param name="expirationTime">the new expiration time</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            RequireNotFrozen();
            expirationTime = Objects.RequireNonNull(expirationTime);
            expirationTimeDuration = null;
            return this;
        }

        public TopicUpdateTransaction SetExpirationTime(Duration expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = null;
            expirationTimeDuration = expirationTime;
            return this;
        }

        /// <summary>
        /// Returns the key which allows updates to the new topic’s fees.
        /// </summary>
        /// <returns>feeScheduleKey</returns>
        public Key GetFeeScheduleKey()
        {
            return feeScheduleKey;
        }

        /// <summary>
        /// Sets the key which allows updates to the new topic’s fees.
        /// </summary>
        /// <param name="feeScheduleKey">the feeScheduleKey</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetFeeScheduleKey(Key feeScheduleKey)
        {
            RequireNotFrozen();
            feeScheduleKey = feeScheduleKey;
            return this;
        }

        public TopicUpdateTransaction ClearFeeScheduleKey()
        {
            RequireNotFrozen();
            feeScheduleKey = new KeyList();
            return this;
        }

        /// <summary>
        /// Returns the keys that will be exempt from paying fees.
        /// </summary>
        /// <returns>{List of feeExemptKeys}</returns>
        public IList<Key> GetFeeExemptKeys()
        {
            return feeExemptKeys;
        }

        /// <summary>
        /// Sets the keys that will be exempt from paying fees.
        /// </summary>
        /// <param name="feeExemptKeys">List of feeExemptKeys</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetFeeExemptKeys(IList<Key> feeExemptKeys)
        {
            Objects.RequireNonNull(feeExemptKeys);
            RequireNotFrozen();
            feeExemptKeys = new List(feeExemptKeys);
            return this;
        }

        /// <summary>
        /// Clears all keys that will be exempt from paying fees.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearFeeExemptKeys()
        {
            RequireNotFrozen();
            feeExemptKeys = new ();
            return this;
        }

        /// <summary>
        /// Adds a key that will be exempt from paying fees.
        /// </summary>
        /// <param name="feeExemptKey">key</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction AddFeeExemptKey(Key feeExemptKey)
        {
            Objects.RequireNonNull(feeExemptKey);
            RequireNotFrozen();
            if (feeExemptKeys == null)
            {
                feeExemptKeys = new ();
            }

            feeExemptKeys.Add(feeExemptKey);
            return this;
        }

        /// <summary>
        /// Returns the fixed fees to assess when a message is submitted to the new topic.
        /// </summary>
        /// <returns>{List of CustomFixedFee}</returns>
        public IList<CustomFixedFee> GetCustomFees()
        {
            return customFees;
        }

        /// <summary>
        /// Sets the fixed fees to assess when a message is submitted to the new topic.
        /// </summary>
        /// <param name="customFees">List of CustomFixedFee customFees</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction SetCustomFees(IList<CustomFixedFee> customFees)
        {
            Objects.RequireNonNull(customFees);
            RequireNotFrozen();
            customFees = new List(customFees);
            return this;
        }

        /// <summary>
        /// Clears fixed fees.
        /// </summary>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction ClearCustomFees()
        {
            RequireNotFrozen();
            customFees = new ();
            return this;
        }

        /// <summary>
        /// Adds fixed fee to assess when a message is submitted to the new topic.
        /// </summary>
        /// <param name="customFixedFee">{CustomFixedFee} customFee</param>
        /// <returns>{@code this}</returns>
        public TopicUpdateTransaction AddCustomFee(CustomFixedFee customFixedFee)
        {
            Objects.RequireNonNull(customFixedFee);
            RequireNotFrozen();
            if (customFees == null)
            {
                customFees = new ();
            }

            customFees.Add(customFixedFee);
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetConsensusUpdateTopic();
            if (body.HasTopicID())
            {
                topicId = TopicId.FromProtobuf(body.GetTopicID());
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

            if (body.HasAutoRenewAccount())
            {
                autoRenewAccountId = AccountId.FromProtobuf(body.GetAutoRenewAccount());
            }

            if (body.HasMemo())
            {
                topicMemo = body.GetMemo().GetValue();
            }

            if (body.HasExpirationTime())
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.GetExpirationTime());
            }

            if (body.HasFeeScheduleKey())
            {
                feeScheduleKey = Key.FromProtobufKey(body.GetFeeScheduleKey());
            }

            if (body.HasFeeExemptKeyList())
            {
                feeExemptKeys = body.GetFeeExemptKeyList().GetKeysList().Stream().Map(Key.FromProtobufKey()).Collect(Collectors.ToList());
            }

            if (body.HasCustomFees())
            {
                customFees = body.GetCustomFees().GetFeesList().Stream().Map((x) => CustomFixedFee.FromProtobuf(x.GetFixedFee())).Collect(Collectors.ToList());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.ConsensusUpdateTopicTransactionBody}</returns>
        ConsensusUpdateTopicTransactionBody.Builder Build()
        {
            var builder = ConsensusUpdateTopicTransactionBody.NewBuilder();
            if (topicId != null)
            {
                builder.SetTopicID(topicId.ToProtobuf());
            }

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

            if (topicMemo != null)
            {
                builder.SetMemo(StringValue.Of(topicMemo));
            }

            if (expirationTime != null)
            {
                builder.SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTime));
            }

            if (expirationTimeDuration != null)
            {
                builder.SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTimeDuration));
            }

            if (feeScheduleKey != null)
            {
                builder.SetFeeScheduleKey(feeScheduleKey.ToProtobufKey());
            }

            if (feeExemptKeys != null)
            {
                var feeExemptKeyList = FeeExemptKeyList.NewBuilder();
                foreach (var feeExemptKey in feeExemptKeys)
                {
                    feeExemptKeyList.AddKeys(feeExemptKey.ToProtobufKey());
                }

                builder.SetFeeExemptKeyList(feeExemptKeyList);
            }

            if (customFees != null)
            {
                var protoCustomFeeList = FixedCustomFeeList.NewBuilder();
                foreach (CustomFixedFee customFee in customFees)
                {
                    protoCustomFeeList.AddFees(customFee.ToTopicFeeProtobuf());
                }

                builder.SetCustomFees(protoCustomFeeList);
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (topicId != null)
            {
                topicId.ValidateChecksum(client);
            }

            if ((autoRenewAccountId != null) && !autoRenewAccountId.Equals(new AccountId(0, 0, 0)))
            {
                autoRenewAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return ConsensusServiceGrpc.GetUpdateTopicMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetConsensusUpdateTopic(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetConsensusUpdateTopic(Build());
        }
    }
}