// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <summary>
    /// Signals that a transaction has failed the pre-check.
    /// <p>
    /// Before a node submits a transaction to the rest of the network,
    /// it attempts some cheap assertions. This process is called the "pre-check".
    /// </summary>
    public class PrecheckStatusException : Exception
    {
        /// <summary>
        /// The status of the failing transaction
        /// </summary>
        public readonly Status status;
        /// <summary>
        /// The ID of the transaction that failed.
        /// <p>
        /// This can be `null` if a query fails pre-check without an
        /// associated payment transaction.
        /// </summary>
        public readonly TransactionId TransactionId;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="status">the status</param>
		/// <param name="transactionId">the transaction id</param>
		internal PrecheckStatusException(Status status, TransactionId transactionId)
        {
            status = status;
            TransactionId = transactionId;
        }

		public override string Message
		{
			get
			{
				var stringBuilder = new StringBuilder();
				if (TransactionId != null)
				{
					stringBuilder.Append("Hedera transaction `").Append(TransactionId).Append("` ");
				}

				stringBuilder.Append("failed pre-check with the status `").Append(status).Append("`");
				return stringBuilder.ToString();
			}
		}
    }
}