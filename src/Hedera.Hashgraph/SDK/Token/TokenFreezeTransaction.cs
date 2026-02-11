// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Block transfers of a token type for an account.<br/>
    /// This, effectively, freezes assets of one account with respect to
    /// one token type. While frozen, that account cannot send or receive tokens
    /// of the identified type.
    /// 
    /// The token MUST have a `freeze_key` set and that key MUST NOT
    /// be an empty `KeyList`.<br/>
    /// The token `freeze_key` MUST sign this transaction.<br/>
    /// The identified token MUST exist, MUST NOT be deleted, MUST NOT be paused,
    /// and MUST NOT be expired.<br/>
    /// The identified account MUST exist, MUST NOT be deleted, and
    /// MUST NOT be expired.<br/>
    /// If the identified account is already frozen with respect to the identified
    /// token, the transaction SHALL succeed, but no change SHALL be made.<br/>
    /// An association between the identified account and the identified
    /// token MUST exist.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenFreezeTransaction : Transaction<TokenFreezeTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenFreezeTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenFreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenFreezeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to "freeze".<br/>
        /// The identified token MUST exist, MUST NOT be deleted, and MUST be
        /// associated to the identified account.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// An account identifier.
        /// <p>
        /// This shall identify the account to "freeze".<br/>
        /// The identified account MUST exist, MUST NOT be deleted, MUST NOT be
        /// expired, and MUST be associated to the identified token.<br/>
        /// The identified account SHOULD NOT be "frozen" with respect to the
        /// identified token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenFreeze;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
				AccountId = AccountId.FromProtobuf(body.Account);
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenFreezeAccountTransactionBody}</returns>
        public virtual Proto.TokenFreezeAccountTransactionBody Build()
        {
            var builder = new Proto.TokenFreezeAccountTransactionBody();

            if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (AccountId != null)
				builder.Account = AccountId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
			AccountId?.ValidateChecksum(client);
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenFreeze = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenFreeze = Build();
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return TokenServiceGrpc.GetFreezeTokenAccountMethod();
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