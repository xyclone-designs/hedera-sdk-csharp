// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;
using Google.Protobuf.WellKnownTypes;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenBurnTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly long testAmount = 69;
        private static readonly List<long> testSerials = [420];
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerializeFungible()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private TokenBurnTransaction SpawnTestTransaction()
        {
            return new TokenBurnTransaction()
                .SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")))
                .SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)))
                .SetTokenId(testTokenId)
                .SetAmount(testAmount)
                .SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenBurnTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerializeNft()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionNft().ToString()).ToMatchSnapshot();
        }

        private TokenBurnTransaction SpawnTestTransactionNft()
        {
            return new TokenBurnTransaction()
                .SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")))
                .SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)))
                .SetTokenId(testTokenId)
                .SetSerials(testSerials)
                .SetMaxTransactionFee(new Hbar(1)).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenBurnTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = TokenBurnTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder()
                .SetTokenBurn(TokenBurnTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenBurnTransaction>(tx);
        }

        public virtual void ConstructTokenBurnTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenBurnTransactionBody
            {
				Token = testTokenId.ToProtobuf(),
				Amount = testAmount,
			
            }.AddAllSerialNumbers(testSerials).Build();
            var tx = new Proto.TransactionBody
            {
                TokenBurn = transactionBody
            };

            var tokenBurnTransaction = new TokenBurnTransaction(tx);
            
            Assert.Equal(tokenBurnTransaction.TokenId, testTokenId);
            Assert.Equal(tokenBurnTransaction.Amount, testAmount);
            Assert.Equal(tokenBurnTransaction.Serials, testSerials);
        }

        public virtual void GetSetTokenId()
        {
            var tokenBurnTransaction = new TokenBurnTransaction().TokenId = (ulong)testTokenId;
            
            Assert.Equal(tokenBurnTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = (ulong)testTokenId);
        }

        public virtual void GetSetAmount()
        {
            var tokenBurnTransaction = new TokenBurnTransaction { Amount = (ulong)testAmount };

            Assert.Equal(tokenBurnTransaction.Amount, testAmount);
        }

        public virtual void GetSetAmountFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Amount = (ulong)testAmount);
        }

        public virtual void GetSetSerials()
        {
            var tokenBurnTransaction = new TokenBurnTransaction
            {
				Serials = testSerials
			};
            
            Assert.Equal(tokenBurnTransaction.Serials, testSerials);
        }

        public virtual void GetSetSerialsFrozen()
        {
            var tx = SpawnTestTransactionNft();
            
            Assert.Throws<InvalidOperationException>(() => tx.Serials = testSerials);
        }
    }
}