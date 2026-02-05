// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TokenDeleteTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenDeleteTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenDeleteTransaction SpawnTestTransaction()
        {
            return new TokenDeleteTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(TokenId.FromString("1.2.3")).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenDeleteTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenDeletion(TokenDeleteTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenDeleteTransaction>(tx);
        }

        virtual void ConstructTokenDeleteTransaction()
        {
            var transaction = new TokenDeleteTransaction();
            AssertThat(transaction.GetTokenId()).IsNull();
        }

        virtual void ConstructTokenDeleteTransactionFromTransactionBodyProtobuf()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var transactionBody = TokenDeleteTransactionBody.NewBuilder().SetToken(tokenId.ToProtobuf()).Build();
            var txBody = TransactionBody.NewBuilder().SetTokenDeletion(transactionBody).Build();
            var tokenDeleteTransaction = new TokenDeleteTransaction(txBody);
            Assert.Equal(tokenDeleteTransaction.GetTokenId(), tokenId);
        }

        virtual void GetSetTokenId()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var transaction = new TokenDeleteTransaction().SetTokenId(tokenId);
            Assert.Equal(transaction.GetTokenId(), tokenId);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var tokenId = TokenId.FromString("1.2.3");
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenId(tokenId));
        }
    }
}