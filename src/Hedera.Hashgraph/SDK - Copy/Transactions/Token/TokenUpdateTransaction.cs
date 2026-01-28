// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
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
        /// The token's id
        /// </summary>
        private TokenId tokenId = null;
        private AccountId treasuryAccountId = null;
        private AccountId autoRenewAccountId = null;
        private string tokenName = "";
        private string tokenSymbol = "";
        private Key adminKey = null;
        private Key kycKey = null;
        private Key freezeKey = null;
        private Key wipeKey = null;
        private Key supplyKey = null;
        private Key feeScheduleKey = null;
        private Key pauseKey = null;
        private Key metadataKey = null;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private Duration autoRenewPeriod = null;
        private string tokenMemo = null;
        private byte[] tokenMetadata = null;
        private TokenKeyValidation tokenKeyVerificationMode = TokenKeyValidation.FullValidation;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
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
        public virtual TokenUpdateTransaction SetTokenId(TokenId tokenId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenId);
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the token name.
        /// </summary>
        /// <returns>                         the token name</returns>
        public virtual string GetTokenName()
        {
            return tokenName;
        }

        /// <summary>
        /// A new name for the token.<br/>
        /// This is generally the "full name" displayed in wallet software.
        /// <p>
        /// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value, if set, MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="name">the token name</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateTransaction SetTokenName(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            RequireNotFrozen();
            tokenName = name;
            return this;
        }

        /// <summary>
        /// Extract the token symbol.
        /// </summary>
        /// <returns>                         the token symbol</returns>
        public virtual string GetTokenSymbol()
        {
            return tokenSymbol;
        }

        /// <summary>
        /// A new symbol to use for the token.
        /// <p>
        /// This value, if set, MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value, if set, MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="symbol">the token symbol</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateTransaction SetTokenSymbol(string symbol)
        {
            ArgumentNullException.ThrowIfNull(symbol);
            RequireNotFrozen();
            tokenSymbol = symbol;
            return this;
        }

        /// <summary>
        /// Extract the treasury account id.
        /// </summary>
        /// <returns>                         the treasury account id</returns>
        public virtual AccountId GetTreasuryAccountId()
        {
            return treasuryAccountId;
        }

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
        public virtual TokenUpdateTransaction SetTreasuryAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            treasuryAccountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the admin key.
        /// </summary>
        /// <returns>                         the admin key</returns>
        public virtual Key GetAdminKey()
        {
            return adminKey;
        }

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
        public virtual TokenUpdateTransaction SetAdminKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            adminKey = key;
            return this;
        }

        /// <summary>
        /// Extract the kyc key.
        /// </summary>
        /// <returns>                         the kyc key</returns>
        public virtual Key GetKycKey()
        {
            return kycKey;
        }

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
        public virtual TokenUpdateTransaction SetKycKey(Key key)
        {
            RequireNotFrozen();
            kycKey = key;
            return this;
        }

        /// <summary>
        /// Extract the freeze key.
        /// </summary>
        /// <returns>                         the freeze key</returns>
        public virtual Key GetFreezeKey()
        {
            return freezeKey;
        }

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
        public virtual TokenUpdateTransaction SetFreezeKey(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            freezeKey = key;
            return this;
        }

        /// <summary>
        /// Extract the wipe key.
        /// </summary>
        /// <returns>                         the wipe key</returns>
        public virtual Key GetWipeKey()
        {
            return wipeKey;
        }

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
        public virtual TokenUpdateTransaction SetWipeKey(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            wipeKey = key;
            return this;
        }

        /// <summary>
        /// Extract the supply key.
        /// </summary>
        /// <returns>                         the supply key</returns>
        public virtual Key GetSupplyKey()
        {
            return supplyKey;
        }

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
        public virtual TokenUpdateTransaction SetSupplyKey(Key key)
        {
            ArgumentNullException.ThrowIfNull(key);
            RequireNotFrozen();
            supplyKey = key;
            return this;
        }

        /// <summary>
        /// Extract the fee schedule key.
        /// </summary>
        /// <returns>                         the fee schedule key</returns>
        public virtual Key GetFeeScheduleKey()
        {
            return feeScheduleKey;
        }

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
        public virtual TokenUpdateTransaction SetFeeScheduleKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            feeScheduleKey = key;
            return this;
        }

        /// <summary>
        /// Extract the pause key.
        /// </summary>
        /// <returns>                         the pause key</returns>
        public virtual Key GetPauseKey()
        {
            return pauseKey;
        }

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
        public virtual TokenUpdateTransaction SetPauseKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            pauseKey = key;
            return this;
        }

        /// <summary>
        /// Extract the metadata key.
        /// </summary>
        /// <returns>                         the metadata key</returns>
        public virtual Key GetMetadataKey()
        {
            return metadataKey;
        }

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
        public virtual TokenUpdateTransaction SetMetadataKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            metadataKey = key;
            return this;
        }

        /// <summary>
        /// Extract the expiration time.
        /// </summary>
        /// <returns>                         the expiration time</returns>
        public virtual Timestamp GetExpirationTime()
        {
            return expirationTime;
        }

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
        public virtual TokenUpdateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            ArgumentNullException.ThrowIfNull(expirationTime);
            RequireNotFrozen();
            autoRenewPeriod = null;
            expirationTime = expirationTime;
            expirationTimeDuration = null;
            return this;
        }

        public virtual TokenUpdateTransaction SetExpirationTime(Duration expirationTime)
        {
            ArgumentNullException.ThrowIfNull(expirationTime);
            RequireNotFrozen();
            autoRenewPeriod = null;
            expirationTime = null;
            expirationTimeDuration = expirationTime;
            return this;
        }

        /// <summary>
        /// Extract the auto renew account id.
        /// </summary>
        /// <returns>                         the auto renew account id</returns>
        public virtual AccountId GetAutoRenewAccountId()
        {
            return autoRenewAccountId;
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
        public virtual TokenUpdateTransaction SetAutoRenewAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            autoRenewAccountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the auto renew period.
        /// </summary>
        /// <returns>                         the auto renew period</returns>
        public virtual Duration GetAutoRenewPeriod()
        {
            return autoRenewPeriod;
        }

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
        public virtual TokenUpdateTransaction SetAutoRenewPeriod(Duration period)
        {
            ArgumentNullException.ThrowIfNull(period);
            RequireNotFrozen();
            autoRenewPeriod = period;
            return this;
        }

        /// <summary>
        /// Extract the token memo.
        /// </summary>
        /// <returns>                         the token memo</returns>
        public virtual string GetTokenMemo()
        {
            return tokenMemo;
        }

        /// <summary>
        /// A short description for this token.
        /// <p>
        /// This value, if set, MUST NOT exceed `transaction.maxMemoUtf8Bytes`
        /// (default 100) bytes when encoded as UTF-8.
        /// </summary>
        /// <param name="memo">the token memo 100 bytes max</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateTransaction SetTokenMemo(string memo)
        {
            ArgumentNullException.ThrowIfNull(memo);
            RequireNotFrozen();
            tokenMemo = memo;
            return this;
        }

        /// <summary>
        /// Remove the token memo.
        /// </summary>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateTransaction ClearMemo()
        {
            RequireNotFrozen();
            tokenMemo = "";
            return this;
        }

        /// <summary>
        /// Extract the metadata.
        /// </summary>
        /// <returns>the metadata</returns>
        public virtual byte[] GetTokenMetadata()
        {
            return tokenMetadata;
        }

        /// <summary>
        /// Assign the metadata.
        /// </summary>
        /// <param name="tokenMetadata">the metadata</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateTransaction SetTokenMetadata(byte[] tokenMetadata)
        {
            RequireNotFrozen();
            tokenMetadata = tokenMetadata;
            return this;
        }

        /// <summary>
        /// Extract the key verification mode
        /// </summary>
        /// <returns>the key verification mode</returns>
        public virtual TokenKeyValidation GetKeyVerificationMode()
        {
            return tokenKeyVerificationMode;
        }

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
        public virtual TokenUpdateTransaction SetKeyVerificationMode(TokenKeyValidation tokenKeyVerificationMode)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenKeyVerificationMode);
            tokenKeyVerificationMode = tokenKeyVerificationMode;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenUpdate;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            if (body.Treasury is not null)
            {
                treasuryAccountId = AccountId.FromProtobuf(body.Treasury);
            }

            if (body.AutoRenewAccount is not null)
            {
                autoRenewAccountId = AccountId.FromProtobuf(body.AutoRenewAccount);
            }

            tokenName = body.Name;
            tokenSymbol = body.Symbol;

            if (body.AdminKey is not null)
            {
                adminKey = Key.FromProtobufKey(body.AdminKey);
            }

            if (body.KycKey is not null)
            {
                kycKey = Key.FromProtobufKey(body.KycKey);
            }

            if (body.FreezeKey is not null)
            {
                freezeKey = Key.FromProtobufKey(body.FreezeKey);
            }

            if (body.WipeKey is not null)
            {
                wipeKey = Key.FromProtobufKey(body.WipeKey);
            }

            if (body.SupplyKey is not null)
            {
                supplyKey = Key.FromProtobufKey(body.SupplyKey);
            }

            if (body.FeeScheduleKey is not null)
            {
                feeScheduleKey = Key.FromProtobufKey(body.FeeScheduleKey);
            }

            if (body.PauseKey is not null)
            {
                pauseKey = Key.FromProtobufKey(body.PauseKey);
            }

            if (body.MetadataKey is not null)
            {
                metadataKey = Key.FromProtobufKey(body.MetadataKey);
            }

            if (body.Expiry is not null)
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.Expiry);
            }

            if (body.AutoRenewPeriod is not null)
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            if (body.Memo is not null)
            {
                tokenMemo = body.Memo;
            }

            if (body.Metadata is not null)
            {
                tokenMetadata = body.Metadata.ToByteArray();
            }

            tokenKeyVerificationMode = (TokenKeyValidation)body.KeyVerificationMode;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenUpdateTransactionBody}</returns>
        public virtual Proto.TokenUpdateTransactionBody Build()
        {
            var builder = new Proto.TokenUpdateTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            if (treasuryAccountId != null)
            {
                builder.Treasury = treasuryAccountId.ToProtobuf();
            }

            if (autoRenewAccountId != null)
            {
                builder.AutoRenewAccount = autoRenewAccountId.ToProtobuf();
            }

            builder.Name = tokenName;
            builder.Symbol = tokenSymbol;

            if (adminKey != null)
            {
                builder.AdminKey = adminKey.ToProtobufKey();
            }

            if (kycKey != null)
            {
                builder.KycKey = kycKey.ToProtobufKey();
            }

            if (freezeKey != null)
            {
                builder.FreezeKey = freezeKey.ToProtobufKey();
            }

            if (wipeKey != null)
            {
                builder.WipeKey = wipeKey.ToProtobufKey();
            }

            if (supplyKey != null)
            {
                builder.SupplyKey = supplyKey.ToProtobufKey();
            }

            if (feeScheduleKey != null)
            {
                builder.FeeScheduleKey = feeScheduleKey.ToProtobufKey();
            }

            if (pauseKey != null)
            {
                builder.PauseKey = pauseKey.ToProtobufKey();
            }

            if (metadataKey != null)
            {
                builder.MetadataKey = metadataKey.ToProtobufKey();
            }

            if (expirationTime != null)
            {
                builder.Expiry = Utils.TimestampConverter.ToProtobuf(expirationTime);
            }

            if (expirationTimeDuration != null)
            {
                builder.Expiry = Utils.TimestampConverter.ToProtobuf(expirationTimeDuration);
            }

            if (autoRenewPeriod != null)
            {
                builder.AutoRenewPeriod = Utils.DurationConverter.ToProtobuf(autoRenewPeriod);
            }

            if (tokenMemo != null)
            {
                builder.Memo = tokenMemo;
            }

            if (tokenMetadata != null)
            {
                builder.Metadata = ByteString.CopyFrom(tokenMetadata);
            }

            builder.KeyVerificationMode = tokenKeyVerificationMode.code;
           
            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            if (treasuryAccountId != null)
            {
                treasuryAccountId.ValidateChecksum(client);
            }

            if (autoRenewAccountId != null)
            {
                autoRenewAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetUpdateTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdate = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdate = Build();
        }
    }
}