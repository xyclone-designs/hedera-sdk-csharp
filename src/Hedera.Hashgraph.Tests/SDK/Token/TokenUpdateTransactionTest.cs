// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.HBar;

using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenUpdateTransactionTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly PublicKey testAdminKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey testKycKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        private static readonly PublicKey testFreezeKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e13").GetPublicKey();
        private static readonly PublicKey testWipeKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e14").GetPublicKey();
        private static readonly PublicKey testSupplyKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e15").GetPublicKey();
        private static readonly PublicKey testFeeScheduleKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e16").GetPublicKey();
        private static readonly PublicKey testPauseKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e17").GetPublicKey();
        private static readonly PublicKey testMetadataKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e18").GetPublicKey();
        private static readonly AccountId testTreasuryAccountId = AccountId.FromString("7.7.7");
        private static readonly AccountId testAutoRenewAccountId = AccountId.FromString("8.8.8");
        private static readonly string testTokenName = "test name";
        private static readonly string testTokenSymbol = "test symbol";
        private static readonly string testTokenMemo = "test memo";
        private static readonly TokenId testTokenId = TokenId.FromString("4.2.0");
        private static readonly TimeSpan testAutoRenewPeriod = TimeSpan.FromSeconds(10);
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.UtcNow;
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
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

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenUpdateTransaction();
            var tx2 = Transaction.FromBytes<TokenUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenUpdateTransaction SpawnTestTransaction()
        {
            return new TokenUpdateTransaction()
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)),
				TokenId = testTokenId,
				FeeScheduleKey = testFeeScheduleKey,
				SupplyKey = testSupplyKey,
				AdminKey = testAdminKey,
				AutoRenewAccountId = testAutoRenewAccountId,
				AutoRenewPeriod = testAutoRenewPeriod.ToDuration(),
				FreezeKey = testFreezeKey,
				WipeKey = testWipeKey,
				TokenSymbol = testTokenSymbol,
				KycKey = testKycKey,
				PauseKey = testPauseKey,
				MetadataKey = testMetadataKey,
				ExpirationTime = validStart.ToTimestamp(),
				TreasuryAccountId = testTreasuryAccountId,
				TokenName = testTokenName,
				TokenMemo = testTokenMemo,
				MaxTransactionFee = new Hbar(1),
				TokenMetadata = testMetadata,
				TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
		
            }.Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<TokenUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				TokenUpdate = new Proto.TokenUpdateTransactionBody()
			};

            var tx = Transaction.FromScheduledTransaction<TokenUpdateTransaction>(transactionBody);
            
            Assert.IsType<TokenUpdateTransaction>(tx);
        }

        public virtual void ConstructTokenUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenUpdateTransactionBody
            {
                Token = testTokenId.ToProtobuf(),
                Name = testTokenName,
                Symbol = testTokenSymbol,
                Treasury = testTreasuryAccountId.ToProtobuf(),
                AdminKey = testAdminKey.ToProtobufKey(),
                KycKey = testKycKey.ToProtobufKey(),
                FreezeKey = testFreezeKey.ToProtobufKey(),
                WipeKey = testWipeKey.ToProtobufKey(),
                SupplyKey = testSupplyKey.ToProtobufKey(),
                AutoRenewAccount = testAutoRenewAccountId.ToProtobuf(),
                AutoRenewPeriod = new Proto.Duration
                {
                    Seconds = (long)testAutoRenewPeriod.TotalSeconds
                },
                Expiry = new Proto.Timestamp
                {
                    Seconds = testExpirationTime.ToUnixTimeSeconds()
                },
                Memo = testTokenMemo,
                FeeScheduleKey = testFeeScheduleKey.ToProtobufKey(),
                PauseKey = testPauseKey.ToProtobufKey(),
                MetadataKey = testMetadataKey.ToProtobufKey(),
                Metadata = ByteString.CopyFrom(testMetadata),
                KeyVerificationMode = Proto.TokenKeyValidation.NoValidation
            };
            
            var tx = new Proto.TransactionBody
            {
                TokenUpdate = transactionBody
            };
            var tokenUpdateTransaction = new TokenUpdateTransaction(tx);

            Assert.Equal(tokenUpdateTransaction.TokenId, testTokenId);
            Assert.Equal(tokenUpdateTransaction.TokenName, testTokenName);
            Assert.Equal(tokenUpdateTransaction.TokenSymbol, testTokenSymbol);
            Assert.Equal(tokenUpdateTransaction.TreasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenUpdateTransaction.AdminKey, testAdminKey);
            Assert.Equal(tokenUpdateTransaction.KycKey, testKycKey);
            Assert.Equal(tokenUpdateTransaction.FreezeKey, testFreezeKey);
            Assert.Equal(tokenUpdateTransaction.WipeKey, testWipeKey);
            Assert.Equal(tokenUpdateTransaction.SupplyKey, testSupplyKey);
            Assert.Equal(tokenUpdateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
            Assert.Equal(tokenUpdateTransaction.AutoRenewPeriod.ToTimeSpan().TotalSeconds, testAutoRenewPeriod.TotalSeconds);
            Assert.Equal(tokenUpdateTransaction.ExpirationTime?.ToDateTimeOffset().ToUnixTimeSeconds(), testExpirationTime.ToUnixTimeSeconds());
            Assert.Equal(tokenUpdateTransaction.TokenMemo, testTokenMemo);
            Assert.Equal(tokenUpdateTransaction.FeeScheduleKey, testFeeScheduleKey);
            Assert.Equal(tokenUpdateTransaction.PauseKey, testPauseKey);
            Assert.Equal(tokenUpdateTransaction.MetadataKey, testMetadataKey);
            Assert.Equal(tokenUpdateTransaction.TokenMetadata, testMetadata);
            Assert.Equal(tokenUpdateTransaction.TokenKeyVerificationMode, TokenKeyValidation.NoValidation);
        }

        public virtual void GetSetTokenId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                TokenId = testTokenId
            };
            Assert.Equal(tokenUpdateTransaction.TokenId, testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenId = testTokenId);
        }

        public virtual void GetSetName()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                TokenName = testTokenName
            };
            Assert.Equal(tokenUpdateTransaction.TokenName, testTokenName);
        }

        public virtual void GetSetNameFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenName = testTokenName);
        }

        public virtual void GetSetSymbol()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                TokenSymbol = testTokenSymbol
            };
            Assert.Equal(tokenUpdateTransaction.TokenSymbol, testTokenSymbol);
        }

        public virtual void GetSetSymbolFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenSymbol = testTokenSymbol);
        }

        public virtual void GetSetTreasuryAccountId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                TreasuryAccountId = testTreasuryAccountId
            };
            Assert.Equal(tokenUpdateTransaction.TreasuryAccountId, testTreasuryAccountId);
        }

        public virtual void GetSetTreasuryAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TreasuryAccountId = testTreasuryAccountId);
        }

        public virtual void GetSetAdminKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                AdminKey = testAdminKey
            };
            Assert.Equal(tokenUpdateTransaction.AdminKey, testAdminKey);
        }

        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AdminKey = testAdminKey);
        }

        public virtual void GetSetKycKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                KycKey = testKycKey
            };
            Assert.Equal(tokenUpdateTransaction.KycKey, testKycKey);
        }

        public virtual void GetSetKycKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.KycKey = testKycKey);
        }

        public virtual void GetSetFreezeKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                FreezeKey = testFreezeKey
            };
            Assert.Equal(tokenUpdateTransaction.FreezeKey, testFreezeKey);
        }

        public virtual void GetSetFreezeKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FreezeKey = testFreezeKey);
        }

        public virtual void GetSetWipeKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                WipeKey = testWipeKey
            };
            Assert.Equal(tokenUpdateTransaction.WipeKey, testWipeKey);
        }

        public virtual void GetSetWipeKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.WipeKey = testWipeKey);
        }

        public virtual void GetSetSupplyKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                SupplyKey = testSupplyKey
            };
            Assert.Equal(tokenUpdateTransaction.SupplyKey, testSupplyKey);
        }

        public virtual void GetSetSupplyKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SupplyKey = testSupplyKey);
        }

        public virtual void GetSetAutoRenewAccountId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                AutoRenewAccountId = testAutoRenewAccountId
            };
            Assert.Equal(tokenUpdateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
        }

        public virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewAccountId = testAutoRenewAccountId);
        }

        public virtual void GetSetAutoRenewPeriod()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                AutoRenewPeriod = testAutoRenewPeriod.ToDuration()
            };
            Assert.Equal(tokenUpdateTransaction.AutoRenewPeriod, testAutoRenewPeriod.ToDuration());
        }

        public virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewPeriod = testAutoRenewPeriod.ToDuration());
        }

        public virtual void GetSetExpirationTime()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                ExpirationTime = testExpirationTime.ToTimestamp()
            };
            Assert.Equal(tokenUpdateTransaction.ExpirationTime, testExpirationTime.ToTimestamp());
        }

        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = testExpirationTime.ToTimestamp());
        }

        public virtual void GetSetTokenMemo()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                TokenMemo = testTokenMemo
            };
            Assert.Equal(tokenUpdateTransaction.TokenMemo, testTokenMemo);
        }

        public virtual void GetSetTokenMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenMemo = testTokenMemo);
        }

        public virtual void GetSetFeeScheduleKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
				FeeScheduleKey = testFeeScheduleKey,
			};
            Assert.Equal(tokenUpdateTransaction.FeeScheduleKey, testFeeScheduleKey);
        }

        public virtual void GetSetFeeScheduleKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.FeeScheduleKey = testFeeScheduleKey);
        }

        public virtual void GetSetPauseKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
                PauseKey = testPauseKey
            };
            Assert.Equal(tokenUpdateTransaction.PauseKey, testPauseKey);
        }

        public virtual void GetSetPauseKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.PauseKey = testPauseKey);
        }

        public virtual void GetSetMetadataKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction
            {
				MetadataKey = testMetadataKey,
			};
            Assert.Equal(tokenUpdateTransaction.MetadataKey, testMetadataKey);
        }

        public virtual void GetSetMetadataKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.MetadataKey = testMetadataKey);
        }

        public virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.TokenMetadata, testMetadata);
        }

        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenMetadata = testMetadata);
        }

        public virtual void GetSetKeyVerificationMode()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.TokenKeyVerificationMode, TokenKeyValidation.NoValidation);
        }

        public virtual void GetSetKeyVerificationModeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.TokenKeyVerificationMode = TokenKeyValidation.NoValidation);
        }
    }
}