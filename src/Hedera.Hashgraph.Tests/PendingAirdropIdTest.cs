// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class PendingAirdropIdTest
    {
        private AccountId sender;
        private AccountId receiver;
        private TokenId tokenId;
        private NftId nftId;
        virtual void SetUp()
        {
            sender = new AccountId(0, 0, 1001);
            receiver = new AccountId(0, 0, 1002);
            tokenId = new TokenId(0, 0, 1003);
            nftId = new NftId(new TokenId(0, 0, 1004), 1);
        }

        virtual void TestConstructorWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            AssertEquals(sender, pendingAirdropId.GetSender());
            AssertEquals(receiver, pendingAirdropId.GetReceiver());
            AssertEquals(tokenId, pendingAirdropId.GetTokenId());
            Assert.Null(pendingAirdropId.GetNftId());
        }

        virtual void TestConstructorWithNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, nftId);
            AssertEquals(sender, pendingAirdropId.GetSender());
            AssertEquals(receiver, pendingAirdropId.GetReceiver());
            AssertEquals(nftId, pendingAirdropId.GetNftId());
            Assert.Null(pendingAirdropId.GetTokenId());
        }

        virtual void TestSetSender()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetSender(sender);
            AssertEquals(sender, pendingAirdropId.GetSender());
        }

        virtual void TestSetReceiver()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetReceiver(receiver);
            AssertEquals(receiver, pendingAirdropId.GetReceiver());
        }

        virtual void TestSetTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetTokenId(tokenId);
            AssertEquals(tokenId, pendingAirdropId.GetTokenId());
        }

        virtual void TestSetNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId();
            pendingAirdropId.SetNftId(nftId);
            AssertEquals(nftId, pendingAirdropId.GetNftId());
        }

        virtual void TestToProtobufWithTokenId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            com.hedera.hashgraph.sdk.proto.PendingAirdropId proto = pendingAirdropId.ToProtobuf();
            AssertNotNull(proto);
            AssertEquals(sender.ToProtobuf(), proto.GetSenderId());
            AssertEquals(receiver.ToProtobuf(), proto.GetReceiverId());
            AssertEquals(tokenId.ToProtobuf(), proto.GetFungibleTokenType());
        }

        virtual void TestToProtobufWithNftId()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, nftId);
            com.hedera.hashgraph.sdk.proto.PendingAirdropId proto = pendingAirdropId.ToProtobuf();
            AssertNotNull(proto);
            AssertEquals(sender.ToProtobuf(), proto.GetSenderId());
            AssertEquals(receiver.ToProtobuf(), proto.GetReceiverId());
            AssertEquals(nftId.ToProtobuf(), proto.GetNonFungibleToken());
        }

        virtual void TestFromProtobufWithTokenId()
        {
            com.hedera.hashgraph.sdk.proto.PendingAirdropId proto = com.hedera.hashgraph.sdk.proto.PendingAirdropId.NewBuilder().SetSenderId(sender.ToProtobuf()).SetReceiverId(receiver.ToProtobuf()).SetFungibleTokenType(tokenId.ToProtobuf()).Build();
            PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            AssertNotNull(pendingAirdropId);
            AssertEquals(sender, pendingAirdropId.GetSender());
            AssertEquals(receiver, pendingAirdropId.GetReceiver());
            AssertEquals(tokenId, pendingAirdropId.GetTokenId());
            Assert.Null(pendingAirdropId.GetNftId());
        }

        virtual void TestFromProtobufWithNftId()
        {
            com.hedera.hashgraph.sdk.proto.PendingAirdropId proto = com.hedera.hashgraph.sdk.proto.PendingAirdropId.NewBuilder().SetSenderId(sender.ToProtobuf()).SetReceiverId(receiver.ToProtobuf()).SetNonFungibleToken(nftId.ToProtobuf()).Build();
            PendingAirdropId pendingAirdropId = PendingAirdropId.FromProtobuf(proto);
            AssertNotNull(pendingAirdropId);
            AssertEquals(sender, pendingAirdropId.GetSender());
            AssertEquals(receiver, pendingAirdropId.GetReceiver());
            AssertEquals(nftId, pendingAirdropId.GetNftId());
            Assert.Null(pendingAirdropId.GetTokenId());
        }

        virtual void TestToString()
        {
            PendingAirdropId pendingAirdropId = new PendingAirdropId(sender, receiver, tokenId);
            string result = pendingAirdropId.ToString();
            AssertTrue(result.Contains("sender"));
            AssertTrue(result.Contains("receiver"));
            AssertTrue(result.Contains("tokenId"));
            AssertTrue(result.Contains("nftId"));
        }
    }
}