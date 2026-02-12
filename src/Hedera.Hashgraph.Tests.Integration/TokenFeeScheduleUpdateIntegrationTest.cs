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
    class TokenFeeScheduleUpdateIntegrationTest
    {
        virtual void CanUpdateToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "ffff");
                Assert.Equal(info.symbol, "F");
                Assert.Equal(info.decimals, 3);
                Assert.Equal(testEnv.operatorId, info.treasuryAccountId);
                AssertThat(info.adminKey).IsNotNull();
                AssertThat(info.freezeKey).IsNotNull();
                AssertThat(info.wipeKey).IsNotNull();
                AssertThat(info.kycKey).IsNotNull();
                AssertThat(info.supplyKey).IsNotNull();
                Assert.Equal(info.adminKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.freezeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.wipeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.kycKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.supplyKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.feeScheduleKey.ToString(), testEnv.operatorKey.ToString());
                AssertThat(info.defaultFreezeStatus).IsNotNull();
                AssertThat(info.defaultFreezeStatus).IsFalse();
                AssertThat(info.defaultKycStatus).IsNotNull();
                AssertThat(info.defaultKycStatus).IsFalse();
                Assert.Equal(info.customFees.Count, 0);
                var customFees = new List<CustomFee>();
                customFees.Add(new CustomFixedFee().SetAmount(10).SetFeeCollectorAccountId(testEnv.operatorId));
                customFees.Add(new CustomFractionalFee().SetNumerator(1).SetDenominator(20).SetMin(1).SetMax(10).SetFeeCollectorAccountId(testEnv.operatorId));
                new TokenFeeScheduleUpdateTransaction().SetTokenId(tokenId).SetCustomFees(customFees).Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
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
                Assert.Equal(info.adminKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.freezeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.wipeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.kycKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.supplyKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.feeScheduleKey.ToString(), testEnv.operatorKey.ToString());
                AssertThat(info.defaultFreezeStatus).IsNotNull();
                AssertThat(info.defaultFreezeStatus).IsFalse();
                AssertThat(info.defaultKycStatus).IsNotNull();
                AssertThat(info.defaultKycStatus).IsFalse();
                var fees = info.customFees;
                Assert.Equal(fees.Count, 2);
                int fixedCount = 0;
                int fractionalCount = 0;
                foreach (var fee in fees)
                {
                    if (fee is CustomFixedFee)
                    {
                        fixedCount++;
                        var fixed = (CustomFixedFee)fee;
                        Assert.Equal(fixed.GetAmount(), 10);
                        Assert.Equal(fixed.GetFeeCollectorAccountId(), testEnv.operatorId);
                        AssertThat(fixed.GetDenominatingTokenId()).IsNull();
                    }
                    else if (fee is CustomFractionalFee)
                    {
                        fractionalCount++;
                        var fractional = (CustomFractionalFee)fee;
                        Assert.Equal(fractional.GetNumerator(), 1);
                        Assert.Equal(fractional.GetDenominator(), 20);
                        Assert.Equal(fractional.GetMin(), 1);
                        Assert.Equal(fractional.GetMax(), 10);
                        Assert.Equal(fractional.GetFeeCollectorAccountId(), testEnv.operatorId);
                    }
                }

                Assert.Equal(fixedCount, 1);
                Assert.Equal(fractionalCount, 1);
            }
        }

        virtual void CannotUpdateWithAnyOtherKey()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFeeScheduleKey(PrivateKey.Generate()).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                var customFees = new List<CustomFee>();
                customFees.Add(new CustomFixedFee().SetAmount(10).SetFeeCollectorAccountId(testEnv.operatorId));
                customFees.Add(new CustomFractionalFee().SetNumerator(1).SetDenominator(20).SetMin(1).SetMax(10).SetFeeCollectorAccountId(testEnv.operatorId));
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenFeeScheduleUpdateTransaction().SetTokenId(tokenId).SetCustomFees(customFees).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }
    }
}