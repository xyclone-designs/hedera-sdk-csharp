// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Create an HTS token.
    /// 
    /// #### Keys
    /// Each token has several keys that, separately, control different functions
    /// for that token. It is *_strongly_* recommended that each key assigned to
    /// a token be unique, or disabled by assigning an empty `KeyList`.
    /// Keys and purpose
    /// - `adminKey` is a general access and may authorize a token update
    ///   transaction as well as _update the other keys_. Even the admin key
    ///   cannot authorize _adding_ a key that is not present, however.<br/>
    ///   The admin key may also delete the token entirely.
    /// - `fee_schedule` may authorize updating the token custom fees. If this
    ///   key is not present, the custom fees for the token are fixed and immutable.
    /// - `freeze` may authorize a token freeze or unfreeze transaction.
    ///   If this key is not present, accounts holding this token cannot have
    ///   their tokens frozen or unfrozen.
    /// - `kyc` may authorize a token grant KYC or revoke KYC transaction.
    ///   If this key is not present, accounts holding this token cannot have
    ///   KYC status granted or revoked.
    /// - `metadata` may authorize token update nfts transactions.
    ///   If this key is not present, the token metadata values for that
    ///   non-fungible/unique token _type_ will be immutable.
    /// - `pause` may authorize a token pause or token unpause transaction.
    ///   If this key is not present, the token cannot be paused (preventing any
    ///   account from transacting in that token) or resumed.
    /// - `supply` may authorize a token mint or burn transaction.
    ///   If this key is not present, the token cannot mint additional supply and
    ///   existing tokens cannot be "burned" from the treasury (but _might_ still be
    ///   "burned" from individual accounts, c.f. `wipeKey` and `tokenWipe`).
    /// - `wipe` may authorize a token wipe account transaction.
    ///   If this key is not present, accounts holding this token cannot have
    ///   their balance or NFTs wiped (effectively burned).
    /// 
    /// #### Requirements
    /// If `tokenType` is fungible/common, the `initialSupply` MUST be strictly
    /// greater than zero(`0`).<br/>
    /// If `tokenType` is non-fungible/unique, the `initialSupply` MUST
    /// be zero(`0`).<br/>
    /// If `tokenSupplyType` is "infinite", the `maxSupply` MUST be zero(`0`).<br/>
    /// If `tokenSupplyType` is "finite", the `maxSupply` MUST be strictly
    /// greater than zero(`0`).<br/>
    /// 
    /// ### Block Stream Effects
    /// If the token is created, the Token Identifier SHALL be in the receipt.<br/>
    /// </summary>
    public class TokenCreateTransaction : Transaction<TokenCreateTransaction>
    {
        private IList<CustomFee> customFees = [];
        private AccountId treasuryAccountId = null;
        private AccountId autoRenewAccountId = null;
        private string tokenName = "";
        private string tokenSymbol = "";
        private int decimals = 0;
        private long initialSupply = 0;
        private Key adminKey = null;
        private Key kycKey = null;
        private Key freezeKey = null;
        private Key wipeKey = null;
        private Key supplyKey = null;
        private Key feeScheduleKey = null;
        private Key pauseKey = null;
        private Key metadataKey = null;
        private bool freezeDefault = false;
        private Timestamp expirationTime = null;
        private Duration expirationTimeDuration = null;
        private Duration autoRenewPeriod = null;
        private string tokenMemo = "";
        private TokenType tokenType = TokenType.FungibleCommon;
        private TokenSupplyType tokenSupplyType = TokenSupplyType.Infinite;
        private long maxSupply = 0;
        private byte[] tokenMetadata = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenCreateTransaction()
        {
            autoRenewPeriod = DEFAULT_AUTO_RENEW_PERIOD;
            defaultMaxTransactionFee = new Hbar(40);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
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
        /// A name for the token.<br/>
        /// This is generally the "full name" displayed in wallet software.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// This value MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="name">the token name</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTokenName(string name)
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
        /// A symbol to use for the token.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// This value MUST NOT exceed 100 bytes when encoded as UTF-8.<br/>
        /// This value MUST NOT contain the Unicode NUL codepoint.
        /// </summary>
        /// <param name="symbol">the token symbol</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTokenSymbol(string symbol)
        {
            ArgumentNullException.ThrowIfNull(symbol);
            RequireNotFrozen();
            tokenSymbol = symbol;
            return this;
        }

        /// <summary>
        /// Extract the decimals.
        /// </summary>
        /// <returns>                         the decimals</returns>
        public virtual int Decimals
        {
            return decimals;
        }

        /// <summary>
        /// A decimal precision of the token's smallest denomination.<br/>
        /// Most values are described in terms of this smallest denomination,
        /// so the token initial supply, for instance, must be divided by
        /// <tt>10<sup>decimals</sup></tt> to get whole tokens.
        /// <p>
        /// This MUST be zero(`0`) for non-fungible/unique tokens.
        /// </summary>
        /// <param name="decimals">the number of decimals</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetDecimals(int decimals)
        {
            RequireNotFrozen();
            decimals = decimals;
            return this;
        }

        /// <summary>
        /// Extract the initial supply of tokens.
        /// </summary>
        /// <returns>                         the initial supply of tokens</returns>
        public virtual long GetInitialSupply()
        {
            return initialSupply;
        }

        /// <summary>
        /// An initial supply, in the smallest denomination for the token.
        /// <p>
        /// This amount SHALL be transferred to the treasury account as part
        /// of this transaction.<br/>
        /// This amount MUST be specified in the smallest denomination for the
        /// token (i.e. <tt>10<sup>-decimals</sup></tt> whole tokens).<br/>
        /// This MUST be zero(`0`) for a non-fungible/unique token.
        /// </summary>
        /// <param name="initialSupply">the initial supply of tokens</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetInitialSupply(long initialSupply)
        {
            RequireNotFrozen();
            initialSupply = initialSupply;
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
        /// A treasury account identifier.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified account SHALL be designated the "treasury" for the
        /// new token, and all tokens "minted" SHALL be delivered to that account,
        /// including the initial supply, if any.<br/>
        /// The identified account MUST exist, MUST NOT be expired, and SHOULD
        /// have a non-zero HBAR balance.<br/>
        /// The identified account SHALL be associated to the new token.
        /// </summary>
        /// <param name="accountId">the treasury account id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTreasuryAccountId(AccountId accountId)
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
        /// An Hedera key for token administration.
        /// <p>
        /// This key, if set, SHALL have administrative authority for this token and
        /// MAY authorize token update and/or token delete transactions.<br/>
        /// If this key is not set, or is an empty `KeyList`, this token SHALL be
        /// immutable, except for expiration and renewal.
        /// </summary>
        /// <param name="key">the admin key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetAdminKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            adminKey = key;
            return this;
        }

        /// <summary>
        /// Extract the know your customer key.
        /// </summary>
        /// <returns>                         the know your customer key</returns>
        public virtual Key GetKycKey()
        {
            return kycKey;
        }

        /// <summary>
        /// An Hedera key for managing account KYC.
        /// <p>
        /// This key, if set, SHALL have KYC authority for this token and
        /// MAY authorize transactions to grant or revoke KYC for accounts.<br/>
        /// If this key is not set, or is an empty `KeyList`, KYC status for this
        /// token SHALL NOT be granted or revoked for any account.<br/>
        /// If this key is removed after granting KYC, those grants SHALL remain
        /// and cannot be revoked.
        /// </summary>
        /// <param name="key">the know your customer key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetKycKey(Key key)
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
        /// An Hedera key for managing asset "freeze".
        /// <p>
        /// This key, if set, SHALL have "freeze" authority for this token and
        /// MAY authorize transactions to freeze or unfreeze accounts
        /// with respect to this token.<br/>
        /// If this key is not set, or is an empty `KeyList`, this token
        /// SHALL NOT be frozen or unfrozen for any account.<br/>
        /// If this key is removed after freezing accounts, those accounts
        /// SHALL remain frozen and cannot be unfrozen.
        /// </summary>
        /// <param name="key">the freeze key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetFreezeKey(Key key)
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
        /// An Hedera key for wiping tokens from accounts.
        /// <p>
        /// This key, if set, SHALL have "wipe" authority for this token and
        /// MAY authorize transactions to "wipe" any amount of this token from
        /// any account, effectively burning the tokens "wiped".<br/>
        /// If this key is not set, or is an empty `KeyList`, it SHALL NOT be
        /// possible to "wipe" this token from an account.
        /// </summary>
        /// <param name="key">the wipe key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetWipeKey(Key key)
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
        /// If this key is not set, or is an empty `KeyList`, it SHALL NOT be
        /// possible to change the supply of tokens and neither "mint" nor "burn"
        /// transactions SHALL be permitted.
        /// </summary>
        /// <param name="key">the supply key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetSupplyKey(Key key)
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
        /// If this key is not set, or is an empty `KeyList`, the `custom_fees`
        /// for this token SHALL NOT be modified.
        /// </summary>
        /// <param name="key">the fee schedule key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetFeeScheduleKey(Key key)
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
        /// If this key is not set, or is an empty `KeyList`, this token
        /// SHALL NOT be paused or unpaused.<br/>
        /// If this key is removed while the token is paused, the token cannot
        /// be unpaused and SHALL remain paused.
        /// </summary>
        /// <param name="key">the pause key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetPauseKey(Key key)
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
        /// An Hedera key for managing the token `metadata`.
        /// <p>
        /// This key, if set, MAY authorize transactions to modify the
        /// `metadata` for this token.<br/>
        /// If this key is not set, or is an empty `KeyList`, the `metadata`
        /// for this token SHALL NOT be modified.
        /// </summary>
        /// <param name="key">the metadata key</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetMetadataKey(Key key)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(key);
            metadataKey = key;
            return this;
        }

        /// <summary>
        /// Extract the freeze default.
        /// </summary>
        /// <returns>                         the freeze default</returns>
        public virtual bool GetFreezeDefault()
        {
            return freezeDefault;
        }

        /// <summary>
        /// An initial Freeze status for accounts associated to this token.
        /// <p>
        /// If this value is set, an account MUST be the subject of a
        /// `tokenUnfreeze` transaction after associating to the token before
        /// that account can send or receive this token.<br/>
        /// If this value is set, the `freezeKey` SHOULD be set.<br/>
        /// If the `freezeKey` is not set, any account associated to this token
        /// while this value is set SHALL be permanently frozen.
        /// <p>
        /// <blockquote>REVIEW NOTE<blockquote>
        /// Should we prevent setting this value true for tokens with no freeze
        /// key?<br/>
        /// Should we set this value to false if a freeze key is removed?
        /// </blockquote></blockquote>
        /// </summary>
        /// <param name="freezeDefault">the freeze default</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetFreezeDefault(bool freezeDefault)
        {
            RequireNotFrozen();
            freezeDefault = freezeDefault;
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
        /// If the `autoRenewAccount` and `autoRenewPeriod` fields are set, this
        /// value SHALL be replaced with the current consensus time extended
        /// by the `autoRenewPeriod` duration.<br/>
        /// If this value is set and token expiration is enabled in network
        /// configuration, this token SHALL expire when consensus time exceeds
        /// this value, and MAY be subsequently removed from the network state.<br/>
        /// If this value is not set, and the automatic renewal account is also not
        /// set, then this value SHALL default to the current consensus time
        /// extended by the "default" expiration period from network configuration.
        /// 
        /// Setting this value will clear the autoRenewPeriod as the autoRenewPeriod period has default value
        /// of 7890000 seconds and leaving it set will override the expiration time
        /// </summary>
        /// <param name="expirationTime">the expiration time</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetExpirationTime(Timestamp expirationTime)
        {
            ArgumentNullException.ThrowIfNull(expirationTime);
            RequireNotFrozen();
            autoRenewPeriod = null;
            expirationTimeDuration = null;
            expirationTime = expirationTime;
            return this;
        }

        public virtual TokenCreateTransaction SetExpirationTime(Duration expirationTime)
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
        /// If this value is set, the token lifetime SHALL be extended by the
        /// _smallest_ of the following:
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
        /// </summary>
        /// <param name="accountId">the auto renew account id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetAutoRenewAccountId(AccountId accountId)
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
        /// This value MUST be set.<br/>
        /// This value MUST be greater than the configured
        /// MIN_AUTORENEW_PERIOD.<br/>
        /// This value MUST be less than the configured MAX_AUTORENEW_PERIOD.
        /// 
        /// If expirationTime is set - autoRenewPeriod will be effectively ignored,
        /// and it's effect will be replaced by expirationTime
        /// </summary>
        /// <param name="period">the auto renew period</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetAutoRenewPeriod(Duration period)
        {
            ArgumentNullException.ThrowIfNull(period);
            RequireNotFrozen();
            autoRenewPeriod = period;
            return this;
        }

        /// <summary>
        /// Extract the token's memo 100 bytes max.
        /// </summary>
        /// <returns>                         the token's memo 100 bytes max</returns>
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
        /// <param name="memo">the token's memo 100 bytes max</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTokenMemo(string memo)
        {
            ArgumentNullException.ThrowIfNull(memo);
            RequireNotFrozen();
            tokenMemo = memo;
            return this;
        }

        /// <summary>
        /// Extract the custom fees.
        /// </summary>
        /// <returns>                         the custom fees</returns>
        public virtual IList<CustomFee> GetCustomFees()
        {
            return CustomFee.DeepCloneList(customFees);
        }

        /// <summary>
        /// A list of custom fees representing a fee schedule.
        /// <p>
        /// This list MAY be empty, which SHALL mean that there
        /// are no custom fees for this token.<br/>
        /// If this token is a non-fungible/unique type, the entries
        /// in this list MUST NOT declare a `fractional_fee`.<br/>
        /// If this token is a fungible/common type, the entries in this
        /// list MUST NOT declare a `royalty_fee`.<br/>
        /// Any token type MAY include entries that declare a `fixed_fee`.
        /// </summary>
        /// <param name="customFees">the custom fees</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetCustomFees(IList<CustomFee> customFees)
        {
            RequireNotFrozen();
            customFees = CustomFee.DeepCloneList(customFees);
            return this;
        }

        /// <summary>
        /// Extract the token type.
        /// </summary>
        /// <returns>                         the token type</returns>
        public virtual TokenType GetTokenType()
        {
            return tokenType;
        }

        /// <summary>
        /// A type for this token, according to IWA classification.
        /// <p>
        /// If this value is not set, the token SHALL have the default type of
        /// fungible/common.<br/>
        /// This field SHALL be immutable.
        /// </summary>
        /// <param name="tokenType">the token type</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTokenType(TokenType tokenType)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenType);
            tokenType = tokenType;
            return this;
        }

        /// <summary>
        /// Extract the supply type.
        /// </summary>
        /// <returns>                         the supply type</returns>
        public virtual TokenSupplyType GetSupplyType()
        {
            return tokenSupplyType;
        }

        /// <summary>
        /// A supply type for this token, according to IWA classification.
        /// <p>
        /// If this value is not set, the token SHALL have the default supply
        /// type of "infinite" (which is, as a practical matter,
        /// (2<sup><i>63</i></sup>-1)/10<sup><i>decimals</i></sup>).<br/>
        /// This field SHALL be immutable.
        /// </summary>
        /// <param name="supplyType">the supply type</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetSupplyType(TokenSupplyType supplyType)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(supplyType);
            tokenSupplyType = supplyType;
            return this;
        }

        /// <summary>
        /// Extract the max supply of tokens.
        /// </summary>
        /// <returns>                         the max supply of tokens</returns>
        public virtual long GetMaxSupply()
        {
            return maxSupply;
        }

        /// <summary>
        /// A maximum supply for this token.
        /// <p>
        /// This SHALL be interpreted in terms of the smallest fractional unit for
        /// this token.<br/>
        /// If `supplyType` is "infinite", this MUST be `0`.<br/>
        /// This field SHALL be immutable.
        /// </summary>
        /// <param name="maxSupply">the max supply of tokens</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetMaxSupply(long maxSupply)
        {
            RequireNotFrozen();
            maxSupply = maxSupply;
            return this;
        }

        /// <summary>
        /// Extract the token metadata.
        /// </summary>
        /// <returns>the token metadata</returns>
        public virtual byte[] GetTokenMetadata()
        {
            return tokenMetadata;
        }

        /// <summary>
        /// Token "Metadata".
        /// <p>
        /// The value, if set, MUST NOT exceed 100 bytes.<br/>
        /// <dl><dt>Examples</dt>
        ///   <dd>hcs://1/0.0.4896575</dd>
        ///   <dd>ipfs://bafkreifd7tcjjuwxxf4qkaibkj62pj4mhfuud7plwrc3pfoygt55al6syi</dd>
        /// </dl>
        /// </summary>
        /// <param name="tokenMetadata">the token metadata</param>
        /// <returns>{@code this}</returns>
        public virtual TokenCreateTransaction SetTokenMetadata(byte[] tokenMetadata)
        {
            RequireNotFrozen();
            tokenMetadata = tokenMetadata;
            return this;
        }

        public override TokenCreateTransaction FreezeWith(Client client)
        {
            if (autoRenewAccountId == null && client != null && client.GetOperatorAccountId() != null && autoRenewPeriod != null && !autoRenewPeriod.IsZero())
            {
                autoRenewAccountId = TransactionIds != null && !TransactionIds.Length == 0 && TransactionIds.GetCurrent() != null ? TransactionIds.GetCurrent().accountId : client.GetOperatorAccountId();
            }

            return base.FreezeWith(client);
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenCreateTransactionBody}</returns>
        public virtual Proto.TokenCreateTransactionBody Build()
        {
            var builder = new Proto.TokenCreateTransactionBody();

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
            builder.Decimals = decimals;
            builder.InitialSupply = initialSupply;

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

            builder.FreezeDefault = freezeDefault;

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

            builder.Memo = tokenMemo;
            builder.TokenType = tokenType.code;
            builder.SupplyType = tokenSupplyType.code;
            builder.MaxSupply = maxSupply;
            builder.Metadata = ByteString.CopyFrom(tokenMetadata);

            foreach (var fee in customFees)
            {
                builder.CustomFees.Add(fee.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenCreation;

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
            decimals = body.Decimals;
            initialSupply = body.InitialSupply;

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

            freezeDefault = body.FreezeDefault;

            if (body.Expiry is not null)
            {
                expirationTime = Utils.TimestampConverter.FromProtobuf(body.Expiry);
            }

            if (body.AutoRenewPeriod is not null)
            {
                autoRenewPeriod = Utils.DurationConverter.FromProtobuf(body.AutoRenewPeriod);
            }

            tokenMemo = body.Memo;
            tokenType = (TokenType) body.TokenType;
            tokenSupplyType = (TokenSupplyType) body.SupplyType;
            maxSupply = body.MaxSupply;
            tokenMetadata = body.Metadata.ToByteArray();

            foreach (var fee in body.CustomFees)
            {
                customFees.Add(CustomFee.FromProtobuf(fee));
            }
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var fee in customFees)
            {
                fee.ValidateChecksums(client);
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
            return TokenServiceGrpc.GetCreateTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenCreation = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenCreation = Build();
        }
    }
}