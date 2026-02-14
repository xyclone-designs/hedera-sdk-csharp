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
    /// <summary>
    /// Reject undesired token(s).<br/>
    /// Transfer one or more token balances held by the requesting account to the
    /// treasury for each token type.
    /// 
    /// Each transfer SHALL be one of the following
    /// - A single non-fungible/unique token.
    /// - The full balance held for a fungible/common token.
    /// A single `tokenReject` transaction SHALL support a maximum
    /// of 10 transfers.<br/>
    /// A token that is `pause`d MUST NOT be rejected.<br/>
    /// If the `owner` account is `frozen` with respect to the identified token(s)
    /// the token(s) MUST NOT be rejected.<br/>
    /// The `payer` for this transaction, and `owner` if set, SHALL NOT be charged
    /// any custom fees or other fees beyond the `tokenReject` transaction fee.
    /// 
    /// ### Block Stream Effects
    /// - Each successful transfer from `payer` to `treasury` SHALL be recorded in
    ///   the `token_transfer_list` for the transaction record.
    /// </summary>
    public class TokenRejectTransaction : Transaction<TokenRejectTransaction>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TokenRejectTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenRejectTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public TokenRejectTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// An account identifier.<br/>
		/// This OPTIONAL field identifies the account holding the
		/// tokens to be rejected.
		/// <p>
		/// If set, this account MUST sign this transaction.
		/// If not set, the `payer` for this transaction SHALL be the effective
		/// `owner` for this transaction.
		/// </summary>
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

		/// <summary>
		/// Add a token to the list of tokens.
		/// </summary>
		/// <param name="tokenId">token to add.</param>
		/// <returns>{@code this}</returns>
		public virtual TokenRejectTransaction AddTokenId(TokenId tokenId)
		{
			RequireNotFrozen();
			TokenIds.Add(tokenId);
			return this;
		}
		/// <summary>
		/// A list of one or more token rejections (a fungible/common token type).
		/// </summary>
		/// <param name="TokenIds">the list of TokenIds.</param>
		/// <returns>{@code this}</returns>
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

		/// <summary>
		/// Add a nft to the list of nfts.
		/// </summary>
		/// <param name="nftId">nft to add.</param>
		/// <returns>{@code this}</returns>
		public virtual TokenRejectTransaction AddNftId(NftId nftId)
		{
			RequireNotFrozen();
			NftIds.Add(nftId);
			return this;
		}
		/// <summary>
		/// A list of one or more token rejections (a single specific serialized non-fungible/unique token).
		/// </summary>
		/// <param name="NftIds">the list of NftIds.</param>
		/// <returns>{@code this}</returns>
		public virtual TokenRejectTransaction SetNftIds(IList<NftId> nftIds)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(NftIds);
			NftIds.Clear();
			foreach (NftId nftId in nftIds) NftIds.Add(nftId);
			return this;
        }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenReject;

			if (body.Owner is not null)
			{
				OwnerId = AccountId.FromProtobuf(body.Owner);
			}

			foreach (Proto.TokenReference tokenReference in body.Rejections)
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
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.TokenRejectTransactionBody}</returns>
		public virtual Proto.TokenRejectTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenRejectTransactionBody();

            if (OwnerId != null)
            {
                builder.Owner = OwnerId.ToProtobuf();
            }

            foreach (TokenId tokenId in TokenIds)
            {
                builder.Rejections.Add(new Proto.TokenReference() { FungibleToken = tokenId.ToProtobuf() });
            }

            foreach (NftId nftId in NftIds)
            {
                builder.Rejections.Add(new Proto.TokenReference() { Nft = nftId.ToProtobuf() });
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
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenReject = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenReject = ToProtobuf();
        }
	
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.rejectToken);

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