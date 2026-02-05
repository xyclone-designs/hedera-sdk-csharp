// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Resume transaction activity for a token.
    /// 
    /// This transaction MUST be signed by the Token `pause_key`.<br/>
    /// The `token` identified MUST exist, and MUST NOT be deleted.<br/>
    /// The `token` identified MAY not be paused; if the token is not paused,
    /// this transaction SHALL have no effect.
    /// The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
    /// a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
    /// An `unpaused` token MAY be transferred or otherwise modified.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenUnpauseTransaction : Transaction<TokenUnpauseTransaction>
    {
        private TokenId tokenId = null;
        /// <summary>
        /// Constructor
        /// </summary>
        public TokenUnpauseTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenUnpauseTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenUnpauseTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// The identified token SHALL be "unpaused". Subsequent transactions
        /// involving that token MAY succeed.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUnpauseTransaction SetTokenId(TokenId tokenId)
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
            var body = SourceTransactionBody.TokenUnpause;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenUnpauseTransactionBody}</returns>
        public virtual Proto.TokenUnpauseTransactionBody Build()
        {
            var builder = new Proto.TokenUnpauseTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            return builder;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetUnpauseTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnpause = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnpause = Build();
        }

        public override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }
    }
}