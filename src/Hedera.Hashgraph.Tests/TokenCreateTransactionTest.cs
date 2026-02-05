// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Common.Collect;
using Com.Google.Protobuf;
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
        private static readonly long testInitialSupply = 30;
        private static readonly long testMaxSupply = 500;
        private static readonly int testDecimals = 3;
        private static readonly bool testFreezeDefault = true;
        private static readonly string testTokenName = "test name";
        private static readonly string testTokenSymbol = "test symbol";
        private static readonly string testTokenMemo = "test memo";
        private static readonly Duration testAutoRenewPeriod = Duration.OfHours(10);
        private static readonly Instant testExpirationTime = Instant.Now();
        private static readonly IList<CustomFee> testCustomFees = Collections.SingletonList(new CustomFixedFee().SetFeeCollectorAccountId(AccountId.FromString("0.0.543")).SetAmount(3).SetDenominatingTokenId(TokenId.FromString("4.3.2")));
        private static readonly byte[] testMetadata = new byte[]
        {
            1,
            2,
            3,
            4,
            5
        };
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerializeFungible()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionFungible().ToString()).ToMatchSnapshot();
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldSerializeNft()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionNft().ToString()).ToMatchSnapshot();
        }

        private TokenCreateTransaction SpawnTestTransactionFungible()
        {
            return new TokenCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetInitialSupply(testInitialSupply).SetFeeScheduleKey(testFeeScheduleKey).SetSupplyKey(testSupplyKey).SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetDecimals(testDecimals).SetFreezeDefault(testFreezeDefault).SetFreezeKey(testFreezeKey).SetWipeKey(testWipeKey).SetTokenSymbol(testTokenSymbol).SetKycKey(testKycKey).SetPauseKey(testPauseKey).SetMetadataKey(testMetadataKey).SetExpirationTime(validStart).SetTreasuryAccountId(testTreasuryAccountId).SetTokenName(testTokenName).SetTokenMemo(testTokenMemo).SetCustomFees(testCustomFees).SetMaxTransactionFee(new Hbar(1)).SetTokenMetadata(testMetadata).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransactionFungible();
            var tx2 = TokenCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenCreateTransaction SpawnTestTransactionNft()
        {
            return new TokenCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart)).SetFeeScheduleKey(testFeeScheduleKey).SetSupplyKey(testSupplyKey).SetMaxSupply(testMaxSupply).SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetSupplyType(TokenSupplyType.FINITE).SetFreezeKey(testFreezeKey).SetWipeKey(testWipeKey).SetTokenSymbol(testTokenSymbol).SetKycKey(testKycKey).SetPauseKey(testPauseKey).SetMetadataKey(testMetadataKey).SetExpirationTime(validStart).SetTreasuryAccountId(testTreasuryAccountId).SetTokenName(testTokenName).SetTokenMemo(testTokenMemo).SetMaxTransactionFee(new Hbar(1)).SetTokenMetadata(testMetadata).Freeze().Sign(unusedPrivateKey);
        }

        virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = TokenCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenCreation(TokenCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenCreateTransaction>(tx);
        }

        virtual void ConstructTokenCreateTransactionFungibleFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenCreateTransactionBody.NewBuilder().SetInitialSupply(testInitialSupply).SetFeeScheduleKey(testFeeScheduleKey.ToProtobufKey()).SetSupplyKey(testSupplyKey.ToProtobufKey()).SetAdminKey(testAdminKey.ToProtobufKey()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).SetAutoRenewPeriod(com.hedera.hashgraph.sdk.proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetDecimals(testDecimals).SetFreezeDefault(testFreezeDefault).SetFreezeKey(testFreezeKey.ToProtobufKey()).SetWipeKey(testWipeKey.ToProtobufKey()).SetSymbol(testTokenSymbol).SetKycKey(testKycKey.ToProtobufKey()).SetPauseKey(testPauseKey.ToProtobufKey()).SetMetadataKey(testMetadataKey.ToProtobufKey()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond())).SetTreasury(testTreasuryAccountId.ToProtobuf()).SetName(testTokenName).SetMemo(testTokenMemo).AddCustomFees(Iterables.GetLast(testCustomFees).ToProtobuf()).SetMetadata(ByteString.CopyFrom(testMetadata)).Build();
            var tx = TransactionBody.NewBuilder().SetTokenCreation(transactionBody).Build();
            var tokenCreateTransaction = new TokenCreateTransaction(tx);
            Assert.Equal(tokenCreateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.GetSupplyKey(), testSupplyKey);
            Assert.Equal(tokenCreateTransaction.GetAdminKey(), testAdminKey);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewPeriod().ToSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(tokenCreateTransaction.GetDecimals(), testDecimals);
            Assert.Equal(tokenCreateTransaction.GetFreezeDefault(), testFreezeDefault);
            Assert.Equal(tokenCreateTransaction.GetFreezeKey(), testFreezeKey);
            Assert.Equal(tokenCreateTransaction.GetWipeKey(), testWipeKey);
            Assert.Equal(tokenCreateTransaction.GetTokenSymbol(), testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.GetKycKey(), testKycKey);
            Assert.Equal(tokenCreateTransaction.GetPauseKey(), testPauseKey);
            Assert.Equal(tokenCreateTransaction.GetMetadataKey(), testMetadataKey);
            Assert.Equal(tokenCreateTransaction.GetExpirationTime().GetEpochSecond(), testExpirationTime.GetEpochSecond());
            Assert.Equal(tokenCreateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.GetTokenName(), testTokenName);
            Assert.Equal(tokenCreateTransaction.GetTokenMemo(), testTokenMemo);
            Assert.Equal(tokenCreateTransaction.GetTokenType(), TokenType.FUNGIBLE_COMMON);
            Assert.Equal(Iterables.GetLast(tokenCreateTransaction.GetCustomFees()).ToBytes(), Iterables.GetLast(testCustomFees).ToBytes());
            Assert.Equal(tokenCreateTransaction.GetTokenMetadata(), testMetadata);
        }

        virtual void ConstructTokenCreateTransactionNftFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenCreateTransactionBody.NewBuilder().SetFeeScheduleKey(testFeeScheduleKey.ToProtobufKey()).SetSupplyKey(testSupplyKey.ToProtobufKey()).SetMaxSupply(testMaxSupply).SetAdminKey(testAdminKey.ToProtobufKey()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).SetAutoRenewPeriod(com.hedera.hashgraph.sdk.proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetTokenType(com.hedera.hashgraph.sdk.proto.TokenType.NON_FUNGIBLE_UNIQUE).SetSupplyType(com.hedera.hashgraph.sdk.proto.TokenSupplyType.FINITE).SetFreezeKey(testFreezeKey.ToProtobufKey()).SetWipeKey(testWipeKey.ToProtobufKey()).SetSymbol(testTokenSymbol).SetKycKey(testKycKey.ToProtobufKey()).SetPauseKey(testPauseKey.ToProtobufKey()).SetMetadataKey(testMetadataKey.ToProtobufKey()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond())).SetTreasury(testTreasuryAccountId.ToProtobuf()).SetName(testTokenName).SetMemo(testTokenMemo).Build();
            var tx = TransactionBody.NewBuilder().SetTokenCreation(transactionBody).Build();
            var tokenCreateTransaction = new TokenCreateTransaction(tx);
            Assert.Equal(tokenCreateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.GetSupplyKey(), testSupplyKey);
            Assert.Equal(tokenCreateTransaction.GetMaxSupply(), testMaxSupply);
            Assert.Equal(tokenCreateTransaction.GetAdminKey(), testAdminKey);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewPeriod().ToSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(tokenCreateTransaction.GetTokenType(), TokenType.NON_FUNGIBLE_UNIQUE);
            Assert.Equal(tokenCreateTransaction.GetSupplyType(), TokenSupplyType.FINITE);
            Assert.Equal(tokenCreateTransaction.GetFreezeKey(), testFreezeKey);
            Assert.Equal(tokenCreateTransaction.GetWipeKey(), testWipeKey);
            Assert.Equal(tokenCreateTransaction.GetTokenSymbol(), testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.GetKycKey(), testKycKey);
            Assert.Equal(tokenCreateTransaction.GetPauseKey(), testPauseKey);
            Assert.Equal(tokenCreateTransaction.GetMetadataKey(), testMetadataKey);
            Assert.Equal(tokenCreateTransaction.GetExpirationTime().GetEpochSecond(), testExpirationTime.GetEpochSecond());
            Assert.Equal(tokenCreateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.GetTokenName(), testTokenName);
            Assert.Equal(tokenCreateTransaction.GetTokenMemo(), testTokenMemo);
        }

        virtual void GetSetName()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenName(testTokenName);
            Assert.Equal(tokenCreateTransaction.GetTokenName(), testTokenName);
        }

        virtual void GetSetNameFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenName(testTokenName));
        }

        virtual void GetSetSymbol()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenSymbol(testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.GetTokenSymbol(), testTokenSymbol);
        }

        virtual void GetSetSymbolFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenSymbol(testTokenSymbol));
        }

        virtual void GetSetDecimals()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetDecimals(testDecimals);
            Assert.Equal(tokenCreateTransaction.GetDecimals(), testDecimals);
        }

        virtual void GetSetDecimalsFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetDecimals(testDecimals));
        }

        virtual void GetSetInitialSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetInitialSupply(testInitialSupply);
            Assert.Equal(tokenCreateTransaction.GetInitialSupply(), testInitialSupply);
        }

        virtual void GetSetInitialSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetInitialSupply(testInitialSupply));
        }

        virtual void GetSetTreasuryAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTreasuryAccountId(testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
        }

        virtual void GetSetTreasuryAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTreasuryAccountId(testTreasuryAccountId));
        }

        virtual void GetSetAdminKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAdminKey(testAdminKey);
            Assert.Equal(tokenCreateTransaction.GetAdminKey(), testAdminKey);
        }

        virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAdminKey(testAdminKey));
        }

        virtual void GetSetKycKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetKycKey(testKycKey);
            Assert.Equal(tokenCreateTransaction.GetKycKey(), testKycKey);
        }

        virtual void GetSetKycKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetKycKey(testKycKey));
        }

        virtual void GetSetFreezeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetFreezeKey(testFreezeKey);
            Assert.Equal(tokenCreateTransaction.GetFreezeKey(), testFreezeKey);
        }

        virtual void GetSetFreezeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFreezeKey(testFreezeKey));
        }

        virtual void GetSetWipeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetWipeKey(testWipeKey);
            Assert.Equal(tokenCreateTransaction.GetWipeKey(), testWipeKey);
        }

        virtual void GetSetWipeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetWipeKey(testWipeKey));
        }

        virtual void GetSetSupplyKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetSupplyKey(testSupplyKey);
            Assert.Equal(tokenCreateTransaction.GetSupplyKey(), testSupplyKey);
        }

        virtual void GetSetSupplyKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetSupplyKey(testSupplyKey));
        }

        virtual void GetSetFeeScheduleKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetFeeScheduleKey(testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
        }

        virtual void GetSetFeeScheduleKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetFeeScheduleKey(testFeeScheduleKey));
        }

        virtual void GetSetPauseKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetPauseKey(testPauseKey);
            Assert.Equal(tokenCreateTransaction.GetPauseKey(), testPauseKey);
        }

        virtual void GetSetPauseKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetPauseKey(testPauseKey));
        }

        virtual void GetSetMetadataKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetMetadataKey(testMetadataKey);
            Assert.Equal(tokenCreateTransaction.GetMetadataKey(), testMetadataKey);
        }

        virtual void GetSetMetadataKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetMetadataKey(testMetadataKey));
        }

        virtual void GetSetExpirationTime()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetExpirationTime(testExpirationTime);
            Assert.Equal(tokenCreateTransaction.GetExpirationTime(), testExpirationTime);
        }

        virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetExpirationTime(testExpirationTime));
        }

        virtual void GetSetAutoRenewAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAutoRenewAccountId(testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
        }

        virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAutoRenewAccountId(testAutoRenewAccountId));
        }

        virtual void GetSetAutoRenewPeriod()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAutoRenewPeriod(testAutoRenewPeriod);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewPeriod(), testAutoRenewPeriod);
        }

        virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAutoRenewPeriod(testAutoRenewPeriod));
        }

        virtual void GetSetTokenMemo()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenMemo(testTokenMemo);
            Assert.Equal(tokenCreateTransaction.GetTokenMemo(), testTokenMemo);
        }

        virtual void GetSetTokenMemoFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenMemo(testTokenMemo));
        }

        virtual void GetSetTokenType()
        {
            TokenType testTokenType = TokenType.FUNGIBLE_COMMON;
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenType(testTokenType);
            Assert.Equal(tokenCreateTransaction.GetTokenType(), testTokenType);
        }

        virtual void GetSetTokenTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenType(TokenType.FUNGIBLE_COMMON));
        }

        virtual void GetSetSupplyType()
        {
            TokenSupplyType testTokenType = TokenSupplyType.FINITE;
            var tokenCreateTransaction = new TokenCreateTransaction().SetSupplyType(testTokenType);
            Assert.Equal(tokenCreateTransaction.GetSupplyType(), testTokenType);
        }

        virtual void GetSetSupplyTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetSupplyType(TokenSupplyType.FINITE));
        }

        virtual void GetSetMaxSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetMaxSupply(testMaxSupply);
            Assert.Equal(tokenCreateTransaction.GetMaxSupply(), testMaxSupply);
        }

        virtual void GetSetMaxSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetMaxSupply(testMaxSupply));
        }

        virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Equal(tx.GetTokenMetadata(), testMetadata);
        }

        virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetTokenMetadata(testMetadata));
        }
    }
}