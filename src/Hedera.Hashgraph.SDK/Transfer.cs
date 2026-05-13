// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;

namespace Hedera.Hashgraph.SDK
{
    /// <include file="Transfer.cs.xml" path='docs/member[@name="T:Transfer"]' />
    public sealed class Transfer(AccountId accountId, Hbar amount)
    {
        /// <include file="Transfer.cs.xml" path='docs/member[@name="M:Transfer.FromProtobuf(Proto.Services.AccountAmount)"]' />
        public static Transfer FromProtobuf(Proto.Services.AccountAmount accountAmount)
		{
			return new Transfer(AccountId.FromProtobuf(accountAmount.AccountId), Hbar.FromTinybars(accountAmount.Amount));
		}

        /// <include file="Transfer.cs.xml" path='docs/member[@name="P:Transfer.Amount"]' />
        public Hbar Amount { get; } = amount;
        /// <include file="Transfer.cs.xml" path='docs/member[@name="P:Transfer.AccountId"]' />
        public AccountId AccountId { get; } = accountId;

        /// <include file="Transfer.cs.xml" path='docs/member[@name="M:Transfer.ToProtobuf"]' />
        public Proto.Services.AccountAmount ToProtobuf()
        {
            return new Proto.Services.AccountAmount
            {
				Amount = Amount.ToTinybars(),
				AccountId = AccountId.ToProtobuf(),
			};
        }
    }
}
