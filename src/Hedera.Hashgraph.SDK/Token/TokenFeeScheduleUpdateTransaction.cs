// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="T:TokenFeeScheduleUpdateTransaction"]/*' />
    public class TokenFeeScheduleUpdateTransaction : Transaction<TokenFeeScheduleUpdateTransaction>
    {
        /// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.#ctor"]/*' />
        public TokenFeeScheduleUpdateTransaction() { }
		/// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TokenFeeScheduleUpdateTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TokenFeeScheduleUpdateTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.RequireNotFrozen"]/*' />
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.DeepCloneList(field)"]/*' />
		public virtual IList<CustomFee> CustomFees 
        { 
            get => CustomFee.DeepCloneList(field);
			set 
            { 
                RequireNotFrozen(); 
                field = CustomFee.DeepCloneList(value); 
            } 
        } = [];

        /// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.InitFromTransactionBody"]/*' />
        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenFeeScheduleUpdate;

            TokenId = TokenId.FromProtobuf(body.TokenId);

			foreach (var fee in body.CustomFees)
            {
                CustomFees.Add(CustomFee.FromProtobuf(fee));
            }
        }

        /// <include file="TokenFeeScheduleUpdateTransaction.cs.xml" path='docs/member[@name="M:TokenFeeScheduleUpdateTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.TokenFeeScheduleUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.TokenFeeScheduleUpdateTransactionBody();

            if (TokenId != null)
            {
                builder.TokenId = TokenId.ToProtobuf();
            }

            builder.CustomFees.Clear();

            foreach (var fee in CustomFees)
            {
                builder.CustomFees.Add(fee.ToProtobuf());
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);

			foreach (CustomFee fee in CustomFees)
				fee.ValidateChecksums(client);
		}
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenFeeScheduleUpdate = ToProtobuf();
        }
		public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule TokenFeeScheduleUpdateTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.TokenService.TokenServiceClient.updateTokenFeeSchedule);

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
