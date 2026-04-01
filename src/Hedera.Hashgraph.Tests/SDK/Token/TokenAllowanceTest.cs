// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenAllowanceTest
    {
        private static readonly TokenId testTokenId = TokenId.FromString("0.6.9");
        private static readonly AccountId testOwnerAccountId = AccountId.FromString("8.8.8");
        private static readonly AccountId testSpenderAccountId = AccountId.FromString("7.7.7");
        private static readonly long testAmount = 4;
        [Fact]
        public virtual void ConstructWithTokenIdOwnerSpenderAmount()
        {
            TokenAllowance tokenAllowance = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount);
            Assert.Equal(tokenAllowance.TokenId, testTokenId);
            Assert.Equal(tokenAllowance.OwnerAccountId, testOwnerAccountId);
            Assert.Equal(tokenAllowance.SpenderAccountId, testSpenderAccountId);
            Assert.Equal(tokenAllowance.Amount, testAmount);
        }

        [Fact]
        public virtual void FromProtobuf()
        {
            var tokenAllowanceProtobuf = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount).ToProtobuf();
            var tokenAllowance = TokenAllowance.FromProtobuf(tokenAllowanceProtobuf);
            Assert.Equal(tokenAllowance.TokenId, testTokenId);
            Assert.Equal(tokenAllowance.OwnerAccountId, testOwnerAccountId);
            Assert.Equal(tokenAllowance.SpenderAccountId, testSpenderAccountId);
            Assert.Equal(tokenAllowance.Amount, testAmount);
        }

        [Fact]
        public virtual void ToProtobuf()
        {
            var tokenAllowanceProtobuf = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount).ToProtobuf();
            Assert.True(tokenAllowanceProtobuf.TokenId is not null);
            Assert.Equal(TokenId.FromProtobuf(tokenAllowanceProtobuf.TokenId), testTokenId);
            Assert.True(tokenAllowanceProtobuf.Owner is not null);
            Assert.Equal(AccountId.FromProtobuf(tokenAllowanceProtobuf.Owner), testOwnerAccountId);
            Assert.True(tokenAllowanceProtobuf.Spender is not null);
            Assert.Equal(AccountId.FromProtobuf(tokenAllowanceProtobuf.Spender), testSpenderAccountId);
        }
    }
}