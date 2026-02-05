// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Deleting a token marks a token as deleted, though it will remain in the
    /// ledger. The operation must be signed by the specified Admin Key of the
    /// Token. If the Admin Key is not set, Transaction will result in
    /// TOKEN_IS_IMMUTABlE. Once deleted update, mint, burn, wipe, freeze,
    /// unfreeze, grant kyc, revoke kyc and token transfer transactions will
    /// resolve to TOKEN_WAS_DELETED.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/delete-a-token">Hedera Documentation</a>
    /// </summary>
    public class TokenDeleteTransaction : Transaction<TokenDeleteTransaction>
    {
        private TokenId tokenId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// This SHALL identify the token type to delete.<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenDeleteTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenDeletion;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenDeleteTransactionBody}</returns>
        public virtual Proto.TokenDeleteTransactionBody Build()
        {
            var builder = new Proto.TokenDeleteTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetDeleteTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDeletion = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDeletion = Build();
        }
    }
}