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

namespace Hedera.Hashgraph.Tests.SDK.Topic
{
    class TopicIdTest
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
            SnapshotMatcher.Expect(TopicId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new TopicId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(TopicId.FromBytes(new TopicId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(TopicId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new TokenId(0, 0, 5005).ToSolidityAddress()).ToMatchSnapshot();
        }

        public virtual void TestTopicIdFromEvmAddressIncorrectAddress()
        {

            // Test with an EVM address that's too short
            ArgumentException exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TopicId.FromEvmAddress(0, 0, "abc123");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with an EVM address that's too long
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TopicId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with a 0x prefix that gets removed but then is too short
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TopicId.FromEvmAddress(0, 0, "0xabc123");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with non-long-zero address
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TopicId.FromEvmAddress(0, 0, "742d35Cc6634C0532925a3b844Bc454e4438f44e");
            });
            Assert.True(exception.GetMessage().Contains("EVM address is not a correct long zero address"));
        }

        public virtual void TestTopicIdFromEvmAddress()
        {

            // Test with a long zero address representing topic 1234
            string evmAddress = "00000000000000000000000000000000000004d2";
            TopicId id = TopicId.FromEvmAddress(0, 0, evmAddress);
            Assert.Equal(0, id.shard);
            Assert.Equal(0, id.realm);
            Assert.Equal(1234, id.num);

            // Test with a different shard and realm
            id = TopicId.FromEvmAddress(1, 1, evmAddress);
            Assert.Equal(1, id.shard);
            Assert.Equal(1, id.realm);
            Assert.Equal(1234, id.num);
        }

        public virtual void TestTopicIdToEvmAddress()
        {

            // Test with a normal topic ID
            TopicId id = new TopicId(0, 0, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());

            // Test with a different shard and realm
            id = new TopicId(1, 1, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());
        }
    }
}