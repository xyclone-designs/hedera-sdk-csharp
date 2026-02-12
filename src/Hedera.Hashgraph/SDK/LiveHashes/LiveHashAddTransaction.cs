// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.LiveHashes
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveHashAddTransaction() { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        internal LiveHashAddTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// The account to which the livehash is attached
		/// </summary>
		/// <param name="accountId">The AccountId to be set</param>
		/// <returns>{@code this}</returns>
		public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The SHA-384 hash of a credential or certificate.
		/// </summary>
		public byte[] Hash { get; set { RequireNotFrozen(); field = value; } } = [];
		/// <summary>
		/// A list of keys (primitive or threshold), all of which must sign to attach the livehash to an
		/// account, and any one of which can later delete it.
		/// </summary>
		/// <param name="keys">The Key or Keys to be set</param>
		/// <returns>{@code this}</returns>
		public KeyList? Keys { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The duration for which the livehash will remain valid
		/// </summary>
		/// <param name="duration">The Duration to be set</param>
		/// <returns>{@code this}</returns>
		public Duration? Duration { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoAddLiveHash;
            var hashBody = body.LiveHash;

            if (hashBody.AccountId is not null)
            {
                AccountId = AccountId.FromProtobuf(hashBody.AccountId);
            }

            Hash = hashBody.Hash.ToByteArray();

            if (hashBody.Keys is not null)
            {
                Keys = KeyList.FromProtobuf(hashBody.Keys, null);
            }

            if (hashBody.Duration is not null)
            {
                Duration = Utils.DurationConverter.FromProtobuf(hashBody.Duration);
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

            if (AccountId != null)
            {
                hashBuilder.AccountId = AccountId.ToProtobuf();
            }

            hashBuilder.Hash = ByteString.CopyFrom(Hash);

            if (Keys != null)
            {
                hashBuilder.Keys = Keys.ToProtobuf();
            }

            if (Duration != null)
            {
                hashBuilder.Duration = Utils.DurationConverter.ToProtobuf(Duration);
            }

            builder.LiveHash = hashBuilder;

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            AccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoAddLiveHash = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashAddTransaction");
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.addLiveHash);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}