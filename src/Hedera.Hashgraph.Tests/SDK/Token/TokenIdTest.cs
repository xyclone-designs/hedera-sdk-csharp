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

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    class TokenIdTest
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
            SnapshotMatcher.Expect(TokenId.FromString("0.0.5005").ToString()).ToMatchSnapshot();
        }

        public virtual void ToBytes()
        {
            SnapshotMatcher.Expect(Hex.ToHexString(new TokenId(0, 0, 5005).ToBytes())).ToMatchSnapshot();
        }

        public virtual void FromBytes()
        {
            SnapshotMatcher.Expect(TokenId.FromBytes(new TokenId(0, 0, 5005).ToBytes()).ToString()).ToMatchSnapshot();
        }

        public virtual void FromSolidityAddress()
        {
            SnapshotMatcher.Expect(TokenId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString()).ToMatchSnapshot();
        }

        public virtual void ToSolidityAddress()
        {
            SnapshotMatcher.Expect(new TokenId(0, 0, 5005).ToSolidityAddress()).ToMatchSnapshot();
        }

        public virtual void UnitTokenIdFromString()
        {
            TokenId tokenId = new TokenId(1, 2, 3);
            TokenId tokenIdFromString = TokenId.FromString(tokenId.ToString());
            Assert.Equal(tokenId, tokenIdFromString);
        }

        public virtual void UnitTokenIdChecksumFromString()
        {
            TokenId tokenId = TokenId.FromString("0.0.123");
            Client client = Client.ForTestnet();
            tokenId.ToStringWithChecksum(client);
            string sol = tokenId.ToSolidityAddress();
            TokenId.FromEvmAddress(0, 0, sol);
            tokenId.Validate(client);

            // Test protobuf conversion
            var pb = tokenId.ToProtobuf();
            TokenId.FromProtobuf(pb);

            // Test bytes conversion
            byte[] idBytes = tokenId.ToBytes();
            TokenId.FromBytes(idBytes);

            // Test comparison
            tokenId.CompareTo(new TokenId(0, 0, 32));
            Assert.Equal(123, tokenId.num);
        }

        public virtual void UnitTokenIdChecksumToString()
        {
            TokenId id = new TokenId(50, 150, 520);
            Assert.Equal("50.150.520", id.ToString());
        }

        public virtual void UnitTokenIdFromStringEVM()
        {
            TokenId id = TokenId.FromString("0.0.434");
            Assert.Equal("0.0.434", id.ToString());
        }

        public virtual void UnitTokenIdProtobuf()
        {
            TokenId tokenId = TokenId.FromString("0.0.434");
            var pb = tokenId.ToProtobuf();
            Assert.Equal(0, pb.GetShardNum());
            Assert.Equal(0, pb.GetRealmNum());
            Assert.Equal(434, pb.GetTokenNum());
            TokenId pbFrom = TokenId.FromProtobuf(pb);
            Assert.Equal(tokenId, pbFrom);
        }

        public virtual void TestTokenIdFromEvmAddressIncorrectAddress()
        {

            // Test with an EVM address that's too short
            ArgumentException exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TokenId.FromEvmAddress(0, 0, "abc123");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with an EVM address that's too long
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TokenId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with a 0x prefix that gets removed but then is too short
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TokenId.FromEvmAddress(0, 0, "0xabc123");
            });
            Assert.True(exception.GetMessage().Contains("Solidity addresses must be 20 bytes or 40 hex chars"));

            // Test with non-long-zero address
            exception = AssertThrows(typeof(ArgumentException), () =>
            {
                TokenId.FromEvmAddress(0, 0, "742d35Cc6634C0532925a3b844Bc454e4438f44e");
            });
            Assert.True(exception.GetMessage().Contains("EVM address is not a correct long zero address"));
        }

        public virtual void TestTokenIdFromEvmAddress()
        {

            // Test with a long zero address representing token 1234
            string evmAddress = "00000000000000000000000000000000000004d2";
            TokenId tokenId = TokenId.FromEvmAddress(0, 0, evmAddress);
            Assert.Equal(0, tokenId.shard);
            Assert.Equal(0, tokenId.realm);
            Assert.Equal(1234, tokenId.num);

            // Test with a different shard and realm
            tokenId = TokenId.FromEvmAddress(1, 1, evmAddress);
            Assert.Equal(1, tokenId.shard);
            Assert.Equal(1, tokenId.realm);
            Assert.Equal(1234, tokenId.num);
        }

        public virtual void TestTokenIdToEvmAddress()
        {

            // Test with a normal token ID
            TokenId id = new TokenId(0, 0, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());

            // Test with a different shard and realm
            id = new TokenId(1, 1, 123);
            Assert.Equal("000000000000000000000000000000000000007b", id.ToEvmAddress());
        }
    }
}