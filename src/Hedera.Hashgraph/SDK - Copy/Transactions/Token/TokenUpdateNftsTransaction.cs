// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Modify the metadata field for an individual non-fungible/unique token (NFT).
    /// 
    /// Updating the metadata of an NFT SHALL NOT affect ownership or
    /// the ability to transfer that NFT.<br/>
    /// This transaction SHALL affect only the specific serial numbered tokens
    /// identified.
    /// This transaction SHALL modify individual token metadata.<br/>
    /// This transaction MUST be signed by the token `metadata_key`.<br/>
    /// The token `metadata_key` MUST be a valid `Key`.<br/>
    /// The token `metadata_key` MUST NOT be an empty `KeyList`.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenUpdateNftsTransaction : Transaction<TokenUpdateNftsTransaction>
    {
        private TokenId tokenId = null;
        private IList<long> serials = [];
        private byte[] metadata = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUpdateNftsTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenUpdateNftsTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        TokenUpdateNftsTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
        }

        /// <summary>
        /// A token identifier.<br/>
        /// This is the token type (i.e. collection) for which to update NFTs.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// The identified token MUST exist, MUST NOT be paused, MUST have the type
        /// non-fungible/unique, and MUST have a valid `metadata_key`.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateNftsTransaction SetTokenId(TokenId tokenId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(tokenId);
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the list of serial numbers.
        /// </summary>
        /// <returns>the list of serial numbers</returns>
        public virtual IList<long> GetSerials()
        {
            return serials;
        }

        /// <summary>
        /// A list of serial numbers to be updated.
        /// <p>
        /// This field is REQUIRED.<br/>
        /// This list MUST have at least one(1) entry.<br/>
        /// This list MUST NOT have more than ten(10) entries.
        /// </summary>
        /// <param name="serials">the list of serial numbers</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateNftsTransaction SetSerials(IList<long> serials)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(serials);
            serials = [.. serials];
            return this;
        }

        /// <summary>
        /// Add a serial number to the list of serial numbers.
        /// </summary>
        /// <param name="serial">the serial number to add</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateNftsTransaction AddSerial(long serial)
        {
            RequireNotFrozen();
            serials.Add(serial);
            return this;
        }

        /// <summary>
        /// Extract the metadata.
        /// </summary>
        /// <returns>the metadata</returns>
        public virtual byte[] GetMetadata()
        {
            return metadata;
        }

        /// <summary>
        /// A new value for the metadata.
        /// <p>
        /// If this field is not set, the metadata SHALL NOT change.<br/>
        /// This value, if set, MUST NOT exceed 100 bytes.
        /// </summary>
        /// <param name="metadata">the metadata</param>
        /// <returns>{@code this}</returns>
        public virtual TokenUpdateNftsTransaction SetMetadata(byte[] metadata)
        {
            RequireNotFrozen();
            metadata = metadata;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenUpdateNfts;

            if (body.Token is not null)

			{
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            serials = body.SerialNumbers;

            if (body.Metadata is not null)
            {
                metadata = body.Metadata.ToByteArray();
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenUpdateNftsTransactionBody}</returns>
        public virtual Proto.TokenUpdateNftsTransactionBody Build()
        {
            var builder = new Proto.TokenUpdateNftsTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            foreach (var serial in serials)
            {
                builder.SerialNumbers.Add(serial);
            }

            if (metadata != null)
            {
                builder.Metadata = ByteString.CopyFrom(metadata);
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

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetUpdateNftsMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdateNfts = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdateNfts = Build();
        }
    }
}