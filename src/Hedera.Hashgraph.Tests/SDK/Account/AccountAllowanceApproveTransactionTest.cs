// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountAllowanceApproveTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("1.2.3");
        private static readonly AccountId testOwnerAccountId = AccountId.FromString("4.5.7");
        private static readonly AccountId testSpenderAccountId = AccountId.FromString("8.9.0");
        private static readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual AccountAllowanceApproveTransaction SpawnTestTransaction()
        {
            var ownerId = AccountId.FromString("5.6.7");

            return new AccountAllowanceApproveTransaction()
            {
				MaxTransactionFee = Hbar.FromTinybars(100000),
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
			}
                .AddHbarAllowance(AccountId.FromString("1.1.1"), new Hbar(3))
                .AddTokenAllowance(TokenId.FromString("2.2.2"), AccountId.FromString("3.3.3"), 6)
                .AddTokenNftAllowance(TokenId.FromString("4.4.4").Nft(123), AccountId.FromString("5.5.5"))
                .AddTokenNftAllowance(TokenId.FromString("4.4.4").Nft(456), AccountId.FromString("5.5.5"))
                .AddTokenNftAllowance(TokenId.FromString("8.8.8").Nft(456), AccountId.FromString("5.5.5"))
                .AddTokenNftAllowance(TokenId.FromString("4.4.4").Nft(789), AccountId.FromString("9.9.9"))
                .AddAllTokenNftAllowance(TokenId.FromString("6.6.6"), AccountId.FromString("7.7.7"))
                
                .ApproveHbarAllowance(ownerId, AccountId.FromString("1.1.1"), new Hbar(3))
                .ApproveTokenAllowance(TokenId.FromString("2.2.2"), ownerId, AccountId.FromString("3.3.3"), 6)
                .ApproveTokenNftAllowance(TokenId.FromString("4.4.4").Nft(123), ownerId, AccountId.FromString("5.5.5"))
                .ApproveTokenNftAllowance(TokenId.FromString("4.4.4").Nft(456), ownerId, AccountId.FromString("5.5.5"))
                .ApproveTokenNftAllowance(TokenId.FromString("8.8.8").Nft(456), ownerId, AccountId.FromString("5.5.5"))
                .ApproveTokenNftAllowance(TokenId.FromString("4.4.4").Nft(789), ownerId, AccountId.FromString("9.9.9"))
                .ApproveTokenNftAllowanceAllSerials(TokenId.FromString("6.6.6"), ownerId, AccountId.FromString("7.7.7"))
                .Freeze()
                .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = AccountAllowanceApproveTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        public virtual void PropertiesTest()
        {
            var tx = SpawnTestTransaction();
            Assert.NotEmpty(tx.GetHbarAllowances());
            Assert.NotEmpty(tx.GetHbarApprovals());
            Assert.NotEmpty(tx.GetTokenAllowances());
            Assert.NotEmpty(tx.GetTokenApprovals());
            Assert.NotEmpty(tx.GetTokenNftAllowances());
            Assert.NotEmpty(tx.GetTokenNftApprovals());
        }
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new AccountAllowanceApproveTransaction();
            var tx2 = AccountAllowanceApproveTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                CryptoApproveAllowance = new Proto.CryptoApproveAllowanceTransactionBody()
            };
            var tx = Transaction<T>.FromScheduledTransaction(transactionBody);

            Assert.IsType<AccountAllowanceApproveTransaction>(tx);
        }
        public virtual void DeleteNftAllowanceAllSerials()
        {
            var accountAllowanceApproveTransaction = new AccountAllowanceApproveTransaction().DeleteTokenNftAllowanceAllSerials(testTokenId, testOwnerAccountId, testSpenderAccountId);

            Assert.Equal(accountAllowanceApproveTransaction.GetTokenNftApprovals().Count, 1);
            Assert.Equal(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].TokenId, testTokenId);
            Assert.Equal(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].OwnerAccountId, testOwnerAccountId);
            Assert.Equal(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].SpenderAccountId, testSpenderAccountId);
            Assert.True(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].SerialNumbers.Count == 0);
            Assert.False(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].AllSerials);
            Assert.Null(accountAllowanceApproveTransaction.GetTokenNftApprovals()[0].DelegatingSpender);
        }
        public virtual void DeleteNftAllowanceAllSerialsFrozen()
        {
            var tx = SpawnTestTransaction();

            Assert.Throws<InvalidOperationException>(() => tx.DeleteTokenNftAllowanceAllSerials(testTokenId, testOwnerAccountId, testSpenderAccountId));
        }
    }
}