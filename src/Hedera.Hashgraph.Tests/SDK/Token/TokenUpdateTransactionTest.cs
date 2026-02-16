// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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
        private static readonly Duration testAutoRenewPeriod = Duration.OfHours(10);
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
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        private TokenUpdateTransaction SpawnTestTransaction()
        {
            return new TokenUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart))).SetTokenId(testTokenId).SetFeeScheduleKey(testFeeScheduleKey).SetSupplyKey(testSupplyKey).SetAdminKey(testAdminKey).SetAutoRenewAccountId(testAutoRenewAccountId).SetAutoRenewPeriod(testAutoRenewPeriod).SetFreezeKey(testFreezeKey).SetWipeKey(testWipeKey).SetTokenSymbol(testTokenSymbol).SetKycKey(testKycKey).SetPauseKey(testPauseKey).SetMetadataKey(testMetadataKey).SetExpirationTime(validStart).SetTreasuryAccountId(testTreasuryAccountId).SetTokenName(testTokenName).SetTokenMemo(testTokenMemo).SetMaxTransactionFee(new Hbar(1)).SetTokenMetadata(testMetadata).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Freeze().Sign(unusedPrivateKey);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = TokenUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetTokenUpdate(TokenUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<TokenUpdateTransaction>(tx);
        }

        public virtual void ConstructTokenUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBody = TokenUpdateTransactionBody.NewBuilder().SetToken(testTokenId.ToProtobuf()).SetName(testTokenName).SetSymbol(testTokenSymbol).SetTreasury(testTreasuryAccountId.ToProtobuf()).SetAdminKey(testAdminKey.ToProtobufKey()).SetKycKey(testKycKey.ToProtobufKey()).SetFreezeKey(testFreezeKey.ToProtobufKey()).SetWipeKey(testWipeKey.ToProtobufKey()).SetSupplyKey(testSupplyKey.ToProtobufKey()).SetAutoRenewAccount(testAutoRenewAccountId.ToProtobuf()).SetAutoRenewPeriod(Proto.Duration.NewBuilder().SetSeconds(testAutoRenewPeriod.ToSeconds()).Build()).SetExpiry(Timestamp.NewBuilder().SetSeconds(testExpirationTime.GetEpochSecond()).Build()).SetMemo(StringValue.NewBuilder().SetValue(testTokenMemo).Build()).SetFeeScheduleKey(testFeeScheduleKey.ToProtobufKey()).SetPauseKey(testPauseKey.ToProtobufKey()).SetMetadataKey(testMetadataKey.ToProtobufKey()).SetMetadata(BytesValue.Of(ByteString.CopyFrom(testMetadata))).SetKeyVerificationMode(Proto.TokenKeyValidation.NO_VALIDATION).Build();
            var tx = TransactionBody.NewBuilder().SetTokenUpdate(transactionBody).Build();
            var tokenUpdateTransaction = new TokenUpdateTransaction(tx);
            Assert.Equal(tokenUpdateTransaction.GetTokenId(), testTokenId);
            Assert.Equal(tokenUpdateTransaction.GetTokenName(), testTokenName);
            Assert.Equal(tokenUpdateTransaction.GetTokenSymbol(), testTokenSymbol);
            Assert.Equal(tokenUpdateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
            Assert.Equal(tokenUpdateTransaction.GetAdminKey(), testAdminKey);
            Assert.Equal(tokenUpdateTransaction.GetKycKey(), testKycKey);
            Assert.Equal(tokenUpdateTransaction.GetFreezeKey(), testFreezeKey);
            Assert.Equal(tokenUpdateTransaction.GetWipeKey(), testWipeKey);
            Assert.Equal(tokenUpdateTransaction.GetSupplyKey(), testSupplyKey);
            Assert.Equal(tokenUpdateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
            Assert.Equal(tokenUpdateTransaction.GetAutoRenewPeriod().ToSeconds(), testAutoRenewPeriod.ToSeconds());
            Assert.Equal(tokenUpdateTransaction.GetExpirationTime().GetEpochSecond(), testExpirationTime.GetEpochSecond());
            Assert.Equal(tokenUpdateTransaction.GetTokenMemo(), testTokenMemo);
            Assert.Equal(tokenUpdateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
            Assert.Equal(tokenUpdateTransaction.GetPauseKey(), testPauseKey);
            Assert.Equal(tokenUpdateTransaction.GetMetadataKey(), testMetadataKey);
            Assert.Equal(tokenUpdateTransaction.GetTokenMetadata(), testMetadata);
            Assert.Equal(tokenUpdateTransaction.GetKeyVerificationMode(), TokenKeyValidation.NO_VALIDATION);
        }

        public virtual void GetSetTokenId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetTokenId(testTokenId);
            Assert.Equal(tokenUpdateTransaction.GetTokenId(), testTokenId);
        }

        public virtual void GetSetTokenIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenId(testTokenId));
        }

        public virtual void GetSetName()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetTokenName(testTokenName);
            Assert.Equal(tokenUpdateTransaction.GetTokenName(), testTokenName);
        }

        public virtual void GetSetNameFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenName(testTokenName));
        }

        public virtual void GetSetSymbol()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetTokenSymbol(testTokenSymbol);
            Assert.Equal(tokenUpdateTransaction.GetTokenSymbol(), testTokenSymbol);
        }

        public virtual void GetSetSymbolFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenSymbol(testTokenSymbol));
        }

        public virtual void GetSetTreasuryAccountId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetTreasuryAccountId(testTreasuryAccountId);
            Assert.Equal(tokenUpdateTransaction.GetTreasuryAccountId(), testTreasuryAccountId);
        }

        public virtual void GetSetTreasuryAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTreasuryAccountId(testTreasuryAccountId));
        }

        public virtual void GetSetAdminKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetAdminKey(testAdminKey);
            Assert.Equal(tokenUpdateTransaction.GetAdminKey(), testAdminKey);
        }

        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAdminKey(testAdminKey));
        }

        public virtual void GetSetKycKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetKycKey(testKycKey);
            Assert.Equal(tokenUpdateTransaction.GetKycKey(), testKycKey);
        }

        public virtual void GetSetKycKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetKycKey(testKycKey));
        }

        public virtual void GetSetFreezeKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetFreezeKey(testFreezeKey);
            Assert.Equal(tokenUpdateTransaction.GetFreezeKey(), testFreezeKey);
        }

        public virtual void GetSetFreezeKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetFreezeKey(testFreezeKey));
        }

        public virtual void GetSetWipeKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetWipeKey(testWipeKey);
            Assert.Equal(tokenUpdateTransaction.GetWipeKey(), testWipeKey);
        }

        public virtual void GetSetWipeKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetWipeKey(testWipeKey));
        }

        public virtual void GetSetSupplyKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetSupplyKey(testSupplyKey);
            Assert.Equal(tokenUpdateTransaction.GetSupplyKey(), testSupplyKey);
        }

        public virtual void GetSetSupplyKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetSupplyKey(testSupplyKey));
        }

        public virtual void GetSetAutoRenewAccountId()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetAutoRenewAccountId(testAutoRenewAccountId);
            Assert.Equal(tokenUpdateTransaction.GetAutoRenewAccountId(), testAutoRenewAccountId);
        }

        public virtual void GetSetAutoRenewAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAutoRenewAccountId(testAutoRenewAccountId));
        }

        public virtual void GetSetAutoRenewPeriod()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetAutoRenewPeriod(testAutoRenewPeriod);
            Assert.Equal(tokenUpdateTransaction.GetAutoRenewPeriod(), testAutoRenewPeriod);
        }

        public virtual void GetSetAutoRenewPeriodFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAutoRenewPeriod(testAutoRenewPeriod));
        }

        public virtual void GetSetExpirationTime()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetExpirationTime(testExpirationTime);
            Assert.Equal(tokenUpdateTransaction.GetExpirationTime(), testExpirationTime);
        }

        public virtual void GetSetExpirationTimeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetExpirationTime(testExpirationTime));
        }

        public virtual void GetSetTokenMemo()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetTokenMemo(testTokenMemo);
            Assert.Equal(tokenUpdateTransaction.GetTokenMemo(), testTokenMemo);
        }

        public virtual void GetSetTokenMemoFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenMemo(testTokenMemo));
        }

        public virtual void GetSetFeeScheduleKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetFeeScheduleKey(testFeeScheduleKey);
            Assert.Equal(tokenUpdateTransaction.GetFeeScheduleKey(), testFeeScheduleKey);
        }

        public virtual void GetSetFeeScheduleKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetFeeScheduleKey(testFeeScheduleKey));
        }

        public virtual void GetSetPauseKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetPauseKey(testPauseKey);
            Assert.Equal(tokenUpdateTransaction.GetPauseKey(), testPauseKey);
        }

        public virtual void GetSetPauseKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetPauseKey(testPauseKey));
        }

        public virtual void GetSetMetadataKey()
        {
            var tokenUpdateTransaction = new TokenUpdateTransaction().SetMetadataKey(testMetadataKey);
            Assert.Equal(tokenUpdateTransaction.GetMetadataKey(), testMetadataKey);
        }

        public virtual void GetSetMetadataKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetMetadataKey(testMetadataKey));
        }

        public virtual void GetSetMetadata()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetTokenMetadata(), testMetadata);
        }

        public virtual void GetSetMetadataFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetTokenMetadata(testMetadata));
        }

        public virtual void GetSetKeyVerificationMode()
        {
            var tx = SpawnTestTransaction();
            Assert.Equal(tx.GetKeyVerificationMode(), TokenKeyValidation.NO_VALIDATION);
        }

        public virtual void GetSetKeyVerificationModeFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION));
        }
    }
}