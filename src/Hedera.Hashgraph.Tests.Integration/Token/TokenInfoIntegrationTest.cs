// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.HBar;

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
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    Decimals = 3,
                    InitialSupply = 1000000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = key1,
                    FreezeKey = key2,
                    WipeKey = key3,
                    KycKey = key4,
                    SupplyKey = key5,
                    PauseKey = key6,
                    MetadataKey = key7,
                    FreezeDefault = false
                }
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var info = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);
                
                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal("ffff", info.Name);
                Assert.Equal("F", info.Symbol);
                Assert.Equal<uint>(3, info.Decimals);
                Assert.Equal(info.TreasuryAccountId, testEnv.OperatorId);
                Assert.NotNull(info.AdminKey);
                Assert.NotNull(info.FreezeKey);
                Assert.NotNull(info.WipeKey);
                Assert.NotNull(info.KycKey);
                Assert.NotNull(info.SupplyKey);
                Assert.NotNull(info.PauseKey);
                Assert.NotNull(info.MetadataKey);
                Assert.Equal(info.AdminKey.ToString(), key1.GetPublicKey().ToString());
                Assert.Equal(info.FreezeKey.ToString(), key2.GetPublicKey().ToString());
                Assert.Equal(info.WipeKey.ToString(), key3.GetPublicKey().ToString());
                Assert.Equal(info.KycKey.ToString(), key4.GetPublicKey().ToString());
                Assert.Equal(info.SupplyKey.ToString(), key5.GetPublicKey().ToString());
                Assert.Equal(info.PauseKey.ToString(), key6.GetPublicKey().ToString());
                Assert.Equal(info.MetadataKey.ToString(), key7.GetPublicKey().ToString());
                Assert.NotNull(info.DefaultFreezeStatus);
                Assert.False(info.DefaultFreezeStatus);
                Assert.NotNull(info.DefaultKycStatus);
                Assert.False(info.DefaultKycStatus);
                Assert.Equal(TokenType.FungibleCommon, info.TokenType);
                Assert.Equal(TokenSupplyType.Infinite, info.SupplyType);

                new TokenDeleteTransaction
                {
					TokenId = tokenId
				}
                .FreezeWith(testEnv.Client)
                .Sign(key1)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
            }
        }
        public virtual void CanQueryTokenInfoWhenTokenIsCreatedWithMinimalProperties()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                
                }.Execute(testEnv.Client);
                
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var info = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "ffff");
                Assert.Equal(info.Symbol, "F");
                Assert.Equal(0, info.Decimals);
                Assert.Equal(0, info.TotalSupply);
                Assert.Equal(info.TreasuryAccountId, testEnv.OperatorId);
                Assert.Null(info.AdminKey);
                Assert.Null(info.FreezeKey);
                Assert.Null(info.WipeKey);
                Assert.Null(info.KycKey);
                Assert.Null(info.SupplyKey);
                Assert.Null(info.PauseKey);
                Assert.Null(info.MetadataKey);
                Assert.Null(info.DefaultFreezeStatus);
                Assert.Null(info.DefaultKycStatus);
                Assert.Equal(info.TokenType, TokenType.FungibleCommon);
                Assert.Equal(info.SupplyType, TokenSupplyType.Infinite);
            }
        }
        public virtual void CanQueryNfts()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique,
                    TokenSupplyType = TokenSupplyType.Finite,
                    MaxSupply = 5000,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = testEnv.OperatorKey
                
                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var mintReceipt = new TokenMintTransaction
                {
					TokenId = tokenId,
					Metadata = NftMetadataGenerator.Generate((byte)10),
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                
                Assert.Equal(10, mintReceipt.Serials.Count);

                var info = new TokenInfoQuery
                {
					TokenId = tokenId
				}
                .Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "ffff");
                Assert.Equal(info.Symbol, "F");
                Assert.Equal<uint>(0, info.Decimals);
                Assert.Equal<ulong>(10, info.TotalSupply);
                Assert.Equal(testEnv.OperatorId, info.TreasuryAccountId);
                Assert.NotNull(info.AdminKey);
                Assert.Null(info.FreezeKey);
                Assert.Null(info.WipeKey);
                Assert.Null(info.KycKey);
                Assert.NotNull(info.SupplyKey);
                Assert.Null(info.PauseKey);
                Assert.Null(info.MetadataKey);
                Assert.Null(info.DefaultFreezeStatus);
                Assert.Null(info.DefaultKycStatus);
                Assert.Equal(info.TokenType, TokenType.NonFungibleUnique);
                Assert.Equal(info.SupplyType, TokenSupplyType.Finite);
                Assert.Equal(info.MaxSupply, 5000);
            }
        }

        public virtual void GetCostQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey
                
                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var infoQuery = new TokenInfoQuery 
                { 
                    TokenId = tokenId 
                };
                var cost = infoQuery.GetCost(testEnv.Client);

                infoQuery.QueryPayment = cost;
                infoQuery.Execute(testEnv.Client);
            }
        }
        public virtual void GetCostBigMaxQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey
                
                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var infoQuery = new TokenInfoQuery
                {
					TokenId = tokenId,
					MaxQueryPayment = new Hbar(1000),
				};
                var cost = infoQuery.GetCost(testEnv.Client);

				infoQuery.QueryPayment = cost;
				infoQuery.Execute(testEnv.Client);
			}
        }
        public virtual void GetCostSmallMaxTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                { 
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey
                
                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
				var infoQuery = new TokenInfoQuery
				{
					TokenId = tokenId,
					MaxQueryPayment = Hbar.FromTinybars(1)
				};

				MaxQueryPaymentExceededException exception = Assert.Throws<MaxQueryPaymentExceededException>(() =>
                {
                    infoQuery.Execute(testEnv.Client);
                });
            }
        }
        public virtual void GetCostInsufficientTxFeeQueryTokenInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey

                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var infoQuery = new TokenInfoQuery
                {
					TokenId = tokenId,
					MaxQueryPayment = new Hbar(1000)
				};

                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    infoQuery.QueryPayment = Hbar.FromTinybars(1);
                    infoQuery.Execute(testEnv.Client);

                }); Assert.Equal(exception.Status.ToString(), "INSUFFICIENT_TX_FEE"));
            }
        }
    }
}