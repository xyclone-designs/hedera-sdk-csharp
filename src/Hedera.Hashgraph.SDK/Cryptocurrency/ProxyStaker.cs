// SPDX-License-Identifier: Apache-2.0

// SPDX-License-Identifier: Apache-2.0
namespace Hedera.Hashgraph.SDK.Cryptocurrency
{
    /// <include file="ProxyStaker.cs.xml" path='docs/member[@name="T:ProxyStaker"]/*' />
    public sealed class ProxyStaker
    {
        /// <include file="ProxyStaker.cs.xml" path='docs/member[@name="M:ProxyStaker.#ctor(AccountId,System.Int64)"]/*' />
        public readonly AccountId AccountId;
        /// <include file="ProxyStaker.cs.xml" path='docs/member[@name="M:ProxyStaker.#ctor(AccountId,System.Int64)_2"]/*' />
        public readonly Hbar Amount;
        /// <include file="ProxyStaker.cs.xml" path='docs/member[@name="M:ProxyStaker.#ctor(AccountId,System.Int64)_3"]/*' />
        private ProxyStaker(AccountId accountId, long amount)
        {
            AccountId = accountId;
            Amount = Hbar.FromTinybars(amount);
        }

        /// <include file="ProxyStaker.cs.xml" path='docs/member[@name="M:ProxyStaker.FromProtobuf(Proto.Services.ProxyStaker)"]/*' />
        public static ProxyStaker FromProtobuf(Proto.Services.ProxyStaker proxyStaker)
        {
            return new ProxyStaker(AccountId.FromProtobuf(proxyStaker.AccountId), proxyStaker.Amount);
        }
    }
}
