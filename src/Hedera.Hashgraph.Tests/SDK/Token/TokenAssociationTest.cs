// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;

using Google.Protobuf;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenAssociationTest
    {
        private static readonly AccountId testAccountId = AccountId.FromString("4.2.0");
        private static readonly TokenId testTokenId = TokenId.FromString("0.6.9");

        public virtual TokenAssociation SpawnTokenAssociationExample()
        {
            return new TokenAssociation(TokenId.FromString("1.2.3"), AccountId.FromString("4.5.6"));
        }

        public virtual void ShouldSerializeAccount()
        {
            var originalTokenAssociation = SpawnTokenAssociationExample();
            byte[] tokenAssociationBytes = originalTokenAssociation.ToBytes();
            var copyTokenAssociation = TokenAssociation.FromBytes(tokenAssociationBytes);
            
            Assert.Equal(Regex.Replace(copyTokenAssociation.ToString(), "@[A-Za-z0-9]+", ""), Regex.Replace(originalTokenAssociation.ToString(), "@[A-Za-z0-9]+", ""));
            
            Verifier.Verify(Regex.Replace(originalTokenAssociation.ToString(), "@[A-Za-z0-9]+", ""));
        }
        [Fact]
        public virtual void FromProtobuf()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            var tokenAssociation = TokenAssociation.FromProtobuf(tokenAssociationProtobuf);
            
            Assert.Equal(tokenAssociation.AccountId, testAccountId);
            Assert.Equal(tokenAssociation.TokenId, testTokenId);
        }
        [Fact]
        public virtual void ToProtobuf()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            
            Assert.True(tokenAssociationProtobuf.AccountId is not null);
            Assert.Equal(tokenAssociationProtobuf.AccountId.ShardNum, testAccountId.Shard);
            Assert.Equal(tokenAssociationProtobuf.AccountId.RealmNum, testAccountId.Realm);
            Assert.Equal(tokenAssociationProtobuf.AccountId.AccountNum, testAccountId.Num);
            Assert.True(tokenAssociationProtobuf.TokenId is not null);
            Assert.Equal(tokenAssociationProtobuf.TokenId.ShardNum, testTokenId.Shard);
            Assert.Equal(tokenAssociationProtobuf.TokenId.RealmNum, testTokenId.Realm);
            Assert.Equal(tokenAssociationProtobuf.TokenId.TokenNum, testTokenId.Num);
        }
        [Fact]
        public virtual void FromBytes()
        {
            var tokenAssociationProtobuf = new TokenAssociation(testTokenId, testAccountId).ToProtobuf();
            var tokenAssociation = TokenAssociation.FromBytes(tokenAssociationProtobuf.ToByteArray());
            
            Assert.Equal(tokenAssociation.AccountId, testAccountId);
            Assert.Equal(tokenAssociation.TokenId, testTokenId);
        }
        [Fact]
        public virtual void ToBytes()
        {
            var tokenAssociation = new TokenAssociation(testTokenId, testAccountId);
            var bytes = tokenAssociation.ToBytes();
            
            Assert.Equal(bytes, tokenAssociation.ToProtobuf().ToByteArray());
        }
    }
}