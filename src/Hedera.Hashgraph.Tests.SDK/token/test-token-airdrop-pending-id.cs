// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Airdrops;

namespace Hedera.Hashgraph.Tests.SDK.Airdrops
{
    /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest"]" />
    public class PendingAirdropIdTest
    {
        private AccountId sender;
        private AccountId receiver;
        private TokenId tokenId;
        private NftId nftId;
        public PendingAirdropIdTest()
        {
            sender = new AccountId(0, 0, 1001);
            receiver = new AccountId(0, 0, 1002);
            tokenId = new TokenId(0, 0, 1003);
            nftId = new NftId(new TokenId(0, 0, 1004), 1);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestConstructorWithTokenId"]" />
        public virtual void TestConstructorWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            
            Assert.Equal(sender, pendingAirdropId.Sender);
            Assert.Equal(receiver, pendingAirdropId.Receiver);
            Assert.Equal(tokenId, pendingAirdropId.TokenId);
            Assert.Null(pendingAirdropId.NftId);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestConstructorWithNftId"]" />
        public virtual void TestConstructorWithNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, nftId);

            Assert.Equal(sender, pendingAirdropId.Sender);
            Assert.Equal(receiver, pendingAirdropId.Receiver);
            Assert.Equal(nftId, pendingAirdropId.NftId);
            Assert.Null(pendingAirdropId.TokenId);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestToProtobufWithTokenId"]" />
        public virtual void TestToProtobufWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new (sender, receiver, tokenId);
            Proto.Services.PendingAirdropID proto = pendingAirdropId.ToProtobuf();
            
            Assert.NotNull(proto);
            Assert.Equal(sender.ToProtobuf(), proto.SenderId);
            Assert.Equal(receiver.ToProtobuf(), proto.ReceiverId);
            Assert.Equal(tokenId.ToProtobuf(), proto.FungibleTokenType);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestToProtobufWithNftId"]" />
        public virtual void TestToProtobufWithNftId()
        {
            PendingAirdropId pendingAirdropId = new (sender, receiver, nftId);
            Proto.Services.PendingAirdropID proto = pendingAirdropId.ToProtobuf();
            
            Assert.NotNull(proto);
            Assert.Equal(sender.ToProtobuf(), proto.SenderId);
            Assert.Equal(receiver.ToProtobuf(), proto.ReceiverId);
            Assert.Equal(nftId.ToProtobuf(), proto.NonFungibleToken);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestFromProtobufWithTokenId"]" />
        public virtual void TestFromProtobufWithTokenId()
        {
            Proto.Services.PendingAirdropID proto = new()
			{
				SenderId = sender.ToProtobuf(),
				ReceiverId = receiver.ToProtobuf(),
                FungibleTokenType = tokenId.ToProtobuf(),
            };

			PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            
            Assert.NotNull(pendingAirdropId);
            Assert.Equal(sender, pendingAirdropId.Sender);
            Assert.Equal(receiver, pendingAirdropId.Receiver);
            Assert.Equal(tokenId, pendingAirdropId.TokenId);
            Assert.Null(pendingAirdropId.NftId);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestFromProtobufWithNftId"]" />
        public virtual void TestFromProtobufWithNftId()
        {
            Proto.Services.PendingAirdropID proto = new ()
            {
				SenderId = sender.ToProtobuf(),
				ReceiverId = receiver.ToProtobuf(),
				NonFungibleToken = nftId.ToProtobuf()
            };
            PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            
            Assert.NotNull(pendingAirdropId);
            Assert.Equal(sender, pendingAirdropId.Sender);
            Assert.Equal(receiver, pendingAirdropId.Receiver);
            Assert.Equal(nftId, pendingAirdropId.NftId);
            Assert.Null(pendingAirdropId.TokenId);
        }
        [Fact]
        /// <include file="test-token-airdrop-pending-id.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Airdrops.PendingAirdropIdTest.TestToString"]" />
        public virtual void TestToString()
        {
            PendingAirdropId pendingAirdropId = new (sender, receiver, tokenId);
            string result = pendingAirdropId.ToString();
            
            Assert.Contains("sender", result);
            Assert.Contains("receiver", result);
            Assert.Contains("tokenId", result);
            Assert.Contains("nftId", result);
        }
    }
}
