// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenCreateIntegrationTest
    {
        private static IList<CustomFee> CreateFixedFeeList(int count, AccountId feeCollector)
        {
            var feeList = new List<CustomFee>();
            for (int i = 0; i < count; i++)
            {
                feeList.Add(new CustomFixedFee().SetAmount(10).SetFeeCollectorAccountId(feeCollector));
            }

            return feeList;
        }

        private static IList<CustomFee> CreateFractionalFeeList(int count, AccountId feeCollector)
        {
            var feeList = new List<CustomFee>();
            for (int i = 0; i < count; i++)
            {
                feeList.Add(new CustomFractionalFee().SetNumerator(1).SetDenominator(20).SetMin(1).SetMax(10).SetFeeCollectorAccountId(feeCollector));
            }

            return feeList;
        }

        public virtual void CanCreateTokenWithOperatorAsAllKeys()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFeeScheduleKey(testEnv.operatorKey).SetPauseKey(testEnv.operatorKey).SetMetadataKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                Objects.RequireNonNull(response.GetReceipt(testEnv.client));
            }
        }

        public virtual void CanCreateTokenWithMinimalPropertiesSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotCreateTokenWhenTokenNameIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.MISSING_TOKEN_NAME.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenSymbolIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.MISSING_TOKEN_SYMBOL.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenTreasuryAccountIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TREASURY_ACCOUNT_FOR_TOKEN.ToString());
            }
        }

        public virtual void CannotCreateTokenWhenTokenTreasuryAccountIDDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(AccountId.FromString("0.0.3")).Execute(testEnv.client).GetReceipt(testEnv.client);
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
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CanCreateTokenWithCustomFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var customFees = new List<CustomFee>();
                customFees.Add(new CustomFixedFee().SetAmount(10).SetFeeCollectorAccountId(testEnv.operatorId));
                customFees.Add(new CustomFractionalFee().SetNumerator(1).SetDenominator(20).SetMin(1).SetMax(10).SetFeeCollectorAccountId(testEnv.operatorId));
                new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetCustomFees(customFees).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotCreateMoreThanTenCustomFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetAdminKey(testEnv.operatorKey).SetTreasuryAccountId(testEnv.operatorId).SetCustomFees(CreateFixedFeeList(11, testEnv.operatorId)).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.CUSTOM_FEES_LIST_TOO_LONG.ToString());
            }
        }

        public virtual void CanCreateTenFixedFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetCustomFees(CreateFixedFeeList(10, testEnv.operatorId)).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanCreateTenFractionalFees()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetAdminKey(testEnv.operatorKey).SetTreasuryAccountId(testEnv.operatorId).SetCustomFees(CreateFractionalFeeList(10, testEnv.operatorId)).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CannotCreateMinGreaterThanMax()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetCustomFees(Collections.SingletonList(new CustomFractionalFee().SetNumerator(1).SetDenominator(3).SetMin(3).SetMax(2).SetFeeCollectorAccountId(testEnv.operatorId))).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.FRACTIONAL_FEE_MAX_AMOUNT_LESS_THAN_MIN_AMOUNT.ToString());
            }
        }

        public virtual void CannotCreateInvalidFeeCollector()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetAdminKey(testEnv.operatorKey).SetTreasuryAccountId(testEnv.operatorId).SetCustomFees(Collections.SingletonList(new CustomFixedFee().SetAmount(1))).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_CUSTOM_FEE_COLLECTOR.ToString());
            }
        }

        public virtual void CannotCreateNegativeFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetAdminKey(testEnv.operatorKey).SetTreasuryAccountId(testEnv.operatorId).SetCustomFees(Collections.SingletonList(new CustomFixedFee().SetAmount(-1).SetFeeCollectorAccountId(testEnv.operatorId))).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.CUSTOM_FEE_MUST_BE_POSITIVE.ToString());
            }
        }

        public virtual void CannotCreateZeroDenominator()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetCustomFees(Collections.SingletonList(new CustomFractionalFee().SetNumerator(1).SetDenominator(0).SetMin(1).SetMax(10).SetFeeCollectorAccountId(testEnv.operatorId))).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.FRACTION_DIVIDES_BY_ZERO.ToString());
            }
        }

        public virtual void CanCreateNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
                Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
            }
        }

        public virtual void CanCreateRoyaltyFee()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetSupplyKey(testEnv.operatorKey).SetAdminKey(testEnv.operatorKey).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetCustomFees(Collections.SingletonList(new CustomRoyaltyFee().SetNumerator(1).SetDenominator(10).SetFallbackFee(new CustomFixedFee().SetHbarAmount(new Hbar(1))).SetFeeCollectorAccountId(testEnv.operatorId))).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        public virtual void CanCreateTokenWithMinimalPropertiesSetAutoRenewAccountShouldBeAutomaticallySet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var tokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                var autoRenewAccount = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client).autoRenewAccount;
                AssertThat(autoRenewAccount).IsNotNull();
                AssertThat(autoRenewAccount).IsEqualByComparingTo(testEnv.operatorId);
            }
        }

        public virtual void CanSetAutoRenewPeriod()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var autoRenewPeriod = Duration.OfSeconds(7890000);
                var expirationTime = DateTimeOffset.UtcNow.Plus(autoRenewPeriod);
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetAutoRenewPeriod(autoRenewPeriod).SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.autoRenewAccount, testEnv.operatorId);
                Assert.Equal(tokenInfo.autoRenewPeriod, autoRenewPeriod);
                Assert.Equal(tokenInfo.expirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }

        public virtual void CanSetExpirationTime()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var expirationTime = DateTimeOffset.UtcNow.PlusSeconds(8000001);
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetExpirationTime(expirationTime).SetTreasuryAccountId(testEnv.operatorId).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.expirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }

        public virtual void WhenTransactionIdIsSetAutoRenewAccountIdShouldBeEqualToAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var accountId = new AccountCreateTransaction().SetKeyWithoutAlias(publicKey).SetInitialBalance(Hbar.From(10)).Execute(testEnv.client).GetReceipt(testEnv.client).accountId;
                var tokenId = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTransactionId(TransactionId.Generate(accountId)).SetTreasuryAccountId(accountId).FreezeWith(testEnv.client).Sign(privateKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId;
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.autoRenewAccount, accountId);
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
                var response = new TokenCreateTransaction().SetTokenName("DecimalTest").SetTokenSymbol("DT").SetDecimals(decimals).SetInitialSupply(expectedInitialSupply).SetMaxSupply(expectedMaxSupply).SetSupplyType(TokenSupplyType.FINITE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.decimals, decimals);
                Assert.Equal(tokenInfo.totalSupply, expectedInitialSupply);
                Assert.Equal(tokenInfo.maxSupply, expectedMaxSupply);
            }
        }

        public virtual void CanCreateNftWithZeroDecimalsAndZeroInitialSupply()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("NFTTest").SetTokenSymbol("NFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetDecimals(0).SetInitialSupply(0).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client);
                var tokenId = response.GetReceipt(testEnv.client).tokenId;
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.tokenType, TokenType.NON_FUNGIBLE_UNIQUE);
                Assert.Equal(tokenInfo.decimals, 0);
                Assert.Equal(tokenInfo.totalSupply, 0);
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
                    var response = new TokenCreateTransaction().SetTokenName("DecimalTest" + decimals).SetTokenSymbol("DT" + decimals).SetDecimals(decimals).SetInitialSupply(expectedSupply).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).Execute(testEnv.client);
                    var tokenId = response.GetReceipt(testEnv.client).tokenId;
                    var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                    Assert.Equal(tokenInfo.decimals, decimals);
                    Assert.Equal(tokenInfo.totalSupply, expectedSupply);
                }
            }
        }

        public virtual void CanCreateTokenWhenAutoRenewPeriodIsNull()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Calculate expiration time 90 days from now
                var expirationTime = DateTimeOffset.UtcNow.PlusSeconds(90 * 24 * 60 * 60);
                var response = new TokenCreateTransaction().SetTokenName("TEST").SetTokenSymbol("TEST").SetTokenType(TokenType.FUNGIBLE_COMMON).SetSupplyType(TokenSupplyType.INFINITE).SetAutoRenewAccountId(testEnv.operatorId).SetInitialSupply(1).SetMaxTransactionFee(new Hbar(100)).SetTreasuryAccountId(testEnv.operatorId).SetExpirationTime(expirationTime).SetDecimals(0).Execute(testEnv.client);
                var receipt = response.GetReceipt(testEnv.client);
                Assert.Equal(receipt.status, Status.SUCCESS);
                var tokenId = receipt.tokenId;
                AssertThat(tokenId).IsNotNull();
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.name, "TEST");
                Assert.Equal(tokenInfo.symbol, "TEST");
                Assert.Equal(tokenInfo.tokenType, TokenType.FUNGIBLE_COMMON);
                Assert.Equal(tokenInfo.supplyType, TokenSupplyType.INFINITE);
                Assert.Equal(tokenInfo.autoRenewAccount, testEnv.operatorId);
                Assert.Equal(tokenInfo.expirationTime.GetEpochSecond(), expirationTime.GetEpochSecond());
            }
        }
    }
}