// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    public class TransactionReceiptTest
    {
        private static readonly DateTimeOffset time = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
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
            return new TransactionReceipt(
                null,
				ResponseStatus.ScheduleAlreadyDeleted, 
                new ExchangeRate(3, 4, time), 
                new ExchangeRate(3, 4, time), 
                AccountId.FromString("1.2.3"), 
                FileId.FromString("4.5.6"), 
                ContractId.FromString("3.2.1"), 
                TopicId.FromString("9.8.7"), 
                TokenId.FromString("6.5.4"), 
                3, 
                ByteString.CopyFromUtf8("how now brown cow"), 30, 
                ScheduleId.FromString("1.1.1"), 
                TransactionId.WithValidStart(AccountId.FromString("3.3.3"), Timestamp.FromDateTimeOffset(time)), 
                [1, 2, 3], 
                1, 
                [],
                []);
        }

        public virtual void ShouldSerialize()
        {
            var originalTransactionReceipt = SpawnReceiptExample();
            byte[] transactionReceiptBytes = originalTransactionReceipt.ToBytes();
            var copyTransactionReceipt = TransactionReceipt.FromBytes(transactionReceiptBytes);
            Assert.Equal(copyTransactionReceipt.ToString(), originalTransactionReceipt.ToString());
            
            SnapshotMatcher.Expect(originalTransactionReceipt.ToString()).ToMatchSnapshot();
        }
    }
}