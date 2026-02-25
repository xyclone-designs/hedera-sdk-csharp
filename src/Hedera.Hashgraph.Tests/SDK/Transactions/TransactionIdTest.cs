// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Account;

using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.SDK.Transactions
{
    class TransactionIdTest
    {
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
            SnapshotMatcher.Expect(TransactionId.FromString("0.0.23847@1588539964.632521325").ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled/3").ToString()).ToMatchSnapshot();
        }

        public virtual void ShouldToBytes()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var copyId = TransactionId.FromProtobuf(originalId.ToProtobuf());
            Assert.Equal(copyId.ToString(), originalId.ToString());
        }

        public virtual void ShouldToBytes2()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled/2");
            var copyId = TransactionId.FromProtobuf(originalId.ToProtobuf());
            Assert.Equal(copyId.ToString(), originalId.ToString());
        }

        public virtual void ShouldFromBytes()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var copyId = TransactionId.FromBytes(originalId.ToProtobuf().ToByteArray());
            Assert.Equal(copyId.ToString(), originalId.ToString());
        }

        public virtual void ShouldParse()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var accountId = transactionId.AccountId;
            var validStart = transactionId.ValidStart;
            Assert.Equal(accountId.Shard, 0);
            Assert.Equal(accountId.Num, 23847);
            Assert.Equal(validStart.ToDateTimeOffset().ToUnixTimeSeconds(), 1588539964);
            Assert.Equal(validStart.Nanos, 632521325);
        }

        public virtual void ShouldParseScheduled()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            var accountId = transactionId.AccountId;
            var validStart = transactionId.ValidStart;
            Assert.Equal(accountId.Shard, 0);
            Assert.Equal(accountId.Num, 23847);
            Assert.Equal(validStart.ToDateTimeOffset().ToUnixTimeSeconds(), 1588539964);
            Assert.Equal(validStart.Nanos, 632521325);
            Assert.True(transactionId.Scheduled);
            Assert.Null(transactionId.Nonce);
            Assert.Equal(transactionId.ToString(), "0.0.23847@1588539964.632521325?scheduled");
        }

        public virtual void ShouldParseNonce()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325/4");
            var accountId = transactionId.AccountId;
            var validStart = transactionId.ValidStart;
            Assert.Equal(accountId.Shard, 0);
            Assert.Equal(accountId.Num, 23847);
            Assert.Equal(validStart.ToDateTimeOffset().ToUnixTimeSeconds(), 1588539964);
            Assert.Equal(validStart.Nanos, 632521325);
            Assert.False(transactionId.Scheduled);
            Assert.Equal(transactionId.Nonce, 4);
            Assert.Equal(transactionId.ToString(), "0.0.23847@1588539964.632521325/4");
        }

        public virtual void Compare()
        {

            // Compare when only one of the txs is schedules
            var transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare when only one of the txs has accountId
            transactionId1 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            transactionId2 = new TransactionId(AccountId.FromString("0.0.23847"), DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(AccountId.FromString("0.0.23847"), DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            transactionId2 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare the AccountIds
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            Assert.Equal(transactionId1, transactionId2);
            transactionId1 = TransactionId.FromString("0.0.23848@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23848@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);

            // Compare when only one of the txs has valid start
            transactionId1 = new TransactionId(null, null);
            transactionId2 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(AccountId.FromString("0.0.23847"), DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            transactionId2 = new TransactionId(null, null);
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare the validStarts
            transactionId1 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539965).ToTimestamp());
            transactionId2 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);
            transactionId1 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539964).ToTimestamp());
            transactionId2 = new TransactionId(null, DateTimeOffset.FromUnixTimeMilliseconds(1588539965).ToTimestamp());
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(null, null);
            transactionId2 = new TransactionId(null, null);
            Assert.Equal(transactionId1, transactionId2);
        }

        public virtual void ShouldFail()
        {
            Assert.Throws<ArgumentException>(() => TransactionId.FromString("0.0.23847.1588539964.632521325/4"));
            Assert.Throws<ArgumentException>(() => TransactionId.FromString("0.0.23847@1588539964/4"));
        }

        public virtual void ShouldAddTrailingZeroesToNanoseconds()
        {
            var txIdString = "0.0.4163533@1681876267.054802581";
            var txId = TransactionId.FromString(txIdString);
            Assert.Equal(txId.ToString(), txIdString);
        }

        public virtual void EqualsHashCodeContractWithNonce()
        {
            AccountId accountId = new AccountId(0, 0, 1000);
            Timestamp now = DateTimeOffset.UtcNow.ToTimestamp();
            TransactionId txnId1 = TransactionId.WithValidStart(accountId, now);
            TransactionId txnId2 = TransactionId.WithValidStart(accountId, now);
            txnId2.Nonce = 0;
            Assert.False(txnId1.Equals(txnId2) && txnId1.GetHashCode() != txnId2.GetHashCode(), "equals/hashCode contract violation: equal objects must have same hashCode");
        }
    }
}