// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A transfer of Hbar that occurred within a transaction.
    /// <p>
    /// Returned with a {@link TransactionRecord}.
    /// </summary>
    public sealed class Transfer
    {
        Transfer(AccountId accountId, Hbar amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

		/// <summary>
		/// The amount that the account sends (negative) or receives (positive).
		/// </summary>
		public Hbar Amount { get; }
		/// <summary>
		/// The Account ID that sends or receives crypto-currency.
		/// </summary>
		public AccountId AccountId { get; }

		/// <summary>
		/// Create a transfer from a protobuf.
		/// </summary>
		/// <param name="accountAmount">the protobuf</param>
		/// <returns>                         the new transfer</returns>
		public static Transfer FromProtobuf(Proto.AccountAmount accountAmount)
        {
            return new Transfer(AccountId.FromProtobuf(accountAmount.AccountID), Hbar.FromTinybars(accountAmount.Amount));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public Proto.AccountAmount ToProtobuf()
        {
            return new Proto.AccountAmount
            {
				Amount = Amount.ToTinybars(),
				AccountID = AccountId.ToProtobuf(),
			};
        }
    }
}