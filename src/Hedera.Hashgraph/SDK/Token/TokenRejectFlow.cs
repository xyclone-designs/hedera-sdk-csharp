// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Reject undesired token(s) and dissociate in a single flow.
    /// </summary>
    public class TokenRejectFlow
    {        
		/// <summary>
		/// An account holding the tokens to be rejected.
		/// </summary>
		public virtual AccountId? OwnerId { get; set; }
		/// <summary>
		/// A list of one or more token rejections (a single specific serialized non-fungible/unique token).
		/// </summary>
		public virtual List<NftId> NftIds { get; set; } = [];
		/// <summary>
		/// Extract the list of tokenIds.
		/// </summary>
		/// <returns>the list of tokenIds.</returns>
		public virtual List<TokenId> TokenIds { get; set; } = [];
		/// <summary>
		/// Set the account IDs of the nodes that this transaction will be submitted to.
		/// <p>
		/// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
		/// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
		/// reports a fatal error, the SDK will try again with a different node.
		/// </summary>
		/// <param name="nodeAccountIds">The list of node AccountIds to be set</param>
		/// <returns>{@code this}</returns>
		public virtual List<AccountId>? NodeAccountIds { get; set; }


		/// <summary>
		/// Set the client that this transaction will be frozen with.
		/// </summary>
		public Client? FreezeWithClient { get; set; }
		/// <summary>
		/// Set the private key that this transaction will be signed with.
		/// </summary>
		private PrivateKey? SignPrivateKey 
        { 
            get;
            set 
            {
				field = value;
				SignPublicKey = null;
				TransactionSigner = null;
			}
        }
		private PublicKey? SignPublicKey { get; set; }
		private Func<byte[], byte[]>? TransactionSigner { get; set; }

        /// <summary>
        /// Set the public key and key list that this transaction will be signed with.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="transactionSigner">the key list</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
        {
            SignPublicKey = publicKey;
            TransactionSigner = transactionSigner;
            SignPrivateKey = null;
            return this;
        }
        /// <summary>
        /// Set the operator that this transaction will be signed with.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SignWithOperator(Client client)
        {
            SignPublicKey = client.Operator_.PublicKey;
            TransactionSigner = client.Operator_.TransactionSigner;
            SignPrivateKey = null;

            return this;
        }

        private void FillOutTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
        {
            if (NodeAccountIds != null)
            {
                transaction.SetNodeAccountIds(NodeAccountIds);
            }

            if (FreezeWithClient != null)
            {
                transaction.FreezeWith(FreezeWithClient);
            }

            if (SignPrivateKey != null)
            {
                transaction.Sign(SignPrivateKey);
            }
            else if (SignPublicKey != null && TransactionSigner != null)
            {
                transaction.SignWith(SignPublicKey, TransactionSigner);
            }
        }
        private TokenRejectTransaction CreateTokenRejectTransaction()
        {
			TokenRejectTransaction tokenRejectTransaction = new ()
            {
				NftIds = NftIds,
				OwnerId = OwnerId,
				TokenIds = TokenIds,
			};

            FillOutTransaction(tokenRejectTransaction);

            return tokenRejectTransaction;
        }
        private TokenDissociateTransaction CreateTokenDissociateTransaction()
        {
			TokenDissociateTransaction tokenDissociateTransaction = new()
			{
				Account = OwnerId,
				TokenIds = [.. TokenIds, .. NftIds.Select(_ => _.TokenId)],
			};

			FillOutTransaction(tokenDissociateTransaction);

            return tokenDissociateTransaction;
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response of TokenRejectTransaction</returns>
        /// <exception cref="PrecheckStatusException">when the precheck fails</exception>
        /// <exception cref="TimeoutException">when the transaction times out</exception>
        public virtual TransactionResponse Execute(Client client, Duration timeoutPerTransaction)
        {
            try
            {
                var tokenRejectTxResponse = CreateTokenRejectTransaction().Execute(client, timeoutPerTransaction);
                tokenRejectTxResponse.GetReceipt(client, timeoutPerTransaction);
                var tokenDissociateTxResponse = CreateTokenDissociateTransaction().Execute(client, timeoutPerTransaction);
                tokenDissociateTxResponse.GetReceipt(client, timeoutPerTransaction);
                return tokenRejectTxResponse;
            }
            catch (ReceiptStatusException e)
            {
                throw new Exception(string.Empty, e);
            }
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>the response</returns>
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            TransactionResponse transactionresponse = await CreateTokenRejectTransaction().ExecuteAsync(client, timeoutPerTransaction);
            TransactionReceipt transactionreceipt = await transactionresponse.GetReceiptQuery().ExecuteAsync(client, timeoutPerTransaction);

            return await CreateTokenDissociateTransaction().ExecuteAsync(client, timeoutPerTransaction);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client), callback);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }
        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}