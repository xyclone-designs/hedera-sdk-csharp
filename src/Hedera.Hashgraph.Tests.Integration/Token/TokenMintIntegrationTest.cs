// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenMintIntegrationTest
    {
        public virtual void CanMintTokens()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var receipt = new TokenMintTransaction().SetAmount(10).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.totalSupply, 1000000 + 10);
            }
        }

        public virtual void CannotMintMoreThanMaxSupply()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(5).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetTokenId(tokenId).SetAmount(6).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_MAX_SUPPLY_REACHED.ToString());
            }
        }

        public virtual void CannotMintTokensWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenMintTransaction().SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TOKEN_ID.ToString());
            }
        }

        public virtual void CanMintTokensWhenAmountIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var receipt = new TokenMintTransaction().SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
            }
        }

        public virtual void CannotMintTokensWhenSupplyKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(key).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetTokenId(tokenId).SetAmount(10).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanMintNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var receipt = new TokenMintTransaction().SetMetadata(NftMetadataGenerator.Generate((byte)10)).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(receipt.serials.Count, 10);
            }
        }

        public virtual void CannotMintNftsIfMetadataTooBig()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenMintTransaction().SetMetadata(NftMetadataGenerator.GenerateOneLarge()).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.METADATA_TOO_LONG.ToString());
            }
        }
    }
}