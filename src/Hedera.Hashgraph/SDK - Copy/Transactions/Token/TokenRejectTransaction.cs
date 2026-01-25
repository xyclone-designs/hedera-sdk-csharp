// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
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
        private IList<TokenId> tokenIds = new ();
        private IList<NftId> nftIds = new ();
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
        /// Extract the Account ID of the Owner.
        /// </summary>
        /// <returns>the Account ID of the Owner.</returns>
        public virtual AccountId GetOwnerId()
        {
            return ownerId;
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
        /// <param name="ownerId">the Account ID of the Owner.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction SetOwnerId(AccountId ownerId)
        {
            Objects.RequireNonNull(ownerId);
            RequireNotFrozen();
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
        /// A list of one or more token rejections (a fungible/common token type).
        /// </summary>
        /// <param name="tokenIds">the list of tokenIds.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction SetTokenIds(IList<TokenId> tokenIds)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(tokenIds);
            tokenIds = new List(tokenIds);
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
        /// Extract the list of nftIds.
        /// </summary>
        /// <returns>the list of nftIds.</returns>
        public virtual IList<NftId> GetNftIds()
        {
            return nftIds;
        }

        /// <summary>
        /// A list of one or more token rejections (a single specific serialized non-fungible/unique token).
        /// </summary>
        /// <param name="nftIds">the list of nftIds.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenRejectTransaction SetNftIds(IList<NftId> nftIds)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(nftIds);
            nftIds = new List(nftIds);
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
        virtual TokenRejectTransactionBody.Builder Build()
        {
            var builder = TokenRejectTransactionBody.NewBuilder();
            if (ownerId != null)
            {
                builder.SetOwner(ownerId.ToProtobuf());
            }

            foreach (TokenId tokenId in tokenIds)
            {
                builder.AddRejections(TokenReference.NewBuilder().SetFungibleToken(tokenId.ToProtobuf()).Build());
            }

            foreach (NftId nftId in nftIds)
            {
                builder.AddRejections(TokenReference.NewBuilder().SetNft(nftId.ToProtobuf()).Build());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenReject();
            if (body.HasOwner())
            {
                ownerId = AccountId.FromProtobuf(body.GetOwner());
            }

            foreach (TokenReference tokenReference in body.GetRejectionsList())
            {
                if (tokenReference.HasFungibleToken())
                {
                    tokenIds.Add(TokenId.FromProtobuf(tokenReference.GetFungibleToken()));
                }
                else if (tokenReference.HasNft())
                {
                    nftIds.Add(NftId.FromProtobuf(tokenReference.GetNft()));
                }
            }
        }

        override void ValidateChecksums(Client client)
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

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetRejectTokenMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenReject(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenReject(Build());
        }
    }
}