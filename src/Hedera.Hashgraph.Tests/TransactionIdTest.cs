// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
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

namespace Com.Hedera.Hashgraph.Sdk
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

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(TransactionId.FromString("0.0.23847@1588539964.632521325").ToString()).ToMatchSnapshot();
        }

        virtual void ShouldSerialize2()
        {
            SnapshotMatcher.Expect(TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled/3").ToString()).ToMatchSnapshot();
        }

        virtual void ShouldToBytes()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var copyId = TransactionId.FromProtobuf(originalId.ToProtobuf());
            AssertThat(copyId).HasToString(originalId.ToString());
        }

        virtual void ShouldToBytes2()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled/2");
            var copyId = TransactionId.FromProtobuf(originalId.ToProtobuf());
            AssertThat(copyId).HasToString(originalId.ToString());
        }

        virtual void ShouldFromBytes()
        {
            var originalId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var copyId = TransactionId.FromBytes(originalId.ToProtobuf().ToByteArray());
            AssertThat(copyId).HasToString(originalId.ToString());
        }

        virtual void ShouldParse()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var accountId = Objects.RequireNonNull(transactionId.accountId);
            var validStart = Objects.RequireNonNull(transactionId.validStart);
            Assert.Equal(accountId.shard, 0);
            Assert.Equal(accountId.num, 23847);
            Assert.Equal(validStart.GetEpochSecond(), 1588539964);
            Assert.Equal(validStart.GetNano(), 632521325);
        }

        virtual void ShouldParseScheduled()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            var accountId = Objects.RequireNonNull(transactionId.accountId);
            var validStart = Objects.RequireNonNull(transactionId.validStart);
            Assert.Equal(accountId.shard, 0);
            Assert.Equal(accountId.num, 23847);
            Assert.Equal(validStart.GetEpochSecond(), 1588539964);
            Assert.Equal(validStart.GetNano(), 632521325);
            AssertThat(transactionId.GetScheduled()).IsTrue();
            AssertThat(transactionId.GetNonce()).IsNull();
            Assert.Equal(transactionId.ToString(), "0.0.23847@1588539964.632521325?scheduled");
        }

        virtual void ShouldParseNonce()
        {
            var transactionId = TransactionId.FromString("0.0.23847@1588539964.632521325/4");
            var accountId = Objects.RequireNonNull(transactionId.accountId);
            var validStart = Objects.RequireNonNull(transactionId.validStart);
            Assert.Equal(accountId.shard, 0);
            Assert.Equal(accountId.num, 23847);
            Assert.Equal(validStart.GetEpochSecond(), 1588539964);
            Assert.Equal(validStart.GetNano(), 632521325);
            AssertThat(transactionId.GetScheduled()).IsFalse();
            Assert.Equal(transactionId.GetNonce(), 4);
            Assert.Equal(transactionId.ToString(), "0.0.23847@1588539964.632521325/4");
        }

        virtual void Compare()
        {

            // Compare when only one of the txs is schedules
            var transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            var transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325?scheduled");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare when only one of the txs has accountId
            transactionId1 = new TransactionId(null, Instant.OfEpochSecond(1588539964));
            transactionId2 = new TransactionId(AccountId.FromString("0.0.23847"), Instant.OfEpochSecond(1588539964));
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(AccountId.FromString("0.0.23847"), Instant.OfEpochSecond(1588539964));
            transactionId2 = new TransactionId(null, Instant.OfEpochSecond(1588539964));
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare the AccountIds
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            AssertThat(transactionId1).IsEqualByComparingTo(transactionId2);
            transactionId1 = TransactionId.FromString("0.0.23848@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);
            transactionId1 = TransactionId.FromString("0.0.23847@1588539964.632521325");
            transactionId2 = TransactionId.FromString("0.0.23848@1588539964.632521325");
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);

            // Compare when only one of the txs has valid start
            transactionId1 = new TransactionId(null, null);
            transactionId2 = new TransactionId(null, Instant.OfEpochSecond(1588539964));
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(AccountId.FromString("0.0.23847"), Instant.OfEpochSecond(1588539964));
            transactionId2 = new TransactionId(null, null);
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);

            // Compare the validStarts
            transactionId1 = new TransactionId(null, Instant.OfEpochSecond(1588539965));
            transactionId2 = new TransactionId(null, Instant.OfEpochSecond(1588539964));
            Assert.Equal(transactionId1.CompareTo(transactionId2), 1);
            transactionId1 = new TransactionId(null, Instant.OfEpochSecond(1588539964));
            transactionId2 = new TransactionId(null, Instant.OfEpochSecond(1588539965));
            Assert.Equal(transactionId1.CompareTo(transactionId2), -1);
            transactionId1 = new TransactionId(null, null);
            transactionId2 = new TransactionId(null, null);
            AssertThat(transactionId1).IsEqualByComparingTo(transactionId2);
        }

        virtual void ShouldFail()
        {
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => TransactionId.FromString("0.0.23847.1588539964.632521325/4"));
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => TransactionId.FromString("0.0.23847@1588539964/4"));
        }

        virtual void ShouldAddTrailingZeroesToNanoseconds()
        {
            var txIdString = "0.0.4163533@1681876267.054802581";
            var txId = TransactionId.FromString(txIdString);
            AssertThat(txId).HasToString(txIdString);
        }

        virtual void EqualsHashCodeContractWithNonce()
        {
            AccountId accountId = new AccountId(0, 0, 1000);
            Instant now = Instant.Now();
            TransactionId txnId1 = TransactionId.WithValidStart(accountId, now);
            TransactionId txnId2 = TransactionId.WithValidStart(accountId, now);
            txnId2.SetNonce(0);
            Assert.False(txnId1.Equals(txnId2) && txnId1.GetHashCode() != txnId2.GetHashCode(), "equals/hashCode contract violation: equal objects must have same hashCode");
        }
    }
}