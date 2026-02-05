// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
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
        private TokenId tokenId = null;
        private AccountId accountId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUnfreezeTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenUnfreezeTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenUnfreezeTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId TokenId
        {
            return tokenId;
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
        public virtual TokenUnfreezeTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

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
        public virtual TokenUnfreezeTransaction SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUnfreeze;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            if (body.Account is not null)
            {
                accountId = AccountId.FromProtobuf(body.Account);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@code @link
        ///         Proto.TokenUnfreezeAccountTransactionBody}</returns>
        public virtual Proto.TokenUnfreezeAccountTransactionBody Build()
        {
            var builder = new Proto.TokenUnfreezeAccountTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            if (accountId != null)
            {
                builder.Account = accountId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetFreezeTokenAccountMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnfreeze = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnfreeze = Build();
        }
    }
}