// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hedera.Hashgraph.SDK.Transactions.LiveHash
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated
    /// This transaction is obsolete, not supported, and SHALL fail with a
    /// pre-check result of `NOT_SUPPORTED`.
    /// 
    /// A Live Hash value associating some item of content to an account.
    /// This message represents a desired entry in the ledger for a SHA-384
    /// hash of some content, an associated specific account, a list of authorized
    /// keys, and a duration the live hash is "valid".
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class LiveHashAddTransaction : Transaction<LiveHashAddTransaction>
    {
        private AccountId accountId = null;
        private byte[] hash = [];
        private KeyList keys = null;
        private Duration duration = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveHashAddTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        LiveHashAddTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
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
        /// The account to which the livehash is attached
        /// </summary>
        /// <param name="accountId">The AccountId to be set</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetAccountId(AccountId accountId)
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
        /// The SHA-384 hash of a credential or certificate.
        /// </summary>
        /// <param name="hash">The array of bytes to be set as the hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetHash(byte[] hash)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(hash);
            hash = hash.CopyArray();
            return this;
        }

        /// <summary>
        /// The SHA-384 hash of a credential or certificate.
        /// </summary>
        /// <param name="hash">The array of bytes to be set as the hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetHash(ByteString hash)
        {
            ArgumentNullException.ThrowIfNull(hash);
            return SetHash(hash.ToByteArray());
        }

        /// <summary>
        /// Extract the key / key list.
        /// </summary>
        /// <returns>                         the key / key list</returns>
        public Collection<Key> GetKeys()
        {
            return keys != null ? [keys] : null;
        }

        /// <summary>
        /// A list of keys (primitive or threshold), all of which must sign to attach the livehash to an
        /// account, and any one of which can later delete it.
        /// </summary>
        /// <param name="keys">The Key or Keys to be set</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetKeys(params Key[] keys)
        {
            RequireNotFrozen();
            this.keys = [..keys];
            return this;
        }

        /// <summary>
        /// Extract the duration.
        /// </summary>
        /// <returns>                         the duration</returns>
        public Duration GetDuration()
        {
            return duration;
        }

        /// <summary>
        /// The duration for which the livehash will remain valid
        /// </summary>
        /// <param name="duration">The Duration to be set</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetDuration(Duration duration)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(duration);
            duration = duration;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoAddLiveHash;
            var hashBody = body.LiveHash;

            if (hashBody.AccountId is not null)
            {
                accountId = AccountId.FromProtobuf(hashBody.AccountId);
            }

            hash = hashBody.Hash.ToByteArray();

            if (hashBody.Keys is not null)
            {
                keys = KeyList.FromProtobuf(hashBody.Keys, null);
            }

            if (hashBody.Duration is not null)
            {
                duration = Utils.DurationConverter.FromProtobuf(hashBody.Duration);
            }
        }

		/// <summary>
		/// Build the correct transaction body.
		/// </summary>
		/// <returns>{@link Proto.CryptoAddLiveHashTransactionBody}</returns>
		public Proto.CryptoAddLiveHashTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoAddLiveHashTransactionBody();
            var hashBuilder = new Proto.LiveHash();
            if (accountId != null)
            {
                hashBuilder.AccountId = accountId.ToProtobuf();
            }

            hashBuilder.Hash = ByteString.CopyFrom(hash);

            if (keys != null)
            {
                hashBuilder.Keys = keys.ToProtobuf();
            }

            if (duration != null)
            {
                hashBuilder.Duration = Utils.DurationConverter.ToProtobuf(duration);
            }

            builder.LiveHash = hashBuilder;

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (accountId != null)
            {
                accountId.ValidateChecksum(client);
            }
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.AddLiveHashMethod;
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoAddLiveHash = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashAddTransaction");
        }
    }
}