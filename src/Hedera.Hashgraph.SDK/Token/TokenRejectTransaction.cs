// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="T:TokenRejectTransaction"]/*' />
    public class TokenRejectTransaction : Transaction<TokenRejectTransaction>
    {
        /// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.#ctor"]/*' />
        public TokenRejectTransaction() { }
		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenRejectTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		public TokenRejectTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.RequireNotFrozen"]/*' />
		public virtual AccountId? OwnerId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }

        public IList<TokenId> TokenIds { protected get; init; } = [];
		public IReadOnlyList<TokenId> TokenIds_Read { get => TokenIds.AsReadOnly(); }

		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.AddTokenId(TokenId)"]/*' />
		public virtual TokenRejectTransaction AddTokenId(TokenId tokenId)
		{
			RequireNotFrozen();
			TokenIds.Add(tokenId);
			return this;
		}
		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.SetTokenIds(System.Collections.Generic.IList{TokenId})"]/*' />
		public virtual TokenRejectTransaction SetTokenIds(IList<TokenId> tokenIds)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenIds);
			TokenIds.Clear();
			foreach (TokenId tokenId in tokenIds) TokenIds.Add(tokenId);
            return this;
        }

		public IList<NftId> NftIds { protected get; init; } = [];
		public IReadOnlyList<NftId> NftIds_Read { get => NftIds.AsReadOnly(); }

		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.AddNftId(NftId)"]/*' />
		public virtual TokenRejectTransaction AddNftId(NftId nftId)
		{
			RequireNotFrozen();
			NftIds.Add(nftId);
			return this;
		}
		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.SetNftIds(System.Collections.Generic.IList{NftId})"]/*' />
		public virtual TokenRejectTransaction SetNftIds(IList<NftId> nftIds)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(NftIds);
			NftIds.Clear();
			foreach (NftId nftId in nftIds) NftIds.Add(nftId);
			return this;
        }

		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenReject;

			if (body.Owner is not null)
			{
				OwnerId = AccountId.FromProtobuf(body.Owner);
			}

			foreach (Proto.Services.TokenReference tokenReference in body.Rejections)
			{
				if (tokenReference.FungibleToken is not null)
				{
					TokenIds.Add(TokenId.FromProtobuf(tokenReference.FungibleToken));
				}
				else if (tokenReference.Nft is not null)
				{
					NftIds.Add(NftId.FromProtobuf(tokenReference.Nft));
				}
			}
		}
		/// <include file="TokenRejectTransaction.cs.xml" path='docs/member[@name="M:TokenRejectTransaction.ToProtobuf"]/*' />
		public virtual Proto.Services.TokenRejectTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenRejectTransactionBody();

            if (OwnerId != null)
            {
                builder.Owner = OwnerId.ToProtobuf();
            }

            foreach (TokenId tokenId in TokenIds)
            {
                builder.Rejections.Add(new Proto.Services.TokenReference() { FungibleToken = tokenId.ToProtobuf() });
            }

            foreach (NftId nftId in NftIds)
            {
                builder.Rejections.Add(new Proto.Services.TokenReference() { Nft = nftId.ToProtobuf() });
            }

            return builder;
        }

		public override void ValidateChecksums(Client client)
        {
			OwnerId?.ValidateChecksum(client);

			foreach (var token in TokenIds)
				token?.ValidateChecksum(client);

			foreach (var nftId in NftIds)
				nftId.TokenId.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenReject = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenReject = ToProtobuf();
        }
	
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.rejectToken);

			return Proto.Services.TokenService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
