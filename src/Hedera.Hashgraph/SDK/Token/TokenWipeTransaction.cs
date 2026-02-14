// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Wipe (administratively burn) tokens held by a non-treasury account.<br/>
    /// On success, the requested tokens will be removed from the identified account
    /// and the token supply will be reduced by the amount "wiped".
    /// 
    /// This transaction MUST be signed by the token `wipe_key`.<br/>
    /// The identified token MUST exist, MUST NOT be deleted,
    /// and MUST NOT be paused.<br/>
    /// The identified token MUST have a valid `Key` set for the `wipe_key` field,
    /// and that key MUST NOT be an empty `KeyList`.<br/>
    /// The identified account MUST exist, MUST NOT be deleted, MUST be
    /// associated to the identified token, MUST NOT be frozen for the identified
    /// token, MUST NOT be the token `treasury`, and MUST hold a balance for the
    /// token or the specific serial numbers provided.<br/>
    /// This transaction SHOULD provide a value for `amount` or `serialNumbers`,
    /// but MUST NOT set both fields.
    /// 
    /// ### Block Stream Effects
    /// The new total supply for the wiped token type SHALL be recorded.
    /// </summary>
    public class TokenWipeTransaction : Transaction<TokenWipeTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenWipeTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenWipeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenWipeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private IList<long> _Serials = [];

		/// <summary>
		/// A token identifier.
		/// <p>
		/// This field is REQUIRED.<br/>
		/// The identified token MUST exist, MUST NOT be paused, MUST NOT be
		/// deleted, and MUST NOT be expired.
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <returns>{@code this}</returns>
		public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An account identifier.<br/>
        /// This identifies the account from which tokens will be wiped.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified account MUST NOT be deleted or expired.<br/>
        /// If the identified token `kyc_key` is set to a valid key, the
        /// identified account MUST have "KYC" granted.<br/>
        /// The identified account MUST NOT be the `treasury` account for the
        /// identified token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// An amount of fungible/common tokens to wipe.
		/// <p>
		/// If the identified token is a non-fungible/unique token type,
		/// this value MUST be exactly zero(`0`).<br/>
		/// If the identified token type is fungible/common:
		/// <ul>
		///   <li>This value SHALL be specified in units of the smallest
		///       denomination possible for the identified token
		///       (<tt>10<sup>-decimals</sup></tt> whole tokens).</li>
		///   <li>This value MUST be strictly less than `Long.MAX_VALUE`.</li>
		///   <li>This value MUST be less than or equal to the current total
		///       supply for the identified token.</li>
		///   <li>This value MUST be less than or equal to the current balance
		///       held by the identified account.</li>
		///   <li>This value MAY be zero(`0`).</li>
		/// </ul>
		/// </summary>
		/// <param name="amount">the amount</param>
		/// <returns>{@code this}</returns>
		public virtual ulong Amount { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A list of serial numbers to wipe.<br/>
        /// The non-fungible/unique tokens with these serial numbers will be
        /// destroyed and cannot be recovered or reused.
        /// <p>
        /// If the identified token type is a fungible/common type, this
        /// list MUST be empty.<br/>
        /// If the identified token type is non-fungible/unique:
        /// <ul>
        ///   <li>This list MUST contain at least one entry if the identified token
        ///       type is non-fungible/unique.>/li>
        ///   <li>This list MUST NOT contain more entries than the current total
        ///       supply for the identified token.</li>
        ///   <li>Every entry in this list MUST be a valid serial number for the
        ///       identified token (i.e. "collection").</li>
        ///   <li>Every entry in this list MUST be owned by the
        ///       identified account</li>
        ///   <li></li>
        /// </ul>
        /// This list MUST NOT contain more entries than the network configuration
        /// value for batch size limit, typically ten(`10`).
        /// </summary>
        /// <param name="serials">the list of serial numbers</param>
        /// <returns>{@code this}</returns>
        public virtual IList<long> Serials { get { RequireNotFrozen(); return _Serials; } set { RequireNotFrozen(); _Serials = value; } } 
        public virtual IReadOnlyList<long> Serials_Read { get => _Serials.AsReadOnly(); }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenWipe;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
                AccountId = AccountId.FromProtobuf(body.Account);

            Amount = body.Amount;
            Serials = body.SerialNumbers;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenWipeAccountTransactionBody}</returns>
        public virtual Proto.TokenWipeAccountTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenWipeAccountTransactionBody
            {
				Amount = Amount
			};

            if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (AccountId != null)
                builder.Account = AccountId.ToProtobuf();

            foreach (var serial in Serials)
				builder.SerialNumbers.Add(serial);

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
            AccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenWipe = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenWipe = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.wipeTokenAccount);

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