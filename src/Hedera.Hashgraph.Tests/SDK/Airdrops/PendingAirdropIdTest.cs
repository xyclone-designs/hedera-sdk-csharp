// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Airdrops
{
    class PendingAirdropIdTest
    {
        private AccountId sender;
        private AccountId receiver;
        private TokenId tokenId;
        private NftId nftId;
        public virtual void SetUp()
        {
            sender = new AccountId(0, 0, 1001);
            receiver = new AccountId(0, 0, 1002);
            tokenId = new TokenId(0, 0, 1003);
            nftId = new NftId(new TokenId(0, 0, 1004), 1);
        }

        public virtual void TestConstructorWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            Assert.Equal(sender, pendingAirdropId.GetSender());
            Assert.Equal(receiver, pendingAirdropId.GetReceiver());
            Assert.Equal(tokenId, pendingAirdropId.GetTokenId());
            Assert.Null(pendingAirdropId.GetNftId());
        }

        public virtual void TestConstructorWithNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, nftId);
            Assert.Equal(sender, pendingAirdropId.GetSender());
            Assert.Equal(receiver, pendingAirdropId.GetReceiver());
            Assert.Equal(nftId, pendingAirdropId.GetNftId());
            Assert.Null(pendingAirdropId.GetTokenId());
        }

        public virtual void TestSetSender()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetSender(sender);
            Assert.Equal(sender, pendingAirdropId.GetSender());
        }

        public virtual void TestSetReceiver()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetReceiver(receiver);
            Assert.Equal(receiver, pendingAirdropId.GetReceiver());
        }

        public virtual void TestSetTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropIdTokenId = tokenId,;
            Assert.Equal(tokenId, pendingAirdropId.GetTokenId());
        }

        public virtual void TestSetNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetNftId(nftId);
            Assert.Equal(nftId, pendingAirdropId.GetNftId());
        }

        public virtual void TestToProtobufWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            Proto.PendingAirdropId proto = pendingAirdropId.ToProtobuf();
            Assert.NotNull(proto);
            Assert.Equal(sender.ToProtobuf(), proto.GetSenderId());
            Assert.Equal(receiver.ToProtobuf(), proto.GetReceiverId());
            Assert.Equal(tokenId.ToProtobuf(), proto.GetFungibleTokenType());
        }

        public virtual void TestToProtobufWithNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, nftId);
            Proto.PendingAirdropId proto = pendingAirdropId.ToProtobuf();
            Assert.NotNull(proto);
            Assert.Equal(sender.ToProtobuf(), proto.GetSenderId());
            Assert.Equal(receiver.ToProtobuf(), proto.GetReceiverId());
            Assert.Equal(nftId.ToProtobuf(), proto.GetNonFungibleToken());
        }

        public virtual void TestFromProtobufWithTokenId()
        {
            Proto.PendingAirdropId proto = Proto.PendingAirdropId.NewBuilder().SetSenderId(sender.ToProtobuf()).SetReceiverId(receiver.ToProtobuf()).SetFungibleTokenType(tokenId.ToProtobuf()).Build();
            PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            Assert.NotNull(pendingAirdropId);
            Assert.Equal(sender, pendingAirdropId.GetSender());
            Assert.Equal(receiver, pendingAirdropId.GetReceiver());
            Assert.Equal(tokenId, pendingAirdropId.GetTokenId());
            Assert.Null(pendingAirdropId.GetNftId());
        }

        public virtual void TestFromProtobufWithNftId()
        {
            Proto.PendingAirdropId proto = Proto.PendingAirdropId.NewBuilder().SetSenderId(sender.ToProtobuf()).SetReceiverId(receiver.ToProtobuf()).SetNonFungibleToken(nftId.ToProtobuf()).Build();
            PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            Assert.NotNull(pendingAirdropId);
            Assert.Equal(sender, pendingAirdropId.GetSender());
            Assert.Equal(receiver, pendingAirdropId.GetReceiver());
            Assert.Equal(nftId, pendingAirdropId.GetNftId());
            Assert.Null(pendingAirdropId.GetTokenId());
        }

        public virtual void TestToString()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            string result = pendingAirdropId.ToString();
            Assert.True(result.Contains("sender"));
            Assert.True(result.Contains("receiver"));
            Assert.True(result.Contains("tokenId"));
            Assert.True(result.Contains("nftId"));
        }
    }
}