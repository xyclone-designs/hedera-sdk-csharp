// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Org.Bouncycastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
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

        virtual void ShouldSerializeFromString()
        {
            SnapshotMatcher.Expect(FileId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new FileId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        virtual void FromBytes()
        {
            SnapshotMatcher.Expect(FileId.FromBytes(new FileId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(FileId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new FileId(0, 0, 5005).ToEvmAddress()).ToMatchSnapshot();
        }

        virtual void GetAddressBookFileIdForReturnsCorrectFileId()
        {
            FileId defaultAddressBook = FileId.GetAddressBookFileIdFor(0, 0);
            AssertNotNull(defaultAddressBook);
            AssertEquals(0, defaultAddressBook.shard);
            AssertEquals(0, defaultAddressBook.realm);
            AssertEquals(102, defaultAddressBook.num);
            long testShard = 5;
            long testRealm = 10;
            FileId customAddressBook = FileId.GetAddressBookFileIdFor(testShard, testRealm);
            AssertNotNull(customAddressBook);
            AssertEquals(testShard, customAddressBook.shard);
            AssertEquals(testRealm, customAddressBook.realm);
            AssertEquals(102, customAddressBook.num);
            AssertEquals("5.10.102", customAddressBook.ToString());
            SnapshotMatcher.Expect(customAddressBook.ToString()).ToMatchSnapshot();
        }

        virtual void GetFeeScheduleFileIdForReturnsCorrectFileId()
        {
            FileId defaultFeeSchedule = FileId.GetFeeScheduleFileIdFor(0, 0);
            AssertNotNull(defaultFeeSchedule);
            AssertEquals(0, defaultFeeSchedule.shard);
            AssertEquals(0, defaultFeeSchedule.realm);
            AssertEquals(111, defaultFeeSchedule.num);
            long testShard = 7;
            long testRealm = 12;
            FileId customFeeSchedule = FileId.GetFeeScheduleFileIdFor(testShard, testRealm);
            AssertNotNull(customFeeSchedule);
            AssertEquals(testShard, customFeeSchedule.shard);
            AssertEquals(testRealm, customFeeSchedule.realm);
            AssertEquals(111, customFeeSchedule.num);
            AssertEquals("7.12.111", customFeeSchedule.ToString());
            SnapshotMatcher.Expect(customFeeSchedule.ToString()).ToMatchSnapshot();
        }

        virtual void GetExchangeRatesFileIdForReturnsCorrectFileId()
        {
            FileId defaultExchangeRates = FileId.GetExchangeRatesFileIdFor(0, 0);
            AssertNotNull(defaultExchangeRates);
            AssertEquals(0, defaultExchangeRates.shard);
            AssertEquals(0, defaultExchangeRates.realm);
            AssertEquals(112, defaultExchangeRates.num);
            long testShard = 3;
            long testRealm = 9;
            FileId customExchangeRates = FileId.GetExchangeRatesFileIdFor(testShard, testRealm);
            AssertNotNull(customExchangeRates);
            AssertEquals(testShard, customExchangeRates.shard);
            AssertEquals(testRealm, customExchangeRates.realm);
            AssertEquals(112, customExchangeRates.num);
            AssertEquals("3.9.112", customExchangeRates.ToString());
            SnapshotMatcher.Expect(customExchangeRates.ToString()).ToMatchSnapshot();
        }

        virtual void TestFileIdFromEvmAddressIncorrectAddress()
        {

            // Test with an EVM address that's too short
            ArgumentException exception = AssertThrows(typeof(ArgumentException), () =>
            {
                FileId.FromEvmAddress(0, 0, "abc123");
            });
            AssertTrue(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with an EVM address that's too long
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                FileId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            AssertTrue(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with a 0x prefix that gets removed but then is too short
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                FileId.FromEvmAddress(0, 0, "0xabc123");
            });
            AssertTrue(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with non-long-zero address
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                FileId.FromEvmAddress(0, 0, "742d35Cc6634C0532925a3b844Bc454e4438f44e");
            });
            AssertTrue(exception.GetMessage().Contains("EVM address is not a correct long zero address"));
        }

        virtual void TestFileIdFromEvmAddress()
        {

            // Test with a long zero address representing file 1234
            string evmAddress = "00000000000000000000000000000000000004d2";
            FileId id = FileId.FromEvmAddress(0, 0, evmAddress);
            AssertEquals(0, id.shard);
            AssertEquals(0, id.realm);
            AssertEquals(1234, id.num);

            // Test with a different shard and realm
            id = FileId.FromEvmAddress(1, 1, evmAddress);
            AssertEquals(1, id.shard);
            AssertEquals(1, id.realm);
            AssertEquals(1234, id.num);
        }

        virtual void TestFileIdToEvmAddress()
        {

            // Test with a normal file ID
            FileId id = new FileId(0, 0, 123);
            AssertEquals("000000000000000000000000000000000000007b", id.ToEvmAddress());

            // Test with a different shard and realm
            id = new FileId(1, 1, 123);
            AssertEquals("000000000000000000000000000000000000007b", id.ToEvmAddress());
        }
    }
}