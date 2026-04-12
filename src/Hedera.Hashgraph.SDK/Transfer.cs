// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="Transfer.cs.xml" path='docs/member[@name="T:Transfer"]/*' />
    public sealed class Transfer
    {
        public Transfer(AccountId accountId, Hbar amount)
        {
            AccountId = accountId;
            Amount = amount;
        }

		/// <include file="Transfer.cs.xml" path='docs/member[@name="M:Transfer.FromProtobuf(Proto.Services.AccountAmount)"]/*' />
		public static Transfer FromProtobuf(Proto.Services.AccountAmount accountAmount)
		{
			return new Transfer(AccountId.FromProtobuf(accountAmount.AccountID), Hbar.FromTinybars(accountAmount.Amount));
		}

		/// <include file="Transfer.cs.xml" path='docs/member[@name="P:Transfer.Amount"]/*' />
		public Hbar Amount { get; }
		/// <include file="Transfer.cs.xml" path='docs/member[@name="P:Transfer.AccountId"]/*' />
		public AccountId AccountId { get; }

        /// <include file="Transfer.cs.xml" path='docs/member[@name="M:Transfer.ToProtobuf"]/*' />
        public Proto.Services.AccountAmount ToProtobuf()
        {
            return new Proto.Services.AccountAmount
            {
				Amount = Amount.ToTinybars(),
				AccountID = AccountId.ToProtobuf(),
			};
        }
    }
}
