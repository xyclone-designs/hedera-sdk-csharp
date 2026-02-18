// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.File;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.File
{
    class FileIdTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerializeFromString()
        {
            SnapshotMatcher.Expect(FileId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new FileId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(FileId.FromBytes(new FileId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(FileId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new FileId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        public virtual void GetAddressBookFileIdForReturnsCorrectFileId()
        {
            FileId defaultAddressBook = FileId.GetAddressBookFileIdFor(0, 0);
            Assert.NotNull(defaultAddressBook);
            Assert.Equal(0, defaultAddressBook.Shard);
            Assert.Equal(0, defaultAddressBook.Realm);
            Assert.Equal(102, defaultAddressBook.Num);
            long testShard = 5;
            long testRealm = 10;
            FileId customAddressBook = FileId.GetAddressBookFileIdFor(testShard, testRealm);
            Assert.NotNull(customAddressBook);
            Assert.Equal(testShard, customAddressBook.Shard);
            Assert.Equal(testRealm, customAddressBook.Realm);
            Assert.Equal(102, customAddressBook.Num);
            Assert.Equal("5.10.102", customAddressBook.ToString());
            SnapshotMatcher.Expect(customAddressBook.ToString()).ToMatchSnapshot();
        }

        public virtual void GetFeeScheduleFileIdForReturnsCorrectFileId()
        {
            FileId defaultFeeSchedule = FileId.GetFeeScheduleFileIdFor(0, 0);
            Assert.NotNull(defaultFeeSchedule);
            Assert.Equal(0, defaultFeeSchedule.Shard);
            Assert.Equal(0, defaultFeeSchedule.Realm);
            Assert.Equal(111, defaultFeeSchedule.Num);
            long testShard = 7;
            long testRealm = 12;
            FileId customFeeSchedule = FileId.GetFeeScheduleFileIdFor(testShard, testRealm);
            Assert.NotNull(customFeeSchedule);
            Assert.Equal(testShard, customFeeSchedule.Shard);
            Assert.Equal(testRealm, customFeeSchedule.Realm);
            Assert.Equal(111, customFeeSchedule.Num);
            Assert.Equal("7.12.111", customFeeSchedule.ToString());
            SnapshotMatcher.Expect(customFeeSchedule.ToString()).ToMatchSnapshot();
        }

        public virtual void GetExchangeRatesFileIdForReturnsCorrectFileId()
        {
            FileId defaultExchangeRates = FileId.GetExchangeRatesFileIdFor(0, 0);
            Assert.NotNull(defaultExchangeRates);
            Assert.Equal(0, defaultExchangeRates.Shard);
            Assert.Equal(0, defaultExchangeRates.Realm);
            Assert.Equal(112, defaultExchangeRates.Num);
            long testShard = 3;
            long testRealm = 9;
            FileId customExchangeRates = FileId.GetExchangeRatesFileIdFor(testShard, testRealm);
            Assert.NotNull(customExchangeRates);
            Assert.Equal(testShard, customExchangeRates.Shard);
            Assert.Equal(testRealm, customExchangeRates.Realm);
            Assert.Equal(112, customExchangeRates.Num);
            Assert.Equal("3.9.112", customExchangeRates.ToString());
            SnapshotMatcher.Expect(customExchangeRates.ToString()).ToMatchSnapshot();
        }

        public virtual void TestFileIdFromEvmAddressIncorrectAddress()
        {

            // Test with an EVM address that's too short
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                FileId.FromEvmAddress(0, 0, "abc123");
            });
            Assert.True(exception.Message.Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with an EVM address that's too long
            exception = Assert.Throws<ArgumentException>(() =>
            {
                FileId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            Assert.True(exception.Message.Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with a 0x prefix that gets removed but then is too short
            exception = Assert.Throws<ArgumentException>(() =>
            {
                FileId.FromEvmAddress(0, 0, "0xabc123");
            });
            Assert.True(exception.Message.Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with non-long-zero address
            exception = Assert.Throws<ArgumentException>(() =>
            {
                FileId.FromEvmAddress(0, 0, "742d35Cc6634C0532925a3b844Bc454e4438f44e");
            });
            Assert.True(exception.Message.Contains("EVM address is not a correct long zero address"));
        }

        public virtual void TestFileIdFromEvmAddress()
        {

            // Test with a long zero address representing file 1234
            string evmAddress = "00000000000000000000000000000000000004d2";
            FileId id = FileId.FromEvmAddress(0, 0, evmAddress);
            Assert.Equal(0, id.Shard);
            Assert.Equal(0, id.Realm);
            Assert.Equal(1234, id.Num);

            // Test with a different shard and realm
            id = FileId.FromEvmAddress(1, 1, evmAddress);
            Assert.Equal(1, id.Shard);
            Assert.Equal(1, id.Realm);
            Assert.Equal(1234, id.Num);
        }

        public virtual void TestFileIdToEvmAddress()
        {

            // Test with a normal file ID
            FileId id = new FileId(0, 0, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());

            // Test with a different shard and realm
            id = new FileId(1, 1, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());
        }
    }
}