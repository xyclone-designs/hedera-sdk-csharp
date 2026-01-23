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
        TokenGrantKycTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
        public virtual TokenId GetTokenId()
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
            Objects.RequireNonNull(tokenId);
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
            Objects.RequireNonNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenGrantKyc();
            if (body.HasToken())
            {
                tokenId = TokenId.FromProtobuf(body.GetToken());
            }

            if (body.HasAccount())
            {
                accountId = AccountId.FromProtobuf(body.GetAccount());
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenGrantKycTransactionBody}</returns>
        virtual TokenGrantKycTransactionBody.Builder Build()
        {
            var builder = TokenGrantKycTransactionBody.NewBuilder();
            if (tokenId != null)
            {
                builder.SetToken(tokenId.ToProtobuf());
            }

            if (accountId != null)
            {
                builder.SetAccount(accountId.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
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

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenGrantKyc(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenGrantKyc(Build());
        }
    }
}