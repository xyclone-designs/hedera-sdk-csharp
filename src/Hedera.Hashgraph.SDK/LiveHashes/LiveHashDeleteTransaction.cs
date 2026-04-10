// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.LiveHashes
{
    /// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public sealed class LiveHashDeleteTransaction : Transaction<LiveHashDeleteTransaction>
    {
        /// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:LiveHashDeleteTransaction"]/*' />
        public LiveHashDeleteTransaction() { }
        /// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:LiveHashDeleteTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
        internal LiveHashDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
        public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_2"]/*' />
		public byte[] Hash { get; set { RequireNotFrozen(); field = value.CopyArray(); } } = [];
        /// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:InitFromTransactionBody"]/*' />
        void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoDeleteLiveHash;
            if (body.AccountOfLiveHash is not null)
            {
                AccountId = AccountId.FromProtobuf(body.AccountOfLiveHash);
            }

            Hash = body.LiveHashToDelete.ToByteArray();
        }

		/// <include file="LiveHashDeleteTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
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

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.deleteLiveHash);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
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

        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}