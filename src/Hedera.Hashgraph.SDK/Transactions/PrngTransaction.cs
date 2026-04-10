// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;

using System;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="PrngTransaction.cs.xml" path='docs/member[@name="T:PrngTransaction"]/*' />
    public class PrngTransaction : Transaction<PrngTransaction>
    {
		/// <include file="PrngTransaction.cs.xml" path='docs/member[@name="P:PrngTransaction.Range"]/*' />
		public virtual int? Range { get; set; }

        public virtual Proto.UtilPrngTransactionBody ToProtobuf()
        {
            var builder = new Proto.UtilPrngTransactionBody();

            if (Range != null)
				builder.Range = Range.Value;

			return builder;
        }

		public override void ValidateChecksums(Client client) { }
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.UtilPrng = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule RngTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.UtilService.UtilServiceClient.prng);

			return Proto.UtilService.Descriptor.FindMethodByName(methodname);
		}

		public override void OnExecute(Client client)
        {
            throw new NotImplementedException();
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