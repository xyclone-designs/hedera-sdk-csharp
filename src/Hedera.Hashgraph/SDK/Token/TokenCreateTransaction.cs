// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenCreateTransaction()
        {
            AutoRenewPeriod = DEFAULT_AUTO_RENEW_PERIOD;
            DefaultMaxTransactionFee = new Hbar(40);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenCreateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenCreateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
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
        public virtual uint Decimals { get; set { RequireNotFrozen(); field = value; } }
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
        public virtual ulong InitialSupply { get; set { RequireNotFrozen(); field = value; } }
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
        public virtual bool FreezeDefault { get; set { RequireNotFrozen(); field = value; } }
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
		public virtual IList<CustomFee> CustomFees
		{
			get => CustomFee.DeepCloneList(field);
			set => field = CustomFee.DeepCloneList(value);
		} = [];
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
		/// A type for this token, according to IWA classification.
		/// <p>
		/// If this value is not set, the token SHALL have the default type of
		/// fungible/common.<br/>
		/// This field SHALL be immutable.
		/// </summary>
		/// <param name="tokenType">the token type</param>
		/// <returns>{@code this}</returns>
		public virtual TokenType TokenType { get; set { RequireNotFrozen(); field = value; } } = TokenType.FungibleCommon;
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
		public virtual TokenSupplyType TokenSupplyType { get; set { RequireNotFrozen(); field = value; } } = TokenSupplyType.Infinite;
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
		public virtual long MaxSupply { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenCreation;

			TokenName = body.Name;
			TokenSymbol = body.Symbol;
			Decimals = body.Decimals;
			InitialSupply = body.InitialSupply;
			FreezeDefault = body.FreezeDefault;
			TokenMemo = body.Memo;
			TokenType = (TokenType)body.TokenType;
			TokenSupplyType = (TokenSupplyType)body.SupplyType;
			MaxSupply = body.MaxSupply;
			TokenMetadata = body.Metadata.ToByteArray();

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

			foreach (var fee in body.CustomFees)
				CustomFees.Add(CustomFee.FromProtobuf(fee));
		}
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.TokenCreateTransactionBody}</returns>
		public virtual Proto.TokenCreateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenCreateTransactionBody
			{
				Name = TokenName,
				Symbol = TokenSymbol,
				Decimals = Decimals,
				InitialSupply = InitialSupply,
				FreezeDefault = FreezeDefault,
				Memo = TokenMemo,
				TokenType = (Proto.TokenType)TokenType,
				SupplyType = (Proto.TokenSupplyType)TokenSupplyType,
				MaxSupply = MaxSupply,
				Metadata = ByteString.CopyFrom(TokenMetadata),
			};

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

            foreach (var fee in CustomFees)
				builder.CustomFees.Add(fee.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var fee in CustomFees)
				fee.ValidateChecksums(client);

			TreasuryAccountId?.ValidateChecksum(client);
			AutoRenewAccountId?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenCreation = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenCreation = ToProtobuf();
        }
		public override TokenCreateTransaction FreezeWith(Client client)
		{
			if (AutoRenewAccountId == null && client.OperatorAccountId != null && AutoRenewPeriod != null && AutoRenewPeriod.Seconds == 0)
			{
				AutoRenewAccountId = TransactionIds != null && TransactionIds.Count != 0 && TransactionIds.GetCurrent() != null ? TransactionIds.GetCurrent().AccountId : client.OperatorAccountId;
			}

			return base.FreezeWith(client);
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.createToken);

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