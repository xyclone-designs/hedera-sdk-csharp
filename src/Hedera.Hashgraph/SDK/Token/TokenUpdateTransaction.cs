// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Update an existing token.
    /// 
    /// This transaction SHALL NOT update any field that is not set.<br/>
    /// Most changes MUST be signed by the current `admin_key` of the token. If the
    /// token does not currently have a valid `admin_key`, then this transaction
    /// MUST NOT set any value other than `expiry` or a non-admin key.<br/>
    /// If the `treasury` is set to a new account, the new account MUST sign this
    /// transaction.<br/>
    /// If the `treasury` is set to a new account for a _non-fungible/unique_ token,
    /// The current treasury MUST NOT hold any tokens, or the network configuration
    /// property `tokens.nfts.useTreasuryWildcards` MUST be set.
    /// 
    /// #### Requirements for Keys
    /// Any of the key values may be changed, even without an admin key, but the
    /// key to be changed MUST have an existing valid key assigned, and both the
    /// current key and the new key MUST sign the transaction.<br/>
    /// A key value MAY be set to an empty `KeyList`. In this case the existing
    /// key MUST sign this transaction, but the new value is not a valid key, and the
    /// update SHALL effectively remove the existing key.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenUpdateTransaction : Transaction<TokenUpdateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to delete.<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.<br/>
        /// If any field other than `expiry` is set, the identified token MUST
        /// have a valid `admin_key`.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A new name for the token.<br/>
        /// This is generally the "full name" displayed in wallet software.
        /// <p>
        /// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value, if set, MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="name">the token name</param>
        /// <returns>{@code this}</returns>
        public virtual string? TokenName { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
        /// <summary>
        /// A new symbol to use for the token.
        /// <p>
        /// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value, if set, MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="symbol">the token symbol</param>
        /// <returns>{@code this}</returns>
        public virtual string? TokenSymbol { get; set { RequireNotFrozen(); field = value; } } = string.Empty;
		/// <summary>
		/// A new treasury account identifier.
		/// <p>
		/// If set,
		/// - The identified account SHALL be designated the "treasury" for the
		///   token, and all tokens "minted" SHALL be delivered to that account
		///   following this transaction.<br/>
		/// - The identified account MUST exist, MUST NOT be expired, MUST NOT be
		///   deleted, and SHOULD have a non-zero HBAR balance.<br/>
		/// - The identified account SHALL be associated to this token.
		/// - The full balance of this token held by the prior treasury account
		///   SHALL be transferred to the new treasury account, if the token type
		///   is fungible/common.
		/// - If the token type is non-fungible/unique, the previous treasury
		///   account MUST NOT hold any tokens of this type.
		/// - The new treasury account key MUST sign this transaction.
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <returns>{@code this}</returns>
		public virtual AccountId? TreasuryAccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A Hedera key for token administration.
        /// <p>
        /// This key, if set, SHALL have administrative authority for this token and
        /// MAY authorize token update and/or token delete transactions.<br/>
        /// If this key is set to an empty `KeyList`, this token SHALL be
        /// immutable thereafter, except for expiration and renewal.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? AdminKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A Hedera key for managing account KYC.
        /// <p>
        /// This key, if set, SHALL have KYC authority for this token and
        /// MAY authorize transactions to grant or revoke KYC for accounts.<br/>
        /// If this key is not set, or is an empty `KeyList`, KYC status for this
        /// token SHALL NOT be granted or revoked for any account.<br/>
        /// If this key is removed after granting KYC, those grants SHALL remain
        /// and cannot be revoked.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the kyc key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? KycKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A Hedera key for managing asset "freeze".
        /// <p>
        /// This key, if set, SHALL have "freeze" authority for this token and
        /// MAY authorize transactions to freeze or unfreeze accounts
        /// with respect to this token.<br/>
        /// If this key is set to an empty `KeyList`, this token
        /// SHALL NOT be frozen or unfrozen for any account.<br/>
        /// If this key is removed after freezing accounts, those accounts
        /// SHALL remain frozen and cannot be unfrozen.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the freeze key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? FreezeKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A Hedera key for wiping tokens from accounts.
        /// <p>
        /// This key, if set, SHALL have "wipe" authority for this token and
        /// MAY authorize transactions to "wipe" any amount of this token from
        /// any account, effectively burning the tokens "wiped".<br/>
        /// If this key is set to an empty `KeyList`, it SHALL NOT be
        /// possible to "wipe" this token from an account.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the wipe key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? WipeKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An Hedera key for "minting" and "burning" tokens.
        /// <p>
        /// This key, if set, MAY authorize transactions to "mint" new tokens to
        /// be delivered to the token treasury or "burn" tokens held by the
        /// token treasury.<br/>
        /// If this key is set to an empty `KeyList`, it SHALL NOT be
        /// possible to change the supply of tokens and neither "mint" nor "burn"
        /// transactions SHALL be permitted.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the supply key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? SupplyKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An Hedera key for managing the token custom fee schedule.
        /// <p>
        /// This key, if set, MAY authorize transactions to modify the
        /// `custom_fees` for this token.<br/>
        /// If this key is set to an empty `KeyList`, the `custom_fees`
        /// for this token SHALL NOT be modified.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the fee schedule key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? FeeScheduleKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An Hedera key for managing token "pause".
        /// <p>
        /// This key, if set, SHALL have "pause" authority for this token and
        /// MAY authorize transactions to pause or unpause this token.<br/>
        /// If this key is set to an empty `KeyList`, this token
        /// SHALL NOT be paused or unpaused.<br/>
        /// If this key is removed while the token is paused, the token cannot
        /// be unpaused and SHALL remain paused.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the pause key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? PauseKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A Hedera key for managing the token `metadata`.
        /// <p>
        /// This key, if set, MAY authorize transactions to modify the
        /// `metadata` for this token.<br/>
        /// If this key is set to an empty `KeyList`, the `metadata`
        /// for this token SHALL NOT be modified.<br/>
        /// If set, this key MUST be a valid key or an empty `KeyList`.<br/>
        /// If set to a valid key, the previous key and new key MUST both
        /// sign this transaction.
        /// </summary>
        /// <param name="key">the metadata key</param>
        /// <returns>{@code this}</returns>
        public virtual Key? MetadataKey { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An expiration timestamp.
        /// <p>
        /// If this value is set, the automatic renewal account is not set for the
        /// identified token, and token expiration is enabled in network
        /// configuration, this token SHALL expire when the consensus time exceeds
        /// this value, and MAY be subsequently removed from the network state.<br/>
        /// If `autoRenewAccount` is set or the `auto_renew_account_id` is set for
        /// the identified token, the token SHALL be subject to automatic renewal
        /// when the consensus time exceeds this value.
        /// </summary>
        /// <param name="expirationTime">the expiration time</param>
        /// <returns>{@code this}</returns>
        public virtual Timestamp? ExpirationTime
		{
            get; 
            set 
            { 
                RequireNotFrozen(); 
                field = value;
                AutoRenewPeriod = null;
                ExpirationTimeDuration = null;

			} 
        }
        public virtual Duration? ExpirationTimeDuration 
        {
            get; 
            set 
            { 
                RequireNotFrozen(); 
                field = value;
                AutoRenewPeriod = null;
                ExpirationTime = null;

			} 
        }
		/// <summary>
		/// An identifier for the account to be charged renewal fees at the token's
		/// expiry to extend the lifetime of the token.
		/// <p>
		/// If this value is set for the identified token, the token lifetime SHALL
		/// be extended by the _smallest_ of the following at expiration:
		/// <ul>
		///   <li>The current `autoRenewPeriod` duration.</li>
		///   <li>The maximum duration that this account has funds to purchase.</li>
		///   <li>The configured MAX_AUTORENEW_PERIOD at the time of automatic
		///       renewal.</li>
		/// </ul>
		/// If this account's HBAR balance is `0` when the token must be
		/// renewed, then the token SHALL be expired, and MAY be subsequently
		/// removed from state.<br/>
		/// If this value is set, the referenced account MUST sign this
		/// transaction.
		/// <p>
		/// <blockquote>Note<blockquote>
		/// It is not currently possible to remove an automatic renewal account.
		/// Once set, it can only be replaced by a valid account.
		/// </blockquote></blockquote>
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <returns>{@code this}</returns>
		public virtual AccountId? AutoRenewAccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A duration between token automatic renewals.<br/>
        /// All entities in state may be charged "rent" occasionally (typically
        /// every 90 days) to prevent unnecessary growth of the ledger. This value
        /// sets the interval between such events for this token.
        /// <p>
        /// If set, this value MUST be greater than the configured
        /// `MIN_AUTORENEW_PERIOD`.<br/>
        /// If set, this value MUST be less than the configured
        /// `MAX_AUTORENEW_PERIOD`.
        /// </summary>
        /// <param name="period">the auto renew period</param>
        /// <returns>{@code this}</returns>
        public virtual Duration? AutoRenewPeriod { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// A short description for this token.
		/// <p>
		/// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
		/// (default 100) bytes when encoded as UTF-8.
		/// </summary>
		/// <param name="memo">the token memo 100 bytes max</param>
		/// <returns>{@code this}</returns>
		public virtual string? TokenMemo { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// Assign the metadata.
        /// </summary>
        /// <param name="tokenMetadata">the metadata</param>
        /// <returns>{@code this}</returns>
        public virtual byte[]? TokenMetadata { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// Set a key validation mode.<br/>
        /// Any key may be updated by a transaction signed by the token `admin_key`.
        /// Each role key may _also_ sign a transaction to update that key.
        /// If a role key signs an update to change that role key both old
        /// and new key must sign the transaction, _unless_ this field is set
        /// to `NO_VALIDATION`, in which case the _new_ key is not required to
        /// sign the transaction (the existing key is still required).<br/>
        /// The primary intent for this field is to allow a role key (e.g. a
        /// `pause_key`) holder to "remove" that key from the token by signing
        /// a transaction to set that role key to an empty `KeyList`.
        /// <p>
        /// If set to `FULL_VALIDATION`, either the `admin_key` or _both_ current
        /// and new key MUST sign this transaction to update a "key" field for the
        /// identified token.<br/>
        /// If set to `NO_VALIDATION`, either the `admin_key` or the current
        /// key MUST sign this transaction to update a "key" field for the
        /// identified token.<br/>
        /// This field SHALL be treated as `FULL_VALIDATION` if not set.
        /// </summary>
        /// <param name="tokenKeyVerificationMode">the key verification mode</param>
        /// <returns>{@code this}</returns>
        public virtual TokenKeyValidation TokenKeyVerificationMode { get; set { RequireNotFrozen(); field = value; } } = TokenKeyValidation.FullValidation;

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUpdate;

			TokenName = body.Name;
			TokenSymbol = body.Symbol;
			TokenKeyVerificationMode = (TokenKeyValidation)body.KeyVerificationMode;

			if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Treasury is not null)
                TreasuryAccountId = AccountId.FromProtobuf(body.Treasury);

            if (body.AutoRenewAccount is not null)
				AutoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);

			if (body.AdminKey is not null)
                AdminKey = Key.FromProtobufKey(body.AdminKey);

            if (body.KycKey is not null)
				KycKey = Key.FromProtobufKey(body.KycKey);

			if (body.FreezeKey is not null)
                FreezeKey = Key.FromProtobufKey(body.FreezeKey);

            if (body.WipeKey is not null)
                WipeKey = Key.FromProtobufKey(body.WipeKey);

            if (body.SupplyKey is not null)
                SupplyKey = Key.FromProtobufKey(body.SupplyKey);

            if (body.FeeScheduleKey is not null)
                FeeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);

            if (body.PauseKey is not null)
                PauseKey = Key.FromProtobufKey(body.PauseKey);

            if (body.MetadataKey is not null)
                MetadataKey = Key.FromProtobufKey(body.MetadataKey);

            if (body.Expiry is not null)
                ExpirationTime = Utils.TimestampConverter.FromProtobuf(body.Expiry);

            if (body.AutoRenewPeriod is not null)
				AutoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);

			if (body.Memo is not null)
				TokenMemo = body.Memo;

			if (body.Metadata is not null)
				TokenMetadata = body.Metadata.ToByteArray();


		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenUpdateTransactionBody}</returns>
        public virtual Proto.TokenUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenUpdateTransactionBody
            {
				Name = TokenName,
				Symbol = TokenSymbol,
				KeyVerificationMode = (Proto.TokenKeyValidation)TokenKeyVerificationMode,
			};

			if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (TreasuryAccountId != null)
                builder.Treasury = TreasuryAccountId.ToProtobuf();

            if (AutoRenewAccountId != null)
                builder.AutoRenewAccount = AutoRenewAccountId.ToProtobuf();

			if (AdminKey != null)
                builder.AdminKey = AdminKey.ToProtobufKey();

            if (KycKey != null)
                builder.KycKey = KycKey.ToProtobufKey();

            if (FreezeKey != null)
                builder.FreezeKey = FreezeKey.ToProtobufKey();

            if (WipeKey != null)
                builder.WipeKey = WipeKey.ToProtobufKey();

            if (SupplyKey != null)
                builder.SupplyKey = SupplyKey.ToProtobufKey();

            if (FeeScheduleKey != null)
                builder.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();

            if (PauseKey != null)
                builder.PauseKey = PauseKey.ToProtobufKey();

            if (MetadataKey != null)
                builder.MetadataKey = MetadataKey.ToProtobufKey();

            if (ExpirationTime != null)
                builder.Expiry = Utils.TimestampConverter.ToProtobuf(ExpirationTime);

            if (ExpirationTimeDuration != null)
                builder.Expiry = Utils.TimestampConverter.ToProtobuf(ExpirationTimeDuration);

            if (AutoRenewPeriod != null)
				builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(AutoRenewPeriod);

			if (TokenMemo != null)
				builder.Memo = TokenMemo;

			if (TokenMetadata != null)
				builder.Metadata = ByteString.CopyFrom(TokenMetadata);
           
            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
            TreasuryAccountId?.ValidateChecksum(client);
            AutoRenewAccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.updateToken);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
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