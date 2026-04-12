// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="T:TokenWipeTransaction"]/*' />
    public class TokenWipeTransaction : Transaction<TokenWipeTransaction>
    {
        /// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.#ctor"]/*' />
        public TokenWipeTransaction() { }
		/// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenWipeTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenWipeTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.RequireNotFrozen"]/*' />
		public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.RequireNotFrozen_2"]/*' />
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.RequireNotFrozen_3"]/*' />
		public virtual ulong Amount { get; set { RequireNotFrozen(); field = value; } }
        /// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.InitFromTransactionBody"]/*' />
        public virtual ListGuarded<long> Serials
		{
			init; get => field ??= new ListGuarded<long>
			{
				OnRequireNotFrozen = RequireNotFrozen
			};
		}

		/// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.InitFromTransactionBody_2"]/*' />
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenWipe;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
                AccountId = AccountId.FromProtobuf(body.Account);

            Amount = body.Amount;
            Serials.ClearAndSet(body.SerialNumbers);
        }

        /// <include file="TokenWipeTransaction.cs.xml" path='docs/member[@name="M:TokenWipeTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenWipeAccountTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenWipeAccountTransactionBody
            {
				Amount = Amount
			};

            if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (AccountId != null)
                builder.Account = AccountId.ToProtobuf();

            foreach (var serial in Serials)
				builder.SerialNumbers.Add(serial);

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
            TokenId?.ValidateChecksum(client);
            AccountId?.ValidateChecksum(client);
        }
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenWipe = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenWipe = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.wipeTokenAccount);

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
