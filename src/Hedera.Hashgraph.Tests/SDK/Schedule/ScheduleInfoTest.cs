// SPDX-License-Identifier: Apache-2.0
using System;
using System.Text.RegularExpressions;

using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Networking;

namespace Hedera.Hashgraph.Tests.SDK.Schedule
{
    public class ScheduleInfoTest
    {
        private static readonly PublicKey unusedPublicKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10").GetPublicKey();
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual ScheduleInfo SpawnScheduleInfoExample()
        {
            return new ScheduleInfo(
                ScheduleId.FromString("1.2.3"), 
                AccountId.FromString("4.5.6"), 
                AccountId.FromString("2.3.4"), 
                new Proto.SchedulableTransactionBody 
                { 
                    CryptoDelete = new Proto.CryptoDeleteTransactionBody 
                    { 
                        DeleteAccountID = AccountId.FromString("6.6.6").ToProtobuf()
                    }
                }, 
                [ unusedPublicKey ], 
                unusedPublicKey, 
                TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)), 
                "memo", 
                validStart.ToTimestamp(), 
                validStart.ToTimestamp(), 
                null, 
                LedgerId.TESTNET, 
                true);
        }

        public virtual ScheduleInfo SpawnScheduleInfoDeletedExample()
        {
            return new ScheduleInfo(
                ScheduleId.FromString("1.2.3"), 
                AccountId.FromString("4.5.6"), 
                AccountId.FromString("2.3.4"), 
                new Proto.SchedulableTransactionBody
                {
                    CryptoDelete = new Proto.CryptoDeleteTransactionBody
                    {
                        DeleteAccountID = AccountId.FromString("6.6.6").ToProtobuf()
                    }
                }, 
                [ unusedPublicKey ], 
                unusedPublicKey, 
                TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), Timestamp.FromDateTimeOffset(validStart)), 
                "memo", 
                validStart.ToTimestamp(), 
                null, 
                validStart.ToTimestamp(), 
                LedgerId.TESTNET, 
                true);
        }

        public virtual void ShouldSerialize()
        {
            var originalScheduleInfo = SpawnScheduleInfoExample();
            byte[] scheduleInfoBytes = originalScheduleInfo.ToBytes();
            var copyScheduleInfo = ScheduleInfo.FromBytes(scheduleInfoBytes);
            
            Assert.Equal(Regex.Replace(copyScheduleInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalScheduleInfo.ToString(), "@[A-Za-z0-9]+", ""));
            
            SnapshotMatcher.Expect(Regex.Replace(originalScheduleInfo.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        public virtual void ShouldSerializeDeleted()
        {
            var originalScheduleInfo = SpawnScheduleInfoDeletedExample();
            byte[] scheduleInfoBytes = originalScheduleInfo.ToBytes();
            var copyScheduleInfo = ScheduleInfo.FromBytes(scheduleInfoBytes);
            
            Assert.Equal(Regex.Replace(copyScheduleInfo.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalScheduleInfo.ToString(), "@[A-Za-z0-9]+", ""));
            
            SnapshotMatcher.Expect(Regex.Replace(originalScheduleInfo.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}