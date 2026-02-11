// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Resume transfers of a token type for an account.<br/>
    /// This releases previously frozen assets of one account with respect to
    /// one token type. Once unfrozen, that account can once again send or
    /// receive tokens of the identified type.
    /// 
    /// The token MUST have a `freeze_key` set and that key MUST NOT
    /// be an empty `KeyList`.<br/>
    /// The token `freeze_key` MUST sign this transaction.<br/>
    /// The identified token MUST exist, MUST NOT be deleted, MUST NOT be paused,
    /// and MUST NOT be expired.<br/>
    /// The identified account MUST exist, MUST NOT be deleted, and
    /// MUST NOT be expired.<br/>
    /// If the identified account is not frozen with respect to the identified
    /// token, the transaction SHALL succeed, but no change SHALL be made.<br/>
    /// An association between the identified account and the identified
    /// token MUST exist.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenUnfreezeTransaction : Transaction<TokenUnfreezeTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUnfreezeTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenUnfreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenUnfreezeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to "unfreeze".<br/>
        /// The identified token MUST exist, MUST NOT be deleted, and MUST be
        /// associated to the identified account.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// An account identifier.
		/// <p>
		/// This shall identify the account to "unfreeze".<br/>
		/// The identified account MUST exist, MUST NOT be deleted, MUST NOT be
		/// expired, and MUST be associated to the identified token.<br/>
		/// The identified account SHOULD be "frozen" with respect to the
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
            var body = SourceTransactionBody.TokenUnfreeze;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
				AccountId = AccountId.FromProtobuf(body.Account);
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@code @link
        ///         Proto.TokenUnfreezeAccountTransactionBody}</returns>
        public virtual Proto.TokenUnfreezeAccountTransactionBody Build()
        {
            var builder = new Proto.TokenUnfreezeAccountTransactionBody();

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
            bodyBuilder.TokenUnfreeze = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnfreeze = Build();
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