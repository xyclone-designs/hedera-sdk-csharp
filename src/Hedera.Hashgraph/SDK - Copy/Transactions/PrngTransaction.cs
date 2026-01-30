// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// Random Number Generator Transaction.
    /// </summary>
    public class PrngTransaction : Transaction<PrngTransaction>
    {
        /// <summary>
        /// If provided and is positive, returns a 32-bit pseudorandom number from the given range in the transaction record.
        /// If not set or set to zero, will return a 384-bit pseudorandom data in the record.
        /// </summary>
        private int? range = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public PrngTransaction()
        {
        }

        /// <summary>
        /// Assign the range.
        /// </summary>
        /// <param name="range">if > 0 32 bit else 384 bit</param>
        /// <returns>{@code this}</returns>
        public virtual PrngTransaction SetRange(int range)
        {
            this.range = range;
            return this;
        }

        /// <summary>
        /// Retrieve the range.
        /// </summary>
        /// <returns>                         the range</returns>
        public virtual int GetRange()
        {
            return range;
        }

        public virtual Proto.UtilPrngTransactionBody Build()
        {
            var builder = new Proto.UtilPrngTransactionBody();

            if (range != null)
				builder.Range = range.Value;

			return builder;
        }

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetUtilPrng = Build();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("cannot schedule RngTransaction");
        }
        public override void ValidateChecksums(Client client) { }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return UtilServiceGrpc.GetPrngMethod();
        }
    }
}