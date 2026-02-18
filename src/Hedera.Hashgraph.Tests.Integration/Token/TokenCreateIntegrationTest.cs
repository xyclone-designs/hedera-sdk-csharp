// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Tests.Integration;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenCreateIntegrationTest
    {
        private static IList<CustomFee> CreateFixedFeeList(int count, AccountId feeCollector)
        {
            var feeList = new List<CustomFee>();
            for (int i = 0; i < count; i++)
				feeList.Add(new CustomFixedFee
				{
					Amount = 10,
					FeeCollectorAccountId = feeCollector
				});

			return feeList;
        }

        private static IList<CustomFee> CreateFractionalFeeList(int count, AccountId feeCollector)
        {
            var feeList = new List<CustomFee>();
            for (int i = 0; i < count; i++)
				feeList.Add(new CustomFractionalFee
				{
					Numerator = 1,
					Denominator = 20,
					Min = 1,
					Max = 10,
					FeeCollectorAccountId = feeCollector,
				});

			return feeList;
        }

        public virtual void CanCreateTokenWithOperatorAsAllKeys()
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
					PauseKey = testEnv.OperatorKey,
					MetadataKey = testEnv.OperatorKey,
					FreezeDefault = false
				}
                .Execute(testEnv.Client);

                response.GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanCreateTokenWithMinimalPropertiesSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction()
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotCreateTokenWhenTokenNameIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction()
                    {
						TokenSymbol = "F",
						TreasuryAccountId = testEnv.OperatorId,
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.MISSING_TOKEN_NAME.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenSymbolIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction()
                    {
						TokenName = "ffff",
						TreasuryAccountId = testEnv.OperatorId,
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.MISSING_TOKEN_SYMBOL.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenTreasuryAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
						TokenSymbol = "F",
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_TREASURY_ACCOUNT_FOR_TOKEN.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenTreasuryAccountIDDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
						TokenSymbol = "F",
						TreasuryAccountId = AccountId.FromString("0.0.3"),
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenAdminKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
						TokenSymbol = "F",
						TreasuryAccountId = testEnv.OperatorId,
						AdminKey = key,
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanCreateTokenWithCustomFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					CustomFees = 
                    [
						new CustomFixedFee
				        {
					        Amount = 10,
					        FeeCollectorAccountId = testEnv.OperatorId,
				        },
						new CustomFractionalFee
				        {
					        Numerator = 1,
					        Denominator = 20,
					        Min = 1,
					        Max = 10,
					        FeeCollectorAccountId = testEnv.OperatorId,
				        }
					]
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotCreateMoreThanTenCustomFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
						TokenSymbol = "F",
						AdminKey = testEnv.OperatorKey,
						TreasuryAccountId = testEnv.OperatorId,
						CustomFees = CreateFixedFeeList(11, testEnv.OperatorId)
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.CUSTOM_FEES_LIST_TOO_LONG.ToString());
            }
        }

        public virtual void CanCreateTenFixedFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction()
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					CustomFees = CreateFixedFeeList(10, testEnv.OperatorId)
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanCreateTenFractionalFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					AdminKey = testEnv.OperatorKey,
					TreasuryAccountId = testEnv.OperatorId,
					CustomFees = CreateFractionalFeeList(10, testEnv.OperatorId),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotCreateMinGreaterThanMax()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
                        TokenName = "ffff",
                        TokenSymbol = "F",
                        TreasuryAccountId = testEnv.OperatorId,
                        AdminKey = testEnv.OperatorKey,
                        CustomFees = [new CustomFractionalFee()
                        {
							Numerator = 1,
							Denominator = 3,
							Min = 3,
							Max = 2,
							FeeCollectorAccountId = testEnv.OperatorId,
						}]
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.FRACTIONAL_FEE_MAX_AMOUNT_LESS_THAN_MIN_AMOUNT.ToString());
            }
        }

        public virtual void CannotCreateInvalidFeeCollector()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
                        TokenName = "ffff",
                        TokenSymbol = "F",
                        AdminKey = testEnv.OperatorKey,
                        TreasuryAccountId = testEnv.OperatorId,
                        CustomFees = [new CustomFixedFee
					    {
						    Amount = 1
					    }]
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.INVALID_CUSTOM_FEE_COLLECTOR.ToString());
            }
        }

        public virtual void CannotCreateNegativeFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
					    TokenSymbol = "F",
                        AdminKey = testEnv.OperatorKey,
                        TreasuryAccountId = testEnv.OperatorId,
                        CustomFees = [ new CustomFixedFee
					    {
						    Amount = -1,
						    FeeCollectorAccountId = testEnv.OperatorId,
					    }]

					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.CUSTOM_FEE_MUST_BE_POSITIVE.ToString());
            }
        }

        public virtual void CannotCreateZeroDenominator()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction
                    {
						TokenName = "ffff",
						TokenSymbol = "F",
						TreasuryAccountId = testEnv.OperatorId,
						AdminKey = testEnv.OperatorKey,
						CustomFees = [new CustomFractionalFee
					    {
						    Numerator = 1,
						    Denominator = 0,
						    Min = 1,
						    Max = 10,
						    FeeCollectorAccountId = testEnv.OperatorId,
					    }]
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }).WithMessageContaining(Status.FRACTION_DIVIDES_BY_ZERO.ToString());
            }
        }

        public virtual void CanCreateNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				}
                .Execute(testEnv.Client);

                response.GetReceipt(testEnv.Client).TokenId;
            }
        }

        public virtual void CanCreateRoyaltyFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					SupplyKey = testEnv.OperatorKey,
					AdminKey = testEnv.OperatorKey,
					TokenType = TokenType.NonFungibleUnique,
					CustomFees = [new CustomRoyaltyFee
                    {
					    Numerator = 1,
					    Denominator = 10,
						FallbackFee = new CustomFixedFee(),
					    HbarAmount = new Hbar(1),
					    FeeCollectorAccountId = testEnv.OperatorId,
					}],
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanCreateTokenWithMinimalPropertiesSetAutoRenewAccountShouldBeAutomaticallySet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                var autoRenewAccount = new TokenInfoQuery()
				{
					TokenId = tokenId
				}
				.Execute(testEnv.Client)
                .autoRenewAccount;
                Assert.NotNull(autoRenewAccount);
                AssertThat(autoRenewAccount).IsEqualByComparingTo(testEnv.OperatorId,;
            }
        }

        public virtual void CanSetAutoRenewPeriod()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var autoRenewPeriod = Duration.OfSeconds(7890000);
                var expirationTime = DateTimeOffset.UtcNow.Plus(autoRenewPeriod);
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					AutoRenewPeriod = autoRenewPeriod,
					TreasuryAccountId = testEnv.OperatorId,
				}
                .Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery()
				{
					TokenId = tokenId
				}
				.Execute(testEnv.Client);
                Assert.Equal(tokenInfo.AutoRenewAccount, testEnv.OperatorId,;
                Assert.Equal(tokenInfo.AutoRenewPeriod, autoRenewPeriod);
                Assert.Equal(tokenInfo.ExpirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }

        public virtual void CanSetExpirationTime()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var expirationTime = DateTimeOffset.UtcNow.PlusSeconds(8000001);
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					ExpirationTime = expirationTime,
					TreasuryAccountId = testEnv.OperatorId,
				}
                .Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery()
				{
					TokenId = tokenId
				}
				.Execute(testEnv.Client);
                Assert.Equal(tokenInfo.ExpirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }

        public virtual void WhenTransactionIdIsSetAutoRenewAccountIdShouldBeEqualToAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var accountId = new AccountCreateTransaction
                {
					KeyWithoutAlias = publicKey,
					InitialBalance = Hbar.From(10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).AccountId;
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TransactionId = TransactionId.Generate(accountId),
					TreasuryAccountId = accountId,
				}
                .FreezeWith(testEnv.Client)
                .Sign(privateKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery()
				{
					TokenId = tokenId
				}
				.Execute(testEnv.Client);
                Assert.Equal(tokenInfo.AutoRenewAccount, accountId);
            }
        }

        public virtual void CanCreateTokenWithDecimalAdjustmentForSupplyValues()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                int decimals = 3;
                long userInputInitialSupply = 1000;
                long userInputMaxSupply = 10000;
                long expectedInitialSupply = userInputInitialSupply * 1000;
                long expectedMaxSupply = userInputMaxSupply * 1000;
                var response = new TokenCreateTransaction
                {
					TokenName = "DecimalTest",
					TokenSymbol = "DT",
					Decimals = decimals,
					InitialSupply = expectedInitialSupply,
					MaxSupply = expectedMaxSupply,
					SupplyType = TokenSupplyType.FINITE,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
				}
                .Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery()
                {
					TokenId = tokenId
				}
                .Execute(testEnv.Client);
                Assert.Equal(tokenInfo.Decimals, decimals);
                Assert.Equal(tokenInfo.TotalSupply, expectedInitialSupply);
                Assert.Equal(tokenInfo.MaxSupply, expectedMaxSupply);
            }
        }

        public virtual void CanCreateNftWithZeroDecimalsAndZeroInitialSupply()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
					TokenName = "NFTTest",
					TokenSymbol = "NFT",
					TokenType = TokenType.NonFungibleUnique,
					Decimals = 0,
					InitialSupply = 0,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,

				}.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery
                {
					TokenId = tokenId
				}.Execute(testEnv.Client);
                Assert.Equal(tokenInfo.TokenType, TokenType.NonFungibleUnique);
                Assert.Equal(tokenInfo.Decimals, 0);
                Assert.Equal(tokenInfo.TotalSupply, 0);
            }
        }

        public virtual void CanCreateTokenWithDifferentDecimalPrecisionValues()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                int[] decimalValues = new[]
                {
                    0,
                    1,
                    2,
                    6,
                    8,
                    18
                };
                foreach (int decimals in decimalValues)
                {
                    long userInputSupply = 100;
                    long expectedSupply = userInputSupply * (long)Math.Pow(10, decimals);
                    var response = new TokenCreateTransaction
                    {
						TokenName = "DecimalTest" + decimals,
						TokenSymbol = "DT" + decimals,
						Decimals = decimals,
						InitialSupply = expectedSupply,
						TreasuryAccountId = testEnv.OperatorId,
						AdminKey = testEnv.OperatorKey,

					}.Execute(testEnv.Client);
                    var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                    var tokenInfo = new TokenInfoQuery
                    {
						TokenId = tokenId
					}
                    .Execute(testEnv.Client);
                    Assert.Equal(tokenInfo.Decimals, decimals);
                    Assert.Equal(tokenInfo.TotalSupply, expectedSupply);
                }
            }
        }

        public virtual void CanCreateTokenWhenAutoRenewPeriodIsNull()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Calculate expiration time 90 days from now
                var expirationTime = DateTimeOffset.UtcNow.PlusSeconds(90 * 24 * 60 * 60);
                var response = new TokenCreateTransaction
                {
					TokenName = "TEST",
					TokenSymbol = "TEST",
					TokenType = TokenType.FUNGIBLE_COMMON,
					SupplyType = TokenSupplyType.INFINITE,
					AutoRenewAccountId = testEnv.OperatorId,
					InitialSupply = 1,
					MaxTransactionFee = new Hbar(100),
					TreasuryAccountId = testEnv.OperatorId,
					ExpirationTime = expirationTime,
					Decimals = 0,
				}
                .Execute(testEnv.Client);
                var receipt = response.GetReceipt(testEnv.Client);
                Assert.Equal(receipt.status, ResponseStatus.Success);
                var tokenId = receipt.TokenId;
                Assert.NotNull(tokenId);
                var tokenInfo = new TokenInfoQuery
                {
					TokenId = tokenId
				}
                .Execute(testEnv.Client);
                Assert.Equal(tokenInfo.Name, "TEST");
                Assert.Equal(tokenInfo.Symbol, "TEST");
                Assert.Equal(tokenInfo.TokenType, TokenType.FUNGIBLE_COMMON,;
                Assert.Equal(tokenInfo.SupplyType, TokenSupplyType.INFINITE,;
                Assert.Equal(tokenInfo.AutoRenewAccount, testEnv.OperatorId,;
                Assert.Equal(tokenInfo.ExpirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }
    }
}