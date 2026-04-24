// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.LiveHashes
{
	/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="T:LiveHashAddTransaction"]/*' />
	[Obsolete("Obsolete")]
    public sealed class LiveHashAddTransaction : Transaction<LiveHashAddTransaction>
    {
        /// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:LiveHashAddTransaction"]/*' />
        public LiveHashAddTransaction() { }
        /// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:LiveHashAddTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
        internal LiveHashAddTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
		public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_2"]/*' />
		public byte[] Hash { get; set { RequireNotFrozen(); field = value; } } = [];
		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_3"]/*' />
		public KeyList? Keys { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:RequireNotFrozen_4"]/*' />
		public TimeSpan? Timespan { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:InitFromTransactionBody"]/*' />
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
                Timespan = hashBody.Duration.ToTimeSpan();
            }
        }

		/// <include file="LiveHashAddTransaction.cs.xml" path='docs/member[@name="M:ToProtobuf"]/*' />
		public Proto.Services.CryptoAddLiveHashTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.CryptoAddLiveHashTransactionBody();
            var hashBuilder = new Proto.Services.LiveHash();

            if (AccountId != null)
            {
                hashBuilder.AccountId = AccountId.ToProtobuf();
            }

            hashBuilder.Hash = ByteString.CopyFrom(Hash);

            if (Keys != null)
            {
                hashBuilder.Keys = Keys.ToProtobuf();
            }

            if (Timespan != null)
            {
                hashBuilder.Duration = Timespan.Value.ToProtoDuration();
            }

            builder.LiveHash = hashBuilder;

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            AccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoAddLiveHash = ToProtobuf();
        }
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule LiveHashAddTransaction");
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.addLiveHash);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
