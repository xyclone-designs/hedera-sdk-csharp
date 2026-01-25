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
    /// Mint tokens and deliver the new tokens to the token treasury account.
    /// 
    /// The token MUST have a `supply_key` set and that key MUST NOT
    /// be an empty `KeyList`.<br/>
    /// The token `supply_key` MUST sign this transaction.<br/>
    /// This operation SHALL increase the total supply for the token type by
    /// the number of tokens "minted".<br/>
    /// The total supply for the token type MUST NOT be increased above the
    /// maximum supply limit (2^63-1) by this transaction.<br/>
    /// The tokens minted SHALL be credited to the token treasury account.<br/>
    /// If the token is a fungible/common type, the amount MUST be specified.<br/>
    /// If the token is a non-fungible/unique type, the metadata bytes for each
    /// unique token MUST be specified in the `metadata` list.<br/>
    /// Each unique metadata MUST not exceed the global metadata size limit defined
    /// by the network configuration value `tokens.maxMetadataBytes`.<br/>
    /// The global batch size limit (`tokens.nfts.maxBatchSizeMint`) SHALL set
    /// the maximum number of individual NFT metadata permitted in a single
    /// `tokenMint` transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenMintTransaction : Transaction<TokenMintTransaction>
    {
        private TokenId tokenId = null;
        private List<byte[]> metadataList = new ();
        private long amount = 0;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenMintTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenMintTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenMintTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to "mint".<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenMintTransaction SetTokenId(TokenId tokenId)
        {
            Objects.RequireNonNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the amount.
        /// </summary>
        /// <returns>                         the amount to mint</returns>
        public virtual long GetAmount()
        {
            return amount;
        }

        /// <summary>
        /// An amount to mint to the Treasury Account.
        /// <p>
        /// This is interpreted as an amount in the smallest possible denomination
        /// for the token (10<sup>-decimals</sup> whole tokens).<br/>
        /// The balance for the token treasury account SHALL receive the newly
        /// minted tokens.<br/>
        /// If this value is equal to zero (`0`), the token SHOULD be a
        /// non-fungible/unique type.<br/>
        /// If this value is non-zero, the token MUST be a fungible/common type.
        /// </summary>
        /// <param name="amount">the amount to mint</param>
        /// <returns>{@code this}</returns>
        public virtual TokenMintTransaction SetAmount(long amount)
        {
            RequireNotFrozen();
            amount = amount;
            return this;
        }

        /// <summary>
        /// Add to the metadata list.
        /// </summary>
        /// <param name="metadata">the metadata 100 bytes max</param>
        /// <returns>{@code this}</returns>
        public virtual TokenMintTransaction AddMetadata(byte[] metadata)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(metadata);
            metadataList.Add(metadata);
            return this;
        }

        /// <summary>
        /// Extract the list of metadata byte array records.
        /// </summary>
        /// <returns>                         the metadata list</returns>
        public virtual List<byte[]> GetMetadata()
        {
            return new List(metadataList);
        }

        /// <summary>
        /// A list of metadata bytes.<br/>
        /// <p>
        /// One non-fungible/unique token SHALL be minted for each entry
        /// in this list.<br/>
        /// Each entry in this list MUST NOT be larger than the limit set by the
        /// current network configuration value `tokens.maxMetadataBytes`.<br/>
        /// This list MUST NOT contain more entries than the current limit set by
        /// the network configuration value `tokens.nfts.maxBatchSizeMint`.<br/>
        /// If this list is not empty, the token MUST be a
        /// non-fungible/unique type.<br/>
        /// If this list is empty, the token MUST be a fungible/common type.
        /// </summary>
        /// <param name="metadataList">the metadata list</param>
        /// <returns>{@code this}</returns>
        public virtual TokenMintTransaction SetMetadata(List<byte[]> metadataList)
        {
            RequireNotFrozen();
            metadataList = new List(metadataList);
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenMint();
            if (body.HasToken())
            {
                tokenId = TokenId.FromProtobuf(body.GetToken());
            }

            amount = body.GetAmount();
            foreach (var metadata in body.GetMetadataList())
            {
                metadataList.Add(metadata.ToByteArray());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenMintTransactionBody}</returns>
        virtual TokenMintTransactionBody.Builder Build()
        {
            var builder = TokenMintTransactionBody.NewBuilder();
            if (tokenId != null)
            {
                builder.SetToken(tokenId.ToProtobuf());
            }

            builder.SetAmount(amount);
            foreach (var metadata in metadataList)
            {
                builder.AddMetadata(ByteString.CopyFrom(metadata));
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetMintTokenMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenMint(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenMint(Build());
        }
    }
}