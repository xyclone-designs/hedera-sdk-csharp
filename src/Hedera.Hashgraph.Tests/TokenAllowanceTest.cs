// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class TokenAllowanceTest
    {
        private static readonly TokenId testTokenId = TokenId.FromString("0.6.9");
        private static readonly AccountId testOwnerAccountId = AccountId.FromString("8.8.8");
        private static readonly AccountId testSpenderAccountId = AccountId.FromString("7.7.7");
        private static readonly long testAmount = 4;
        virtual void ConstructWithTokenIdOwnerSpenderAmount()
        {
            TokenAllowance tokenAllowance = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount);
            Assert.Equal(tokenAllowance.tokenId, testTokenId);
            Assert.Equal(tokenAllowance.ownerAccountId, testOwnerAccountId);
            Assert.Equal(tokenAllowance.spenderAccountId, testSpenderAccountId);
            Assert.Equal(tokenAllowance.amount, testAmount);
        }

        virtual void FromProtobuf()
        {
            var tokenAllowanceProtobuf = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount).ToProtobuf();
            var tokenAllowance = TokenAllowance.FromProtobuf(tokenAllowanceProtobuf);
            Assert.Equal(tokenAllowance.tokenId, testTokenId);
            Assert.Equal(tokenAllowance.ownerAccountId, testOwnerAccountId);
            Assert.Equal(tokenAllowance.spenderAccountId, testSpenderAccountId);
            Assert.Equal(tokenAllowance.amount, testAmount);
        }

        virtual void ToProtobuf()
        {
            var tokenAllowanceProtobuf = new TokenAllowance(testTokenId, testOwnerAccountId, testSpenderAccountId, testAmount).ToProtobuf();
            AssertTrue(tokenAllowanceProtobuf.HasTokenId());
            Assert.Equal(TokenId.FromProtobuf(tokenAllowanceProtobuf.GetTokenId()), testTokenId);
            AssertTrue(tokenAllowanceProtobuf.HasOwner());
            Assert.Equal(AccountId.FromProtobuf(tokenAllowanceProtobuf.GetOwner()), testOwnerAccountId);
            AssertTrue(tokenAllowanceProtobuf.HasSpender());
            Assert.Equal(AccountId.FromProtobuf(tokenAllowanceProtobuf.GetSpender()), testSpenderAccountId);
        }
    }
}