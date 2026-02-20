// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Common.Collect;
using Com.Google.Protobuf;
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
        private static readonly DateTimeOffset testExpirationTime = DateTimeOffset.UtcNow;
        private static readonly List<CustomFee> testCustomFees = [new CustomFixedFee(].SetFeeCollectorAccountId(AccountId.FromString("0.0.543")).SetAmount(3).SetDenominatingTokenId(TokenId.FromString("4.3.2")));
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

        public virtual void ShouldSerializeFungible()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionFungible().ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new TokenCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldSerializeNft()
        {
            SnapshotMatcher.Expect(SpawnTestTransactionNft().ToString()).ToMatchSnapshot();
        }

        private TokenCreateTransaction SpawnTestTransactionFungible()
        {
            return new TokenCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetInitialSupply(testInitialSupply).SetFeeScheduleKey(testFeeScheduleKey).SetSupplyKey(testSupplyKey).SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetDecimals(testDecimals).SetFreezeDefault(testFreezeDefault).SetFreezeKey(testFreezeKey).SetWipeKey(testWipeKey).SetTokenSymbol(testTokenSymbol).SetKycKey(testKycKey).SetPauseKey(testPauseKey).SetMetadataKey(testMetadataKey).SetExpirationTime(validStart).SetTreasuryAccountId(testTreasuryAccountId).SetTokenName(testTokenName).SetTokenMemo(testTokenMemo).SetCustomFees(testCustomFees).SetMaxTransactionFee(new Hbar(1)).SetTokenMetadata(testMetadata).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesFungible()
        {
            var tx = SpawnTestTransactionFungible();
            var tx2 = TokenCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenCreateTransaction SpawnTestTransactionNft()
        {
            return new TokenCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetFeeScheduleKey(testFeeScheduleKey).SetSupplyKey(testSupplyKey,MaxSupply = testMaxSupply,.SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetTokenType(TokenType.NonFungibleUnique,SupplyType = TokenSupplyType.Finite,.SetFreezeKey(testFreezeKey).SetWipeKey(testWipeKey).SetTokenSymbol(testTokenSymbol).SetKycKey(testKycKey).SetPauseKey(testPauseKey).SetMetadataKey(testMetadataKey).SetExpirationTime(validStart).SetTreasuryAccountId(testTreasuryAccountId).SetTokenName(testTokenName).SetTokenMemo(testTokenMemo).SetMaxTransactionFee(new Hbar(1)).SetTokenMetadata(testMetadata).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytesNft()
        {
            var tx = SpawnTestTransactionNft();
            var tx2 = TokenCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenCreation(TokenCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenCreateTransaction>(tx);
        }

        public virtual void ConstructTokenCreateTransactionFungibleFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenCreateTransactionBody.NewBuilder().SetInitialSupply(testInitialSupply).SetFeeScheduleKey(testFeeScheduleKey.ToProtobufKey()).SetSupplyKey(testSupplyKey.ToProtobufKey()).SetAdminKey(testAdminKey.ToProtobufKey()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).SetAutoRenewPeriod(Proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetDecimals(testDecimals).SetFreezeDefault(testFreezeDefault).SetFreezeKey(testFreezeKey.ToProtobufKey()).SetWipeKey(testWipeKey.ToProtobufKey()).SetSymbol(testTokenSymbol).SetKycKey(testKycKey.ToProtobufKey()).SetPauseKey(testPauseKey.ToProtobufKey()).SetMetadataKey(testMetadataKey.ToProtobufKey()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond())).SetTreasury(testTreasuryAccountId.ToProtobuf()).SetName(testTokenName).SetMemo(testTokenMemo).AddCustomFees(Iterables.GetLast(testCustomFees).ToProtobuf()).SetMetadata(ByteString.CopyFrom(testMetadata)).Build();
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

        public virtual void ConstructTokenCreateTransactionNftFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenCreateTransactionBody.NewBuilder().SetFeeScheduleKey(testFeeScheduleKey.ToProtobufKey()).SetSupplyKey(testSupplyKey.ToProtobufKey(),MaxSupply = testMaxSupply,.SetAdminKey(testAdminKey.ToProtobufKey()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).SetAutoRenewPeriod(Proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetTokenType(Proto.TokenType.NonFungibleUnique,SupplyType = Proto.TokenSupplyType.Finite,.SetFreezeKey(testFreezeKey.ToProtobufKey()).SetWipeKey(testWipeKey.ToProtobufKey()).SetSymbol(testTokenSymbol).SetKycKey(testKycKey.ToProtobufKey()).SetPauseKey(testPauseKey.ToProtobufKey()).SetMetadataKey(testMetadataKey.ToProtobufKey()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond())).SetTreasury(testTreasuryAccountId.ToProtobuf()).SetName(testTokenName).SetMemo(testTokenMemo).Build();
            var tx = TransactionBody.NewBuilder().SetTokenCreation(transactionBody).Build();
            var tokenCreateTransaction = new TokenCreateTransaction(tx);
            Assert.Equal(tokenCreateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.GetSupplyKey(), testSupplyKey);
            Assert.Equal(tokenCreateTransaction.GetMaxSupply(), testMaxSupply);
            Assert.Equal(tokenCreateTransaction.GetAdminKey(), testAdminKey);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewPeriod().ToSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(tokenCreateTransaction.GetTokenType(), TokenType.NonFungibleUnique);
            Assert.Equal(tokenCreateTransaction.GetSupplyType(), TokenSupplyType.Finite);
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

        public virtual void GetSetName()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenName(testTokenName);
            Assert.Equal(tokenCreateTransaction.GetTokenName(), testTokenName);
        }

        public virtual void GetSetNameFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenName(testTokenName));
        }

        public virtual void GetSetSymbol()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenSymbol(testTokenSymbol);
            Assert.Equal(tokenCreateTransaction.GetTokenSymbol(), testTokenSymbol);
        }

        public virtual void GetSetSymbolFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenSymbol(testTokenSymbol));
        }

        public virtual void GetSetDecimals()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetDecimals(testDecimals);
            Assert.Equal(tokenCreateTransaction.GetDecimals(), testDecimals);
        }

        public virtual void GetSetDecimalsFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetDecimals(testDecimals));
        }

        public virtual void GetSetInitialSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetInitialSupply(testInitialSupply);
            Assert.Equal(tokenCreateTransaction.GetInitialSupply(), testInitialSupply);
        }

        public virtual void GetSetInitialSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetInitialSupply(testInitialSupply));
        }

        public virtual void GetSetTreasuryAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTreasuryAccountId(testTreasuryAccountId);
            Assert.Equal(tokenCreateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
        }

        public virtual void GetSetTreasuryAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTreasuryAccountId(testTreasuryAccountId));
        }

        public virtual void GetSetAdminKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAdminKey(testAdminKey);
            Assert.Equal(tokenCreateTransaction.GetAdminKey(), testAdminKey);
        }

        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetAdminKey(testAdminKey));
        }

        public virtual void GetSetKycKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetKycKey(testKycKey);
            Assert.Equal(tokenCreateTransaction.GetKycKey(), testKycKey);
        }

        public virtual void GetSetKycKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetKycKey(testKycKey));
        }

        public virtual void GetSetFreezeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetFreezeKey(testFreezeKey);
            Assert.Equal(tokenCreateTransaction.GetFreezeKey(), testFreezeKey);
        }

        public virtual void GetSetFreezeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetFreezeKey(testFreezeKey));
        }

        public virtual void GetSetWipeKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetWipeKey(testWipeKey);
            Assert.Equal(tokenCreateTransaction.GetWipeKey(), testWipeKey);
        }

        public virtual void GetSetWipeKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetWipeKey(testWipeKey));
        }

        public virtual void GetSetSupplyKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetSupplyKey(testSupplyKey);
            Assert.Equal(tokenCreateTransaction.GetSupplyKey(), testSupplyKey);
        }

        public virtual void GetSetSupplyKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetSupplyKey(testSupplyKey));
        }

        public virtual void GetSetFeeScheduleKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetFeeScheduleKey(testFeeScheduleKey);
            Assert.Equal(tokenCreateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
        }

        public virtual void GetSetFeeScheduleKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetFeeScheduleKey(testFeeScheduleKey));
        }

        public virtual void GetSetPauseKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetPauseKey(testPauseKey);
            Assert.Equal(tokenCreateTransaction.GetPauseKey(), testPauseKey);
        }

        public virtual void GetSetPauseKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetPauseKey(testPauseKey));
        }

        public virtual void GetSetMetadataKey()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetMetadataKey(testMetadataKey);
            Assert.Equal(tokenCreateTransaction.GetMetadataKey(), testMetadataKey);
        }

        public virtual void GetSetMetadataKeyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetMetadataKey(testMetadataKey));
        }

        public virtual void GetSetExpirationTime()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetExpirationTime(testExpirationTime);
            Assert.Equal(tokenCreateTransaction.GetExpirationTime(), testExpirationTime);
        }

        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetExpirationTime(testExpirationTime));
        }

        public virtual void GetSetAutoRenewAccountId()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAutoRenewAccountId(testAutoRenewAccountId);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
        }

        public virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetAutoRenewAccountId(testAutoRenewAccountId));
        }

        public virtual void GetSetAutoRenewPeriod()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetAutoRenewPeriod(testAutoRenewPeriod);
            Assert.Equal(tokenCreateTransaction.GetAutoRenewPeriod(), testAutoRenewPeriod);
        }

        public virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetAutoRenewPeriod(testAutoRenewPeriod));
        }

        public virtual void GetSetTokenMemo()
        {
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenMemo(testTokenMemo);
            Assert.Equal(tokenCreateTransaction.GetTokenMemo(), testTokenMemo);
        }

        public virtual void GetSetTokenMemoFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenMemo(testTokenMemo));
        }

        public virtual void GetSetTokenType()
        {
            TokenType testTokenType = TokenType.FUNGIBLE_COMMON;
            var tokenCreateTransaction = new TokenCreateTransaction().SetTokenType(testTokenType);
            Assert.Equal(tokenCreateTransaction.GetTokenType(), testTokenType);
        }

        public virtual void GetSetTokenTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenType(TokenType.FUNGIBLE_COMMON));
        }

        public virtual void GetSetSupplyType()
        {
            TokenSupplyType testTokenType = TokenSupplyType.Finite;
            var tokenCreateTransaction = new TokenCreateTransaction(,SupplyType = testTokenType,;
            Assert.Equal(tokenCreateTransaction.GetSupplyType(), testTokenType);
        }

        public virtual void GetSetSupplyTypeFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => txSupplyType = TokenSupplyType.Finite));
        }

        public virtual void GetSetMaxSupply()
        {
            var tokenCreateTransaction = new TokenCreateTransaction(,MaxSupply = testMaxSupply,;
            Assert.Equal(tokenCreateTransaction.GetMaxSupply(), testMaxSupply);
        }

        public virtual void GetSetMaxSupplyFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetMaxSupply(testMaxSupply));
        }

        public virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Equal(tx.GetTokenMetadata(), testMetadata);
        }

        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransactionFungible();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenMetadata(testMetadata));
        }
    }
}