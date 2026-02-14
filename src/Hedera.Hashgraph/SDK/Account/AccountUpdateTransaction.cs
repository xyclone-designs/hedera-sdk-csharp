// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
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
        private List<long> _HookIdsToDelete = [];
        private List<HookCreationDetails> _HookCreationDetails = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		public AccountUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal AccountUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal AccountUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Sets the account ID which is being updated in this transaction.
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
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
		public Key? Key { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// </summary>
		/// <param name="aliasKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
		/// <remarks>
		/// @deprecatedwith no replacement
		/// 
		/// Sets the new key.
		/// </remarks>
        [Obsolete]
		public Key? AliasKey { get; set { RequireNotFrozen(); field = value; } }
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
		public AccountId? ProxyAccountId { get; set { RequireNotFrozen(); field = value; } }
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
		public Timestamp? ExpirationTime { get; set { RequireNotFrozen(); field = value; } }
		public Duration? ExpirationTimeDuration { get; set { RequireNotFrozen(); field = value; ExpirationTime = null; } }
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
        public Duration? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Removed to distinguish between unset and a default value.<br/>
		/// Do NOT use this field to set a false value because the server cannot
		/// distinguish from the default value. Use receiverSigRequiredWrapper
		/// field for this purpose.
		/// </summary>
		/// <param name="receiverSignatureRequired">The bool to be set</param>
		/// <returns>{@code this}</returns>
		public bool? ReceiverSigRequired { get; set { RequireNotFrozen(); field = value; } }
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
		public int? MaxAutomaticTokenAssociations { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// A short description of this Account.
		/// <p>
		/// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
		/// (default 100) bytes when encoded as UTF-8.
		/// </summary>
		/// <param name="memo">the memo</param>
		/// <returns>                         {@code this}</returns>
		public string? AccountMemo { get; set { RequireNotFrozen(); field = value; } } 
        /// <summary>
        /// ID of the account to which this account is staking its balances.
        /// <p>
        /// If this account is not currently staking its balances, then this
        /// field, if set, MUST be the sentinel value of `0.0.0`.
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this account will stake.</param>
        /// <returns>{@code this}</returns>
        public AccountId? StakedAccountId { get; set { RequireNotFrozen(); field = value; StakedNodeId = null; ; } }
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
        public long? StakedNodeId { get; set { RequireNotFrozen(); field = value; StakedAccountId = null; } }
        /// <summary>
        /// A bool indicating that this account has chosen to decline rewards for
        /// staking its balances.
        /// <p>
        /// This account MAY still stake its balances, but SHALL NOT receive reward
        /// payments for doing so, if this value is set, and `true`.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the account declines receiving a staking reward. The default value is false.</param>
        /// <returns>{@code this}</returns>
        public bool? DeclineStakingReward { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Set hooks to be created with the account.
		/// </summary>
		/// <param name="hookDetails">list of hook creation details</param>
		/// <returns>{@code this}</returns>
		public IList<HookCreationDetails> HookCreationDetails
		{
			get
			{
				RequireNotFrozen();
				return _HookCreationDetails;

			}
			set
			{
				RequireNotFrozen();
				_HookCreationDetails = [.. value];
			}
		}
		public IReadOnlyList<HookCreationDetails> HookCreationDetails_Read { get => _HookCreationDetails.AsReadOnly(); }
		/// <summary>
		/// Mark hooks for deletion from the account.
		/// </summary>
		/// <param name="hookIds">list of hook ids to delete</param>
		/// <returns>{@code this}</returns>
		public IList<long> HookIdsToDelete
		{
			get
			{
				RequireNotFrozen();
				return _HookIdsToDelete;

			}
			set
			{
				RequireNotFrozen();
				_HookIdsToDelete = [.. value];
			}
		}
		public IReadOnlyList<long> HookIdsToDelete_Read { get => _HookIdsToDelete.AsReadOnly(); }        

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoUpdateAccount;

            if (body.AccountIDToUpdate is not null)
            {
                AccountId = AccountId.FromProtobuf(body.AccountIDToUpdate);
            }

            if (body.ProxyAccountID is not null)
            {
                ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            }

            if (body.Key is not null)
            {
                Key = Key.FromProtobufKey(body.Key);
            }

            if (body.ExpirationTime is not null)
            {
                ExpirationTime = TimestampConverter.FromProtobuf(body.ExpirationTime);
            }

            if (body.AutoRenewPeriod is not null)
            {
                AutoRenewPeriod = DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            if (body.ReceiverSigRequiredWrapper is not null)
            {
                ReceiverSigRequired = body.ReceiverSigRequiredWrapper.Value;
            }

            if (body.Memo is not null)
            {
                AccountMemo = body.Memo;
            }

            if (body.MaxAutomaticTokenAssociations is not null)
            {
                MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            }

            if (body.DeclineReward is not null)
            {
                DeclineStakingReward = body.DeclineReward.Value;
            }

            if (body.StakedAccountId is not null)
            {
                StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            }

			StakedNodeId = body.StakedNodeId;

			// Initialize hook create/delete details
			_HookCreationDetails.Clear();
			_HookCreationDetails.AddRange(body.HookCreationDetails.Select(_ => Hook.HookCreationDetails.FromProtobuf(_)));

			_HookIdsToDelete.Clear();
            _HookIdsToDelete.AddRange(body.HookIdsToDelete);
        }

        /// <summary>
        /// Create the builder.
        /// </summary>
        /// <returns>                         the transaction builder</returns>
        public Proto.CryptoUpdateTransactionBody ToProtobuf()
        {
			Proto.CryptoUpdateTransactionBody proto = new ()
            {
				ReceiverSigRequiredWrapper = ReceiverSigRequired
			};

            if (AccountId != null)
            {
                proto.AccountIDToUpdate = AccountId.ToProtobuf();
            }

            if (ProxyAccountId != null)
            {
                proto.ProxyAccountID = ProxyAccountId.ToProtobuf();
            }

            if (Key != null)
            {
                proto.Key = Key.ToProtobufKey();
            }

            if (ExpirationTime != null)
            {
                proto.ExpirationTime = TimestampConverter.ToProtobuf(ExpirationTime);
            }

            if (ExpirationTimeDuration != null)
            {
                proto.ExpirationTime = TimestampConverter.ToProtobuf(ExpirationTimeDuration);
            }

            if (AutoRenewPeriod != null)
            {
                proto.AutoRenewPeriod = DurationConverter.ToProtobuf(AutoRenewPeriod);
            }

            if (AccountMemo != null)
            {
                proto.Memo = AccountMemo;
            }

            if (MaxAutomaticTokenAssociations != null)
            {
                proto.MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations;
            }

            if (StakedAccountId != null)
            {
                proto.StakedAccountId = StakedAccountId.ToProtobuf();
            }
            else if (StakedNodeId != null)
            {
                proto.StakedNodeId = StakedNodeId.Value;
            }

            if (DeclineStakingReward != null)
            {
                proto.DeclineReward = DeclineStakingReward;
            }

            foreach (HookCreationDetails hookDetails in _HookCreationDetails)
            {
                proto.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            if (_HookIdsToDelete.Count != 0)
            {
                proto.HookIdsToDelete.AddRange(_HookIdsToDelete);
            }

            return proto;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.updateAccount);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{
			AccountId?.ValidateChecksum(client);
			ProxyAccountId?.ValidateChecksum(client);
			StakedAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoUpdateAccount = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoUpdateAccount = ToProtobuf();
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