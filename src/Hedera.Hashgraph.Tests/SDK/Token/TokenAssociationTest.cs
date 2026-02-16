// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenAssociationTest
    {
        private static readonly AccountId testAccountId = AccountId.FromString("4.2.0");
        private static readonly TokenId testTokenId = TokenId.FromString("0.6.9");
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual TokenAssociation SpawnTokenAssociationExample()
        {
            return new TokenAssociation(TokenId.FromString("1.2.3"), AccountId.FromString("4.5.6"));
        }

        public virtual void ShouldSerializeAccount()
        {
            var originalTokenAssociation = SpawnTokenAssociationExample();
            byte[] tokenAssociationBytes = originalTokenAssociation.ToBytes();
            var copyTokenAssociation = TokenAssociation.FromBytes(tokenAssociationBytes);
            Assert.Equal(copyTokenAssociation.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalTokenAssociation.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalTokenAssociation.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }

        public virtual void FromProtobuf()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            var tokenAssociation = TokenAssociation.FromProtobuf(tokenAssociationProtobuf);
            Assert.Equal(tokenAssociation.accountId, testAccountId);
            Assert.Equal(tokenAssociation.tokenId, testTokenId);
        }

        public virtual void ToProtobuf()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            Assert.True(tokenAssociationProtobuf.HasAccountId());
            Assert.Equal(tokenAssociationProtobuf.GetAccountId().GetShardNum(), testAccountId.shard);
            Assert.Equal(tokenAssociationProtobuf.GetAccountId().GetRealmNum(), testAccountId.realm);
            Assert.Equal(tokenAssociationProtobuf.GetAccountId().GetAccountNum(), testAccountId.num);
            Assert.True(tokenAssociationProtobuf.HasTokenId());
            Assert.Equal(tokenAssociationProtobuf.GetTokenId().GetShardNum(), testTokenId.shard);
            Assert.Equal(tokenAssociationProtobuf.GetTokenId().GetRealmNum(), testTokenId.realm);
            Assert.Equal(tokenAssociationProtobuf.GetTokenId().GetTokenNum(), testTokenId.num);
        }

        public virtual void FromBytes()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            var tokenAssociation = TokenAssociation.FromBytes(tokenAssociationProtobuf.ToByteArray());
            Assert.Equal(tokenAssociation.accountId, testAccountId);
            Assert.Equal(tokenAssociation.tokenId, testTokenId);
        }

        public virtual void ToBytes()
        {
            var tokenAssociation = new TokenAssociation(testTokenId, testAccountId);
            var bytes = tokenAssociation.ToBytes();
            Assert.Equal(bytes, tokenAssociation.ToProtobuf().ToByteArray());
        }
    }
}