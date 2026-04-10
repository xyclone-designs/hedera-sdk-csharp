// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Exceptions
{
    /// <include file="PrecheckStatusException.cs.xml" path='docs/member[@name="T:PrecheckStatusException"]/*' />
    public class PrecheckStatusException : Exception
    {
        /// <include file="PrecheckStatusException.cs.xml" path='docs/member[@name="M:PrecheckStatusException.#ctor(ResponseStatus,TransactionId)"]/*' />
        public readonly ResponseStatus Status;
        /// <include file="PrecheckStatusException.cs.xml" path='docs/member[@name="M:PrecheckStatusException.#ctor(ResponseStatus,TransactionId)_2"]/*' />
        public readonly TransactionId TransactionId;
		/// <include file="PrecheckStatusException.cs.xml" path='docs/member[@name="M:PrecheckStatusException.#ctor(ResponseStatus,TransactionId)_3"]/*' />
		internal PrecheckStatusException(ResponseStatus status, TransactionId transactionId)
        {
            Status = status;
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

				stringBuilder.Append("failed pre-check with the status `").Append(Status).Append("`");
				return stringBuilder.ToString();
			}
		}
    }
}