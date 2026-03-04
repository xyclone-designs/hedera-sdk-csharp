// SPDX-License-Identifier: Apache-2.0
using System;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenGrantKycTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly AccountId testAccountId = AccountId.FromString("6.9.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private TokenGrantKycTransaction SpawnTestTransaction()
        {
            return new TokenGrantKycTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")]

				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				AccountId = testAccountId,
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenGrantKycTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenGrantKycTransaction();
            var tx2 = Transaction.FromBytes<TokenGrantKycTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                TokenGrantKyc = new Proto.TokenGrantKycTransactionBody()
            };
            
            var tx = Transaction.FromScheduledTransaction<TokenGrantKycTransaction>(transactionBody);

            Assert.IsType<TokenGrantKycTransaction>(tx);
        }

        public virtual void ConstructTokenGrantKycTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenGrantKycTransactionBody
            {
				Account = testAccountId.ToProtobuf(),
				Token = testTokenId.ToProtobuf(),
			};
            var tx = new Proto.TransactionBody
            {
				TokenGrantKyc = transactionBody
			};
            var tokenGrantKycTransaction = new TokenGrantKycTransaction(tx);
            
            Assert.Equal(tokenGrantKycTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetAccountId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction
            {
				AccountId = testAccountId
			};
            
            Assert.Equal(tokenGrantKycTransaction.AccountId, testAccountId);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            
            Assert.Throws<InvalidOperationException>(() => tx.AccountId = testAccountId);
        }

        public virtual void GetSetTokenId()
        {
            var tokenGrantKycTransaction = new TokenGrantKycTransaction
            {
				TokenId = testTokenId
			};

            Assert.Equal(tokenGrantKycTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();

            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}