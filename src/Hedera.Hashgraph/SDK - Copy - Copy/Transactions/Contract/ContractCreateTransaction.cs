// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// Start a new smart contract instance.
    /// After the instance is created,
    /// the ContractID for it is in the receipt.
    /// <p>
    /// The instance will exist for autoRenewPeriod seconds. When that is reached, it will renew itself for another
    /// autoRenewPeriod seconds by charging its associated cryptocurrency account (which it creates here).
    /// If it has insufficient cryptocurrency to extend that long, it will extend as long as it can.
    /// If its balance is zero, the instance will be deleted.
    /// <p>
    /// A smart contract instance normally enforces rules, so "the code is law". For example, an
    /// ERC-20 contract prevents a transfer from being undone without a signature by the recipient of the transfer.
    /// This is always enforced if the contract instance was created with the adminKeys being null.
    /// But for some uses, it might be desirable to create something like an ERC-20 contract that has a
    /// specific group of trusted individuals who can act as a "supreme court" with the ability to override the normal
    /// operation, when a sufficient number of them agree to do so. If adminKeys is not null, then they can
    /// sign a transaction that can change the state of the smart contract in arbitrary ways, such as to reverse
    /// a transaction that violates some standard of behavior that is not covered by the code itself.
    /// The admin keys can also be used to change the autoRenewPeriod, and change the adminKeys field itself.
    /// The API currently does not implement this ability. But it does allow the adminKeys field to be set and
    /// queried, and will in the future implement such admin abilities for any instance that has a non-null adminKeys.
    /// <p>
    /// If this constructor stores information, it is charged gas to store it. There is a fee in hbars to
    /// maintain that storage until the expiration time, and that fee is added as part of the transaction fee.
    /// <p>
    /// An entity (account, file, or smart contract instance) must be created in a particular realm.
    /// If the realmID is left null, then a new realm will be created with the given admin key. If a new realm has
    /// a null adminKey, then anyone can create/modify/delete entities in that realm. But if an admin key is given,
    /// then any transaction to create/modify/delete an entity in that realm must be signed by that key,
    /// though anyone can still call functions on smart contract instances that exist in that realm.
    /// A realm ceases to exist when everything within it has expired and no longer exists.
    /// <p>
    /// The current API ignores shardID, realmID, and newRealmAdminKey, and creates everything in shard 0 and realm 0,
    /// with a null key. Future versions of the API will support multiple realms and multiple shards.
    /// <p>
    /// The optional memo field can contain a string whose length is up to 100 bytes. That is the size after Unicode
    /// NFD then UTF-8 conversion. This field can be used to describe the smart contract. It could also be used for
    /// other purposes. One recommended purpose is to hold a hexadecimal string that is the SHA-384 hash of a
    /// PDF file containing a human-readable legal contract. Then, if the admin keys are the
    /// public keys of human arbitrators, they can use that legal document to guide their decisions during a binding
    /// arbitration tribunal, convened to consider any changes to the smart contract in the future. The memo field can only
    /// be changed using the admin keys. If there are no admin keys, then it cannot be
    /// changed after the smart contract is created.
    /// </summary>
    public sealed class ContractCreateTransaction : Transaction<ContractCreateTransaction>
    {        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractCreateTransaction()
        {
            AutoRenewPeriod = DEFAULT_AUTO_RENEW_PERIOD;
            defaultMaxTransactionFee = new Hbar(20);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		private ContractCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		private ContractCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private FileId bytecodeFileId;
		private byte[] bytecode;
		private Key adminKey;
		private long gas;
		private Hbar initialBalance;
		private AccountId proxyAccountId;
		private int maxAutomaticTokenAssociations;
		private Duration autoRenewPeriod;
		
		private string contractMemo;
		private AccountId stakedAccountId;
		private long? stakedNodeId;
		private bool declineStakingReward;
		private AccountId autoRenewAccountId;

		private byte[] constructorParameters;
		private IList<HookCreationDetails> hookCreationDetails = [];

		/// <summary>The file containing the contract bytecode.</summary>
		public FileId? BytecodeFileId
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				Bytecode = null;
				field = value;
			}
		}

		/// <summary>The raw EVM bytecode.</summary>
		public byte[]? Bytecode
		{
			get => field?.CopyArray();
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				BytecodeFileId = null;
				field = value.CopyArray();
			}
		}

		/// <summary>Admin key controlling contract mutability.</summary>
		public Key AdminKey
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Maximum gas for constructor execution.</summary>
		public long Gas
		{
			get;
			set
			{
				RequireNotFrozen();

				if (value < 0)
					throw new ArgumentException("Gas must be non-negative", nameof(value));

				field = value;
			}
		}

		/// <summary>Initial HBAR balance of the contract.</summary>
		public Hbar InitialBalance
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>
		/// @deprecated — no replacement.
		/// Proxy staking account.
		/// </summary>
		public AccountId ProxyAccountId
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Maximum number of automatic token associations.</summary>
		public int MaxAutomaticTokenAssociations
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Auto-renew period for the contract.</summary>
		public Duration AutoRenewPeriod
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Constructor parameters as raw bytes.</summary>
		public ByteString ConstructorParameters
		{
			set => field = value.Copy();
			get => field ?? ByteString.Empty;
		}
		public ContractFunctionParameters ConstructorParameters_Function
		{
			set => ConstructorParameters_Bytes = value.ToBytes(null).ToByteArray();
		}
		public byte[] ConstructorParameters_Bytes
		{
			set
			{
				RequireNotFrozen();
				ConstructorParameters = ByteString.CopyFrom(value);
			}
		}


		/// <summary>Contract memo.</summary>
		public string ContractMemo
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Account this contract stakes to.</summary>
		public AccountId? StakedAccountId
		{
			get;
			set
			{
				RequireNotFrozen();
				StakedNodeId = null;
				field = value;
			}
		}

		/// <summary>Node this contract stakes to.</summary>
		public long? StakedNodeId
		{
			get;
			set
			{
				RequireNotFrozen();
				StakedAccountId = null;
				field = value;
			}
		}

		/// <summary>Declines staking rewards.</summary>
		public bool DeclineStakingReward
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}

		/// <summary>Auto-renew account.</summary>
		public AccountId AutoRenewAccountId
		{
			get;
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				RequireNotFrozen();
				field = value;
			}
		}





		/// <summary>
		/// Get the hook creation details for this contract.
		/// </summary>
		/// <returns>a copy of the hook creation details list</returns>
		public IList<HookCreationDetails> GetHooks()
        {
            return [..hookCreationDetails];
        }

        /// <summary>
        /// Add a hook creation detail to this contract.
        /// </summary>
        /// <param name="hookDetails">the hook creation details to add</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction AddHook(HookCreationDetails hookDetails)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails.Add(hookDetails);
            return this;
        }

        /// <summary>
        /// Set the hook creation details for this contract.
        /// </summary>
        /// <param name="hookDetails">the list of hook creation details</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetHooks(IList<HookCreationDetails> hookDetails)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails = [..hookDetails];
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractCreateTransactionBody}</returns>
        public Proto.ContractCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.ContractCreateTransactionBody();
            
            if (bytecodeFileId != null)
            {
                builder.FileID = bytecodeFileId.ToProtobuf();
            }

            if (bytecode != null)
            {
                builder.Initcode = ByteString.CopyFrom(bytecode);
            }

            if (proxyAccountId != null)
            {
                builder.ProxyAccountID = proxyAccountId.ToProtobuf();
            }

            if (adminKey != null)
            {
                builder.AdminKey = adminKey.ToProtobufKey();
            }

            builder.MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations;

            if (autoRenewPeriod != null)
            {
                builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(autoRenewPeriod);
            }

            builder.Gas = gas;
            builder.InitialBalance = initialBalance.ToTinybars();
            builder.ConstructorParameters = ByteString.CopyFrom(constructorParameters);
            builder.Memo = contractMemo;
            builder.DeclineReward = declineStakingReward;

            if (stakedAccountId != null)
				builder.StakedAccountId = stakedAccountId.ToProtobuf();
			else if (stakedNodeId != null)
				builder.StakedNodeId = stakedNodeId.Value;

			if (autoRenewAccountId != null)
				builder.AutoRenewAccountId = autoRenewAccountId.ToProtobuf();

			foreach (HookCreationDetails hookDetails in hookCreationDetails)
            {
                builder.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            bytecodeFileId?.ValidateChecksum(client);
            proxyAccountId?.ValidateChecksum(client);
            stakedAccountId?.ValidateChecksum(client);
            autoRenewAccountId?.ValidateChecksum(client);
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.ContractCreateInstance;

			bytecodeFileId = FileId.FromProtobuf(body.FileID);
			bytecode = body.Initcode.ToByteArray();
			proxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
			adminKey = Key.FromProtobufKey(body.AdminKey);

			maxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
			autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);

			gas = body.Gas;
            initialBalance = Hbar.FromTinybars(body.InitialBalance);
            constructorParameters = body.ConstructorParameters.ToByteArray();
            contractMemo = body.Memo;
            declineStakingReward = body.DeclineReward;

			stakedNodeId = body.StakedNodeId;
			stakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
			autoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccountId);


            // Initialize hook creation details
            hookCreationDetails.Clear();
            foreach (var protoHookDetails in body.HookCreationDetails)
            {
                hookCreationDetails.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
            }
        }

        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetCreateContractMethod;
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.ContractCreateInstance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.ContractCreateInstance = ToProtobuf();
        }
    }
}