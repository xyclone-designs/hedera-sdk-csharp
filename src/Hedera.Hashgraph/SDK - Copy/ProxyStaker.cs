// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions.Account;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Information about a single account that is proxy staking.
    /// </summary>
    public sealed class ProxyStaker
    {
        /// <summary>
        /// The Account ID that is proxy staking.
        /// </summary>
        public readonly AccountId AccountId;
        /// <summary>
        /// The number of hbars that are currently proxy staked.
        /// </summary>
        public readonly Hbar Amount;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="amount">the amount</param>
        private ProxyStaker(AccountId accountId, long amount)
        {
            AccountId = accountId;
            Amount = Hbar.FromTinybars(amount);
        }

        /// <summary>
        /// Create a proxy staker object from a protobuf.
        /// </summary>
        /// <param name="proxyStaker">the protobuf</param>
        /// <returns>                         the new proxy staker object</returns>
        public static ProxyStaker FromProtobuf(Proto.ProxyStaker proxyStaker)
        {
            return new ProxyStaker(AccountId.FromProtobuf(proxyStaker.AccountID), proxyStaker.Amount);
        }
    }
}