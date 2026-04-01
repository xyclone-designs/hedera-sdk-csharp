// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Airdrops;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenCancelAirdropTransactionTest
    {
        private static readonly PrivateKey privateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private TokenCancelAirdropTransaction transaction;

        private TokenCancelAirdropTransaction SpawnTestTransaction()
        {
            IList<PendingAirdropId> pendingAirdropIds = [];
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            
            return new TokenCancelAirdropTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
				MaxTransactionFee = Hbar.FromTinybars(100000),
				PendingAirdropIds = pendingAirdropIds,
			}
            .Freeze()
            .Sign(privateKey);
        }

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenCancelAirdropTransaction();
            var tx2 = Transaction.FromBytes<TokenCancelAirdropTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void SetUp()
        {
            transaction = new TokenCancelAirdropTransaction();
        }
        [Fact]
        public virtual void TestConstructorSetsDefaultMaxTransactionFee()
        {
            Assert.Equal(Hbar.From(1), transaction.DefaultMaxTransactionFee);
        }
        [Fact]
        public virtual void TestGetAndSetPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = [];
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123)));
            pendingAirdropIds.Add(new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123)));
            transaction.PendingAirdropIds = pendingAirdropIds;
            Assert.Equal(pendingAirdropIds, transaction.PendingAirdropIds);
        }
        [Fact]
        public virtual void TestClearPendingAirdropIds()
        {
            IList<PendingAirdropId> pendingAirdropIds = [];
            PendingAirdropId pendingAirdropId = new (new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            pendingAirdropIds.Add(pendingAirdropId);
            transaction.PendingAirdropIds = pendingAirdropIds;
            transaction.PendingAirdropIds.Clear();
            Assert.True(transaction.PendingAirdropIds.Count == 0);
        }
        [Fact]
        public virtual void TestAddAllPendingAirdrops()
        {
            PendingAirdropId pendingAirdropId1 = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new TokenId(0, 0, 123));
            PendingAirdropId pendingAirdropId2 = new PendingAirdropId(new AccountId(0, 0, 458), new AccountId(0, 0, 459), new TokenId(0, 0, 123));
            
            transaction.PendingAirdropIds.Add(pendingAirdropId1);
            transaction.PendingAirdropIds.Add(pendingAirdropId2);
            
            Assert.Equal(2, transaction.PendingAirdropIds.Count);
            Assert.True(transaction.PendingAirdropIds.Contains(pendingAirdropId1));
            Assert.True(transaction.PendingAirdropIds.Contains(pendingAirdropId2));
        }
        [Fact]
        public virtual void TestAddAllPendingAirdropsNullThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => transaction.PendingAirdropIds.Add(null));
        }
        [Fact]
        public virtual void TestBuildTransactionBody()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(new AccountId(0, 0, 457), new AccountId(0, 0, 456), new NftId(new TokenId(0, 0, 1234), 123));
            transaction.PendingAirdropIds.Add(pendingAirdropId);
            Proto.TokenCancelAirdropTransactionBody builder = transaction.ToProtobuf();

            Assert.Equal(1, builder.PendingAirdrops.Count);
            Assert.Equal(pendingAirdropId.ToProtobuf(), builder.PendingAirdrops[0]);
        }
        [Fact]
        public virtual void TestGetMethodDescriptor()
        {
            //Assert.Equal(TokenServiceGrpc.GetCancelAirdropMethod(), transaction.GetMethodDescriptor());
        }
        [Fact]
        public virtual void TestOnFreeze()
        {
            var bodyBuilder = new Proto.TransactionBody();
            
            transaction.OnFreeze(bodyBuilder);

            Assert.True(bodyBuilder.TokenCancelAirdrop is not null);
        }
        [Fact]
        public virtual void TestOnScheduled()
        {
            Proto.SchedulableTransactionBody scheduled = new ();
            
            transaction.OnScheduled(scheduled);
            
            Assert.True(scheduled.TokenCancelAirdrop is not null);
        }
    }
}