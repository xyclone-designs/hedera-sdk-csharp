// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="T:TokenMintTransaction"]/*' />
    public class TokenMintTransaction : Transaction<TokenMintTransaction>
    {
        /// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.#ctor"]/*' />
        public TokenMintTransaction() { }
		/// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenMintTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenMintTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.RequireNotFrozen_2"]/*' />
		public virtual long Amount { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.RequireNotFrozen_3"]/*' />
        public virtual List<byte[]> Metadata { get; set { RequireNotFrozen(); field = [.. value]; } } = [];

		/// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenMint;

            if (body.Token is not null)
            {
                TokenId = TokenId.FromProtobuf(body.Token);
            }

            Amount = (long)body.Amount;

            foreach (var metadata in body.Metadata)
            {
                Metadata.Add(metadata.ToByteArray());
            }
        }

        /// <include file="TokenMintTransaction.cs.xml" path='docs/member[@name="M:TokenMintTransaction.ToProtobuf"]/*' />
        public virtual Proto.TokenMintTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenMintTransactionBody();

            if (TokenId != null)
            {
                builder.Token = TokenId.ToProtobuf();
            }

            builder.Amount = (ulong)Amount;

            foreach (var metadata in Metadata)
            {
                builder.Metadata.Add(ByteString.CopyFrom(metadata));
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenMint = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenMint = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.mintToken);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
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