// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ScheduleInfoTest
    {
        private static readonly PublicKey unusedPublicKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10").GetPublicKey();
        readonly Instant validStart = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual ScheduleInfo SpawnScheduleInfoExample()
        {
            return new ScheduleInfo(ScheduleId.FromString("1.2.3"), AccountId.FromString("4.5.6"), AccountId.FromString("2.3.4"), SchedulableTransactionBody.NewBuilder().SetCryptoDelete(CryptoDeleteTransactionBody.NewBuilder().SetDeleteAccountID(AccountId.FromString("6.6.6").ToProtobuf()).Build()).Build(), KeyList.Of(unusedPublicKey), unusedPublicKey, TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart), "memo", validStart, validStart, null, LedgerId.TESTNET, true);
        }

        virtual ScheduleInfo SpawnScheduleInfoDeletedExample()
        {
            return new ScheduleInfo(ScheduleId.FromString("1.2.3"), AccountId.FromString("4.5.6"), AccountId.FromString("2.3.4"), SchedulableTransactionBody.NewBuilder().SetCryptoDelete(CryptoDeleteTransactionBody.NewBuilder().SetDeleteAccountID(AccountId.FromString("6.6.6").ToProtobuf()).Build()).Build(), KeyList.Of(unusedPublicKey), unusedPublicKey, TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), validStart), "memo", validStart, null, validStart, LedgerId.TESTNET, true);
        }

        virtual void ShouldSerialize()
        {
            var originalScheduleInfo = SpawnScheduleInfoExample();
            byte[] scheduleInfoBytes = originalScheduleInfo.ToBytes();
            var copyScheduleInfo = ScheduleInfo.FromBytes(scheduleInfoBytes);
            Assert.Equal(copyScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        virtual void ShouldSerializeDeleted()
        {
            var originalScheduleInfo = SpawnScheduleInfoDeletedExample();
            byte[] scheduleInfoBytes = originalScheduleInfo.ToBytes();
            var copyScheduleInfo = ScheduleInfo.FromBytes(scheduleInfoBytes);
            Assert.Equal(copyScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalScheduleInfo.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}