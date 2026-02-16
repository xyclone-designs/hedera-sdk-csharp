// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    class TokenClaimAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private TokenClaimAirdropTransaction transaction;
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private TokenClaimAirdropTransaction SpawnTestTransaction()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            return new TokenClaimAirdropTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetMaxTransactionFee(Hbar.FromTinybars(100000)).SetPendingAirdropIds(pendingAirdropIds).Freeze().Sign(privateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenClaimAirdropTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void SetUp()
        {
            transaction = new TokenClaimAirdropTransaction();
        }

        public virtual void TestConstructorSetsDefaultMaxTransactionFee()
        {
            Assertions.Assert.Equal(Hbar.From(1), transaction.GetDefaultMaxTransactionFee());
        }

        public virtual void TestGetAndSetPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            transaction.SetPendingAirdropIds(pendingAirdropIds);
            Assertions.Assert.Equal(pendingAirdropIds, transaction.GetPendingAirdropIds());
        }

        public virtual void TestSetPendingAirdropIdsNullThrowsException()
        {
            Assertions.Assert.Throws<NullReferenceException>(() => transaction.SetPendingAirdropIds(null));
        }

        public virtual void TestClearPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = new List();
            PendingAirdropId pendingAirdropId = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            pendingAirdropIds.Add(pendingAirdropId);
            transaction.SetPendingAirdropIds(pendingAirdropIds);
            transaction.ClearPendingAirdropIds();
            Assertions.Assert.True(transaction.GetPendingAirdropIds().Count == 0);
        }

        public virtual void TestAddAllPendingAirdrops()
        {
            PendingAirdropId pendingAirdropId1 = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            PendingAirdropId pendingAirdropId2 = new PendingAirdropId(new AccountId(0, 0, 458), new AccountId(0, 0, 459), new TokenId(0, 0, 123));
            transaction.AddPendingAirdrop(pendingAirdropId1);
            transaction.AddPendingAirdrop(pendingAirdropId2);
            Assertions.Assert.Equal(2, transaction.GetPendingAirdropIds().Count);
            Assertions.Assert.True(transaction.GetPendingAirdropIds().Contains(pendingAirdropId1));
            Assertions.Assert.True(transaction.GetPendingAirdropIds().Contains(pendingAirdropId2));
        }

        public virtual void TestAddAllPendingAirdropsNullThrowsException()
        {
            Assertions.Assert.Throws<NullReferenceException>(() => transaction.AddPendingAirdrop(null));
        }

        public virtual void TestBuildTransactionBody()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123));
            transaction.AddPendingAirdrop(pendingAirdropId);
            TokenClaimAirdropTransactionBody builder = transaction.Build();
            Assertions.Assert.Equal(1, builder.GetPendingAirdropsCount());
            Assertions.Assert.Equal(pendingAirdropId.ToProtobuf(), builder.GetPendingAirdrops(0));
        }

        public virtual void TestGetMethodDescriptor()
        {
            Assertions.Assert.Equal(TokenServiceGrpc.GetClaimAirdropMethod(), transaction.GetMethodDescriptor());
        }

        public virtual void TestOnFreeze()
        {
            var bodyBuilder = TransactionBody.NewBuilder();
            transaction.OnFreeze(bodyBuilder);
            Assertions.Assert.True(bodyBuilder.HasTokenClaimAirdrop());
        }

        public virtual void TestOnScheduled()
        {
            SchedulableTransactionBody scheduled = SchedulableTransactionBody.NewBuilder();
            transaction.OnScheduled(scheduled);
            Assertions.Assert.True(scheduled.HasTokenClaimAirdrop());
        }
    }
}