// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Pause transaction activity for a token.
    /// 
    /// This transaction MUST be signed by the Token `pause_key`.<br/>
    /// The `token` identified MUST exist, and MUST NOT be deleted.<br/>
    /// The `token` identified MAY be paused; if the token is already paused,
    /// this transaction SHALL have no effect.
    /// The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
    /// a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
    /// A `paused` token SHALL NOT be transferred or otherwise modified except to
    /// "up-pause" the token with `unpauseToken` or in a `rejectToken` transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenPauseTransaction : Transaction<TokenPauseTransaction>
    {
        private TokenId tokenId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenPauseTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenPauseTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenPauseTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// The identified token SHALL be paused. Subsequent transactions
        /// involving that token SHALL fail until the token is "unpaused".
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenPauseTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenPause;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenPauseTransactionBody}</returns>
        public virtual Proto.TokenPauseTransactionBody Build()
        {
            var builder = new Proto.TokenPauseTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            return builder;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetPauseTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenPause = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenPause = Build();
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }
    }
}