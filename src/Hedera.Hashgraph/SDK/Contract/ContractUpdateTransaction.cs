// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Modify a smart contract instance to have the given parameter values.
    /// <p>
    /// Any null field is ignored (left unchanged).
    /// <p>
    /// If only the contractInstanceExpirationTime is being modified, then no signature is
    /// needed on this transaction other than for the account paying for the transaction itself.
    /// <p>
    /// But if any of the other fields are being modified, then it must be signed by the adminKey.
    /// <p>
    /// The use of adminKey is not currently supported in this API, but in the future will
    /// be implemented to allow these fields to be modified, and also to make modifications
    /// to the state of the instance.
    /// <p>
    /// If the contract is created with no admin key, then none of the fields can be
    /// changed that need an admin signature, and therefore no admin key can ever be added.
    /// So if there is no admin key, then things like the bytecode are immutable.
    /// But if there is an admin key, then they can be changed. For example, the
    /// admin key might be a threshold key, which requires 3 of 5 binding arbitration judges to
    /// agree before the bytecode can be changed. This can be used to add flexibility to the management
    /// of smart contract behavior. But this is optional. If the smart contract is created
    /// without an admin key, then such a key can never be added, and its bytecode will be immutable.
    /// </summary>
    public sealed class ContractUpdateTransaction : Transaction<ContractUpdateTransaction>
    {
		/// <summary>
		/// Contract.
		/// </summary>
		public ContractUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal ContractUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Contract.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) record</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal ContractUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<HookCreationDetails> _HookCreationDetails = [];
		private List<long> _HookIdsToDelete = [];

		/// <summary>
		/// Sets the Contract ID instance to update.
		/// </summary>
		public ContractId? ContractId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// If set, modify the time at which this contract will expire.<br/>
		/// An expired contract requires a rent payment to "renew" the contract.
		/// A transaction to update this field is how that rent payment is made.
		/// <p>
		/// This value MUST NOT be less than the current `expirationTime`
		/// of the contract. If this value is earlier than the current
		/// value, the transaction SHALL fail with response
		/// code `EXPIRATION_REDUCTION_NOT_ALLOWED`.
		/// </summary>
		public Timestamp? ExpirationTime
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTimeDuration = null;
			}
		}
		public Duration? ExpirationTimeDuration
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
				ExpirationTime = null;
			}
		}
		/// <summary>
		/// If set, modify the key that authorizes updates to the contract.
		/// <p>
		/// If this field is set to a valid Key, this key and the previously set key
		/// MUST both sign this transaction.<br/>
		/// If this value is an empty `KeyList`, the prior key MUST sign this
		/// transaction, and the smart contract SHALL be immutable after this
		/// transaction completes, except for expiration and renewal.<br/>
		/// If this value is not an empty `KeyList`, but does not contain any
		/// cryptographic keys, or is otherwise malformed, this transaction SHALL
		/// fail with response code `INVALID_ADMIN_KEY`.
		/// </summary>
		/// <param name="adminKey">The Key to be set</param>
		/// <returns>{@code this}</returns>
		public Key? AdminKey
		{
			get;
			set;
		}
		/// <summary>
		/// Sets the ID of the account to which this account is proxy staked.
		/// <p>
		/// If proxyAccountID is null, or is an invalid account, or is an account
		/// that isn't a node, then this account is automatically proxy staked to a
		/// node chosen by the network, but without earning payments.
		/// <p>
		/// If the proxyAccountID account refuses to accept proxy staking, or if it is
		/// not currently running a node, then it will behave as if proxyAccountID was null.
		/// </summary>
		/// <param name="proxyAccountId">The AccountId to be set</param>
		/// <returns>{@code this}</returns>
		public AccountId? ProxyAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// If set, modify the maximum number of tokens that can be auto-associated with the
		/// contract.
		/// <p>
		/// If this is set and less than or equal to `used_auto_associations`, or 0, then this contract
		/// MUST manually associate with a token before transacting in that token.<br/>
		/// This value MAY also be `-1` to indicate no limit.<br/>
		/// This value MUST NOT be less than `-1`.
		/// </summary>
		/// <param name="maxAutomaticTokenAssociations">The maximum automatic token associations</param>
		/// <returns> {@code this}</returns>
		public int? MaxAutomaticTokenAssociations
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// If set, modify the duration added to expiration time by each
		/// auto-renewal to this value.
		/// </summary>
		/// <param name="autoRenewPeriod">The Duration to be set for auto-renewal</param>
		/// <returns>{@code this}</returns>
		public Duration? AutoRenewPeriod
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// </summary>
		/// <param name="bytecodeFileId">The FileId to be set</param>
		/// <returns>{@code this}</returns>
		/// <remarks>
		/// @deprecatedwith no replacement
		/// 
		/// Sets the file ID of file containing the smart contract byte code.
		/// <p>
		/// A copy will be made and held by the contract instance, and have the same expiration
		/// time as the instance.
		/// </remarks>
		public FileId? BytecodeFileId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Sets the memo associated with the contract (max: 100 bytes).
		/// </summary>
		/// <param name="memo">The memo to be set</param>
		/// <returns>{@code this}</returns>
		public string? ContractMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// An account identifier.<br/>
		/// A staked account acts as a proxy, and this contract effectively
		/// nominates the same node as the identified account.
		/// <p>
		/// If set, modify this smart contract such that it SHALL stake its HBAR
		/// to the same node as the identified account.<br/>
		/// If this field is set to a default AccountID value (`0.0.0`), any
		/// pre-existing `staked_account_id` value SHALL be removed on success.
		/// </summary>
		/// <param name="stakedAccountId">ID of the account to which this contract will stake.</param>
		/// <returns>{@code this}</returns>
		public AccountId? StakedAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                StakedNodeId = null;
			}
		}
		public long? StakedNodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
                StakedAccountId = null;
			}
		}
		/// <summary>
		/// A flag indicating if staking rewards are declined.<br/>
		/// If set, modify the flag indicating if this contract declines to accept
		/// rewards for staking its HBAR to secure the network.
		/// <p>
		/// If set to true, this smart contract SHALL NOT receive any reward for
		/// staking its HBAR balance to help secure the network, regardless of
		/// staking configuration, but MAY stake HBAR to support the network
		/// without reward.
		/// </summary>
		/// <param name="declineStakingReward">- If true, the contract declines receiving a staking reward. The default value is false.</param>
		/// <returns>{@code this}</returns>
		public bool? DeclineStakingReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// If set, modify the account, in the same shard and realm as this smart
		/// contract, that has agreed to allow the network to use its balance, when
		/// needed, to automatically extend this contract's expiration time.
		/// <p>
		/// If this field is set to a non-default value, that Account MUST sign this
		/// transaction.<br/>
		/// If this field is set to a default AccountID value (`0.0.0`), any
		/// pre-existing `auto_renew_account_id` value SHALL be removed on success.
		/// </summary>
		/// <param name="autoRenewAccountId">The AccountId to be set for auto-renewal</param>
		/// <returns>{@code this}</returns>
		public AccountId? AutoRenewAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// Get the list of hooks to be created.
		/// </summary>
		/// <returns>a copy of the hook creation details list</returns>
		public IList<HookCreationDetails> HookCreationDetails_
		{
			get { RequireNotFrozen(); return _HookCreationDetails; }
			set
			{
				RequireNotFrozen();
				_HookCreationDetails = [.. value];
			}
		}
		/// <summary>
		/// Get the list of hook IDs to be deleted.
		/// </summary>
		/// <returns>a copy of the hook IDs list</returns>
		public IList<long> HookIdsToDelete
		{
			get { RequireNotFrozen(); return _HookIdsToDelete; }
			set
			{
				RequireNotFrozen();
				_HookIdsToDelete = [.. value];
			}
		}
		public IReadOnlyList<HookCreationDetails> HookCreationDetails_Read { get => _HookCreationDetails.AsReadOnly(); }
		public IReadOnlyList<long> HookIdsToDelete_Read { get => _HookIdsToDelete.AsReadOnly(); }


		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractUpdateInstance;

            ContractId = ContractId.FromProtobuf(body.ContractID);
            ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.ExpirationTime);

            if (body.AdminKey is not null)
				AdminKey = Key.FromProtobufKey(body.AdminKey);

			MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            AutoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            ContractMemo = body.MemoWrapper;
            DeclineStakingReward = body.DeclineReward;
            StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            StakedNodeId = body.StakedNodeId;

            AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccountId);

			_HookCreationDetails.Clear();
			_HookCreationDetails.AddRange(body.HookCreationDetails.Select(_ => HookCreationDetails.FromProtobuf(_)));

			_HookIdsToDelete.Clear();
            _HookIdsToDelete.AddRange(body.HookIdsToDelete);
        }

		/// <summary>
		/// Build the correct transaction body.
		/// </summary>
		/// <returns>{@link Proto.ContractUpdateTransactionBody builder }</returns>
		public Proto.ContractUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.ContractUpdateTransactionBody { };

            if (ContractId != null)
				builder.ContractID = ContractId.ToProtobuf();

            if (ProxyAccountId != null)
				builder.ProxyAccountID = ProxyAccountId.ToProtobuf();

            if (ExpirationTime != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime);

            if (ExpirationTimeDuration != null)
				builder.ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);

            if (AdminKey != null)
				builder.AdminKey = AdminKey.ToProtobufKey();

            if (MaxAutomaticTokenAssociations != null)
				builder.MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations;

			if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod);

            if (ContractMemo != null)
				builder.MemoWrapper = ContractMemo;

            if (StakedAccountId != null)
				builder.StakedAccountId = StakedAccountId.ToProtobuf();

            if (StakedNodeId != null)
				builder.StakedNodeId = StakedNodeId.Value;

            if (DeclineStakingReward != null)
				builder.DeclineReward = DeclineStakingReward.Value;

			if (HookIdsToDelete.Count != 0)
				builder.HookIdsToDelete.AddRange(HookIdsToDelete);

			if (AutoRenewAccountId != null)
			{
				if (AutoRenewAccountId.ToString().Equals("0.0.0"))
					builder.AutoRenewAccountId = new Proto.AccountID { };
				else
					builder.AutoRenewAccountId = AutoRenewAccountId.ToProtobuf();
			}

			builder.HookCreationDetails.AddRange(HookCreationDetails_.Select(_ => _.ToProtobuf()));

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            ContractId?.ValidateChecksum(client);
            ProxyAccountId?.ValidateChecksum(client);
            StakedAccountId?.ValidateChecksum(client);
            AutoRenewAccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractUpdateInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractUpdateInstance = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.SmartContractService.SmartContractServiceClient.updateContract);

			return Proto.SmartContractService.Descriptor.FindMethodByName(methodname);
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