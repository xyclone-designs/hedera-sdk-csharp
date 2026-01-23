// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Time;
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
        private byte[] hash = new[]
        {
        };
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
        LiveHashAddTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
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
            Objects.RequireNonNull(accountId);
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
            Objects.RequireNonNull(hash);
            hash = Array.CopyOf(hash, hash.Length);
            return this;
        }

        /// <summary>
        /// The SHA-384 hash of a credential or certificate.
        /// </summary>
        /// <param name="hash">The array of bytes to be set as the hash</param>
        /// <returns>{@code this}</returns>
        public LiveHashAddTransaction SetHash(ByteString hash)
        {
            Objects.RequireNonNull(hash);
            return SetHash(hash.ToByteArray());
        }

        /// <summary>
        /// Extract the key / key list.
        /// </summary>
        /// <returns>                         the key / key list</returns>
        public Collection<Key> GetKeys()
        {
            return keys != null ? Collections.UnmodifiableCollection(keys) : null;
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
            keys = KeyList.Of(keys);
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
            Objects.RequireNonNull(duration);
            duration = duration;
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.GetCryptoAddLiveHash();
            var hashBody = body.GetLiveHash();
            if (hashBody.HasAccountId())
            {
                accountId = AccountId.FromProtobuf(hashBody.GetAccountId());
            }

            hash = hashBody.GetHash().ToByteArray();
            if (hashBody.HasKeys())
            {
                keys = KeyList.FromProtobuf(hashBody.GetKeys(), null);
            }

            if (hashBody.HasDuration())
            {
                duration = Utils.DurationConverter.FromProtobuf(hashBody.GetDuration());
            }
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoAddLiveHashTransactionBody}</returns>
        CryptoAddLiveHashTransactionBody.Builder Build()
        {
            var builder = CryptoAddLiveHashTransactionBody.NewBuilder();
            var hashBuilder = LiveHash.NewBuilder();
            if (accountId != null)
            {
                hashBuilder.SetAccountId(accountId.ToProtobuf());
            }

            hashBuilder.SetHash(ByteString.CopyFrom(hash));
            if (keys != null)
            {
                hashBuilder.SetKeys(keys.ToProtobuf());
            }

            if (duration != null)
            {
                hashBuilder.SetDuration(Utils.DurationConverter.ToProtobuf(duration));
            }

            return builder.SetLiveHash(hashBuilder);
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
            return CryptoServiceGrpc.GetAddLiveHashMethod();
        }

        override void OnFreeze(TransactionBody.Builder bodyBuilder)
        {
            bodyBuilder.SetCryptoAddLiveHash(Build());
        }

        override void OnScheduled(SchedulableTransactionBody.Builder scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashAddTransaction");
        }
    }
}