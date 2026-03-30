// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;

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
            IList<PendingAirdropId> pendingAirdropIds = 
            [
				new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)),
			    new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123))
			];
            
            return new TokenClaimAirdropTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				PendingAirdropIds = pendingAirdropIds,
			
            }.Freeze().Sign(privateKey);
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenClaimAirdropTransaction();
            var tx2 = Transaction.FromBytes<TokenClaimAirdropTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void SetUp()
        {
            transaction = new TokenClaimAirdropTransaction();
        }

        public virtual void TestConstructorSetsDefaultMaxTransactionFee()
        {
            Assert.Equal(Hbar.From(1), transaction.DefaultMaxTransactionFee);
        }

        public virtual void TestGetAndSetPendingAirdropIds()
        {
            transaction.PendingAirdropIds =
			[
				new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)),
				new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)),
			];

            Assert.Equal(pendingAirdropIds, transaction.PendingAirdropIds);
        }

        public virtual void TestSetPendingAirdropIdsNullThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => transaction.PendingAirdropIds = null);
        }

        public virtual void TestClearPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = [];
            PendingAirdropId pendingAirdropId = new (new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            pendingAirdropIds.Add(pendingAirdropId);
            transaction.PendingAirdropIds = pendingAirdropIds;
            transaction.PendingAirdropIds.Clear();
            Assert.True(transaction.PendingAirdropIds.Count == 0);
        }

        public virtual void TestAddAllPendingAirdrops()
        {
            PendingAirdropId pendingAirdropId1 = new (new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            PendingAirdropId pendingAirdropId2 = new (new AccountId(0, 0, 458), new AccountId(0, 0, 459), new TokenId(0, 0, 123));
            
            transaction.PendingAirdropIds.Add(pendingAirdropId1);
            transaction.PendingAirdropIds.Add(pendingAirdropId2);

            Assert.Equal(2, transaction.PendingAirdropIds.Count);
            Assert.True(transaction.PendingAirdropIds.Contains(pendingAirdropId1));
            Assert.True(transaction.PendingAirdropIds.Contains(pendingAirdropId2));
        }

        public virtual void TestBuildTransactionBody()
        {
            PendingAirdropId pendingAirdropId = new (new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123));

            transaction.PendingAirdropIds.Add(pendingAirdropId);
            Proto.TokenClaimAirdropTransactionBody builder = transaction;
            
            Assert.Equal(1, builder.PendingAirdrops.Count);
            Assert.Equal(pendingAirdropId.ToProtobuf(), builder.PendingAirdrops[0]);
        }

        public virtual void TestGetMethodDescriptor()
        {
            Assert.Equal(TokenServiceGrpc.ClaimAirdropMethod, transaction.GetMethodDescriptor());
        }

        public virtual void TestOnFreeze()
        {
            var bodyBuilder = new Proto.TransactionBody();
            transaction.OnFreeze(bodyBuilder);

            Assert.True(bodyBuilder.TokenClaimAirdrop is not null);
        }

        public virtual void TestOnScheduled()
        {
            Proto.SchedulableTransactionBody scheduled = new ();
            transaction.OnScheduled(scheduled);
            
            Assert.True(scheduled.TokenClaimAirdrop is not null);
        }
    }
}