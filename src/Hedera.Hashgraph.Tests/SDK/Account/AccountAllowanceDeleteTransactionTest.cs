// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountAllowanceDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual AccountAllowanceDeleteTransaction SpawnTestTransaction()
        {
            var ownerId = AccountId.FromString("5.6.7");
            return new AccountAllowanceDeleteTransaction()
                .SetNodeAccountIds([  ])
                .SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).DeleteAllHbarAllowances(ownerId).DeleteAllTokenAllowances(TokenId.FromString("2.2.2"), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(123), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(456), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("8.8.8").Nft(456), ownerId).DeleteAllTokenNftAllowances(TokenId.FromString("4.4.4").Nft(789), ownerId).SetMaxTransactionFee(Hbar.FromTinybars(100000)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = AccountAllowanceDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountAllowanceDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody 
            {
				CryptoDeleteAllowance = new Proto.CryptoDeleteAllowanceTransactionBody()
			};
            var tx = Transaction<T>.FromScheduledTransaction(transactionBody);

            Assert.IsType<AccountAllowanceDeleteTransaction>(tx);
        }
    }
}