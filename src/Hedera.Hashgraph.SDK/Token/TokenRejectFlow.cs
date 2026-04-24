// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="T:TokenRejectFlow"]/*' />
    public class TokenRejectFlow
    {        
		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="P:TokenRejectFlow.OwnerId"]/*' />
		public virtual AccountId? OwnerId { get; set; }
		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="P:TokenRejectFlow.NftIds"]/*' />
		public virtual List<NftId> NftIds { get; set; } = [];
		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="P:TokenRejectFlow.TokenIds"]/*' />
		public virtual List<TokenId> TokenIds { get; set; } = [];
		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="P:TokenRejectFlow.NodeAccountIds"]/*' />
		public virtual List<AccountId>? NodeAccountIds { get; set; }


		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="P:TokenRejectFlow.FreezeWithClient"]/*' />
		public Client? FreezeWithClient { get; set; }
		/// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="T:TokenRejectFlow_2"]/*' />
		public PrivateKey? SignPrivateKey 
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

        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.SignWith(PublicKey,System.Func{System.Byte[],System.Byte[]})"]/*' />
        public virtual TokenRejectFlow SignWith(PublicKey publicKey, Func<byte[], byte[]> transactionSigner)
        {
            SignPublicKey = publicKey;
            TransactionSigner = transactionSigner;
            SignPrivateKey = null;
            return this;
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.SignWithOperator(Client)"]/*' />
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
				AccountId = OwnerId,
				TokenIds = [.. TokenIds, .. NftIds.Select(_ => _.TokenId)],
			};

			FillOutTransaction(tokenDissociateTransaction);

            return tokenDissociateTransaction;
        }

        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.Execute(Client)"]/*' />
        public virtual TransactionResponse Execute(Client client)
        {
            return Execute(client, client.RequestTimeout);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.Execute(Client,System.TimeSpan)"]/*' />
        public virtual TransactionResponse Execute(Client client, TimeSpan timeoutPerTransaction)
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
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client)"]/*' />
        public virtual Task<TransactionResponse> ExecuteAsync(Client client)
        {
            return ExecuteAsync(client, client.RequestTimeout);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client,System.TimeSpan)"]/*' />
        public virtual async Task<TransactionResponse> ExecuteAsync(Client client, TimeSpan timeoutPerTransaction)
        {
            TransactionResponse transactionresponse = await CreateTokenRejectTransaction().ExecuteAsync(client, timeoutPerTransaction);
            TransactionReceipt transactionreceipt = await transactionresponse.GetReceiptQuery().ExecuteAsync(client, timeoutPerTransaction);

            return await CreateTokenDissociateTransaction().ExecuteAsync(client, timeoutPerTransaction);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client), callback);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse,System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse?, Exception?> callback)
        {
            Utils.ActionHelper.Action(ExecuteAsync(client, timeoutPerTransaction), callback);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client), onSuccess, onFailure);
        }
        /// <include file="TokenRejectFlow.cs.xml" path='docs/member[@name="M:TokenRejectFlow.ExecuteAsync(Client,System.TimeSpan,System.Action{TransactionResponse},System.Action{System.Exception})"]/*' />
        public virtual void ExecuteAsync(Client client, TimeSpan timeoutPerTransaction, Action<TransactionResponse> onSuccess, Action<Exception> onFailure)
        {
            Utils.ActionHelper.TwoActions(ExecuteAsync(client, timeoutPerTransaction), onSuccess, onFailure);
        }
    }
}