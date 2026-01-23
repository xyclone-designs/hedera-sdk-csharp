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

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Burns tokens from the Token's treasury Account.
    /// 
    /// The token MUST have a `supply_key` set and that key MUST NOT
    /// be an empty `KeyList`.<br/>
    /// The token `supply_key` MUST sign this transaction.<br/>
    /// This operation SHALL decrease the total supply for the token type by
    /// the number of tokens "burned".<br/>
    /// The total supply for the token type MUST NOT be reduced below zero (`0`)
    /// by this transaction.<br/>
    /// The tokens to burn SHALL be deducted from the token treasury account.<br/>
    /// If the token is a fungible/common type, the amount MUST be specified.<br/>
    /// If the token is a non-fungible/unique type, the specific serial numbers
    /// MUST be specified.<br/>
    /// The global batch size limit (`tokens.nfts.maxBatchSizeBurn`) SHALL set
    /// the maximum number of individual NFT serial numbers permitted in a single
    /// `tokenBurn` transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenBurnTransaction : Transaction<TokenBurnTransaction>
    {
        private TokenId tokenId = null;
        private long amount = 0;
        private IList<long> serials = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenBurnTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenBurnTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenBurnTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// This SHALL identify the token type to "burn".<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenBurnTransaction SetTokenId(TokenId tokenId)
        {
            Objects.RequireNonNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the amount of tokens to burn.
        /// </summary>
        /// <returns>                         the amount of tokens to burn</returns>
        public virtual long GetAmount()
        {
            return amount;
        }

        /// <summary>
        /// Assign the amount of tokens to burn.
        /// 
        /// The amount provided must be in the lowest denomination possible.
        /// 
        /// Example: Token A has 2 decimals. In order to burn 100 tokens, one must
        /// provide an amount of 10000. In order to burn 100.55 tokens, one must
        /// provide an amount of 10055.
        /// 
        /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/burn-a-token">Hedera Documentation</a>
        /// </summary>
        /// <param name="amount">the amount of tokens to burn</param>
        /// <returns>{@code this}</returns>
        public virtual TokenBurnTransaction SetAmount(long amount)
        {
            RequireNotFrozen();
            amount = amount;
            return this;
        }

        /// <summary>
        /// Extract the of token serials.
        /// </summary>
        /// <returns>                         list of token serials</returns>
        public virtual IList<long> GetSerials()
        {
            return new List(serials);
        }

        /// <summary>
        /// A list of serial numbers to burn from the Treasury Account.
        /// <p>
        /// This list MUST NOT contain more entries than the current limit set by
        /// the network configuration value `tokens.nfts.maxBatchSizeBurn`.<br/>
        /// The treasury account for the token MUST hold each unique token
        /// identified in this list.<br/>
        /// If this list is not empty, the token MUST be a
        /// non-fungible/unique type.<br/>
        /// If this list is empty, the token MUST be a fungible/common type.
        /// </summary>
        /// <param name="serials">list of token serials</param>
        /// <returns>{@code this}</returns>
        public virtual TokenBurnTransaction SetSerials(IList<long> serials)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(serials);
            serials = new List(serials);
            return this;
        }

        /// <summary>
        /// Add a serial number to the list of serials.
        /// </summary>
        /// <param name="serial">the serial number to add</param>
        /// <returns>{@code this}</returns>
        public virtual TokenBurnTransaction AddSerial(long serial)
        {
            RequireNotFrozen();
            serials.Add(serial);
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenBurnTransactionBody}</returns>
        virtual TokenBurnTransactionBody.Builder Build()
        {
            var builder = TokenBurnTransactionBody.NewBuilder();
            if (tokenId != null)
            {
                builder.SetToken(tokenId.ToProtobuf());
            }

            builder.SetAmount(amount);
            foreach (var serial in serials)
            {
                builder.AddSerialNumbers(serial);
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenBurn();
            if (body.HasToken())
            {
                tokenId = TokenId.FromProtobuf(body.GetToken());
            }

            amount = body.GetAmount();
            serials = body.GetSerialNumbersList();
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
            return TokenServiceGrpc.GetBurnTokenMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenBurn(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenBurn(Build());
        }
    }
}