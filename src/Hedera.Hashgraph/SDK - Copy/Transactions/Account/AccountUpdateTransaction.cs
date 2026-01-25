// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// Modify the current state of an account.
    /// 
    /// ### Requirements
    /// - The `key` for this account MUST sign all account update transactions.
    /// - If the `key` field is set for this transaction, then _both_ the current
    ///   `key` and the new `key` MUST sign this transaction, for security and to
    ///   prevent setting the `key` field to an invalid value.
    /// - If the `auto_renew_account` field is set for this transaction, the account
    ///   identified in that field MUST sign this transaction.
    /// - Fields set to non-default values in this transaction SHALL be updated on
    ///   success. Fields not set to non-default values SHALL NOT be
    ///   updated on success.
    /// - All fields that may be modified in this transaction SHALL have a
    ///   default value of unset (a.k.a. `null`).
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class AccountUpdateTransaction : Transaction<AccountUpdateTransaction>
    {
        private AccountId accountId = null;
        private AccountId proxyAccountId = null;
        private Key key = null;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private Duration autoRenewPeriod = null;
        private bool receiverSigRequired = null;
        private string accountMemo = null;
        private int maxAutomaticTokenAssociations = null;
        private Key aliasKey;
        private AccountId stakedAccountId = null;
        private long stakedNodeId = null;
        private bool declineStakingReward = null;
        private IList<long> hookIdsToDelete = new ();
        private IList<HookCreationDetails> hookCreationDetails = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        AccountUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AccountUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// Sets the account ID which is being updated in this transaction.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetAccountId(AccountId accountId)
        {
            Objects.RequireNonNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the key.
        /// </summary>
        /// <returns>                         the key</returns>
        public Key GetKey()
        {
            return key;
        }

        /// <summary>
        /// An account key.<br/>
        /// This may be a "primitive" key (a singly cryptographic key), or a
        /// composite key.
        /// <p>
        /// If set, this key MUST be a valid key.<br/>
        /// If set, the previous key and new key MUST both sign this transaction.
        /// </summary>
        /// <param name="key">The Key to be set</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetKey(Key key)
        {
            Objects.RequireNonNull(key);
            RequireNotFrozen();
            key = key;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>the alias key</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public Key GetAliasKey()
        {
            return aliasKey;
        }

        /// <summary>
        /// </summary>
        /// <param name="aliasKey">The Key to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// 
        /// Sets the new key.
        /// </remarks>
        public AccountUpdateTransaction SetAliasKey(Key aliasKey)
        {
            Objects.RequireNonNull(aliasKey);
            RequireNotFrozen();
            aliasKey = aliasKey;
            return this;
        }

        /// <summary>
        /// Extract the proxy account id.
        /// </summary>
        /// <returns>                         the proxy account id</returns>
        public AccountId GetProxyAccountId()
        {
            return proxyAccountId;
        }

        /// <summary>
        /// Sets the ID of the account to which this account is proxy staked.
        /// <p>
        /// If proxyAccountID is null, or is an invalid account, or is an account
        /// that isn't a node, then this account is automatically proxy staked to
        /// a node chosen by the network, but without earning payments.
        /// <p>
        /// If the proxyAccountID account refuses to accept proxy staking, or
        /// if it is not currently running a node, then it
        /// will behave as if proxyAccountID was null.
        /// </summary>
        /// <param name="proxyAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetProxyAccountId(AccountId proxyAccountId)
        {
            Objects.RequireNonNull(proxyAccountId);
            RequireNotFrozen();
            proxyAccountId = proxyAccountId;
            return this;
        }

        /// <summary>
        /// Extract the expiration time.
        /// </summary>
        /// <returns>                         the expiration time</returns>
        public Timestamp GetExpirationTime()
        {
            return expirationTime;
        }

        /// <summary>
        /// A new account expiration time, in seconds since the epoch.
        /// <p>
        /// For this purpose, `epoch` SHALL be the UNIX epoch with 0
        /// at `1970-01-01T00:00:00.000Z`.<br/>
        /// If set, this value MUST be later than the current consensus time.<br/>
        /// If set, this value MUST be earlier than the current consensus time
        /// extended by the current maximum expiration time configured for the
        /// network.
        /// </summary>
        /// <param name="expirationTime">The Timestamp to be set as the expiration time</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = expirationTime;
            return this;
        }

        public AccountUpdateTransaction SetExpirationTime(Duration expirationTime)
        {
            Objects.RequireNonNull(expirationTime);
            RequireNotFrozen();
            expirationTime = null;
            expirationTimeDuration = expirationTime;
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
        /// A duration to extend account expiration.<br/>
        /// An amount of time, in seconds, to extend the expiration date for this
        /// account when _automatically_ renewed.
        /// <p>
        /// This duration MUST be between the current configured minimum and maximum
        /// values defined for the network.<br/>
        /// This duration SHALL be applied only when _automatically_ extending the
        /// account expiration.
        /// </summary>
        /// <param name="autoRenewPeriod">The Duration to be set for auto renewal</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            Objects.RequireNonNull(autoRenewPeriod);
            RequireNotFrozen();
            autoRenewPeriod = autoRenewPeriod;
            return this;
        }

        /// <summary>
        /// Is the receiver required to sign?
        /// </summary>
        /// <returns>                         is the receiver required to sign</returns>
        public bool GetReceiverSignatureRequired()
        {
            return receiverSigRequired;
        }

        /// <summary>
        /// Removed to distinguish between unset and a default value.<br/>
        /// Do NOT use this field to set a false value because the server cannot
        /// distinguish from the default value. Use receiverSigRequiredWrapper
        /// field for this purpose.
        /// </summary>
        /// <param name="receiverSignatureRequired">The bool to be set</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetReceiverSignatureRequired(bool receiverSignatureRequired)
        {
            RequireNotFrozen();
            receiverSigRequired = receiverSignatureRequired;
            return this;
        }

        /// <summary>
        /// Extract the maximum automatic token associations.
        /// </summary>
        /// <returns>                         the max automatic token associations</returns>
        public int GetMaxAutomaticTokenAssociations()
        {
            return maxAutomaticTokenAssociations;
        }

        /// <summary>
        /// A maximum number of tokens that can be auto-associated
        /// with this account.<br/>
        /// By default this value is 0 for all accounts except for automatically
        /// created accounts (i.e smart contracts) which default to -1.
        /// <p>
        /// If this value is `0`, then this account MUST manually associate to
        /// a token before holding or transacting in that token.<br/>
        /// This value MAY also be `-1` to indicate no limit.<br/>
        /// If set, this value MUST NOT be less than `-1`.<br/>
        /// </summary>
        /// <param name="amount">the amount of tokens</param>
        /// <returns>                         {@code this}</returns>
        public AccountUpdateTransaction SetMaxAutomaticTokenAssociations(int amount)
        {
            RequireNotFrozen();
            maxAutomaticTokenAssociations = amount;
            return this;
        }

        /// <summary>
        /// Extract the account memo.
        /// </summary>
        /// <returns>                         the account memo</returns>
        public string GetAccountMemo()
        {
            return accountMemo;
        }

        /// <summary>
        /// A short description of this Account.
        /// <p>
        /// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
        /// (default 100) bytes when encoded as UTF-8.
        /// </summary>
        /// <param name="memo">the memo</param>
        /// <returns>                         {@code this}</returns>
        public AccountUpdateTransaction SetAccountMemo(string memo)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(memo);
            accountMemo = memo;
            return this;
        }

        /// <summary>
        /// Erase the memo field.
        /// </summary>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction ClearMemo()
        {
            RequireNotFrozen();
            accountMemo = "";
            return this;
        }

        /// <summary>
        /// ID of the account to which this account will stake
        /// </summary>
        /// <returns>ID of the account to which this account will stake.</returns>
        public AccountId GetStakedAccountId()
        {
            return stakedAccountId;
        }

        /// <summary>
        /// ID of the account to which this account is staking its balances.
        /// <p>
        /// If this account is not currently staking its balances, then this
        /// field, if set, MUST be the sentinel value of `0.0.0`.
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this account will stake.</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetStakedAccountId(AccountId stakedAccountId)
        {
            RequireNotFrozen();
            stakedAccountId = stakedAccountId;
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// Clear the staked account ID
        /// </summary>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction ClearStakedAccountId()
        {
            RequireNotFrozen();
            stakedAccountId = new AccountId(0, 0, 0);
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// The node to which this account will stake
        /// </summary>
        /// <returns>ID of the node this account will be staked to.</returns>
        public long GetStakedNodeId()
        {
            return stakedNodeId;
        }

        /// <summary>
        /// ID of the node this account is staked to.
        /// <p>
        /// If this account is not currently staking its balances, then this
        /// field, if set, SHALL be the sentinel value of `-1`.<br/>
        /// Wallet software SHOULD surface staking issues to users and provide a
        /// simple mechanism to update staking to a new node ID in the event the
        /// prior staked node ID ceases to be valid.
        /// </summary>
        /// <param name="stakedNodeId">ID of the node this account will be staked to.</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetStakedNodeId(long stakedNodeId)
        {
            RequireNotFrozen();
            stakedNodeId = stakedNodeId;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// Clear the staked node
        /// </summary>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction ClearStakedNodeId()
        {
            RequireNotFrozen();
            stakedNodeId = -1;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// If true, the account declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <returns>If true, the account declines receiving a staking reward. The default value is false.</returns>
        public bool GetDeclineStakingReward()
        {
            return declineStakingReward;
        }

        /// <summary>
        /// A bool indicating that this account has chosen to decline rewards for
        /// staking its balances.
        /// <p>
        /// This account MAY still stake its balances, but SHALL NOT receive reward
        /// payments for doing so, if this value is set, and `true`.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the account declines receiving a staking reward. The default value is false.</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetDeclineStakingReward(bool declineStakingReward)
        {
            RequireNotFrozen();
            declineStakingReward = declineStakingReward;
            return this;
        }

        /// <summary>
        /// Clear decline staking reward
        /// </summary>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction ClearDeclineStakingReward()
        {
            RequireNotFrozen();
            declineStakingReward = null;
            return this;
        }

        /// <summary>
        /// Add a hook to be created for the account.
        /// </summary>
        /// <param name="hookDetails">the hook creation details to add</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction AddHookToCreate(HookCreationDetails hookDetails)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails.Add(hookDetails);
            return this;
        }

        /// <summary>
        /// Set hooks to be created with the account.
        /// </summary>
        /// <param name="hookDetails">list of hook creation details</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetHooksToCreate(IList<HookCreationDetails> hookDetails)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails = new List(hookDetails);
            return this;
        }

        /// <summary>
        /// Mark a hook for deletion from the account.
        /// </summary>
        /// <param name="hookId">the hook id to delete</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction AddHookToDelete(long hookId)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookId, "hookId cannot be null");
            hookIdsToDelete.Add(hookId);
            return this;
        }

        /// <summary>
        /// Mark hooks for deletion from the account.
        /// </summary>
        /// <param name="hookIds">list of hook ids to delete</param>
        /// <returns>{@code this}</returns>
        public AccountUpdateTransaction SetHooksToDelete(IList<long> hookIds)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(hookIds, "hookIds cannot be null");
            hookIdsToDelete = new List(hookIds);
            return this;
        }

        /// <summary>
        /// Get the list of hooks to be created.
        /// </summary>
        /// <returns>a copy of the hook creation details list</returns>
        public IList<HookCreationDetails> GetHooksToCreate()
        {
            return new List(hookCreationDetails);
        }

        /// <summary>
        /// Get the list of hook IDs to be deleted.
        /// </summary>
        /// <returns>a copy of the hook IDs list</returns>
        public IList<long> GetHooksToDelete()
        {
            return new List(hookIdsToDelete);
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }

            if (proxyAccountId != null)
            {
                proxyAccountId.ValidateChecksum(client);
            }

            if (stakedAccountId != null)
            {
                stakedAccountId.ValidateChecksum(client);
            }
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetCryptoUpdateAccount();
            if (body.HasAccountIDToUpdate())
            {
                accountId = AccountId.FromProtobuf(body.GetAccountIDToUpdate());
            }

            if (body.HasProxyAccountID())
            {
                proxyAccountId = AccountId.FromProtobuf(body.GetProxyAccountID());
            }

            if (body.HasKey())
            {
                key = Key.FromProtobufKey(body.GetKey());
            }

            if (body.HasExpirationTime())
            {
                expirationTime = TimestampConverter.FromProtobuf(body.GetExpirationTime());
            }

            if (body.HasAutoRenewPeriod())
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.GetAutoRenewPeriod());
            }

            if (body.HasReceiverSigRequiredWrapper())
            {
                receiverSigRequired = body.GetReceiverSigRequiredWrapper().GetValue();
            }

            if (body.HasMemo())
            {
                accountMemo = body.GetMemo().GetValue();
            }

            if (body.HasMaxAutomaticTokenAssociations())
            {
                maxAutomaticTokenAssociations = body.GetMaxAutomaticTokenAssociations().GetValue();
            }

            if (body.HasDeclineReward())
            {
                declineStakingReward = body.GetDeclineReward().GetValue();
            }

            if (body.HasStakedAccountId())
            {
                stakedAccountId = AccountId.FromProtobuf(body.GetStakedAccountId());
            }

            if (body.HasStakedNodeId())
            {
                stakedNodeId = body.GetStakedNodeId();
            }


            // Initialize hook create/delete details
            hookCreationDetails.Clear();
            foreach (var protoHookDetails in body.GetHookCreationDetailsList())
            {
                hookCreationDetails.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
            }

            hookIdsToDelete.Clear();
            hookIdsToDelete.AddAll(body.GetHookIdsToDeleteList());
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetUpdateAccountMethod();
        }

        /// <summary>
        /// Create the builder.
        /// </summary>
        /// <returns>                         the transaction builder</returns>
        CryptoUpdateTransactionBody.Builder Build()
        {
            var builder = CryptoUpdateTransactionBody.NewBuilder();
            if (accountId != null)
            {
                builder.SetAccountIDToUpdate(accountId.ToProtobuf());
            }

            if (proxyAccountId != null)
            {
                builder.SetProxyAccountID(proxyAccountId.ToProtobuf());
            }

            if (key != null)
            {
                builder.SetKey(key.ToProtobufKey());
            }

            if (expirationTime != null)
            {
                builder.SetExpirationTime(TimestampConverter.ToProtobuf(expirationTime));
            }

            if (expirationTimeDuration != null)
            {
                builder.SetExpirationTime(TimestampConverter.ToProtobuf(expirationTimeDuration));
            }

            if (autoRenewPeriod != null)
            {
                builder.SetAutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod));
            }

            if (receiverSigRequired != null)
            {
                builder.SetReceiverSigRequiredWrapper(BoolValue.Of(receiverSigRequired));
            }

            if (accountMemo != null)
            {
                builder.SetMemo(StringValue.Of(accountMemo));
            }

            if (maxAutomaticTokenAssociations != null)
            {
                builder.SetMaxAutomaticTokenAssociations(Int32Value.Of(maxAutomaticTokenAssociations));
            }

            if (stakedAccountId != null)
            {
                builder.SetStakedAccountId(stakedAccountId.ToProtobuf());
            }
            else if (stakedNodeId != null)
            {
                builder.SetStakedNodeId(stakedNodeId);
            }

            if (declineStakingReward != null)
            {
                builder.SetDeclineReward(BoolValue.NewBuilder().SetValue(declineStakingReward).Build());
            }

            foreach (HookCreationDetails hookDetails in hookCreationDetails)
            {
                builder.AddHookCreationDetails(hookDetails.ToProtobuf());
            }

            if (!hookIdsToDelete.IsEmpty())
            {
                builder.AddAllHookIdsToDelete(hookIdsToDelete);
            }

            return builder;
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetCryptoUpdateAccount(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetCryptoUpdateAccount(Build());
        }
    }
}