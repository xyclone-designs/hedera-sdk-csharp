// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
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
        private IList<HookCreationDetails> _HookCreationDetails = [];

        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountCreateTransaction()
        {
            DefaultMaxTransactionFee = Hbar.From(5);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal AccountCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal AccountCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private static EvmAddress ExtractAlias(Key key)
		{
			if (key is PrivateKeyECDSA privatekeyecdsa)
			{
				return privatekeyecdsa.GetPublicKey().ToEvmAddress();
			}
			else if (key is PublicKeyECDSA publickeyecdsa)
			{
				return publickeyecdsa.ToEvmAddress();
			}
			else
			{
				throw new BadKeyException("Private key is not ECDSA");
			}
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
            RequireNotFrozen();
			Key = key;
            Alias = ExtractAlias(key);
            return this;
        }
		/// <summary>
		/// Sets key where it is explicitly called out that the Alias is not set
		/// </summary>
		/// <param name="key"></param>
		/// <returns>this</returns>
		public AccountCreateTransaction SetKeyWithoutAlias(Key key)
		{
			RequireNotFrozen();
			Key = key;
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
            RequireNotFrozen();
            Key = key;
            Alias = ExtractAlias(ecdsaKey);
            return this;
        }

        /// <summary>
        /// An amount, in tinybar, to deposit to the newly created account.
        /// <p>
        /// The deposited amount SHALL be debited to the "payer" account for this
        /// transaction.
        /// </summary>
        /// <param name="InitialBalance">the initial balance.</param>
        /// <returns>{@code this}</returns>
        public Hbar InitialBalance { get; set { RequireNotFrozen(); field = value; } } = new(0);
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
        public bool ReceiverSigRequired { get; set { RequireNotFrozen(); field = value; } }
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
		public Key? Key { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// </summary>
		/// <param name="ProxyAccountId">the proxy account ID.</param>
		/// <returns>{@code this}</returns>
		/// <remarks>
		/// @deprecatedwith no replacement
		/// 
		/// Set the ID of the account to which this account is proxy staked.
		/// 
		/// Use `staked_id` instead.<br/>
		/// An account identifier for a staking proxy.
		/// </remarks>
		public AccountId? ProxyAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// Set the auto renew period for this account.
		/// 
		/// <p>A Hedera™ account is charged to extend its expiration date every renew period. If it
		/// doesn't have enough balance, it extends as long as possible. If the balance is zero when it
		/// expires, then the account is deleted.
		/// 
		/// <p>This is defaulted to 3 months by the SDK.
		/// </summary>
		/// <param name="AutoRenewPeriod">the auto renew period for this account.</param>
		/// <returns>{@code this}</returns>
		public Duration AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } } = DEFAULT_AUTO_RENEW_PERIOD;
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
		public int MaxAutomaticTokenAssociations { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// A short description of this Account.
		/// <p>
		/// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
		/// (default 100) bytes when encoded as UTF-8.
		/// </summary>
		/// <param name="memo">the memo</param>
		/// <returns>                         {@code this}</returns>
		public string AccountMemo { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <summary>
		/// Set the account to which this account will stake
		/// </summary>
		/// <param name="StakedAccountId">ID of the account to which this account will stake.</param>
		/// <returns>{@code this}</returns>
		public AccountId? StakedAccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// ID of the node this account is staked to.
		/// <p>
		/// If this account is not currently staking its balances, then this
		/// field, if set, SHALL be the sentinel value of `-1`.<br/>
		/// Wallet software SHOULD surface staking issues to users and provide a
		/// simple mechanism to update staking to a new node ID in the event the
		/// prior staked node ID ceases to be valid.
		/// </summary>
		/// <param name="StakedNodeId">ID of the node this account will be staked to.</param>
		/// <returns>{@code this}</returns>
		public long? StakedNodeId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// If true, the account declines receiving a staking reward. The default value is false.
		/// </summary>
		/// <param name="DeclineStakingReward">- If true, the account declines receiving a staking reward. The default value is false.</param>
		/// <returns>{@code this}</returns>
		public bool DeclineStakingReward { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The bytes to be used as the account's Alias.
		/// <p>
		/// The bytes must be formatted as the calcluated last 20 bytes of the
		/// keccak-256 of the ECDSA primitive key.
		/// <p>
		/// All other types of keys, including but not limited to ED25519, ThresholdKey, KeyList, ContractID, and
		/// delegatable_contract_id, are not supported.
		/// <p>
		/// At most only one account can ever have a given Alias on the network.
		/// </summary>
		/// <param name="Alias">The ethereum account 20-byte EVM address</param>
		/// <returns>{@code this}</returns>
		public EvmAddress? Alias { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The ethereum account 20-byte EVM address to be used as the account's Alias. This EVM address may be either
		/// the encoded form of the shard.realm.num or the keccak-256 hash of a ECDSA_SECP256K1 primitive key.
		/// <p>
		/// A given Alias can map to at most one account on the network at a time. This uniqueness will be enforced
		/// relative to Aliases currently on the network at Alias assignment.
		/// <p>
		/// If a transaction creates an account using an Alias, any further crypto transfers to that Alias will
		/// simply be deposited in that account, without creating anything, and with no creation fee being charged.
		/// </summary>
		/// <param name="AliasEvmAddress">The ethereum account 20-byte EVM address</param>
		/// <returns>{@code this}</returns>
		/// <exception cref="IllegalArgumentException">when evmAddress is invalid</exception>
		public string Alias_String 
        { 
            set 
            {
				if ((value.StartsWith("0x") && value.Length == 42) || value.Length == 40)
					Alias = EvmAddress.FromString(value);
				else throw new ArgumentException("evmAddress must be an a valid EVM address with \"0x\" prefix");
			}
        }

        /// <summary>
        /// Get the hook creation details for this account.
        /// </summary>
        /// <returns>a copy of the hook creation details list</returns>
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
				_HookCreationDetails = value;
            } 
        }
		public IReadOnlyList<HookCreationDetails> HookCreationDetails_Read { get { return _HookCreationDetails.AsReadOnly(); } }

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.CryptoCreateTransactionBody}</returns>
		public Proto.CryptoCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoCreateTransactionBody
            {
				InitialBalance = (ulong)InitialBalance.ToTinybars(),
				ReceiverSigRequired = ReceiverSigRequired,
				AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod),
				Memo = AccountMemo,
				MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations,
				DeclineReward = DeclineStakingReward,
			};

            if (ProxyAccountId != null)
            {
                builder.ProxyAccountID = ProxyAccountId.ToProtobuf();
            }

            if (Key != null)
            {
                builder.Key = Key.ToProtobufKey();
            }

            if (Alias != null)
            {
                builder.Alias = ByteString.CopyFrom(Alias.ToBytes());
            }

            if (StakedAccountId != null)
            {
                builder.StakedAccountId = StakedAccountId.ToProtobuf();
            }
            else if (StakedNodeId != null)
            {
                builder.StakedNodeId = StakedNodeId.Value;
            }

            foreach (HookCreationDetails hookDetails in HookCreationDetails)
            {
                builder.HookCreationDetails.Add(hookDetails.ToProtobuf());
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			ProxyAccountId?.ValidateChecksum(client);
			StakedAccountId?.ValidateChecksum(client);
		}

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoCreateAccount;

            if (body.ProxyAccountID is not null)
            {
                ProxyAccountId = AccountId.FromProtobuf(body.ProxyAccountID);
            }

            if (body.Key is not null)
            {
                Key = Key.FromProtobufKey(body.Key);
            }

            if (body.AutoRenewPeriod is not null)
            {
                AutoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            InitialBalance = Hbar.FromTinybars((long)body.InitialBalance);
            AccountMemo = body.Memo;
            ReceiverSigRequired = body.ReceiverSigRequired;
            MaxAutomaticTokenAssociations = body.MaxAutomaticTokenAssociations;
            DeclineStakingReward = body.DeclineReward;
            if (body.StakedAccountId is not null)
            {
                StakedAccountId = AccountId.FromProtobuf(body.StakedAccountId);
            }

			StakedNodeId = body.StakedNodeId;
            Alias = EvmAddress.FromAliasBytes(body.Alias);

			// Initialize hook creation details
			_HookCreationDetails = [.. body.HookCreationDetails.Select(_ => Hook.HookCreationDetails.FromProtobuf(_))];
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.createAccount);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoCreateAccount = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoCreateAccount = ToProtobuf();
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