// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /*
     * Create a new Hedera™ account.
     *
     * If the auto_renew_account field is set, the key of the referenced account
     * MUST sign this transaction.
     * Current limitations REQUIRE that `shardID` and `realmID` both MUST be `0`.
     * This is expected to change in the future.
     *
     * ### Block Stream Effects
     * The newly created account SHALL be included in State Changes.
     */
    public sealed class AccountCreateTransaction : Transaction<AccountCreateTransaction>
    {
        private AccountId proxyAccountId = null;
        private Key key = null;
        private string accountMemo = "";
        private Hbar initialBalance = new Hbar(0);
        private bool receiverSigRequired = false;
        private Duration autoRenewPeriod = DEFAULT_AUTO_RENEW_PERIOD;
        private int maxAutomaticTokenAssociations = 0;
        private AccountId stakedAccountId = null;
        private long? stakedNodeId = null;
        private bool declineStakingReward = false;
        private EvmAddress alias = null;
        private IList<HookCreationDetails> hookCreationDetails = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountCreateTransaction()
        {
            defaultMaxTransactionFee = Hbar.From(5);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        AccountCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AccountCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the key.
        /// </summary>
        /// <returns>                         the creating account's key</returns>
        public Key GetKey()
        {
            return key;
        }

        /// <summary>
        /// The identifying key for this account.
        /// This key represents the account owner, and is required for most actions
        /// involving this account that do not modify the account itself. This key
        /// may also identify the account for smart contracts.
        /// <p>
        /// This field is REQUIRED.
        /// This `Key` MUST NOT be an empty `KeyList` and MUST contain at least one
        /// "primitive" (i.e. cryptographic) key value.
        /// </summary>
        /// <param name="key">the key for this account.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetKey(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            key = key;
            return this;
        }

        /// <summary>
        /// Sets ECDSA private key, derives and sets it's EVM address in the background. Essentially does
        /// {@link AccountCreateTransaction#setKey(Key)} +
        /// {@link AccountCreateTransaction#setAlias(EvmAddress)}
        /// </summary>
        /// <param name="key"></param>
        /// <returns>this</returns>
        public AccountCreateTransaction SetKeyWithAlias(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            key = key;
            alias = ExtractAlias(key);
            return this;
        }

        /// <summary>
        /// Sets the account key and a separate ECDSA key that the EVM address is derived from.
        /// A user must sign the transaction with both keys for this flow to be successful.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ecdsaKey"></param>
        /// <returns>this</returns>
        public AccountCreateTransaction SetKeyWithAlias(Key key, Key ecdsaKey)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            key = key;
            alias = ExtractAlias(ecdsaKey);
            return this;
        }

        /// <summary>
        /// Sets key where it is explicitly called out that the alias is not set
        /// </summary>
        /// <param name="key"></param>
        /// <returns>this</returns>
        public AccountCreateTransaction SetKeyWithoutAlias(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            key = key;
            return this;
        }

        /// <summary>
        /// Extract the amount in hbar.
        /// </summary>
        /// <returns>                         the initial balance for the new account</returns>
        public Hbar GetInitialBalance()
        {
            return initialBalance;
        }

        /// <summary>
        /// An amount, in tinybar, to deposit to the newly created account.
        /// <p>
        /// The deposited amount SHALL be debited to the "payer" account for this
        /// transaction.
        /// </summary>
        /// <param name="initialBalance">the initial balance.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetInitialBalance(Hbar initialBalance)
        {
            ArgumentNullException.ThrowIfNull(initialBalance);
            RequireNotFrozen();
            initialBalance = initialBalance;
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
        /// A flag indicating the account holder must authorize all incoming
        /// token transfers.
        /// <p>
        /// If this flag is set then any transaction that would result in adding
        /// hbar or other tokens to this account balance MUST be signed by the
        /// identifying key of this account (that is, the `key` field).<br/>
        /// If this flag is set, then the account key (`key` field) MUST sign
        /// this create transaction, in addition to the transaction payer.
        /// </summary>
        /// <param name="receiveSignatureRequired">true to require a signature when receiving hbars.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetReceiverSignatureRequired(bool receiveSignatureRequired)
        {
            RequireNotFrozen();
            receiverSigRequired = receiveSignatureRequired;
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
        /// <param name="proxyAccountId">the proxy account ID.</param>
        /// <returns>{@code this}</returns>
        /// <remarks>
        /// @deprecatedwith no replacement
        /// 
        /// Set the ID of the account to which this account is proxy staked.
        /// 
        /// Use `staked_id` instead.<br/>
        /// An account identifier for a staking proxy.
        /// </remarks>
        public AccountCreateTransaction SetProxyAccountId(AccountId proxyAccountId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(proxyAccountId);
            proxyAccountId = proxyAccountId;
            return this;
        }

        /// <summary>
        /// Extract the duration for the auto renew period.
        /// </summary>
        /// <returns>                         the duration for auto-renew</returns>
        public Duration GetAutoRenewPeriod()
        {
            return autoRenewPeriod;
        }

        /// <summary>
        /// Set the auto renew period for this account.
        /// 
        /// <p>A Hedera™ account is charged to extend its expiration date every renew period. If it
        /// doesn't have enough balance, it extends as long as possible. If the balance is zero when it
        /// expires, then the account is deleted.
        /// 
        /// <p>This is defaulted to 3 months by the SDK.
        /// </summary>
        /// <param name="autoRenewPeriod">the auto renew period for this account.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetAutoRenewPeriod(Duration autoRenewPeriod)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(autoRenewPeriod);
            autoRenewPeriod = autoRenewPeriod;
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
        /// created accounts (e.g. smart contracts), which default to -1.
        /// <p>
        /// If this value is `0`, then this account MUST manually associate to
        /// a token before holding or transacting in that token.<br/>
        /// This value MAY also be `-1` to indicate no limit.<br/>
        /// This value MUST NOT be less than `-1`.
        /// </summary>
        /// <param name="amount">the amount of tokens</param>
        /// <returns>                         {@code this}</returns>
        public AccountCreateTransaction SetMaxAutomaticTokenAssociations(int amount)
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
        public AccountCreateTransaction SetAccountMemo(string memo)
        {
            ArgumentNullException.ThrowIfNull(memo);
            RequireNotFrozen();
            accountMemo = memo;
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
        /// Set the account to which this account will stake
        /// </summary>
        /// <param name="stakedAccountId">ID of the account to which this account will stake.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetStakedAccountId(AccountId stakedAccountId)
        {
            RequireNotFrozen();
            stakedAccountId = stakedAccountId;
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
        public AccountCreateTransaction SetStakedNodeId(long stakedNodeId)
        {
            RequireNotFrozen();
            stakedNodeId = stakedNodeId;
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
        /// If true, the account declines receiving a staking reward. The default value is false.
        /// </summary>
        /// <param name="declineStakingReward">- If true, the account declines receiving a staking reward. The default value is false.</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetDeclineStakingReward(bool declineStakingReward)
        {
            RequireNotFrozen();
            declineStakingReward = declineStakingReward;
            return this;
        }

        /// <summary>
        /// The bytes to be used as the account's alias.
        /// <p>
        /// The bytes must be formatted as the calcluated last 20 bytes of the
        /// keccak-256 of the ECDSA primitive key.
        /// <p>
        /// All other types of keys, including but not limited to ED25519, ThresholdKey, KeyList, ContractID, and
        /// delegatable_contract_id, are not supported.
        /// <p>
        /// At most only one account can ever have a given alias on the network.
        /// </summary>
        public EvmAddress GetAlias()
        {
            return alias;
        }

        /// <summary>
        /// The bytes to be used as the account's alias.
        /// <p>
        /// The bytes must be formatted as the calcluated last 20 bytes of the
        /// keccak-256 of the ECDSA primitive key.
        /// <p>
        /// All other types of keys, including but not limited to ED25519, ThresholdKey, KeyList, ContractID, and
        /// delegatable_contract_id, are not supported.
        /// <p>
        /// At most only one account can ever have a given alias on the network.
        /// </summary>
        /// <param name="alias">The ethereum account 20-byte EVM address</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetAlias(EvmAddress alias)
        {
            RequireNotFrozen();
            alias = alias;
            return this;
        }

        /// <summary>
        /// The ethereum account 20-byte EVM address to be used as the account's alias. This EVM address may be either
        /// the encoded form of the shard.realm.num or the keccak-256 hash of a ECDSA_SECP256K1 primitive key.
        /// <p>
        /// A given alias can map to at most one account on the network at a time. This uniqueness will be enforced
        /// relative to aliases currently on the network at alias assignment.
        /// <p>
        /// If a transaction creates an account using an alias, any further crypto transfers to that alias will
        /// simply be deposited in that account, without creating anything, and with no creation fee being charged.
        /// </summary>
        /// <param name="aliasEvmAddress">The ethereum account 20-byte EVM address</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalArgumentException">when evmAddress is invalid</exception>
        public AccountCreateTransaction SetAlias(string aliasEvmAddress)
        {
            if ((aliasEvmAddress.StartsWith("0x") && aliasEvmAddress.Length == 42) || aliasEvmAddress.Length == 40)
            {
                EvmAddress address = EvmAddress.FromString(aliasEvmAddress);
                return SetAlias(address);
            }
            else
            {
                throw new ArgumentException("evmAddress must be an a valid EVM address with \"0x\" prefix");
            }
        }

        /// <summary>
        /// Get the hook creation details for this account.
        /// </summary>
        /// <returns>a copy of the hook creation details list</returns>
        public IList<HookCreationDetails> GetHooks()
        {
            return [.. hookCreationDetails];
        }

        /// <summary>
        /// Add a hook creation detail to this account.
        /// </summary>
        /// <param name="hookDetails">the hook creation details to add</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction AddHook(HookCreationDetails hookDetails)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails.Add(hookDetails);
            return this;
        }

        /// <summary>
        /// Set the hook creation details for this account.
        /// </summary>
        /// <param name="hookDetails">the list of hook creation details</param>
        /// <returns>{@code this}</returns>
        public AccountCreateTransaction SetHooks(IList<HookCreationDetails> hookDetails)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hookDetails, "hookDetails cannot be null");
            hookCreationDetails = [.. hookDetails];
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoCreateTransactionBody}</returns>
        Proto.CryptoCreateTransactionBody Build()
        {
            var builder = new Proto.CryptoCreateTransactionBody
            {
				InitialBalance = (ulong)initialBalance.ToTinybars(),
				ReceiverSigRequired = receiverSigRequired,
				AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(autoRenewPeriod),
				Memo = accountMemo,
				MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations,
				DeclineReward = declineStakingReward,
			};

            if (proxyAccountId != null)
            {
                builder.ProxyAccountID = proxyAccountId.ToProtobuf();
            }

            if (key != null)
            {
                builder.Key = key.ToProtobufKey();
            }

            if (alias != null)
            {
                builder.Alias = ByteString.CopyFrom(alias.ToBytes());
            }

            if (stakedAccountId != null)
            {
                builder.StakedAccountId = stakedAccountId.ToProtobuf();
            }
            else if (stakedNodeId != null)
            {
                builder.StakedNodeId = stakedNodeId.Value;
            }

            foreach (HookCreationDetails hookDetails in hookCreationDetails)
            {
                builder.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
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
            var body = sourceTransactionBody.CryptoCreateAccount;
            if (body.ProxyAccountID is not null)
            {
                proxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            }

            if (body.Key is not null)
            {
                key = Key.FromProtobufKey(body.Key);
            }

            if (body.AutoRenewPeriod is not null)
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            initialBalance = Hbar.FromTinybars((long)body.InitialBalance);
            accountMemo = body.Memo;
            receiverSigRequired = body.ReceiverSigRequired;
            maxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            declineStakingReward = body.DeclineReward;
            if (body.StakedAccountId is not null)
            {
                stakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            }

			stakedNodeId = body.StakedNodeId;

            alias = EvmAddress.FromAliasBytes(body.Alias);

            // Initialize hook creation details
            hookCreationDetails.Clear();
            foreach (var protoHookDetails in body.HookCreationDetails)
            {
                hookCreationDetails.Add(HookCreationDetails.FromProtobuf(protoHookDetails));
            }
        }

        private EvmAddress ExtractAlias(Key key)
        {
            var isPrivateEcdsaKey = key is PrivateKeyECDSA;
            var isPublicEcdsaKey = key is PublicKeyECDSA;
            if (isPrivateEcdsaKey)
            {
                return ((PrivateKeyECDSA)key).GetPublicKey().ToEvmAddress();
            }
            else if (isPublicEcdsaKey)
            {
                return ((PublicKeyECDSA)key).ToEvmAddress();
            }
            else
            {
                throw new BadKeyException("Private key is not ECDSA");
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCreateAccountMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoCreateAccount = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoCreateAccount = Build();
        }
    }
}