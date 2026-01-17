using System;
using System.Text;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Signals that a transaction has failed the pre-check.
	 * <p>
	 * Before a node submits a transaction to the rest of the network,
	 * it attempts some cheap assertions. This process is called the "pre-check".
	 */
	public class PrecheckStatusException : Exception
	{
		/**
		 * Constructor.
		 *
		 * @param status                    the status
		 * @param transactionId             the transaction id
		 */
		public PrecheckStatusException(Status status, TransactionId? transactionId) : base()
		{
			Status = status;
			TransactionId = transactionId;
		}

		/**
		 * The status of the failing transaction
		 */
		public Status Status { get; }

		/**
		 * The ID of the transaction that failed.
		 * <p>
		 * This can be `null` if a query fails pre-check without an
		 * associated payment transaction.
		 */
		public TransactionId? TransactionId { get; }

        public override string Message
		{
			get
			{
				StringBuilder stringBuilder = new ();

				if (TransactionId != null)
					stringBuilder
						.Append("Hedera transaction `")
						.Append(TransactionId)
						.Append("` ");

				stringBuilder
					.Append("failed pre-check with the status `")
					.Append(Status)
					.Append('`');

				return stringBuilder.ToString();
			}
		}
	}
}