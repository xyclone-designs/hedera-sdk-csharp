// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenCreateTransactionTest
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
        private static readonly ulong testInitialSupply = 30;
        private static readonly long testMaxSupply = 500;
        private static readonly uint testDecimals = 3;
        private static readonly bool testFreezeDefault = true;
        private static readonly string testTokenName = "test name";
        private static readonly string testTokenSymbol = "test symbol";
        private static readonly string testTokenMemo = "test memo";
        private static readonly TimeSpan testAutoRenewPeriod = TimeSpan.FromHours(10);
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.UtcNow;
        private static readonly List<CustomFee> testCustomFees = [new CustomFixedFee 
        {
			FeeCollectorAccountId = AccountId.FromString("0.0.543"), 
            Amount = 3, 
            DenominatingTokenId = TokenId.FromString("4.3.2")
		}];
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerializeFungible()
        {
            Verifier.Verify(SpawnTestTransactionFungible().ToString());
        }
        [Fact]
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenCreateTransaction();
            var tx2 = Transaction.FromBytes<TokenCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        public virtual void ShouldSerializeNft()
        {
            Verifier.Verify(SpawnTestTransactionNft().ToString());
        }

        private TokenCreateTransaction SpawnTestTransactionFungible()
        {
            return new TokenCreateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart),
                InitialSupply = testInitialSupply,
                FeeScheduleKey = testFeeScheduleKey,
                SupplyKey = testSupplyKey,
                AdminKey = testAdminKey,
                AutoRenewAccountId = testAutoRenewAccountId,
                AutoRenewPeriod = testAutoRenewPeriod,
                Decimals = testDecimals,
                FreezeDefault = testFreezeDefault,
                FreezeKey = testFreezeKey,
                WipeKey = testWipeKey,
                TokenSymbol = testTokenSymbol,
                KycKey = testKycKey,
                PauseKey = testPauseKey,
                MetadataKey = testMetadataKey,
                ExpirationTime = validStart,
                TreasuryAccountId = testTreasuryAccountId,
                TokenName = testTokenName,
                TokenMemo = testTokenMemo,
                CustomFees = testCustomFees,
                MaxTransactionFee = new Hbar(1),
                TokenMetadata = testMetadata

            }.Freeze().Sign(unusedPrivateKey);
        }
        [Fact]
		public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransactionFungible();
            var tx2 = Transaction.FromBytes<TokenCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenCreateTransaction SpawnTestTransactionNft()
        {
            return new TokenCreateTransaction 
            { 
                NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006") ], 
                TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart), 
                FeeScheduleKey = testFeeScheduleKey,
                SupplyKey = testSupplyKey,
                MaxSupply = testMaxSupply,
                AdminKey = testAdminKey,
                AutoRenewAccountId = testAutoRenewAccountId,
                AutoRenewPeriod = testAutoRenewPeriod,
                TokenType = TokenType.NonFungibleUnique,
                TokenSupplyType = TokenSupplyType.Finite,
                FreezeKey = testFreezeKey,
                WipeKey = testWipeKey, 
                TokenSymbol = testTokenSymbol, 
                KycKey = testKycKey,
                PauseKey = testPauseKey,
                MetadataKey = testMetadataKey, 
                ExpirationTime = validStart,
                TreasuryAccountId = testTreasuryAccountId,
                TokenName = testTokenName, 
                TokenMemo = testTokenMemo, 
                MaxTransactionFee = new Hbar(1),
                TokenMetadata = testMetadata
            
            }.Freeze().Sign(unusedPrivateKey);
		}
        [Fact]
        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = Transaction.FromBytes<TokenCreateTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }
        [Fact]
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody { TokenCreation = new Proto.TokenCreateTransactionBody() };
            var tx = Transaction.FromScheduledTransaction<TokenCreateTransaction>(transactionBody);
            
            Assert.IsType<TokenCreateTransaction>(tx);
        }
        [Fact]
        public virtual void ConstructTokenCreateTransactionFungibleFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenCreateTransactionBody
            {
                InitialSupply = testInitialSupply,
                FeeScheduleKey = testFeeScheduleKey.ToProtobufKey(),
                SupplyKey = testSupplyKey.ToProtobufKey(),
                AdminKey = testAdminKey.ToProtobufKey(),
                AutoRenewAccount = testAutoRenewAccountId.ToProtobuf(),
                AutoRenewPeriod = new Proto.Duration { Seconds = (long)testAutoRenewPeriod.TotalSeconds },
				Expiry = new Proto.Timestamp { Seconds = testExpirationTime.ToUnixTimeSeconds() },
				Decimals = testDecimals,
				FreezeDefault = testFreezeDefault,
				FreezeKey = testFreezeKey.ToProtobufKey(),
				WipeKey = testWipeKey.ToProtobufKey(),
				Symbol = testTokenSymbol,
				KycKey = testKycKey.ToProtobufKey(),
				PauseKey = testPauseKey.ToProtobufKey(),
                MetadataKey = testMetadataKey.ToProtobufKey(),
				Treasury = testTreasuryAccountId.ToProtobuf(),
				Name = testTokenName,
				Memo = testTokenMemo,
				Metadata = ByteString.CopyFrom(testMetadata),
			};
            var tx = new Proto.Services.TransactionBody { TokenCreation = transactionBody };
            var tokenCreateTransaction = new TokenCreateTransaction(tx);

            Assert.Equal(tokenCreateTransaction.FeeScheduleKey, testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.SupplyKey, testSupplyKey);
            Assert.Equal(tokenCreateTransaction.AdminKey, testAdminKey);
            Assert.Equal(tokenCreateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.AutoRenewPeriod?.TotalSeconds, testAutoRenewPeriod.TotalSeconds);
            Assert.Equal(tokenCreateTransaction.Decimals, (uint)testDecimals);
            Assert.Equal(tokenCreateTransaction.FreezeDefault, testFreezeDefault);
            Assert.Equal(tokenCreateTransaction.FreezeKey, testFreezeKey);
            Assert.Equal(tokenCreateTransaction.WipeKey, testWipeKey);
            Assert.Equal(tokenCreateTransaction.TokenSymbol, testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.KycKey, testKycKey);
            Assert.Equal(tokenCreateTransaction.PauseKey, testPauseKey);
            Assert.Equal(tokenCreateTransaction.MetadataKey, testMetadataKey);
            Assert.Equal(tokenCreateTransaction.ExpirationTime?.ToUnixTimeSeconds(), testExpirationTime.ToUnixTimeSeconds());
            Assert.Equal(tokenCreateTransaction.TreasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.TokenName, testTokenName);
            Assert.Equal(tokenCreateTransaction.TokenMemo, testTokenMemo);
            Assert.Equal(tokenCreateTransaction.TokenType, TokenType.FungibleCommon);
            Assert.Equal(tokenCreateTransaction.TokenMetadata, testMetadata);
        }
        [Fact]
        public virtual void ConstructTokenCreateTransactionNftFromTransactionBodyProtobuf()
        {
            var transactionBody = new Proto.TokenCreateTransactionBody
            {
                FeeScheduleKey = testFeeScheduleKey.ToProtobufKey(),
                SupplyKey = testSupplyKey.ToProtobufKey(),
                MaxSupply = testMaxSupply,
                AdminKey = testAdminKey.ToProtobufKey(),
                AutoRenewAccount = testAutoRenewAccountId.ToProtobuf(),
                AutoRenewPeriod = new Proto.Duration { Seconds = (long)testAutoRenewPeriod.TotalSeconds },
                Expiry = new Proto.Timestamp { Seconds = testExpirationTime.ToUnixTimeSeconds() },
                TokenType = Proto.TokenType.NonFungibleUnique,
                SupplyType = Proto.TokenSupplyType.Finite,
                FreezeKey = testFreezeKey.ToProtobufKey(),
                WipeKey = testWipeKey.ToProtobufKey(),
                Symbol = testTokenSymbol,
                KycKey = testKycKey.ToProtobufKey(),
                PauseKey = testPauseKey.ToProtobufKey(),
                MetadataKey = testMetadataKey.ToProtobufKey(),
                Treasury = testTreasuryAccountId.ToProtobuf(),
                Name = testTokenName,
                Memo = testTokenMemo
            };
            var tx = new Proto.Services.TransactionBody { TokenCreation = transactionBody };
            var tokenCreateTransaction = new TokenCreateTransaction(tx);
            Assert.Equal(tokenCreateTransaction.FeeScheduleKey, testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.SupplyKey, testSupplyKey);
            Assert.Equal(tokenCreateTransaction.MaxSupply, testMaxSupply);
            Assert.Equal(tokenCreateTransaction.AdminKey, testAdminKey);
            Assert.Equal(tokenCreateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.AutoRenewPeriod?.TotalSeconds, testAutoRenewPeriod.TotalSeconds);
            Assert.Equal(tokenCreateTransaction.TokenType, TokenType.NonFungibleUnique);
            Assert.Equal(tokenCreateTransaction.TokenSupplyType, TokenSupplyType.Finite);
            Assert.Equal(tokenCreateTransaction.FreezeKey, testFreezeKey);
            Assert.Equal(tokenCreateTransaction.WipeKey, testWipeKey);
            Assert.Equal(tokenCreateTransaction.TokenSymbol, testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.KycKey, testKycKey);
            Assert.Equal(tokenCreateTransaction.PauseKey, testPauseKey);
            Assert.Equal(tokenCreateTransaction.MetadataKey, testMetadataKey);
            Assert.Equal(tokenCreateTransaction.ExpirationTime?.ToUnixTimeSeconds(), testExpirationTime.ToUnixTimeSeconds());
            Assert.Equal(tokenCreateTransaction.TreasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.TokenName, testTokenName);
            Assert.Equal(tokenCreateTransaction.TokenMemo, testTokenMemo);
        }
        [Fact]
        public virtual void GetSetName()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { TokenName = testTokenName };
            Assert.Equal(tokenCreateTransaction.TokenName, testTokenName);
        }
        [Fact]
        public virtual void GetSetNameFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenName = testTokenName);
        }
        [Fact]
        public virtual void GetSetSymbol()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { TokenSymbol = testTokenSymbol };
            Assert.Equal(tokenCreateTransaction.TokenSymbol, testTokenSymbol);
        }
        [Fact]
        public virtual void GetSetSymbolFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenSymbol = testTokenSymbol);
        }
        [Fact]
        public virtual void GetSetDecimals()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { Decimals = (uint)testDecimals };
            Assert.Equal(tokenCreateTransaction.Decimals, (uint)testDecimals);
        }
        [Fact]
        public virtual void GetSetDecimalsFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.Decimals = (uint)testDecimals);
        }
        [Fact]
        public virtual void GetSetInitialSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { InitialSupply = (ulong)testInitialSupply };
            Assert.Equal(tokenCreateTransaction.InitialSupply, (ulong)testInitialSupply);
        }
        [Fact]
        public virtual void GetSetInitialSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.InitialSupply = (ulong)testInitialSupply);
        }
        [Fact]
        public virtual void GetSetTreasuryAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { TreasuryAccountId = testTreasuryAccountId };
            Assert.Equal(tokenCreateTransaction.TreasuryAccountId, testTreasuryAccountId);
        }
        [Fact]
        public virtual void GetSetTreasuryAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TreasuryAccountId = testTreasuryAccountId);
        }
        [Fact]
        public virtual void GetSetAdminKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { AdminKey = testAdminKey };
            Assert.Equal(tokenCreateTransaction.AdminKey, testAdminKey);
        }
        [Fact]
        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.AdminKey = testAdminKey);
        }
        [Fact]
        public virtual void GetSetKycKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { KycKey = testKycKey };
            Assert.Equal(tokenCreateTransaction.KycKey, testKycKey);
        }
        [Fact]
        public virtual void GetSetKycKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.KycKey = testKycKey);
        }
        [Fact]
        public virtual void GetSetFreezeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { FreezeKey = testFreezeKey };
            Assert.Equal(tokenCreateTransaction.FreezeKey, testFreezeKey);
        }
        [Fact]
        public virtual void GetSetFreezeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.FreezeKey = testFreezeKey);
        }
        [Fact]
        public virtual void GetSetWipeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { WipeKey = testWipeKey };
            Assert.Equal(tokenCreateTransaction.WipeKey, testWipeKey);
        }
        [Fact]
        public virtual void GetSetWipeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.WipeKey = testWipeKey);
        }
        [Fact]
        public virtual void GetSetSupplyKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { SupplyKey = testSupplyKey };
            Assert.Equal(tokenCreateTransaction.SupplyKey, testSupplyKey);
        }
        [Fact]
        public virtual void GetSetSupplyKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SupplyKey = testSupplyKey);
        }
        [Fact]
        public virtual void GetSetFeeScheduleKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { FeeScheduleKey = testFeeScheduleKey, };
            Assert.Equal(tokenCreateTransaction.FeeScheduleKey, testFeeScheduleKey);
        }
        [Fact]
        public virtual void GetSetFeeScheduleKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.FeeScheduleKey = testFeeScheduleKey);
        }
        [Fact]
        public virtual void GetSetPauseKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { PauseKey = testPauseKey };
            Assert.Equal(tokenCreateTransaction.PauseKey, testPauseKey);
        }
        [Fact]
        public virtual void GetSetPauseKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.PauseKey = testPauseKey);
        }
        [Fact]
        public virtual void GetSetMetadataKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { MetadataKey = testMetadataKey, };
            Assert.Equal(tokenCreateTransaction.MetadataKey, testMetadataKey);
        }
        [Fact]
        public virtual void GetSetMetadataKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.MetadataKey = testMetadataKey);
        }
        [Fact]
        public virtual void GetSetExpirationTime()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { ExpirationTime = testExpirationTime };
            Assert.Equal(tokenCreateTransaction.ExpirationTime, testExpirationTime);
        }
        [Fact]
        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.ExpirationTime = testExpirationTime);
        }
        [Fact]
        public virtual void GetSetAutoRenewAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { AutoRenewAccountId = testAutoRenewAccountId };
            Assert.Equal(tokenCreateTransaction.AutoRenewAccountId, testAutoRenewAccountId);
        }
        [Fact]
        public virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewAccountId = testAutoRenewAccountId);
        }
        [Fact]
        public virtual void GetSetAutoRenewPeriod()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { AutoRenewPeriod = testAutoRenewPeriod };
            Assert.Equal(tokenCreateTransaction.AutoRenewPeriod, testAutoRenewPeriod);
        }
        [Fact]
        public virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.AutoRenewPeriod = testAutoRenewPeriod);
        }
        [Fact]
        public virtual void GetSetTokenMemo()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { TokenMemo = testTokenMemo };
            Assert.Equal(tokenCreateTransaction.TokenMemo, testTokenMemo);
        }
        [Fact]
        public virtual void GetSetTokenMemoFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenMemo = testTokenMemo);
        }
        [Fact]
        public virtual void GetSetTokenType()
        {
            TokenType testTokenType = TokenType.FungibleCommon;
            var tokenCreateTransaction = new TokenCreateTransaction { TokenType = testTokenType };
            Assert.Equal(tokenCreateTransaction.TokenType, testTokenType);
        }
        [Fact]
        public virtual void GetSetTokenTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenType = TokenType.FungibleCommon);
        }
        [Fact]
        public virtual void GetSetSupplyType()
        {
            TokenSupplyType testTokenType = TokenSupplyType.Finite;
            var tokenCreateTransaction = new TokenCreateTransaction { TokenSupplyType = testTokenType };
            Assert.Equal(tokenCreateTransaction.TokenSupplyType, testTokenType);
        }
        [Fact]
        public virtual void GetSetSupplyTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenSupplyType = TokenSupplyType.Finite);
        }
        [Fact]
        public virtual void GetSetMaxSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction { MaxSupply = testMaxSupply };
            Assert.Equal(tokenCreateTransaction.MaxSupply, testMaxSupply);
        }
        [Fact]
        public virtual void GetSetMaxSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.MaxSupply = testMaxSupply);
        }
        [Fact]
        public virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Equal(tx.TokenMetadata, testMetadata);
        }
        [Fact]
        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.TokenMetadata = testMetadata);
        }
    }
}