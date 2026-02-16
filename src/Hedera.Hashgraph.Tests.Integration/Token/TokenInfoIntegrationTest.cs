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
    class TokenInfoIntegrationTest
    {
        public virtual void CanQueryTokenInfoWhenAllKeysAreDifferent()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key1 = PrivateKey.GenerateED25519();
                var key2 = PrivateKey.GenerateED25519();
                var key3 = PrivateKey.GenerateED25519();
                var key4 = PrivateKey.GenerateED25519();
                var key5 = PrivateKey.GenerateED25519();
                var key6 = PrivateKey.GenerateED25519();
                var key7 = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(key1).SetFreezeKey(key2).SetWipeKey(key3).SetKycKey(key4).SetSupplyKey(key5).SetPauseKey(key6).SetMetadataKey(key7).SetFreezeDefault(false).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "ffff");
                Assert.Equal(info.symbol, "F");
                Assert.Equal(info.decimals, 3);
                Assert.Equal(info.treasuryAccountId, testEnv.operatorId);
                AssertThat(info.adminKey).IsNotNull();
                AssertThat(info.freezeKey).IsNotNull();
                AssertThat(info.wipeKey).IsNotNull();
                AssertThat(info.kycKey).IsNotNull();
                AssertThat(info.supplyKey).IsNotNull();
                AssertThat(info.pauseKey).IsNotNull();
                AssertThat(info.metadataKey).IsNotNull();
                Assert.Equal(info.adminKey.ToString(), key1.GetPublicKey().ToString());
                Assert.Equal(info.freezeKey.ToString(), key2.GetPublicKey().ToString());
                Assert.Equal(info.wipeKey.ToString(), key3.GetPublicKey().ToString());
                Assert.Equal(info.kycKey.ToString(), key4.GetPublicKey().ToString());
                Assert.Equal(info.supplyKey.ToString(), key5.GetPublicKey().ToString());
                Assert.Equal(info.pauseKey.ToString(), key6.GetPublicKey().ToString());
                Assert.Equal(info.metadataKey.ToString(), key7.GetPublicKey().ToString());
                AssertThat(info.defaultFreezeStatus).IsNotNull();
                AssertThat(info.defaultFreezeStatus).IsFalse();
                AssertThat(info.defaultKycStatus).IsNotNull();
                AssertThat(info.defaultKycStatus).IsFalse();
                Assert.Equal(info.tokenType, TokenType.FUNGIBLE_COMMON);
                Assert.Equal(info.supplyType, TokenSupplyType.INFINITE);
                new TokenDeleteTransaction().SetTokenId(tokenId).FreezeWith(testEnv.client).Sign(key1).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanQueryTokenInfoWhenTokenIsCreatedWithMinimalProperties()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "ffff");
                Assert.Equal(info.symbol, "F");
                Assert.Equal(info.decimals, 0);
                Assert.Equal(info.totalSupply, 0);
                Assert.Equal(info.treasuryAccountId, testEnv.operatorId);
                AssertThat(info.adminKey).IsNull();
                AssertThat(info.freezeKey).IsNull();
                AssertThat(info.wipeKey).IsNull();
                AssertThat(info.kycKey).IsNull();
                AssertThat(info.supplyKey).IsNull();
                AssertThat(info.pauseKey).IsNull();
                AssertThat(info.metadataKey).IsNull();
                AssertThat(info.defaultFreezeStatus).IsNull();
                AssertThat(info.defaultKycStatus).IsNull();
                Assert.Equal(info.tokenType, TokenType.FUNGIBLE_COMMON);
                Assert.Equal(info.supplyType, TokenSupplyType.INFINITE);
            }
        }

        public virtual void CanQueryNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(5000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(NftMetadataGenerator.Generate((byte)10)).Execute(testEnv.client).GetReceipt(testEnv.client);
                Assert.Equal(mintReceipt.serials.Count, 10);
                var info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "ffff");
                Assert.Equal(info.symbol, "F");
                Assert.Equal(info.decimals, 0);
                Assert.Equal(info.totalSupply, 10);
                Assert.Equal(testEnv.operatorId, info.treasuryAccountId);
                AssertThat(info.adminKey).IsNotNull();
                AssertThat(info.freezeKey).IsNull();
                AssertThat(info.wipeKey).IsNull();
                AssertThat(info.kycKey).IsNull();
                AssertThat(info.supplyKey).IsNotNull();
                AssertThat(info.pauseKey).IsNull();
                AssertThat(info.metadataKey).IsNull();
                AssertThat(info.defaultFreezeStatus).IsNull();
                AssertThat(info.defaultKycStatus).IsNull();
                Assert.Equal(info.tokenType, TokenType.NON_FUNGIBLE_UNIQUE);
                Assert.Equal(info.supplyType, TokenSupplyType.FINITE);
                Assert.Equal(info.maxSupply, 5000);
            }
        }

        public virtual void GetCostQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var infoQuery = new TokenInfoQuery().SetTokenId(tokenId);
                var cost = infoQuery.GetCost(testEnv.client);
                infoQuery.SetQueryPayment(cost).Execute(testEnv.client);
            }
        }

        public virtual void GetCostBigMaxQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var infoQuery = new TokenInfoQuery().SetTokenId(tokenId).SetMaxQueryPayment(new Hbar(1000));
                var cost = infoQuery.GetCost(testEnv.client);
                infoQuery.SetQueryPayment(cost).Execute(testEnv.client);
            }
        }

        public virtual void GetCostSmallMaxTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var infoQuery = new TokenInfoQuery().SetTokenId(tokenId).SetMaxQueryPayment(Hbar.FromTinybars(1));
                Assert.Throws(typeof(MaxQueryPaymentExceededException), () =>
                {
                    infoQuery.Execute(testEnv.client);
                });
            }
        }

        public virtual void GetCostInsufficientTxFeeQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var infoQuery = new TokenInfoQuery().SetTokenId(tokenId).SetMaxQueryPayment(new Hbar(1000));
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    infoQuery.SetQueryPayment(Hbar.FromTinybars(1)).Execute(testEnv.client);
                }).Satisfies((error) => Assert.Equal(error.status.ToString(), "INSUFFICIENT_TX_FEE"));
            }
        }
    }
}