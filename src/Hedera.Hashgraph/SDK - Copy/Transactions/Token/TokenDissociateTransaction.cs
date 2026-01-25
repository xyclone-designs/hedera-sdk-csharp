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
    /// Dissociate an account from one or more HTS tokens.
    /// 
    /// If the identified account is not found,
    /// the transaction SHALL return `INVALID_ACCOUNT_ID`.<br/>
    /// If the identified account has been deleted,
    /// the transaction SHALL return `ACCOUNT_DELETED`.<br/>
    /// If any of the identified tokens is not found,
    /// the transaction SHALL return `INVALID_TOKEN_REF`.<br/>
    /// If any of the identified tokens has been deleted,
    /// the transaction SHALL return `TOKEN_WAS_DELETED`.<br/>
    /// If an association does not exist for any of the identified tokens,
    /// the transaction SHALL return `TOKEN_NOT_ASSOCIATED_TO_ACCOUNT`.<br/>
    /// If the identified account has a nonzero balance for any of the identified
    /// tokens, and that token is neither deleted nor expired, the
    /// transaction SHALL return `TRANSACTION_REQUIRES_ZERO_TOKEN_BALANCES`.<br/>
    /// If one of the identified tokens is a fungible/common token that is expired,
    /// the account MAY disassociate from that token, even if that token balance is
    /// not zero for that account.<br/>
    /// If one of the identified tokens is a non-fungible/unique token that is
    /// expired, the account MUST NOT disassociate if that account holds any
    /// individual NFT of that token. In this situation the transaction SHALL
    /// return `TRANSACTION_REQUIRED_ZERO_TOKEN_BALANCES`.<br/>
    /// The identified account MUST sign this transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenDissociateTransaction : Transaction<TokenDissociateTransaction>
    {
        private AccountId accountId = null;
        private IList<TokenId> tokenIds = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenDissociateTransaction()
        {
            defaultMaxTransactionFee = new Hbar(5);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenDissociateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenDissociateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
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
        /// The identified account SHALL be dissociated from each of the
        /// tokens identified in the `tokens` field.
        /// This field is REQUIRED and MUST be a valid account identifier.<br/>
        /// The identified account MUST exist in state.<br/>
        /// The identified account MUST NOT be deleted.<br/>
        /// The identified account MUST NOT be expired.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenDissociateTransaction SetAccountId(AccountId accountId)
        {
            Objects.RequireNonNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the list of token id's.
        /// </summary>
        /// <returns>                         the list of token id's</returns>
        public virtual IList<TokenId> GetTokenIds()
        {
            return new List(tokenIds);
        }

        /// <summary>
        /// A list of token identifiers.
        /// <p>
        /// Each token identified in this list SHALL be dissociated from
        /// the account identified in the `account` field.<br/>
        /// This list MUST NOT be empty.
        /// Each entry in this list MUST be a valid token identifier.<br/>
        /// Each entry in this list MUST be currently associated to the
        /// account identified in `account`.<br/>
        /// Entries in this list MAY be expired, if the token type is
        /// fungible/common.<br/>
        /// Each entry in this list MUST NOT be deleted.
        /// </summary>
        /// <param name="tokens">the list of token id's.</param>
        /// <returns>{@code this}</returns>
        public virtual TokenDissociateTransaction SetTokenIds(IList<TokenId> tokens)
        {
            RequireNotFrozen();
            tokenIds = new List(tokens);
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetTokenDissociate();
            if (body.HasAccount())
            {
                accountId = AccountId.FromProtobuf(body.GetAccount());
            }

            foreach (var token in body.GetTokensList())
            {
                tokenIds.Add(TokenId.FromProtobuf(token));
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenDissociateTransactionBody}</returns>
        virtual TokenDissociateTransactionBody.Builder Build()
        {
            var builder = TokenDissociateTransactionBody.NewBuilder();
            if (accountId != null)
            {
                builder.SetAccount(accountId.ToProtobuf());
            }

            foreach (var token in tokenIds)
            {
                if (token != null)
                {
                    builder.AddTokens(token.ToProtobuf());
                }
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }

            foreach (var token in tokenIds)
            {
                if (token != null)
                {
                    token.ValidateChecksum(client);
                }
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetDissociateTokensMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetTokenDissociate(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetTokenDissociate(Build());
        }
    }
}