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
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

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
        private IList<long> serials = new ();
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
        TokenUpdateNftsTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
            Objects.RequireNonNull(tokenId);
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
            Objects.RequireNonNull(serials);
            serials = new List(serials);
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
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenUpdateNfts();
            if (body.HasToken())
            {
                tokenId = TokenId.FromProtobuf(body.GetToken());
            }

            serials = body.GetSerialNumbersList();
            if (body.HasMetadata())
            {
                metadata = body.GetMetadata().GetValue().ToByteArray();
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenUpdateNftsTransactionBody}</returns>
        virtual TokenUpdateNftsTransactionBody.Builder Build()
        {
            var builder = TokenUpdateNftsTransactionBody.NewBuilder();
            if (tokenId != null)
            {
                builder.SetToken(tokenId.ToProtobuf());
            }

            foreach (var serial in serials)
            {
                builder.AddSerialNumbers(serial);
            }

            if (metadata != null)
            {
                builder.SetMetadata(BytesValue.Of(ByteString.CopyFrom(metadata)));
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

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenUpdateNfts(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenUpdateNfts(Build());
        }
    }
}