// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
{
    /// <summary>
    /// Grant "Know Your Customer"(KYC) for one account for a single token.
    /// 
    /// This transaction MUST be signed by the `kyc_key` for the token.<br/>
    /// The identified token MUST have a `kyc_key` set to a valid `Key` value.<br/>
    /// The token `kyc_key` MUST NOT be an empty `KeyList`.<br/>
    /// The identified token MUST exist and MUST NOT be deleted.<br/>
    /// The identified account MUST exist and MUST NOT be deleted.<br/>
    /// The identified account MUST have an association to the identified token.<br/>
    /// On success the association between the identified account and the identified
    /// token SHALL be marked as "KYC granted".
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenGrantKycTransaction : Transaction<TokenGrantKycTransaction>
    {
        private TokenId tokenId = null;
        private AccountId accountId = null;
        /// <summary>
        /// Configure.
        /// </summary>
        public TokenGrantKycTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenGrantKycTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenGrantKycTransaction(Proto.TransactionBody txBody) : base(txBody)
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
        /// The identified token SHALL grant "KYC" for the account
        /// identified by the `account` field.<br/>
        /// The identified token MUST be associated to the account identified
        /// by the `account` field.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenGrantKycTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public virtual AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// An account identifier.
        /// <p>
        /// The token identified by the `token` field SHALL grant "KYC" for the
        /// identified account.<br/>
        /// This account MUST be associated to the token identified
        /// by the `token` field.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenGrantKycTransaction SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenGrantKyc;

            if (body.Token is not null)
            {
                tokenId = TokenId.FromProtobuf(body.Token);
            }

            if (body.Account is not null)
            {
                accountId = AccountId.FromProtobuf(body.Account);
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenGrantKycTransactionBody}</returns>
        public virtual Proto.TokenGrantKycTransactionBody Build()
        {
            var builder = new Proto.TokenGrantKycTransactionBody();

            if (tokenId != null)
            {
                builder.Token = tokenId.ToProtobuf();
            }

            if (accountId != null)
            {
                builder.Account = accountId.ToProtobuf();
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetFreezeTokenAccountMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenGrantKyc = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenGrantKyc = Build();
        }
    }
}