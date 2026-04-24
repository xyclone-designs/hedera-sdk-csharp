// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="T:TokenBurnTransaction"]/*' />
    public class TokenBurnTransaction : Transaction<TokenBurnTransaction>
    {
		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.#ctor"]/*' />
		public TokenBurnTransaction() { }
		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenBurnTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenBurnTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<long> _Serials = [];

		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.RequireNotFrozen"]/*' />
		public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.RequireNotFrozen_2"]/*' />
		public virtual ulong Amount { get; set { RequireNotFrozen(); field = value; } }

		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.ToProtobuf"]/*' />
		public virtual ListGuarded<long> Serials
		{
			init; get => field ??= new ListGuarded<long>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.ToProtobuf_2"]/*' />
		public virtual Proto.Services.TokenBurnTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenBurnTransactionBody
			{
				Amount = Amount
			};

            if (TokenId != null)
				builder.Token = TokenId.ToProtobuf();

			foreach (var serial in Serials)
            {
                builder.SerialNumbers.Add(serial);
            }

            return builder;
        }

        /// <include file="TokenBurnTransaction.cs.xml" path='docs/member[@name="M:TokenBurnTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenBurn;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);

			Amount = body.Amount;
            Serials.ClearAndSet(body.SerialNumbers);
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenBurn = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenBurn = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.burnToken);

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
