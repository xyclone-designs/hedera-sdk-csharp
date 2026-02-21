// SPDX-License-Identifier: Apache-2.0

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Keys;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenUpdateIntegrationTest
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
					PauseKey = testEnv.OperatorKey,
					MetadataKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client);

                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var info = new TokenInfoQuery
                {
					TokenId = tokenId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "ffff");
                Assert.Equal(info.Symbol, "F");
                Assert.Equal<uint>(info.Decimals, 3);
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
                Assert.Equal(info.PauseKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.MetadataKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.False(info.DefaultFreezeStatus);
                Assert.False(info.DefaultKycStatus);
                
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenName = "aaaa",
					TokenSymbol = "A",

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                info = new TokenInfoQuery
                {
					TokenId = tokenId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(info.TokenId, tokenId);
                Assert.Equal(info.Name, "aaaa");
                Assert.Equal(info.Symbol, "A");
                Assert.Equal<uint>(info.Decimals, 3);
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
                Assert.Equal(info.PauseKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.MetadataKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.NotNull(info.DefaultFreezeStatus);
                Assert.False(info.DefaultFreezeStatus);
                Assert.NotNull(info.DefaultKycStatus);
                Assert.False(info.DefaultKycStatus);
            }
        }

        public virtual void CannotUpdateImmutableToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client);
                
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						TokenName = "aaaa",
						TokenSymbol = "A",
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); 
                
                Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception.Message);
            }
        }

        public virtual void CanUpdateFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };

                // create a fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId, }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMetadata = updatedTokenMetadata
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery
                {
					TokenId = tokenId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterMetadataUpdate.Metadata, updatedTokenMetadata);
            }
        }

        public virtual void CanUpdateNonFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };

                // create a non fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                var tokenInfoAfterCreation = new TokenInfoQuery
                {
					TokenId = tokenId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMetadata = updatedTokenMetadata
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery
                {
					TokenId = tokenId,
				
                }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterMetadataUpdate.Metadata, updatedTokenMetadata);
            }
        }

        public virtual void CanUpdateImmutableFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };
                var metadataKey = PrivateKey.GenerateED25519();

                // create a fungible token with metadata and metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					MetadataKey = metadataKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMetadata = updatedTokenMetadata,
				
                }.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterMetadataUpdate.Metadata, updatedTokenMetadata);
            }
        }

        public virtual void CanUpdateImmutableNonFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };
                var metadataKey = PrivateKey.GenerateED25519();

                // create a non fungible token with metadata and metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMetadata = updatedTokenMetadata,
				
                }.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterMetadataUpdate.Metadata, updatedTokenMetadata);
            }
        }

        public virtual void CannotUpdateFungibleTokenMetadataWhenItsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };

                // create a fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMemo = "abc",
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMemoUpdate.Metadata, initialTokenMetadata);
            }
        }

        public virtual void CannotUpdateNonFungibleTokenMetadataWhenItsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };

                // create a non fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					TokenMemo = "abc",
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMemoUpdate.Metadata, initialTokenMetadata);
            }
        }

        public virtual void CanEraseFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var emptyTokenMetadata = new byte[]
                {
                };

                // create a fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
                    TokenMetadata = emptyTokenMetadata
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.Metadata, emptyTokenMetadata);
            }
        }

        public virtual void CanEraseNonFungibleTokenMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var emptyTokenMetadata = new byte[]
                {
                };

                // create a non fungible token with metadata
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterCreation.Metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
                    TokenMetadata = emptyTokenMetadata
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.Metadata, emptyTokenMetadata);
            }
        }

        public virtual void CannotUpdateFungibleTokenMetadataWhenTransactionIsNotSignedWithMetadataKey()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };
                var adminKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // create a fungible token with metadata and metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					TreasuryAccountId = testEnv.OperatorId,
					Decimals = 3,
					InitialSupply = 1000000,
					AdminKey = adminKey,
					MetadataKey = metadataKey,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						TokenMetadata = updatedTokenMetadata,

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CannotUpdateNonFungibleTokenMetadataWhenTransactionIsNotSignedWithMetadataKey()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };
                var adminKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // create a non fungible token with metadata and metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						TokenMetadata = updatedTokenMetadata,
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CannotUpdateFungibleTokenMetadataWhenMetadataKeyNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };

                // create a fungible token with metadata and without a metadata key and admin key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.FungibleCommon,
					TreasuryAccountId = testEnv.OperatorId,
					Decimals = 3,
					InitialSupply = 1000000,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						TokenMetadata = updatedTokenMetadata,
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception.Message);
            }
        }

        public virtual void CannotUpdateNonFungibleTokenMetadataWhenMetadataKeyNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var initialTokenMetadata = new byte[]
                {
                    1,
                    1,
                    1,
                    1,
                    1
                };
                var updatedTokenMetadata = new byte[]
                {
                    2,
                    2,
                    2,
                    2,
                    2
                };

                // create a non fungible token with metadata and without a metadata key and admin key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenMetadata = initialTokenMetadata,
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					SupplyKey = testEnv.OperatorKey,
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						TokenMetadata = updatedTokenMetadata,
					
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception.Message);
            }
        }

        public virtual void CanMakeTokenImmutableWhenUpdatingKeysToEmptyKeyListSigningWithAdminKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Make a token immutable by removing all of its keys when updating them to an empty KeyList,
                // signing with an Admin Key, and setting the key verification mode to NO_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = emptyKeyList,
					KycKey = emptyKeyList,
					FreezeKey = emptyKeyList,
					PauseKey = emptyKeyList,
					SupplyKey = emptyKeyList,
					FeeScheduleKey = emptyKeyList,
					MetadataKey = emptyKeyList,
					AdminKey = emptyKeyList,
					TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Null(tokenInfoAfterUpdate.AdminKey);
                Assert.Null(tokenInfoAfterUpdate.WipeKey);
                Assert.Null(tokenInfoAfterUpdate.KycKey);
                Assert.Null(tokenInfoAfterUpdate.FreezeKey);
                Assert.Null(tokenInfoAfterUpdate.PauseKey);
                Assert.Null(tokenInfoAfterUpdate.SupplyKey);
                Assert.Null(tokenInfoAfterUpdate.FeeScheduleKey);
                Assert.Null(tokenInfoAfterUpdate.MetadataKey);
            }
        }

        public virtual void CanRemoveAllLowerPrivilegeKeysWhenUpdatingKeysToEmptyKeyListSigningWithAdminKeyWithKeyVerificationSetToFullValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Remove all of token’s lower-privilege keys when updating them to an empty KeyList,
                // signing with an Admin Key, and setting the key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = emptyKeyList,
					KycKey = emptyKeyList,
					FreezeKey = emptyKeyList,
					PauseKey = emptyKeyList,
					SupplyKey = emptyKeyList,
					FeeScheduleKey = emptyKeyList,
					MetadataKey = emptyKeyList,
					TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Null(tokenInfoAfterUpdate.WipeKey);
                Assert.Null(tokenInfoAfterUpdate.KycKey);
                Assert.Null(tokenInfoAfterUpdate.FreezeKey);
                Assert.Null(tokenInfoAfterUpdate.PauseKey);
                Assert.Null(tokenInfoAfterUpdate.SupplyKey);
                Assert.Null(tokenInfoAfterUpdate.FeeScheduleKey);
                Assert.Null(tokenInfoAfterUpdate.MetadataKey);
            }
        }

        public virtual void CanUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithAdminKeyWithKeyVerificationSetToFullValidationAndThenRevertPreviousKeys()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.E., all-zeros key),
                // signing with an Admin Key, and setting the key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = PublicKey.UnusableKey(),
					KycKey = PublicKey.UnusableKey(),
					FreezeKey = PublicKey.UnusableKey(),
					PauseKey = PublicKey.UnusableKey(),
					SupplyKey = PublicKey.UnusableKey(),
					FeeScheduleKey = PublicKey.UnusableKey(),
					MetadataKey = PublicKey.UnusableKey(),
					TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterUpdate.WipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.KycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FreezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.PauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.SupplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FeeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.MetadataKey.ToString(), PublicKey.UnusableKey().ToString());

                // Set all lower-privilege keys back by signing with an Admin Key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
					TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterRevert = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterRevert.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());
            }
        }

        public virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningWithAdminKeyAndNewLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // New Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var newWipeKey = PrivateKey.GenerateED25519();
                var newKycKey = PrivateKey.GenerateED25519();
                var newFreezeKey = PrivateKey.GenerateED25519();
                var newPauseKey = PrivateKey.GenerateED25519();
                var newSupplyKey = PrivateKey.GenerateED25519();
                var newFeeScheduleKey = PrivateKey.GenerateED25519();
                var newMetadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an Admin Key and new respective
                // lower-privilege key,
                // and setting key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = newWipeKey.GetPublicKey(),
					KycKey = newKycKey.GetPublicKey(),
					FreezeKey = newFreezeKey.GetPublicKey(),
					PauseKey = newPauseKey.GetPublicKey(),
					SupplyKey = newSupplyKey.GetPublicKey(),
					FeeScheduleKey = newFeeScheduleKey.GetPublicKey(),
					MetadataKey = newMetadataKey.GetPublicKey(),
					TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Sign(newWipeKey).Sign(newKycKey).Sign(newFreezeKey).Sign(newPauseKey).Sign(newSupplyKey).Sign(newFeeScheduleKey).Sign(newMetadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterUpdate.WipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.KycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FreezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.PauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.SupplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FeeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.MetadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        public virtual void CannotMakeTokenImmutableWhenUpdatingKeysToEmptyKeyListSigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Make the token immutable when updating all of its keys to an empty KeyList
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from an Admin Key (implicitly with an operator key),
                // and setting the key verification mode to NO_VALIDATION
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        WipeKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        KycKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);
                
                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FreezeKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);
                
                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        PauseKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);
                
                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        SupplyKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);
                
                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);
                
                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);
                
                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        AdminKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);
            }
        }

        public virtual void CannotMakeTokenImmutableWhenUpdatingKeysToUnusableKeySigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin, Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var adminKey = PrivateKey.GenerateED25519();
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Make the token immutable when updating all of its keys to an unusable key (i.E. all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from an Admin Key (implicitly with an operator key),
                // and setting the key verification mode to NO_VALIDATION
                
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        WipeKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        KycKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);
                
                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FreezeKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);
                
                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        PauseKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);
                
                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        SupplyKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);
                
                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);
                
                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);
                
                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        AdminKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);
            }
        }

        public virtual void CannotUpdateAdminKeyToUnusableKeySigningWithAdminKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin and supply keys
                var adminKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = adminKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
				
                }.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.AdminKey.ToString(), adminKey.GetPublicKey().ToString());

                // Update the Admin Key to an unusable key (i.E., all-zeros key),
                // signing with an Admin Key, and setting the key verification mode to NO_VALIDATION
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
						TokenId = tokenId,
						AdminKey = PublicKey.UnusableKey(),
						TokenKeyVerificationMode = TokenKeyValidation.NoValidation,

					}.FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
            }
        }

        public virtual void CanUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.E., all-zeros key),
                // when signing with a respective lower-privilege key,
                // and setting the key verification mode to NO_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = PublicKey.UnusableKey(),
					KycKey = PublicKey.UnusableKey(),
					FreezeKey = PublicKey.UnusableKey(),
					PauseKey = PublicKey.UnusableKey(),
					SupplyKey = PublicKey.UnusableKey(),
					FeeScheduleKey = PublicKey.UnusableKey(),
					MetadataKey = PublicKey.UnusableKey(),
					TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
				
                }.FreezeWith(testEnv.Client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);

                Assert.Equal(tokenInfoAfterUpdate.WipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.KycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FreezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.PauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.SupplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FeeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.MetadataKey.ToString(), PublicKey.UnusableKey().ToString());
            }
        }

        public virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningWithOldLowerPrivilegeKeyAndNewLowerPrivilegeKeyWithKeyVerificationSetToFulValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // New Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var newWipeKey = PrivateKey.GenerateED25519();
                var newKycKey = PrivateKey.GenerateED25519();
                var newFreezeKey = PrivateKey.GenerateED25519();
                var newPauseKey = PrivateKey.GenerateED25519();
                var newSupplyKey = PrivateKey.GenerateED25519();
                var newFeeScheduleKey = PrivateKey.GenerateED25519();
                var newMetadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = newWipeKey.GetPublicKey(),
					KycKey = newKycKey.GetPublicKey(),
					FreezeKey = newFreezeKey.GetPublicKey(),
					PauseKey = newPauseKey.GetPublicKey(),
					SupplyKey = newSupplyKey.GetPublicKey(),
					FeeScheduleKey = newFeeScheduleKey.GetPublicKey(),
					MetadataKey = newMetadataKey.GetPublicKey(),
					TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
				
                }.FreezeWith(testEnv.Client).Sign(wipeKey).Sign(newWipeKey).Sign(kycKey).Sign(newKycKey).Sign(freezeKey).Sign(newFreezeKey).Sign(pauseKey).Sign(newPauseKey).Sign(supplyKey).Sign(newSupplyKey).Sign(feeScheduleKey).Sign(newFeeScheduleKey).Sign(metadataKey).Sign(newMetadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterUpdate.WipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.KycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FreezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.PauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.SupplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FeeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.MetadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        public virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningOnlyWithOldLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // New Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var newWipeKey = PrivateKey.GenerateED25519();
                var newKycKey = PrivateKey.GenerateED25519();
                var newFreezeKey = PrivateKey.GenerateED25519();
                var newPauseKey = PrivateKey.GenerateED25519();
                var newSupplyKey = PrivateKey.GenerateED25519();
                var newFeeScheduleKey = PrivateKey.GenerateED25519();
                var newMetadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction
                {
					TokenId = tokenId,
					WipeKey = newWipeKey.GetPublicKey(),
					KycKey = newKycKey.GetPublicKey(),
					FreezeKey = newFreezeKey.GetPublicKey(),
					PauseKey = newPauseKey.GetPublicKey(),
					SupplyKey = newSupplyKey.GetPublicKey(),
					FeeScheduleKey = newFeeScheduleKey.GetPublicKey(),
					MetadataKey = newMetadataKey.GetPublicKey(),
					TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
				
                }.FreezeWith(testEnv.Client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoAfterUpdate.WipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.KycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FreezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.PauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.SupplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.FeeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.MetadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        public virtual void CannotRemoveAllLowerPrivilegeKeysWhenUpdatingKeysToEmptyKeyListSigningWithRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Remove all of token’s lower-privilege keys
                // when updating them to an empty KeyList (trying to remove keys one by one to check all errors),
                // signing with a respective lower-privilege key,
                // and setting the key verification mode to NO_VALIDATION
                
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        WipeKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception1.Message);
                
                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        KycKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception2.Message);
                
                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        FreezeKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception3.Message);
                
                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        PauseKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception4.Message);
                
                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        SupplyKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception5.Message);
                
                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception6.Message);
                
                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = emptyKeyList,
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation
                    
                    }.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.TokenIsImmutable.ToString(), exception7.Message);
            }
        }

        public virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.E. all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from a respective lower-privilege key (implicitly with an operator
                // key),
                // and setting the key verification mode to NO_VALIDATION
                
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        WipeKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);
                
                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        KycKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);
                
                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FreezeKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);
                
                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        PauseKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);
                
                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        SupplyKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);
                
                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);
                
                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
                    }
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);
            }
        }

        public virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.E., all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing ONLY with an old respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        WipeKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(wipeKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);
                
                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        KycKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(kycKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);
                
                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FreezeKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(freezeKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);
                
                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        PauseKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(pauseKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);
                
                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        SupplyKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(supplyKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);
                
                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(feeScheduleKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);
                
                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = PublicKey.UnusableKey(),
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(metadataKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);
            }
        }

        public virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithOldRespectiveLowerPrivilegeKeyAndNewRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // New Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var newWipeKey = PrivateKey.GenerateED25519();
                var newKycKey = PrivateKey.GenerateED25519();
                var newFreezeKey = PrivateKey.GenerateED25519();
                var newPauseKey = PrivateKey.GenerateED25519();
                var newSupplyKey = PrivateKey.GenerateED25519();
                var newFeeScheduleKey = PrivateKey.GenerateED25519();
                var newMetadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.E., all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with an old respective lower-privilege key and new respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        WipeKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(wipeKey)
                    .Sign(newWipeKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);

                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        KycKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(kycKey)
                    .Sign(newKycKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);

                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FreezeKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(freezeKey)
                    .Sign(newFreezeKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);

                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        PauseKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(pauseKey)
                    .Sign(newPauseKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);

                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        SupplyKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(supplyKey)
                    .Sign(newSupplyKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);

                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        FeeScheduleKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(feeScheduleKey)
                    .Sign(newFeeScheduleKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);

                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction
                    {
                        TokenId = tokenId,
                        MetadataKey = PublicKey.UnusableKey(), 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation
                    }
                    .FreezeWith(testEnv.Client)
                    .Sign(metadataKey)
                    .Sign(newMetadataKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);
            }
        }

        public virtual void CannotUpdateAllLowerPrivilegeKeysWhenSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // New Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var newWipeKey = PrivateKey.GenerateED25519();
                var newKycKey = PrivateKey.GenerateED25519();
                var newFreezeKey = PrivateKey.GenerateED25519();
                var newPauseKey = PrivateKey.GenerateED25519();
                var newSupplyKey = PrivateKey.GenerateED25519();
                var newFeeScheduleKey = PrivateKey.GenerateED25519();
                var newMetadataKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys
                // (trying to update keys one by one to check all errors),
                // signing ONLY with an old respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                ReceiptStatusException exception1 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        WipeKey = newWipeKey,
						TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception1.Message);

                ReceiptStatusException exception2 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        KycKey = newKycKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception2.Message);

                ReceiptStatusException exception3 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        FreezeKey = newFreezeKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception3.Message);

                ReceiptStatusException exception4 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        PauseKey = newPauseKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception4.Message);

                ReceiptStatusException exception5 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        SupplyKey = newSupplyKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception5.Message);

                ReceiptStatusException exception6 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        FeeScheduleKey = newFeeScheduleKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception6.Message);

                ReceiptStatusException exception7 = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        MetadataKey = newMetadataKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception7.Message);

            }
        }

        public virtual void CannotUpdateAllLowerPrivilegeKeysWhenUpdatingKeysToStructurallyInvalidKeysSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata keys
                var wipeKey = PrivateKey.GenerateED25519();
                var kycKey = PrivateKey.GenerateED25519();
                var freezeKey = PrivateKey.GenerateED25519();
                var pauseKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();
                var feeScheduleKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();

                // create a non-fungible token
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "Test NFT",
					TokenSymbol = "TNFT",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					WipeKey = wipeKey.GetPublicKey(),
					KycKey = kycKey.GetPublicKey(),
					FreezeKey = freezeKey.GetPublicKey(),
					PauseKey = pauseKey.GetPublicKey(),
					SupplyKey = supplyKey.GetPublicKey(),
					FeeScheduleKey = feeScheduleKey.GetPublicKey(),
					MetadataKey = metadataKey.GetPublicKey(),
				
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery { TokenId = tokenId }.Execute(testEnv.Client);
                
                Assert.Equal(tokenInfoBeforeUpdate.WipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.KycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FreezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.PauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.SupplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.FeeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // invalid ecdsa key
                var ecdsaKey = PublicKey.FromBytesECDSA(new byte[33]);

                // update all of token’s lower-privilege keys
                // to a structurally invalid key (trying to update keys one by one to check all errors),
                // signing with an old respective lower-privilege
                // and setting key verification mode to NO_VALIDATION
                PrecheckStatusException exception1 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        WipeKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidWipeKey.ToString(), exception1.Message);

                PrecheckStatusException exception2 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        KycKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidKycKey.ToString(), exception2.Message);

                PrecheckStatusException exception3 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        FreezeKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidFreezeKey.ToString(), exception3.Message);

                PrecheckStatusException exception4 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        PauseKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidPauseKey.ToString(), exception4.Message);

                PrecheckStatusException exception5 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        SupplyKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidSupplyKey.ToString(), exception5.Message);

                PrecheckStatusException exception6 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        FeeScheduleKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidCustomFeeScheduleKey.ToString(), exception6.Message);

                PrecheckStatusException exception7 = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction 
                    { 
                        TokenId = tokenId,
                        MetadataKey = ecdsaKey, 
                        TokenKeyVerificationMode = TokenKeyValidation.FullValidation 
                    
                    }.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                });
                Assert.Contains(ResponseStatus.InvalidMetadataKey.ToString(), exception7.Message);

            }
        }
    }
}