// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenFeeScheduleUpdateIntegrationTest
    {
        public virtual void CanUpdateToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    Decimals = 3,
                    InitialSupply = 1000000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FreezeKey = testEnv.OperatorKey,
                    WipeKey = testEnv.OperatorKey,
                    KycKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey,
                    FeeScheduleKey = testEnv.OperatorKey,
                    FreezeDefault = false,

                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var info = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "ffff");
                Assert.Equal(info.Symbol, "F");
                Assert.Equal<decimal>(3, info.Decimals);
                Assert.Equal(testEnv.OperatorId, info.TreasuryAccountId);
                Assert.NotNull(info.AdminKey);
                Assert.NotNull(info.FreezeKey);
                Assert.NotNull(info.WipeKey);
                Assert.NotNull(info.KycKey);
                Assert.NotNull(info.SupplyKey);
                Assert.Equal(info.AdminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.FreezeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.WipeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.KycKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.SupplyKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.FeeScheduleKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.NotNull(info.DefaultFreezeStatus);
                Assert.False(info.DefaultFreezeStatus);
                Assert.NotNull(info.DefaultKycStatus);
                Assert.False(info.DefaultKycStatus);
                Assert.Equal(info.CustomFees.Count, 0);

                new TokenFeeScheduleUpdateTransaction
                {
					TokenId = tokenId,
					CustomFees = 
                    [
						new CustomFixedFee
				        {
					        Amount = 10,
					        FeeCollectorAccountId = testEnv.OperatorId
				        },
				        new CustomFractionalFee
				        {
					        Numerator = 1,
					        Denominator = 20,
					        Min = 1,
					        Max = 10,
					        FeeCollectorAccountId = testEnv.OperatorId
				        }
					]
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                info = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "ffff");
                Assert.Equal(info.Symbol, "F");
                Assert.Equal<decimal>(info.Decimals, 3);
                Assert.Equal(info.TreasuryAccountId, testEnv.OperatorId);
                Assert.NotNull(info.AdminKey);
                Assert.NotNull(info.FreezeKey);
                Assert.NotNull(info.WipeKey);
                Assert.NotNull(info.KycKey);
                Assert.NotNull(info.SupplyKey);
                Assert.Equal(info.AdminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.FreezeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.WipeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.KycKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.SupplyKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.FeeScheduleKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.NotNull(info.DefaultFreezeStatus);
                Assert.False(info.DefaultFreezeStatus);
                Assert.NotNull(info.DefaultKycStatus);
                Assert.False(info.DefaultKycStatus);
                
                var fees = info.CustomFees;
                Assert.Equal(fees.Count, 2);

                int fixedCount = 0;
                int fractionalCount = 0;
                foreach (var fee in fees)
                {
                    if (fee is CustomFixedFee _fixed)
                    {
                        fixedCount++;

                        Assert.Equal(_fixed.Amount, 10);
                        Assert.Equal(_fixed.FeeCollectorAccountId, testEnv.OperatorId);
                        Assert.Null(_fixed.DenominatingTokenId);
                    }
                    else if (fee is CustomFractionalFee fractional)
                    {
                        fractionalCount++;

                        Assert.Equal(fractional.Numerator, 1);
                        Assert.Equal(fractional.Denominator, 20);
                        Assert.Equal(fractional.Min, 1);
                        Assert.Equal(fractional.Max, 10);
                        Assert.Equal(fractional.FeeCollectorAccountId, testEnv.OperatorId);
                    }
                }

                Assert.Equal(fixedCount, 1);
                Assert.Equal(fractionalCount, 1);
            }
        }

        public virtual void CannotUpdateWithAnyOtherKey()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    FeeScheduleKey = PrivateKey.Generate(), 
                    FreezeDefault = false
                
                }.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenFeeScheduleUpdateTransaction
                    {
						TokenId = tokenId,
						CustomFees = 
                        [
							new CustomFixedFee
				            {
					            Amount = 10,
					            FeeCollectorAccountId = testEnv.OperatorId
				            },
							new CustomFractionalFee
				            {
					            Numerator = 1,
					            Denominator = 20,
					            Min = 1,
					            Max = 10,
					            FeeCollectorAccountId = testEnv.OperatorId
				            }
						],
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }
    }
}