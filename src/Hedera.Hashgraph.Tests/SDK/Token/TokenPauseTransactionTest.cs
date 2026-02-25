// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;

using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenPauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual TokenPauseTransaction SpawnTestTransaction()
        {
            return new TokenPauseTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TokenId = testTokenId,
				MaxTransactionFee = new Hbar(1),
			}
            .Freeze()
            .Sign(unusedPrivateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenPauseTransaction();
            var tx2 = Transaction.FromBytes<TokenPauseTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenPause = new Proto.TokenPauseTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<TokenPauseTransaction>(transactionBody);

            Assert.IsType<TokenPauseTransaction>(tx);
        }

        public virtual void ConstructTokenPauseTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenPauseTransactionBody
            {
				Token = testTokenId.ToProtobuf()
			};
            var tx = new Proto.TransactionBody
            {
				TokenPause = transactionBody
			};
            var tokenPauseTransaction = new TokenPauseTransaction(tx);

            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenId()
        {
            var tokenPauseTransaction = new TokenPauseTransaction
            {
				TokenId = testTokenId
			};
            Assert.Equal(tokenPauseTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }
    }
}