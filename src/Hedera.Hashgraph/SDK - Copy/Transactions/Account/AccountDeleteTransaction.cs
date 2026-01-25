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

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// Delete an account.<br/>
    /// This will mark an account deleted, and transfer all tokens to a "sweep"
    /// account.
    /// 
    /// A deleted account SHALL NOT hold a balance in any token type.<br/>
    /// A deleted account SHALL remain in state until it expires.<br/>
    /// Transfers that would increase the balance of a deleted account
    /// SHALL fail.<br/>
    /// A deleted account MAY be subject of a `cryptoUpdate` transaction to extend
    /// its expiration.<br/>
    /// When a deleted account expires it SHALL be removed entirely, and SHALL NOT
    /// be archived.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class AccountDeleteTransaction : Transaction<AccountDeleteTransaction>
    {
        private AccountId accountId = null;
        private AccountId transferAccountId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        AccountDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AccountDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        public AccountId GetAccountId()
        {
            return accountId;
        }

        /// <summary>
        /// An account identifier.
        /// <p>
        /// This account SHALL be deleted if this transaction succeeds.<br/>
        /// This account SHOULD NOT hold any balance other than HBAR.<br/>
        /// If this account _does_ hold balances, the `transferAccountID` value
        /// MUST be set to a valid transfer account.<br/>
        /// This account MUST sign this transaction.<br/>
        /// This field MUST be set to a valid account identifier.
        /// </summary>
        /// <param name="deleteAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountDeleteTransaction SetAccountId(AccountId deleteAccountId)
        {
            Objects.RequireNonNull(deleteAccountId);
            RequireNotFrozen();
            accountId = deleteAccountId;
            return this;
        }

        /// <summary>
        /// Extract the receiving account id.
        /// </summary>
        /// <returns>                         the account id that receives the hbar</returns>
        public AccountId GetTransferAccountId()
        {
            return transferAccountId;
        }

        /// <summary>
        /// An account identifier.
        /// <p>
        /// The identified account SHALL receive all tokens, token balances,
        /// and non-fungible/unique from the deleted account.<br/>
        /// The identified account MUST sign this transaction.<br/>
        /// If not set, the account to be deleted MUST NOT have a balance in any
        /// token, a balance in HBAR, or hold any NFT.
        /// </summary>
        /// <param name="transferAccountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public AccountDeleteTransaction SetTransferAccountId(AccountId transferAccountId)
        {
            RequireNotFrozen();
            Objects.RequireNonNull(transferAccountId);
            transferAccountId = transferAccountId;
            return this;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }

            if (transferAccountId != null)
            {
                transferAccountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetCryptoDeleteMethod();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link CryptoDeleteTransactionBody}</returns>
        CryptoDeleteTransactionBody.Builder Build()
        {
            var builder = CryptoDeleteTransactionBody.NewBuilder();
            if (accountId != null)
            {
                builder.SetDeleteAccountID(accountId.ToProtobuf());
            }

            if (transferAccountId != null)
            {
                builder.SetTransferAccountID(transferAccountId.ToProtobuf());
            }

            return builder;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetCryptoDelete();
            if (body.HasDeleteAccountID())
            {
                accountId = AccountId.FromProtobuf(body.GetDeleteAccountID());
            }

            if (body.HasTransferAccountID())
            {
                transferAccountId = AccountId.FromProtobuf(body.GetTransferAccountID());
            }
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetCryptoDelete(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            scheduled.SetCryptoDelete(Build());
        }
    }
}