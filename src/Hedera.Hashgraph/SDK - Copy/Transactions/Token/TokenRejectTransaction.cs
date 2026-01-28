// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
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

namespace Hedera.Hashgraph.SDK.Transactions.Token
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
        private AccountId ownerId = null;
        private IList<TokenId> tokenIds = [];
        private IList<NftId> nftIds = [];
        /// <summary>
        /// Constructor
        /// </summary>
        public TokenRejectTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenRejectTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenRejectTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        public virtual TokenRejectTransaction OwnerId
        {
            get;
            set
            {
				RequireNotFrozen();
				field = value;
			}
        }
		public IList<TokenId> TokenIds { get; }
		public IList<NftId> NftIds { get; }


		/// <summary>
		/// A list of one or more token rejections (a fungible/common token type).
		/// </summary>
		/// <param name="tokenIds">the list of tokenIds.</param>
		/// <returns>{@code this}</returns>
		public virtual TokenRejectTransaction SetTokenIds(IList<TokenId> tokenIds)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenIds);
            tokenIds = [.. tokenIds];
            return this;
        }

        /// <summary>
        /// Add a token to the list of tokens.
        /// </summary>
        /// <param name="tokenId">token to add.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction AddTokenId(TokenId tokenId)
        {
            RequireNotFrozen();
            tokenIds.Add(tokenId);
            return this;
        }


        /// <summary>
        /// A list of one or more token rejections (a single specific serialized non-fungible/unique token).
        /// </summary>
        /// <param name="nftIds">the list of nftIds.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction SetNftIds(IList<NftId> nftIds)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftIds);
            nftIds = [.. nftIds];
            return this;
        }

        /// <summary>
        /// Add a nft to the list of nfts.
        /// </summary>
        /// <param name="nftId">nft to add.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction AddNftId(NftId nftId)
        {
            RequireNotFrozen();
            nftIds.Add(nftId);
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenRejectTransactionBody}</returns>
        public virtual Proto.TokenRejectTransactionBody Build()
        {
            var builder = new Proto.TokenRejectTransactionBody();

            if (ownerId != null)
            {
                builder.Owner = ownerId.ToProtobuf();
            }

            foreach (TokenId tokenId in tokenIds)
            {
                builder.Rejections.Add(new Proto.TokenReference() { FungibleToken = tokenId.ToProtobuf() });
            }

            foreach (NftId nftId in nftIds)
            {
                builder.Rejections.Add(new Proto.TokenReference() { Nft = nftId.ToProtobuf() });
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenReject;

            if (body.Owner is not null)
            {
                ownerId = AccountId.FromProtobuf(body.Owner);
            }

            foreach (Proto.TokenReference tokenReference in body.Rejections)
            {
                if (tokenReference.FungibleToken is not null)
                {
                    tokenIds.Add(TokenId.FromProtobuf(tokenReference.FungibleToken));
                }
                else if (tokenReference.Nft is not null)
                {
                    nftIds.Add(NftId.FromProtobuf(tokenReference.Nft));
                }
            }
        }

		public override void ValidateChecksums(Client client)
        {
            if (ownerId != null)
            {
                ownerId.ValidateChecksum(client);
            }

            foreach (var token in tokenIds)
            {
                if (token != null)
                {
                    token.ValidateChecksum(client);
                }
            }

            foreach (var nftId in nftIds)
            {
                nftId.TokenId.ValidateChecksum(client);
            }
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenReject = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenReject = Build();
        }
		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
		{
			return TokenServiceGrpc.GetRejectTokenMethod();
		}
	}
}