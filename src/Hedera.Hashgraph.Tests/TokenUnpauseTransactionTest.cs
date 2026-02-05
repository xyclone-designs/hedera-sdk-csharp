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
    public class TokenUnpauseTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual TokenUnpauseTransaction SpawnTestTransaction()
        {
            return new TokenUnpauseTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetTokenId(testTokenId).SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenUnpauseTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenUnpause(TokenUnpauseTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenUnpauseTransaction>(tx);
        }

        virtual void ConstructTokenUnpauseTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenUnpauseTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).Build();
            var tx = TransactionBody.NewBuilder().SetTokenUnpause(transactionBody).Build();
            var tokenUnpauseTransaction = new TokenUnpauseTransaction(tx);
            Assert.Equal(tokenUnpauseTransaction.GetTokenId(), testTokenId);
        }

        virtual void GetSetTokenId()
        {
            var tokenUnpauseTransaction = new TokenUnpauseTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenUnpauseTransaction.GetTokenId(), testTokenId);
        }

        virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }
    }
}