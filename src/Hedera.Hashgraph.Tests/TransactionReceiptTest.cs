// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Java.Nio.Charset;
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
    public class TransactionReceiptTest
    {
        static readonly Instant time = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        static TransactionReceipt SpawnReceiptExample()
        {
            return new TransactionReceipt(null, Status.SCHEDULE_ALREADY_DELETED, new ExchangeRate(3, 4, time), new ExchangeRate(3, 4, time), AccountId.FromString("1.2.3"), FileId.FromString("4.5.6"), ContractId.FromString("3.2.1"), TopicId.FromString("9.8.7"), TokenId.FromString("6.5.4"), 3, ByteString.CopyFrom("how now brown cow", StandardCharsets.UTF_8), 30, ScheduleId.FromString("1.1.1"), TransactionId.WithValidStart(AccountId.FromString("3.3.3"), time), List.Of(1, 2, 3), 1, new List(), new List());
        }

        virtual void ShouldSerialize()
        {
            var originalTransactionReceipt = SpawnReceiptExample();
            byte[] transactionReceiptBytes = originalTransactionReceipt.ToBytes();
            var copyTransactionReceipt = TransactionReceipt.FromBytes(transactionReceiptBytes);
            Assert.Equal(copyTransactionReceipt.ToString(), originalTransactionReceipt.ToString());
            SnapshotMatcher.Expect(originalTransactionReceipt.ToString()).ToMatchSnapshot();
        }
    }
}