// SPDX-License-Identifier: Apache-2.0
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Java.Util.Stream;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;

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
        private AccountId ownerId = null;
        /// <summary>
        /// A list of one or more token rejections (a fungible/common token type).
        /// </summary>
        private IList<TokenId> tokenIds = new ();
        /// <summary>
        /// A list of one or more token rejections (a single specific serialized non-fungible/unique token).
        /// </summary>
        private IList<NftId> nftIds = new ();
        private IList<AccountId> nodeAccountIds = null;
        private Client freezeWithClient = null;
        private PrivateKey signPrivateKey = null;
        private PublicKey signPublicKey = null;
        private Func<byte[], byte[]> transactionSigner = null;
        public TokenRejectFlow()
        {
        }

        /// <summary>
        /// Extract the Account ID of the Owner.
        /// </summary>
        /// <returns>the Account ID of the Owner.</returns>
        public virtual AccountId GetOwnerId()
        {
            return ownerId;
        }

        /// <summary>
        /// Assign the Account ID of the Owner.
        /// </summary>
        /// <param name="ownerId">the Account ID of the Owner.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SetOwnerId(AccountId ownerId)
        {
            Objects.RequireNonNull(ownerId);
            ownerId = ownerId;
            return this;
        }

        /// <summary>
        /// Extract the list of tokenIds.
        /// </summary>
        /// <returns>the list of tokenIds.</returns>
        public virtual IList<TokenId> GetTokenIds()
        {
            return tokenIds;
        }

        /// <summary>
        /// Assign the list of tokenIds.
        /// </summary>
        /// <param name="tokenIds">the list of tokenIds.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SetTokenIds(IList<TokenId> tokenIds)
        {
            Objects.RequireNonNull(tokenIds);
            tokenIds = new List(tokenIds);
            return this;
        }

        /// <summary>
        /// Add a token to the list of tokens.
        /// </summary>
        /// <param name="tokenId">token to add.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow AddTokenId(TokenId tokenId)
        {
            tokenIds.Add(tokenId);
            return this;
        }

        /// <summary>
        /// Extract the list of nftIds.
        /// </summary>
        /// <returns>the list of nftIds.</returns>
        public virtual IList<NftId> GetNftIds()
        {
            return nftIds;
        }

        /// <summary>
        /// Assign the list of nftIds.
        /// </summary>
        /// <param name="nftIds">the list of nftIds.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SetNftIds(IList<NftId> nftIds)
        {
            Objects.RequireNonNull(nftIds);
            nftIds = new List(nftIds);
            return this;
        }

        /// <summary>
        /// Add a nft to the list of nfts.
        /// </summary>
        /// <param name="nftId">nft to add.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow AddNftId(NftId nftId)
        {
            nftIds.Add(nftId);
            return this;
        }

        /// <summary>
        /// Set the account IDs of the nodes that this transaction will be submitted to.
        /// <p>
        /// Providing an explicit node account ID interferes with client-side load balancing of the network. By default, the
        /// SDK will pre-generate a transaction for 1/3 of the nodes on the network. If a node is down, busy, or otherwise
        /// reports a fatal error, the SDK will try again with a different node.
        /// </summary>
        /// <param name="nodeAccountIds">The list of node AccountIds to be set</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SetNodeAccountIds(IList<AccountId> nodeAccountIds)
        {
            Objects.RequireNonNull(nodeAccountIds);
            nodeAccountIds = new List(nodeAccountIds);
            return this;
        }

        /// <summary>
        /// Set the client that this transaction will be frozen with.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow FreezeWith(Client client)
        {
            freezeWithClient = client;
            return this;
        }

        /// <summary>
        /// Set the private key that this transaction will be signed with.
        /// </summary>
        /// <param name="privateKey">the private key used for signing</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow Sign(PrivateKey privateKey)
        {
            signPrivateKey = privateKey;
            signPublicKey = null;
            transactionSigner = null;
            return this;
        }

        /// <summary>
        /// Set the public key and key list that this transaction will be signed with.
        /// </summary>
        /// <param name="publicKey">the public key</param>
        /// <param name="transactionSigner">the key list</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
        {
            signPublicKey = publicKey;
            transactionSigner = transactionSigner;
            signPrivateKey = null;
            return this;
        }

        /// <summary>
        /// Set the operator that this transaction will be signed with.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectFlow SignWithOperator(Client client)
        {
            var operator = Objects.RequireNonNull(client.GetOperator());
            signPublicKey = @operator.publicKey;
            transactionSigner = @operator.transactionSigner;
            signPrivateKey = null;
            return this;
        }

        private void FillOutTransaction(Transaction<TWildcardTodo> transaction)
        {
            if (nodeAccountIds != null)
            {
                transaction.SetNodeAccountIds(nodeAccountIds);
            }

            if (freezeWithClient != null)
            {
                transaction.FreezeWith(freezeWithClient);
            }

            if (signPrivateKey != null)
            {
                transaction.Sign(signPrivateKey);
            }
            else if (signPublicKey != null && transactionSigner != null)
            {
                transaction.SignWith(signPublicKey, transactionSigner);
            }
        }

        private TokenRejectTransaction CreateTokenRejectTransaction()
        {
            var tokenRejectTransaction = new TokenRejectTransaction().SetOwnerId(ownerId).SetTokenIds(tokenIds).SetNftIds(nftIds);
            FillOutTransaction(tokenRejectTransaction);
            return tokenRejectTransaction;
        }

        private TokenDissociateTransaction CreateTokenDissociateTransaction()
        {
            IList<TokenId> tokenIdsToReject = Stream.Concat(tokenIds.Stream(), nftIds.Stream().Map((nftId) => nftId.tokenId)).Distinct().Collect(Collectors.ToList());
            var tokenDissociateTransaction = new TokenDissociateTransaction().SetAccountId(ownerId).SetTokenIds(tokenIdsToReject);
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
            return Execute(client, client.GetRequestTimeout());
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
            return ExecuteAsync(client, client.GetRequestTimeout());
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <returns>the response</returns>
        public virtual Task<TransactionResponse> ExecuteAsync(Client client, Duration timeoutPerTransaction)
        {
            return CreateTokenRejectTransaction().ExecuteAsync(client, timeoutPerTransaction).ThenCompose((tokenRejectResponse) => tokenRejectResponse.GetReceiptQuery().ExecuteAsync(client, timeoutPerTransaction)).ThenCompose((receipt) => CreateTokenDissociateTransaction().ExecuteAsync(client, timeoutPerTransaction));
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="timeoutPerTransaction">The timeout after which each transaction's execution attempt will be cancelled.</param>
        /// <param name="callback">a Action which handles the result or error.</param>
        public virtual void ExecuteAsync(Client client, Duration timeoutPerTransaction, Action<TransactionResponse, Exception> callback)
        {
            ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }

        /// <summary>
        /// Execute the transactions in the flow with the passed in client asynchronously.
        /// </summary>
        /// <param name="client">the client with the transaction to execute</param>
        /// <param name="onSuccess">a Action which consumes the result on success.</param>
        /// <param name="onFailure">a Action which consumes the error on failure.</param>
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
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
            ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}