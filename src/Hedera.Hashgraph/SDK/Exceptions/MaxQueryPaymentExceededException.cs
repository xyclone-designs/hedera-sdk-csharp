// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;

using System;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Signals that a query will cost more than a pre-configured maximum payment amount.
    /// </summary>
    public sealed class MaxQueryPaymentExceededException : Exception
    {
        /// <summary>
        /// The cost of the query that was attempted as returned by {@link Query#getCost(Client)}.
        /// </summary>
        public readonly Hbar QueryCost;
        /// <summary>
        /// The limit for a single automatic query payment, set by
        /// {@link Client#setDefaultMaxQueryPayment(Hbar)} (Hbar)} or {@link Query#setMaxQueryPayment(Hbar)}.
        /// </summary>
        public readonly Hbar MaxQueryPayment;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="builder">the query builder object</param>
		/// <param name="cost">the query cost</param>
		/// <param name="maxQueryPayment">the maximum query payment</param>
		internal MaxQueryPaymentExceededException(Type queryType, Hbar cost, Hbar maxQueryPayment) : 
        base(string.Format("cost for {0}, of {1}, without explicit payment is greater than " + "the maximum allowed payment of {2}", queryType.Name, cost, maxQueryPayment))
        {
            QueryCost = cost;
            MaxQueryPayment = maxQueryPayment;
        }
    }
}