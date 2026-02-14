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
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to delete.<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenDeletion;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenDeleteTransactionBody}</returns>
        public virtual Proto.TokenDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenDeleteTransactionBody();

            if (TokenId != null)
				builder.Token = TokenId.ToProtobuf();

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDeletion = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDeletion = ToProtobuf();
        }

        public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.deleteToken);

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