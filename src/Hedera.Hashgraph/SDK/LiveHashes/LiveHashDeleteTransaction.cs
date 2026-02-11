// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
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
    /// Delete a specific live Hash associated to a given account.
    /// This transaction MUST be signed by either the key of the associated account,
    /// or at least one of the keys listed in the live Hash.
    /// ### Block Stream Effects
    /// None
    /// </remarks>
    [Obsolete("Obsolete")]
    public sealed class LiveHashDeleteTransaction : Transaction<LiveHashDeleteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public LiveHashDeleteTransaction() { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        internal LiveHashDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// The account owning the liveHash
        /// </summary>
        public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// The SHA-384 liveHash to delete from the account
		/// </summary>
		public byte[] Hash { get; set { RequireNotFrozen(); field = value.CopyArray(); } } = [];
        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoDeleteLiveHash;
            if (body.AccountOfLiveHash is not null)
            {
                AccountId = AccountId.FromProtobuf(body.AccountOfLiveHash);
            }

            Hash = body.LiveHashToDelete.ToByteArray();
        }

		/// <summary>
		/// Build the correct transaction body.
		/// </summary>
		/// <returns>{@link Proto.CryptoAddLiveHashTransactionBody}</returns>
		public Proto.CryptoDeleteLiveHashTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoDeleteLiveHashTransactionBody();

            if (AccountId != null)
            {
                builder.AccountOfLiveHash = AccountId.ToProtobuf();
            }

            builder.LiveHashToDelete = ByteString.CopyFrom(Hash);

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            if (AccountId != null)
            {
                AccountId.ValidateChecksum(client);
            }
        }

		public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.DeleteLiveHashMethod;
        }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {

            bodyBuilder.CryptoDeleteLiveHash = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashDeleteTransaction");
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