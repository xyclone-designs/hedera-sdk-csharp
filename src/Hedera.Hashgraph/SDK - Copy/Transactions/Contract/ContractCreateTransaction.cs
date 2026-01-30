// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
using Java.Util;
using Javax.Annotation;
using Org.Bouncycastle.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Transactions.Contract
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
        private FileId bytecodeFileId = null;
        private byte[] bytecode = null;
        /// <summary>
        /// </summary>
        /// <remarks>@deprecatedwith no replacement</remarks>
        private AccountId proxyAccountId = null;
        private Key adminKey = null;
        private long gas = 0;
        private Hbar initialBalance = new Hbar(0);
        private int maxAutomaticTokenAssociations = 0;
        private Duration autoRenewPeriod = null;
        private byte[] constructorParameters = new[]
        {
        };
        private string contractMemo = "";
        private AccountId stakedAccountId = null;
        private long stakedNodeId = null;
        private bool declineStakingReward = false;
        private AccountId autoRenewAccountId = null;
        private IList<HookCreationDetails> hookCreationDetails = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContractCreateTransaction()
        {
            SetAutoRenewPeriod(DEFAULT_AUTO_RENEW_PERIOD);
            defaultMaxTransactionFee = new Hbar(20);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        ContractCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        ContractCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the file id.
        /// </summary>
        /// <returns>                         the file id as a byte code</returns>
        public FileId GetBytecodeFileId()
        {
            return bytecodeFileId;
        }

        /// <summary>
        /// Sets the file containing the smart contract byte code.
        /// <p>
        /// A copy will be made and held by the contract instance, and have the same expiration time as
        /// the instance.
        /// <p>
        /// The file must be the ASCII hexadecimal representation of the smart contract bytecode.
        /// The contract bytecode is limited in size only by the
        /// network file size limit.
        /// </summary>
        /// <param name="bytecodeFileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetBytecodeFileId(FileId bytecodeFileId)
        {
            ArgumentNullException.ThrowIfNull(bytecodeFileId);
            RequireNotFrozen();
            bytecode = null;
            bytecodeFileId = bytecodeFileId;
            return this;
        }

        /// <summary>
        /// Extract the bytecode.
        /// </summary>
        /// <returns>                         the bytecode</returns>
        public byte[] GetBytecode()
        {
            return bytecode != null ? Array.CopyOf(bytecode, bytecode.Length) : null;
        }

        /// <summary>
        /// Sets the source for the smart contract EVM bytecode.
        /// <p>
        /// The bytes of the smart contract initCode. A copy of the contents
        /// SHALL be made and held as `bytes` in smart contract state.<br/>
        /// This value is limited in length by the network transaction size
        /// limit. This entire transaction, including all fields and signatures,
        /// MUST be less than the network transaction size limit.
        /// </summary>
        /// <param name="bytecode">The bytecode</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetBytecode(byte[] bytecode)
        {
            ArgumentNullException.ThrowIfNull(bytecode);
            RequireNotFrozen();
            bytecodeFileId = null;
            bytecode = Array.CopyOf(bytecode, bytecode.Length);
            return this;
        }

        /// <summary>
        /// Get the admin key
        /// </summary>
        /// <returns>the adminKey</returns>
        public Key GetAdminKey()
        {
            return adminKey;
        }

        /// <summary>
        /// Access control for modification of the smart contract after
        /// it is created.
        /// <p>
        /// If this field is set, that key MUST sign this transaction.<br/>
        /// If this field is set, that key MUST sign each future transaction to
        /// update or delete the contract.<br/>
        /// An updateContract transaction that _only_ extends the topic
        /// expirationTime (a "manual renewal" transaction) SHALL NOT require
        /// admin key signature.
        /// <p>
        /// A contract without an admin key SHALL be immutable, except for
        /// expiration and renewal.
        /// </summary>
        /// <param name="adminKey">The Key to be set</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetAdminKey(Key adminKey)
        {
            ArgumentNullException.ThrowIfNull(adminKey);
            RequireNotFrozen();
            adminKey = adminKey;
            return this;
        }

        /// <summary>
        /// Extract the gas.
        /// </summary>
        /// <returns>                         the gas amount that was set</returns>
        public long GetGas()
        {
            return gas;
        }

        /// <summary>
        /// A maximum limit to the amount of gas to use for the constructor call.
        /// <p>
        /// The network SHALL charge the greater of the following, but SHALL NOT
        /// charge more than the value of this field.
        /// <ol>
        ///   <li>The actual gas consumed by the smart contract
        ///       constructor call.</li>
        ///   <li>`80%` of this value.</li>
        /// </ol>
        /// The `80%` factor encourages reasonable estimation, while allowing for
        /// some overage to ensure successful execution.
        /// </summary>
        /// <param name="gas">The long to be set as gas</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetGas(long gas)
        {
            RequireNotFrozen();
            if (gas < 0)
            {
                throw new ArgumentException("Gas must be non-negative");
            }

            gas = gas;
            return this;
        }

        /// <summary>
        /// Extract the initial balance.
        /// </summary>
        /// <returns>                         the initial balance in hbar</returns>
        public Hbar GetInitialBalance()
        {
            return initialBalance;
        }

        /// <summary>
        /// The amount of HBAR to use as an initial balance for the account
        /// representing the new smart contract.
        /// <p>
        /// This value is presented in tinybar
        /// (10<sup><strong>-</strong>8</sup> HBAR).<br/>
        /// The HBAR provided here will be withdrawn from the payer account that
        /// signed this transaction.
        /// </summary>
        /// <param name="initialBalance">The Hbar to be set as the initial balance</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetInitialBalance(Hbar initialBalance)
        {
            ArgumentNullException.ThrowIfNull(initialBalance);
            RequireNotFrozen();
            initialBalance = initialBalance;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>                         the proxy account id</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// 
        /// Extract the proxy account id.
        /// </remarks>
        public AccountId GetProxyAccountId()
        {
            return proxyAccountId;
        }

        /// <summary>
        /// </summary>
        /// <param name="proxyAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// 
        /// Sets the ID of the account to which this account is proxy staked.
        /// <p>
        /// If proxyAccountID is null, or is an invalid account, or is an account that isn't a node,
        /// then this account is automatically proxy staked to a node chosen by the network, but without earning payments.
        /// <p>
        /// If the proxyAccountID account refuses to accept proxy staking , or if it is not currently running a node,
        /// then it will behave as if  proxyAccountID was null.
        /// </remarks>
        public ContractCreateTransaction SetProxyAccountId(AccountId proxyAccountId)
        {
            ArgumentNullException.ThrowIfNull(proxyAccountId);
            RequireNotFrozen();
            proxyAccountId = proxyAccountId;
            return this;
        }

        /// <summary>
        /// Get the maximum number of tokens that this contract can be
        /// automatically associated with (i.e., receive air-drops from).
        /// </summary>
        /// <returns>the maxAutomaticTokenAssociations</returns>
        public int GetMaxAutomaticTokenAssociations()
        {
            return maxAutomaticTokenAssociations;
        }

        /// <summary>
        /// The maximum number of tokens that can be auto-associated with this
        /// smart contract.
        /// <p>
        /// If this is less than or equal to `used_auto_associations` (or 0), then
        /// this contract MUST manually associate with a token before transacting
        /// in that token.<br/>
        /// Following HIP-904 This value may also be `-1` to indicate no limit.<br/>
        /// This value MUST NOT be less than `-1`.
        /// </summary>
        /// <param name="maxAutomaticTokenAssociations">The maximum automatic token associations</param>
        /// <returns> {@code this}</returns>
        public ContractCreateTransaction SetMaxAutomaticTokenAssociations(int maxAutomaticTokenAssociations)
        {
            RequireNotFrozen();
            maxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
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
        /// The initial lifetime, in seconds, for the smart contract, and the number
        /// of seconds for which the smart contract will be automatically renewed
        /// upon expiration.
        /// <p>
        /// This value MUST be set.<br/>
        /// This value MUST be greater than the configured MIN_AUTORENEW_PERIOD.<br/>
        /// This value MUST be less than the configured MAX_AUTORENEW_PERIOD.<br/>
        /// </summary>
        /// <param name="autoRenewPeriod">The Duration to be set for auto renewal</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            ArgumentNullException.ThrowIfNull(autoRenewPeriod);
            RequireNotFrozen();
            autoRenewPeriod = autoRenewPeriod;
            return this;
        }

        /// <summary>
        /// Extract the constructor parameters.
        /// </summary>
        /// <returns>                         the byte string representation</returns>
        public ByteString GetConstructorParameters()
        {
            return ByteString.CopyFrom(constructorParameters);
        }

        /// <summary>
        /// Sets the constructor parameters as their raw bytes.
        /// <p>
        /// Use this instead of {@link #setConstructorParameters(ContractFunctionParameters)} if you have already
        /// pre-encoded a solidity function call.
        /// </summary>
        /// <param name="constructorParameters">The constructor parameters</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetConstructorParameters(byte[] constructorParameters)
        {
            RequireNotFrozen();
            constructorParameters = Array.CopyOf(constructorParameters, constructorParameters.Length);
            return this;
        }

        /// <summary>
        /// Sets the parameters to pass to the constructor.
        /// </summary>
        /// <param name="constructorParameters">The contructor parameters</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetConstructorParameters(ContractFunctionParameters constructorParameters)
        {
            ArgumentNullException.ThrowIfNull(constructorParameters);
            return SetConstructorParameters(constructorParameters.ToBytes(null).ToByteArray());
        }

        /// <summary>
        /// Extract the contract memo.
        /// </summary>
        /// <returns>                         the contract's memo</returns>
        public string GetContractMemo()
        {
            return contractMemo;
        }

        /// <summary>
        /// A short memo for this smart contract.
        /// <p>
        /// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
        /// (default 100) bytes when encoded as UTF-8.
        /// </summary>
        /// <param name="memo">The String to be set as the memo</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetContractMemo(string memo)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(memo);
            contractMemo = memo;
            return this;
        }

        /// <summary>
        /// ID of the account to which this contract will stake
        /// </summary>
        /// <returns>ID of the account to which this contract will stake.</returns>
        public AccountId GetStakedAccountId()
        {
            return stakedAccountId;
        }

        /// <summary>
        /// Set the account to which this contract will stake
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this contract will stake.</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetStakedAccountId(AccountId stakedAccountId)
        {
            RequireNotFrozen();
            stakedAccountId = stakedAccountId;
            stakedNodeId = null;
            return this;
        }

        /// <summary>
        /// The node to which this contract will stake
        /// </summary>
        /// <returns>ID of the node this contract will be staked to.</returns>
        public long GetStakedNodeId()
        {
            return stakedNodeId;
        }

        /// <summary>
        /// The ID of a network node.
        /// <p>
        /// This smart contract SHALL stake its HBAR to this node.
        /// <p>
        /// <blockquote>Note: node IDs do fluctuate as node operators change.
        /// Most contracts are immutable, and a contract staking to an invalid
        /// node ID SHALL NOT participate in staking. Immutable contracts MAY
        /// find it more reliable to use a proxy account for staking
        /// (via `staked_account_id`) to enable updating the _effective_ staking
        /// node ID when necessary through updating the proxy
        /// account.</blockquote>
        /// </summary>
        /// <param name="stakedNodeId">ID of the node this contract will be staked to.</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetStakedNodeId(long stakedNodeId)
        {
            RequireNotFrozen();
            stakedNodeId = stakedNodeId;
            stakedAccountId = null;
            return this;
        }

        /// <summary>
        /// If true, the contract declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <returns>If true, the contract declines receiving a staking reward. The default value is false.</returns>
        public bool GetDeclineStakingReward()
        {
            return declineStakingReward;
        }

        /// <summary>
        /// A flag indicating that this smart contract declines to receive any
        /// reward for staking its HBAR balance to help secure the network.
        /// <p>
        /// If set to true, this smart contract SHALL NOT receive any reward for
        /// staking its HBAR balance to help secure the network, regardless of
        /// staking configuration, but MAY stake HBAR to support the network
        /// without reward.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the contract declines receiving a staking reward. The default value is false.</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetDeclineStakingReward(bool declineStakingReward)
        {
            RequireNotFrozen();
            declineStakingReward = declineStakingReward;
            return this;
        }

        /// <summary>
        /// Get the auto renew accountId.
        /// </summary>
        /// <returns>                         the auto renew accountId</returns>
        public AccountId GetAutoRenewAccountId()
        {
            return autoRenewAccountId;
        }

        /// <summary>
        /// The id of an account, in the same shard and realm as this smart
        /// contract, that has signed this transaction, allowing the network to use
        /// its balance, when needed, to automatically extend this contract's
        /// expiration time.
        /// <p>
        /// If this field is set, that key MUST sign this transaction.<br/>
        /// If this field is set, then the network SHALL deduct the necessary fees
        /// from the designated auto-renew account, if that account has sufficient
        /// balance. If the auto-renewal account does not have sufficient balance,
        /// then the fees for contract renewal SHALL be deducted from the HBAR
        /// balance held by the smart contract.<br/>
        /// If this field is not set, then all renewal fees SHALL be deducted from
        /// the HBAR balance held by this contract.
        /// </summary>
        /// <param name="autoRenewAccountId">The AccountId to be set for auto-renewal</param>
        /// <returns>{@code this}</returns>
        public ContractCreateTransaction SetAutoRenewAccountId(AccountId autoRenewAccountId)
        {
            ArgumentNullException.ThrowIfNull(autoRenewAccountId);
            RequireNotFrozen();
            autoRenewAccountId = autoRenewAccountId;
            return this;
        }

        /// <summary>
        /// Get the hook creation details for this contract.
        /// </summary>
        /// <returns>a copy of the hook creation details list</returns>
        public IList<HookCreationDetails> GetHooks()
        {
            return new List(hookCreationDetails);
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
            hookCreationDetails = new List(hookDetails);
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link ContractCreateTransactionBody}</returns>
        ContractCreateTransactionBody Build()
        {
            var builder = ContractCreateTransactionBody.NewBuilder();
            if (bytecodeFileId != null)
            {
                builder.FileID(bytecodeFileId.ToProtobuf());
            }

            if (bytecode != null)
            {
                builder.Initcode(ByteString.CopyFrom(bytecode));
            }

            if (proxyAccountId != null)
            {
                builder.ProxyAccountID(proxyAccountId.ToProtobuf());
            }

            if (adminKey != null)
            {
                builder.AdminKey(adminKey.ToProtobufKey());
            }

            builder.MaxAutomaticTokenAssociations(maxAutomaticTokenAssociations);
            if (autoRenewPeriod != null)
            {
                builder.AutoRenewPeriod(Utils.DurationConverter.ToProtobuf(autoRenewPeriod));
            }

            builder.Gas(gas);
            builder.InitialBalance(initialBalance.ToTinybars());
            builder.ConstructorParameters(ByteString.CopyFrom(constructorParameters));
            builder.Memo(contractMemo);
            builder.DeclineReward(declineStakingReward);
            if (stakedAccountId != null)
            {
                builder.StakedAccountId(stakedAccountId.ToProtobuf());
            }
            else if (stakedNodeId != null)
            {
                builder.StakedNodeId(stakedNodeId);
            }

            if (autoRenewAccountId != null)
            {
                builder.AutoRenewAccountId(autoRenewAccountId.ToProtobuf());
            }

            foreach (HookCreationDetails hookDetails in hookCreationDetails)
            {
                builder.AddHookCreationDetails(hookDetails.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (bytecodeFileId != null)
            {
                bytecodeFileId.ValidateChecksum(client);
            }

            if (proxyAccountId != null)
            {
                proxyAccountId.ValidateChecksum(client);
            }

            if (stakedAccountId != null)
            {
                stakedAccountId.ValidateChecksum(client);
            }

            if (autoRenewAccountId != null)
            {
                autoRenewAccountId.ValidateChecksum(client);
            }
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.ContractCreateInstance();
            if (body.HasFileID())
            {
                bytecodeFileId = FileId.FromProtobuf(body.GetFileID());
            }

            if (body.HasInitcode())
            {
                bytecode = body.GetInitcode().ToByteArray();
            }

            if (body.HasProxyAccountID())
            {
                proxyAccountId = AccountId.FromProtobuf(body.GetProxyAccountID());
            }

            if (body.AdminKey is not null)
            {
                adminKey = Key.FromProtobufKey(body.AdminKey);
            }

            maxAutomaticTokenAssociations = body.GetMaxAutomaticTokenAssociations();
            if (body.HasAutoRenewPeriod())
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.GetAutoRenewPeriod());
            }

            gas = body.GetGas();
            initialBalance = Hbar.FromTinybars(body.GetInitialBalance());
            constructorParameters = body.GetConstructorParameters().ToByteArray();
            contractMemo = body.GetMemo();
            declineStakingReward = body.DeclineReward;
            if (body.HasStakedAccountId())
            {
                stakedAccountId = AccountId.FromProtobuf(body.GetStakedAccountId());
            }

            if (body.HasStakedNodeId())
            {
                stakedNodeId = body.GetStakedNodeId();
            }

            if (body.HasAutoRenewAccountId())
            {
                autoRenewAccountId = AccountId.FromProtobuf(body.GetAutoRenewAccountId());
            }


            // Initialize hook creation details
            hookCreationDetails.Clear();
            foreach (var protoHookDetails in body.GetHookCreationDetailsList())
            {
                hookCreationDetails.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return SmartContractServiceGrpc.GetCreateContractMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetContractCreateInstance(Build());
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.SetContractCreateInstance(Build());
        }
    }
}