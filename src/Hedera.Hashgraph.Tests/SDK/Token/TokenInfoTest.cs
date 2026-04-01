// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Networking;

using Google.Protobuf;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    // TODO: update this, test deepClone()
    public class TokenInfoTest
    {
        /*
        if we will init PrivateKey using method `PrivateKey.fromSeedECDSAsecp256k1(byte[] seed)` (like in C++ SDK, for example)
        => we will get public key each time we run tests on different machines
        => io.github.jsonSnapshot.SnapshotMatcher will fail tests
        => we need to init PrivateKey fromString to get the same key each time
        => `toProtobuf()` tests uses getEd25519() method to assert equality
        */
        private static readonly PublicKey testAdminKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey testKycKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        private static readonly PublicKey testFreezeKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e13").GetPublicKey();
        private static readonly PublicKey testWipeKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e14").GetPublicKey();
        private static readonly PublicKey testSupplyKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e15").GetPublicKey();
        private static readonly PublicKey testFeeScheduleKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e16").GetPublicKey();
        private static readonly PublicKey testPauseKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e17").GetPublicKey();
        private static readonly PublicKey testMetadataKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e18").GetPublicKey();
        private static readonly TokenId testTokenId = TokenId.FromString("0.6.9");
        private static readonly TokenSupplyType testTokenSupplyType = TokenSupplyType.Finite;
        private static readonly AccountId testTreasuryAccountId = AccountId.FromString("7.7.7");
        private static readonly AccountId testAutoRenewAccountId = AccountId.FromString("8.9.0");
        private static readonly string testTokenName = "test token name";
        private static readonly string testTokenSymbol = "TTN";
        private static readonly string testTokenMemo = "memo";
        private static readonly uint testTokenDecimals = 3;
        private static readonly ulong testTokenTotalSupply = 1000;
        private static readonly bool testTokenFreezeStatus = true;
        private static readonly bool testTokenKycStatus = true;
        private static readonly bool testTokenIsDeleted = false;
        private static readonly List<CustomFee> testTokenCustomFees =
        [
            new CustomFixedFee
            {
				FeeCollectorAccountId = new AccountId(0, 0, 4322),
			    DenominatingTokenId = new TokenId(0, 0, 483902),
			    Amount = 10
			},
			new CustomFractionalFee
			{
				FeeCollectorAccountId = new AccountId(0, 0, 389042),
				Numerator = 3,
				Denominator = 7,
				Min = 3,
				Max = 100,
			}
		];
        private static readonly TokenType testTokenType = TokenType.FungibleCommon;
        private static readonly TokenSupplyType testTokenTokenSupplyType = TokenSupplyType.Finite;
        private static readonly long testTokenMaxSupply = 1000000;
        private static readonly bool testTokenPauseStatus = true;
        private static readonly LedgerId testTokenLedgerId = LedgerId.MAINNET;
        private static readonly TimeSpan testAutoRenewPeriod = TimeSpan.FromHours(10);
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
        private static TokenInfo SpawnTokenInfoExample()
        {
            return new TokenInfo(
                testTokenId, 
                testTokenName, 
                testTokenSymbol, 
                testTokenDecimals, 
                testTokenTotalSupply, 
                testTreasuryAccountId, 
                testAdminKey, 
                testKycKey, 
                testFreezeKey, 
                testWipeKey, 
                testSupplyKey, 
                testFeeScheduleKey, 
                testTokenFreezeStatus, 
                testTokenKycStatus, 
                testTokenIsDeleted, 
                testAutoRenewAccountId, 
                testAutoRenewPeriod, 
                testExpirationTime, 
                testTokenMemo, 
                testTokenCustomFees, 
                testTokenType,
                testTokenSupplyType, 
                testTokenMaxSupply, 
                testPauseKey, 
                testTokenPauseStatus, 
                testMetadata, 
                testMetadataKey, 
                testTokenLedgerId);
        }
        [Fact]
        public virtual void ShouldSerialize()
        {
            var originalTokenInfo = SpawnTokenInfoExample();
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenInfo.FromBytes(tokenInfoBytes);
            
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());

            Verifier.Verify(originalTokenInfo.ToString());
        }
        [Fact]
        public virtual void FromProtobuf()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();
            var tokenInfo = TokenInfo.FromProtobuf(tokenInfoProto);

            Assert.Equal(tokenInfo.TokenId, testTokenId);
            Assert.Equal(tokenInfo.Name, testTokenName);
            Assert.Equal(tokenInfo.Symbol, testTokenSymbol);
            Assert.Equal(tokenInfo.Decimals, (ulong)testTokenDecimals);
            Assert.Equal(tokenInfo.TotalSupply, (ulong)testTokenTotalSupply);
            Assert.Equal(tokenInfo.TreasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenInfo.AdminKey.ToBytes(), testAdminKey.ToBytes());
            Assert.Equal(tokenInfo.KycKey.ToBytes(), testKycKey.ToBytes());
            Assert.Equal(tokenInfo.FreezeKey.ToBytes(), testFreezeKey.ToBytes());
            Assert.Equal(tokenInfo.WipeKey.ToBytes(), testWipeKey.ToBytes());
            Assert.Equal(tokenInfo.SupplyKey.ToBytes(), testSupplyKey.ToBytes());
            Assert.Equal(tokenInfo.DefaultFreezeStatus, testTokenFreezeStatus);
            Assert.Equal(tokenInfo.DefaultKycStatus, testTokenKycStatus);
            Assert.Equal(tokenInfo.IsDeleted, testTokenIsDeleted);
            Assert.Equal(tokenInfo.AutoRenewAccount, testAutoRenewAccountId);
            Assert.Equal(tokenInfo.AutoRenewPeriod, testAutoRenewPeriod);
            Assert.Equal(tokenInfo.ExpirationTime, testExpirationTime);
            Assert.Equal(tokenInfo.TokenMemo, testTokenMemo);
            Assert.Equal(tokenInfo.TokenType, testTokenType);
            Assert.Equal(tokenInfo.SupplyType, testTokenSupplyType);
            Assert.Equal(tokenInfo.MaxSupply, testTokenMaxSupply);
            Assert.Equal(tokenInfo.FeeScheduleKey.ToBytes(), testFeeScheduleKey.ToBytes());
			Assert.Equal(tokenInfo.CustomFees.Count, testTokenCustomFees.Count);
            Assert.Equal(tokenInfo.PauseKey.ToBytes(), testPauseKey.ToBytes());
            Assert.Equal(tokenInfo.PauseStatus, testTokenPauseStatus);
            Assert.Equal(tokenInfo.Metadata, testMetadata);
            Assert.Equal(tokenInfo.MetadataKey.ToBytes(), testMetadataKey.ToBytes());
            Assert.Equal(tokenInfo.LedgerId, testTokenLedgerId);
        }
        [Fact]
        public virtual void FromBytes()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();
            var tokenInfo = TokenInfo.FromBytes(tokenInfoProto.ToByteArray());

            Assert.Equal(tokenInfo.TokenId, testTokenId);
            Assert.Equal(tokenInfo.Name, testTokenName);
            Assert.Equal(tokenInfo.Symbol, testTokenSymbol);
            Assert.Equal(tokenInfo.Decimals, (ulong)testTokenDecimals);
            Assert.Equal(tokenInfo.TotalSupply, (ulong)testTokenTotalSupply);
            Assert.Equal(tokenInfo.TreasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenInfo.AdminKey.ToBytes(), testAdminKey.ToBytes());
            Assert.Equal(tokenInfo.KycKey.ToBytes(), testKycKey.ToBytes());
            Assert.Equal(tokenInfo.FreezeKey.ToBytes(), testFreezeKey.ToBytes());
            Assert.Equal(tokenInfo.WipeKey.ToBytes(), testWipeKey.ToBytes());
            Assert.Equal(tokenInfo.SupplyKey.ToBytes(), testSupplyKey.ToBytes());
            Assert.Equal(tokenInfo.DefaultFreezeStatus, testTokenFreezeStatus);
            Assert.Equal(tokenInfo.DefaultKycStatus, testTokenKycStatus);
            Assert.Equal(tokenInfo.IsDeleted, testTokenIsDeleted);
            Assert.Equal(tokenInfo.AutoRenewAccount, testAutoRenewAccountId);
            Assert.Equal(tokenInfo.AutoRenewPeriod, testAutoRenewPeriod);
            Assert.Equal(tokenInfo.ExpirationTime, testExpirationTime);
            Assert.Equal(tokenInfo.TokenMemo, testTokenMemo);
            Assert.Equal(tokenInfo.TokenType, testTokenType);
            Assert.Equal(tokenInfo.SupplyType, testTokenSupplyType);
            Assert.Equal(tokenInfo.MaxSupply, testTokenMaxSupply);
            Assert.Equal(tokenInfo.FeeScheduleKey.ToBytes(), testFeeScheduleKey.ToBytes());
			Assert.Equal(tokenInfo.CustomFees.Count, testTokenCustomFees.Count);
            Assert.Equal(tokenInfo.PauseKey.ToBytes(), testPauseKey.ToBytes());
            Assert.Equal(tokenInfo.PauseStatus, testTokenPauseStatus);
            Assert.Equal(tokenInfo.Metadata, testMetadata);
            Assert.Equal(tokenInfo.MetadataKey.ToBytes(), testMetadataKey.ToBytes());
            Assert.Equal(tokenInfo.LedgerId, testTokenLedgerId);
        }
        [Fact]
        public virtual void ToProtobuf()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();

            Assert.Equal(tokenInfoProto.TokenInfo.TokenId.ShardNum, testTokenId.Shard);
            Assert.Equal(tokenInfoProto.TokenInfo.TokenId.RealmNum, testTokenId.Realm);
            Assert.Equal(tokenInfoProto.TokenInfo.TokenId.TokenNum, testTokenId.Num);
            Assert.Equal(tokenInfoProto.TokenInfo.Name, testTokenName);
            Assert.Equal(tokenInfoProto.TokenInfo.Symbol, testTokenSymbol);
            Assert.Equal(tokenInfoProto.TokenInfo.Decimals, (ulong)testTokenDecimals);
            Assert.Equal(tokenInfoProto.TokenInfo.TotalSupply, (ulong)testTokenTotalSupply);
            Assert.Equal(tokenInfoProto.TokenInfo.Treasury.ShardNum, testTreasuryAccountId.Shard);
            Assert.Equal(tokenInfoProto.TokenInfo.Treasury.RealmNum, testTreasuryAccountId.Realm);
            Assert.Equal(tokenInfoProto.TokenInfo.Treasury.AccountNum, testTreasuryAccountId.Num);
            Assert.Equal(tokenInfoProto.TokenInfo.AdminKey.Ed25519.ToByteArray(), testAdminKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.KycKey.Ed25519.ToByteArray(), testKycKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.FreezeKey.Ed25519.ToByteArray(), testFreezeKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.WipeKey.Ed25519.ToByteArray(), testWipeKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.SupplyKey.Ed25519.ToByteArray(), testSupplyKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.DefaultFreezeStatus, TokenInfo.FreezeStatusToProtobuf(testTokenFreezeStatus));
            Assert.Equal(tokenInfoProto.TokenInfo.DefaultKycStatus, TokenInfo.KycStatusToProtobuf(testTokenKycStatus));
            Assert.Equal(tokenInfoProto.TokenInfo.Deleted, testTokenIsDeleted);
            Assert.Equal(tokenInfoProto.TokenInfo.AutoRenewAccount.ShardNum, testAutoRenewAccountId.Shard);
            Assert.Equal(tokenInfoProto.TokenInfo.AutoRenewAccount.RealmNum, testAutoRenewAccountId.Realm);
            Assert.Equal(tokenInfoProto.TokenInfo.AutoRenewAccount.AccountNum, testAutoRenewAccountId.Num);
            Assert.Equal(tokenInfoProto.TokenInfo.AutoRenewPeriod.Seconds, testAutoRenewPeriod.TotalSeconds);
            Assert.Equal(tokenInfoProto.TokenInfo.Expiry.Seconds, testExpirationTime.ToUnixTimeSeconds());
            Assert.Equal(tokenInfoProto.TokenInfo.Memo, testTokenMemo);
            Assert.Equal(tokenInfoProto.TokenInfo.TokenType, (Proto.TokenType)testTokenType);
            Assert.Equal(tokenInfoProto.TokenInfo.SupplyType, (Proto.TokenSupplyType)testTokenSupplyType);
            Assert.Equal(tokenInfoProto.TokenInfo.MaxSupply, testTokenMaxSupply);
            Assert.Equal(tokenInfoProto.TokenInfo.FeeScheduleKey.Ed25519.ToByteArray(), testFeeScheduleKey.ToBytesRaw());
			Assert.Equal(tokenInfoProto.TokenInfo.CustomFees.Count, testTokenCustomFees.Count);
            Assert.Equal(tokenInfoProto.TokenInfo.PauseKey.Ed25519.ToByteArray(), testPauseKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.PauseStatus, TokenInfo.PauseStatusToProtobuf(testTokenPauseStatus));
            Assert.Equal(tokenInfoProto.TokenInfo.Metadata.ToByteArray(), testMetadata);
            Assert.Equal(tokenInfoProto.TokenInfo.MetadataKey.Ed25519.ToByteArray(), testMetadataKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.TokenInfo.LedgerId, testTokenLedgerId.ToByteString());
        }
        [Fact]
        public virtual void ToBytes()
        {
            var tokenInfo = SpawnTokenInfoExample();
            var bytes = tokenInfo.ToBytes();

            Assert.Equal(bytes, tokenInfo.ToProtobuf().ToByteArray());
        }
    }
}