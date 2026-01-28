// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.LiveHash
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated
    /// This transaction is obsolete, not supported, and SHALL fail with a
    /// pre-check result of `NOT_SUPPORTED`.
    /// 
    /// Delete a specific live hash associated to a given account.
    /// This transaction MUST be signed by either the key of the associated account,
    /// or at least one of the keys listed in the live hash.
    /// ### Block Stream Effects
    /// None
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class LiveHashDeleteTransaction : Transaction<LiveHashDeleteTransaction>
    {
        private AccountId accountId = null;
        private byte[] hash = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveHashDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        LiveHashDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
        /// The account owning the livehash
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public LiveHashDeleteTransaction SetAccountId(AccountId accountId)
        {
            ArgumentNullException.ThrowIfNull(accountId);
            RequireNotFrozen();
            accountId = accountId;
            return this;
        }

        /// <summary>
        /// Extract the hash.
        /// </summary>
        /// <returns>                         the hash</returns>
        public ByteString GetHash()
        {
            return ByteString.CopyFrom(hash);
        }

        /// <summary>
        /// The SHA-384 livehash to delete from the account
        /// </summary>
        /// <param name="hash">The array of bytes to be set as hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashDeleteTransaction SetHash(byte[] hash)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hash);
            hash = hash.CopyArray();
            return this;
        }

        /// <summary>
        /// The SHA-384 livehash to delete from the account
        /// </summary>
        /// <param name="hash">The array of bytes to be set as hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashDeleteTransaction SetHash(ByteString hash)
        {
            ArgumentNullException.ThrowIfNull(hash);
            return SetHash(hash.ToByteArray());
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.CryptoDeleteLiveHash;
            if (body.AccountOfLiveHash is not null)
            {
                accountId = AccountId.FromProtobuf(body.AccountOfLiveHash);
            }

            hash = body.LiveHashToDelete.ToByteArray();
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoAddLiveHashTransactionBody}</returns>
        Proto.CryptoDeleteLiveHashTransactionBody Build()
        {
            var builder = new Proto.CryptoDeleteLiveHashTransactionBody();

            if (accountId != null)
            {
                builder.AccountOfLiveHash = accountId.ToProtobuf();
            }

            builder.LiveHashToDelete = ByteString.CopyFrom(hash);

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.DeleteLiveHashMethod;
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDeleteLiveHash = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashDeleteTransaction");
        }
    }
}