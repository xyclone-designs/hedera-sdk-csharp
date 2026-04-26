// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

using VerifyXunit;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class PrngTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        private PrngTransaction SpawnTestTransaction()
        {
            return new PrngTransaction()
            {
                NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006") ],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                MaxTransactionFee = Hbar.FromTinybars(100000)
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        private PrngTransaction SpawnTestTransaction2()
        {
            return new PrngTransaction()
            {
                Range = 100,
                NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006") ],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                MaxTransactionFee = Hbar.FromTinybars(100000)
            }
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        public virtual void ShouldSerialize2()
        {
            Verifier.Verify(SpawnTestTransaction2().ToString());
        }
    }
}