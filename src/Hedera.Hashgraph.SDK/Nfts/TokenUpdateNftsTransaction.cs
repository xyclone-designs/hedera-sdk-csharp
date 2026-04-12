// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="T:TokenUpdateNftsTransaction"]/*' />
	public class TokenUpdateNftsTransaction : Transaction<TokenUpdateNftsTransaction>
    {
        /// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.#ctor"]/*' />
        public TokenUpdateNftsTransaction() { }
		internal TokenUpdateNftsTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenUpdateNftsTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<long> _Serials = [];

		/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.RequireNotFrozen"]/*' />
		public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="T:TokenUpdateNftsTransaction_2"]/*' />
		public virtual ListGuarded<long> Serials
		{
			init; get => field ??= new ListGuarded<long>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}
		/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.RequireNotFrozen_2"]/*' />
		public virtual byte[]? Metadata { get; set { RequireNotFrozen(); field = value; } } = [];

		/// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUpdateNfts;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);

			Serials.ClearAndSet(body.SerialNumbers);

            if (body.Metadata is not null)
                Metadata = body.Metadata.ToByteArray();
        }

        /// <include file="TokenUpdateNftsTransaction.cs.xml" path='docs/member[@name="M:TokenUpdateNftsTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenUpdateNftsTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenUpdateNftsTransactionBody();

            if (TokenId != null)
				builder.Token = TokenId.ToProtobuf();

			foreach (var serial in Serials)
				builder.SerialNumbers.Add(serial);

			if (Metadata != null)
				builder.Metadata = ByteString.CopyFrom(Metadata);

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdateNfts = ToProtobuf();
        }
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdateNfts = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.updateNfts);

			return Proto.Services.TokenService.Descriptor.FindMethodByName(methodname);
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
