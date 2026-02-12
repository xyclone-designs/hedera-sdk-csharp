// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Account;

using System;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Random Number Generator Transaction.
    /// </summary>
    public class PrngTransaction : Transaction<PrngTransaction>
    {
		/// <summary>
		/// Assign the range.
		/// If provided and is positive, returns a 32-bit pseudorandom number from the given range in the transaction record.
		/// If not set or set to zero, will return a 384-bit pseudorandom data in the record.
		/// </summary>
		/// <param name="range">if > 0 32 bit else 384 bit</param>
		/// <returns>{@code this}</returns>
		public virtual int? Range { get; set; }

        public virtual Proto.UtilPrngTransactionBody ToProtobuf()
        {
            var builder = new Proto.UtilPrngTransactionBody();

            if (Range != null)
				builder.Range = Range.Value;

			return builder;
        }

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.UtilPrng = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule RngTransaction");
        }
        public override void ValidateChecksums(Client client) { }
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
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}