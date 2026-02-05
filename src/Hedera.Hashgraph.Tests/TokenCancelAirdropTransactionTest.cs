// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
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
    class TokenCancelAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        private TokenCancelAirdropTransaction transaction;
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private TokenCancelAirdropTransaction SpawnTestTransaction()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            return new TokenCancelAirdropTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetPendingAirdropIds(pendingAirdropIds).Freeze().Sign(privateKey);
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenCancelAirdropTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void SetUp()
        {
            transaction = new TokenCancelAirdropTransaction();
        }

        virtual void TestConstructorSetsDefaultMaxTransactionFee()
        {
            Assertions.AssertEquals(Hbar.From(1), transaction.GetDefaultMaxTransactionFee());
        }

        virtual void TestGetAndSetPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            transaction.SetPendingAirdropIds(pendingAirdropIds);
            Assertions.AssertEquals(pendingAirdropIds, transaction.GetPendingAirdropIds());
        }

        virtual void TestSetPendingAirdropIdsNullThrowsException()
        {
            Assertions.await Assert.ThrowsAsync<NullReferenceException>(() => transaction.SetPendingAirdropIds(null));
        }

        virtual void TestClearPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            PendingAirdropId pendingAirdropId = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            pendingAirdropIds.Add(pendingAirdropId);
            transaction.SetPendingAirdropIds(pendingAirdropIds);
            transaction.ClearPendingAirdropIds();
            Assertions.AssertTrue(transaction.GetPendingAirdropIds().IsEmpty());
        }

        virtual void TestAddAllPendingAirdrops()
        {
            PendingAirdropId pendingAirdropId1 = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            PendingAirdropId pendingAirdropId2 = new PendingAirdropId(new AccountId(0, 0, 458), new AccountId(0, 0, 459), new TokenId(0, 0, 123));
            transaction.AddPendingAirdrop(pendingAirdropId1);
            transaction.AddPendingAirdrop(pendingAirdropId2);
            Assertions.AssertEquals(2, transaction.GetPendingAirdropIds().Count);
            Assertions.AssertTrue(transaction.GetPendingAirdropIds().Contains(pendingAirdropId1));
            Assertions.AssertTrue(transaction.GetPendingAirdropIds().Contains(pendingAirdropId2));
        }

        virtual void TestAddAllPendingAirdropsNullThrowsException()
        {
            Assertions.await Assert.ThrowsAsync<NullReferenceException>(() => transaction.AddPendingAirdrop(null));
        }

        virtual void TestBuildTransactionBody()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123));
            transaction.AddPendingAirdrop(pendingAirdropId);
            TokenCancelAirdropTransactionBody.Builder builder = transaction.Build();
            Assertions.AssertEquals(1, builder.GetPendingAirdropsCount());
            Assertions.AssertEquals(pendingAirdropId.ToProtobuf(), builder.GetPendingAirdrops(0));
        }

        virtual void TestGetMethodDescriptor()
        {
            Assertions.AssertEquals(TokenServiceGrpc.GetCancelAirdropMethod(), transaction.GetMethodDescriptor());
        }

        virtual void TestOnFreeze()
        {
            var bodyBuilder = TransactionBody.NewBuilder();
            transaction.OnFreeze(bodyBuilder);
            Assertions.AssertTrue(bodyBuilder.HasTokenCancelAirdrop());
        }

        virtual void TestOnScheduled()
        {
            SchedulableTransactionBody.Builder scheduled = SchedulableTransactionBody.NewBuilder();
            transaction.OnScheduled(scheduled);
            Assertions.AssertTrue(scheduled.HasTokenCancelAirdrop());
        }
    }
}