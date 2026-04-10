// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="MaxQueryPaymentExceededException.cs.xml" path='docs/member[@name="T:MaxQueryPaymentExceededException"]/*' />
    public sealed class MaxQueryPaymentExceededException : Exception
    {
        /// <include file="MaxQueryPaymentExceededException.cs.xml" path='docs/member[@name="F:MaxQueryPaymentExceededException.QueryCost"]/*' />
        public readonly Hbar QueryCost;
        /// <include file="MaxQueryPaymentExceededException.cs.xml" path='docs/member[@name="M:MaxQueryPaymentExceededException.#ctor(System.Type,Hbar,Hbar)"]/*' />
        public readonly Hbar MaxQueryPayment;
		/// <include file="MaxQueryPaymentExceededException.cs.xml" path='docs/member[@name="M:MaxQueryPaymentExceededException.#ctor(System.Type,Hbar,Hbar)_2"]/*' />
		internal MaxQueryPaymentExceededException(Type queryType, Hbar cost, Hbar maxQueryPayment) : 
        base(string.Format("cost for {0}, of {1}, without explicit payment is greater than " + "the maximum allowed payment of {2}", queryType.Name, cost, maxQueryPayment))
        {
            QueryCost = cost;
            MaxQueryPayment = maxQueryPayment;
        }
    }
}