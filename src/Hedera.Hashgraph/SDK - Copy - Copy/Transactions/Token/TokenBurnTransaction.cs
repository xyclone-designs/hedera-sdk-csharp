// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

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
        private ulong amount = 0;
        private IList<long> serials = [];
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
        TokenBurnTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
        public virtual TokenId TokenId
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
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the amount of tokens to burn.
        /// </summary>
        /// <returns>                         the amount of tokens to burn</returns>
        public virtual ulong GetAmount()
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
            return [..serials];
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
            ArgumentNullException.ThrowIfNull(serials);
            serials = [..serials];
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
        public virtual Proto.TokenBurnTransactionBody Build()
        {
            var builder = new Proto.TokenBurnTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            builder.Amount = amount;

            foreach (var serial in serials)
            {
                builder.SerialNumbers.Add(serial);
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenBurn;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            amount = body.Amount;
            serials = body.SerialNumbers;
        }

        public override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetBurnTokenMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenBurn = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenBurn = Build();
        }
    }
}