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
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenMintTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenMintTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenMintTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to "mint".<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="TokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
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
		public virtual long Amount { get; set { RequireNotFrozen(); field = value; } }
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
        public virtual List<byte[]> MetadataList { get; set { RequireNotFrozen(); field = [.. value]; } } = [];

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenMint;

            if (body.Token is not null)
            {
                TokenId = TokenId.FromProtobuf(body.Token);
            }

            Amount = (long)body.Amount;

            foreach (var metadata in body.Metadata)
            {
                MetadataList.Add(metadata.ToByteArray());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenMintTransactionBody}</returns>
        public virtual Proto.TokenMintTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenMintTransactionBody();

            if (TokenId != null)
            {
                builder.Token = TokenId.ToProtobuf();
            }

            builder.Amount = (ulong)Amount;

            foreach (var metadata in MetadataList)
            {
                builder.Metadata.Add(ByteString.CopyFrom(metadata));
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenMint = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenMint = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.mintToken);

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