// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
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
        private static readonly AccountId testTreasuryAccountId = AccountId.FromString("7.7.7");
        private static readonly AccountId testAutoRenewAccountId = AccountId.FromString("8.9.0");
        private static readonly string testTokenName = "test token name";
        private static readonly string testTokenSymbol = "TTN";
        private static readonly string testTokenMemo = "memo";
        private static readonly int testTokenDecimals = 3;
        private static readonly long testTokenTotalSupply = 1000;
        private static readonly bool testTokenFreezeStatus = true;
        private static readonly bool testTokenKycStatus = true;
        private static readonly bool testTokenIsDeleted = false;
        private static readonly IList<CustomFee> testTokenCustomFees = Arrays.AsList(new CustomFixedFee().SetFeeCollectorAccountId(new AccountId(0, 0, 4322)).SetDenominatingTokenId(new TokenId(0, 0, 483902)).SetAmount(10), new CustomFractionalFee().SetFeeCollectorAccountId(new AccountId(0, 0, 389042)).SetNumerator(3).SetDenominator(7).SetMin(3).SetMax(100));
        private static readonly TokenType testTokenType = TokenType.FUNGIBLE_COMMON;
        private static readonly TokenSupplyType testTokenSupplyType = TokenSupplyType.FINITE;
        private static readonly long testTokenMaxSupply = 1000000;
        private static readonly bool testTokenPauseStatus = true;
        private static readonly LedgerId testTokenLedgerId = LedgerId.MAINNET;
        private static readonly Duration testAutoRenewPeriod = Duration.OfHours(10);
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private static TokenInfo SpawnTokenInfoExample()
        {
            return new TokenInfo(testTokenId, testTokenName, testTokenSymbol, testTokenDecimals, testTokenTotalSupply, testTreasuryAccountId, testAdminKey, testKycKey, testFreezeKey, testWipeKey, testSupplyKey, testFeeScheduleKey, testTokenFreezeStatus, testTokenKycStatus, testTokenIsDeleted, testAutoRenewAccountId, testAutoRenewPeriod, testExpirationTime, testTokenMemo, testTokenCustomFees, testTokenType, testTokenSupplyType, testTokenMaxSupply, testPauseKey, testTokenPauseStatus, testMetadata, testMetadataKey, testTokenLedgerId);
        }

        public virtual void ShouldSerialize()
        {
            var originalTokenInfo = SpawnTokenInfoExample();
            byte[] tokenInfoBytes = originalTokenInfo.ToBytes();
            var copyTokenInfo = TokenInfo.FromBytes(tokenInfoBytes);
            Assert.Equal(copyTokenInfo.ToString(), originalTokenInfo.ToString());
            SnapshotMatcher.Expect(originalTokenInfo.ToString()).ToMatchSnapshot();
        }

        public virtual void FromProtobuf()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();
            var tokenInfo = TokenInfo.FromProtobuf(tokenInfoProto);
            Assert.Equal(tokenInfo.tokenId, testTokenId);
            Assert.Equal(tokenInfo.name, testTokenName);
            Assert.Equal(tokenInfo.symbol, testTokenSymbol);
            Assert.Equal(tokenInfo.decimals, testTokenDecimals);
            Assert.Equal(tokenInfo.totalSupply, testTokenTotalSupply);
            Assert.Equal(tokenInfo.treasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenInfo.adminKey.ToBytes(), testAdminKey.ToBytes());
            Assert.Equal(tokenInfo.kycKey.ToBytes(), testKycKey.ToBytes());
            Assert.Equal(tokenInfo.freezeKey.ToBytes(), testFreezeKey.ToBytes());
            Assert.Equal(tokenInfo.wipeKey.ToBytes(), testWipeKey.ToBytes());
            Assert.Equal(tokenInfo.supplyKey.ToBytes(), testSupplyKey.ToBytes());
            Assert.Equal(tokenInfo.defaultFreezeStatus, testTokenFreezeStatus);
            Assert.Equal(tokenInfo.defaultKycStatus, testTokenKycStatus);
            Assert.Equal(tokenInfo.isDeleted, testTokenIsDeleted);
            Assert.Equal(tokenInfo.autoRenewAccount, testAutoRenewAccountId);
            Assert.Equal(tokenInfo.autoRenewPeriod, testAutoRenewPeriod);
            Assert.Equal(tokenInfo.expirationTime, testExpirationTime);
            Assert.Equal(tokenInfo.tokenMemo, testTokenMemo);
            Assert.Equal(tokenInfo.tokenType, testTokenType);
            Assert.Equal(tokenInfo.supplyType, testTokenSupplyType);
            Assert.Equal(tokenInfo.maxSupply, testTokenMaxSupply);
            Assert.Equal(tokenInfo.feeScheduleKey.ToBytes(), testFeeScheduleKey.ToBytes());
            AssertThat(tokenInfo.customFees).HasSize(testTokenCustomFees.Count);
            Assert.Equal(tokenInfo.pauseKey.ToBytes(), testPauseKey.ToBytes());
            Assert.Equal(tokenInfo.pauseStatus, testTokenPauseStatus);
            Assert.Equal(tokenInfo.metadata, testMetadata);
            Assert.Equal(tokenInfo.metadataKey.ToBytes(), testMetadataKey.ToBytes());
            Assert.Equal(tokenInfo.ledgerId, testTokenLedgerId);
        }

        public virtual void FromBytes()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();
            var tokenInfo = TokenInfo.FromBytes(tokenInfoProto.ToByteArray());
            Assert.Equal(tokenInfo.tokenId, testTokenId);
            Assert.Equal(tokenInfo.name, testTokenName);
            Assert.Equal(tokenInfo.symbol, testTokenSymbol);
            Assert.Equal(tokenInfo.decimals, testTokenDecimals);
            Assert.Equal(tokenInfo.totalSupply, testTokenTotalSupply);
            Assert.Equal(tokenInfo.treasuryAccountId, testTreasuryAccountId);
            Assert.Equal(tokenInfo.adminKey.ToBytes(), testAdminKey.ToBytes());
            Assert.Equal(tokenInfo.kycKey.ToBytes(), testKycKey.ToBytes());
            Assert.Equal(tokenInfo.freezeKey.ToBytes(), testFreezeKey.ToBytes());
            Assert.Equal(tokenInfo.wipeKey.ToBytes(), testWipeKey.ToBytes());
            Assert.Equal(tokenInfo.supplyKey.ToBytes(), testSupplyKey.ToBytes());
            Assert.Equal(tokenInfo.defaultFreezeStatus, testTokenFreezeStatus);
            Assert.Equal(tokenInfo.defaultKycStatus, testTokenKycStatus);
            Assert.Equal(tokenInfo.isDeleted, testTokenIsDeleted);
            Assert.Equal(tokenInfo.autoRenewAccount, testAutoRenewAccountId);
            Assert.Equal(tokenInfo.autoRenewPeriod, testAutoRenewPeriod);
            Assert.Equal(tokenInfo.expirationTime, testExpirationTime);
            Assert.Equal(tokenInfo.tokenMemo, testTokenMemo);
            Assert.Equal(tokenInfo.tokenType, testTokenType);
            Assert.Equal(tokenInfo.supplyType, testTokenSupplyType);
            Assert.Equal(tokenInfo.maxSupply, testTokenMaxSupply);
            Assert.Equal(tokenInfo.feeScheduleKey.ToBytes(), testFeeScheduleKey.ToBytes());
            AssertThat(tokenInfo.customFees).HasSize(testTokenCustomFees.Count);
            Assert.Equal(tokenInfo.pauseKey.ToBytes(), testPauseKey.ToBytes());
            Assert.Equal(tokenInfo.pauseStatus, testTokenPauseStatus);
            Assert.Equal(tokenInfo.metadata, testMetadata);
            Assert.Equal(tokenInfo.metadataKey.ToBytes(), testMetadataKey.ToBytes());
            Assert.Equal(tokenInfo.ledgerId, testTokenLedgerId);
        }

        public virtual void ToProtobuf()
        {
            var tokenInfoProto = SpawnTokenInfoExample().ToProtobuf();
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTokenId().GetShardNum(), testTokenId.shard);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTokenId().GetRealmNum(), testTokenId.realm);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTokenId().GetTokenNum(), testTokenId.num);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetName(), testTokenName);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetSymbol(), testTokenSymbol);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetDecimals(), testTokenDecimals);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTotalSupply(), testTokenTotalSupply);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTreasury().GetShardNum(), testTreasuryAccountId.shard);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTreasury().GetRealmNum(), testTreasuryAccountId.realm);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTreasury().GetAccountNum(), testTreasuryAccountId.num);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetAdminKey().GetEd25519().ToByteArray(), testAdminKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetKycKey().GetEd25519().ToByteArray(), testKycKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetFreezeKey().GetEd25519().ToByteArray(), testFreezeKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetWipeKey().GetEd25519().ToByteArray(), testWipeKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetSupplyKey().GetEd25519().ToByteArray(), testSupplyKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetDefaultFreezeStatus(), TokenInfo.FreezeStatusToProtobuf(testTokenFreezeStatus));
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetDefaultKycStatus(), TokenInfo.KycStatusToProtobuf(testTokenKycStatus));
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetDeleted(), testTokenIsDeleted);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetAutoRenewAccount().GetShardNum(), testAutoRenewAccountId.shard);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetAutoRenewAccount().GetRealmNum(), testAutoRenewAccountId.realm);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetAutoRenewAccount().GetAccountNum(), testAutoRenewAccountId.num);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetAutoRenewPeriod().GetSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetExpiry().GetSeconds(), testExpirationTime.GetEpochSecond());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetMemo(), testTokenMemo);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetTokenType(), Proto.TokenType.ValueOf(testTokenType.Name()));
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetSupplyType(), Proto.TokenSupplyType.ValueOf(testTokenSupplyType.Name()));
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetMaxSupply(), testTokenMaxSupply);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetFeeScheduleKey().GetEd25519().ToByteArray(), testFeeScheduleKey.ToBytesRaw());
            AssertThat(tokenInfoProto.GetTokenInfo().GetCustomFeesList()).HasSize(testTokenCustomFees.Count);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetPauseKey().GetEd25519().ToByteArray(), testPauseKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetPauseStatus(), TokenInfo.PauseStatusToProtobuf(testTokenPauseStatus));
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetMetadata().ToByteArray(), testMetadata);
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetMetadataKey().GetEd25519().ToByteArray(), testMetadataKey.ToBytesRaw());
            Assert.Equal(tokenInfoProto.GetTokenInfo().GetLedgerId(), testTokenLedgerId.ToByteString());
        }

        public virtual void ToBytes()
        {
            var tokenInfo = SpawnTokenInfoExample();
            var bytes = tokenInfo.ToBytes();
            Assert.Equal(bytes, tokenInfo.ToProtobuf().ToByteArray());
        }
    }
}