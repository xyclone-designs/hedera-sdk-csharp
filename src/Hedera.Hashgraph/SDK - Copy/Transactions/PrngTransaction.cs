// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;

namespace Hedera.Hashgraph.SDK
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
        private int range = null;
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

        public virtual UtilPrngTransactionBody.Builder Build()
        {
            var builder = UtilPrngTransactionBody.NewBuilder();
            if (range != null)
            {
                builder.Range(range);
            }

            return builder;
        }

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.SetUtilPrng(Build());
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