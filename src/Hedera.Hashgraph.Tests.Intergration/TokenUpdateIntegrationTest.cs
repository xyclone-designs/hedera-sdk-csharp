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
    class TokenUpdateIntegrationTest
    {
        virtual void CanUpdateToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetPauseKey(testEnv.operatorKey).SetMetadataKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client);
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
                Assert.Equal(info.adminKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.freezeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.wipeKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.kycKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.supplyKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.pauseKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.metadataKey.ToString(), testEnv.operatorKey.ToString());
                AssertThat(info.defaultFreezeStatus).IsNotNull().IsFalse();
                AssertThat(info.defaultKycStatus).IsNotNull().IsFalse();
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenName("aaaa").SetTokenSymbol("A").Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "aaaa");
                Assert.Equal(info.symbol, "A");
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
                Assert.Equal(info.pauseKey.ToString(), testEnv.operatorKey.ToString());
                Assert.Equal(info.metadataKey.ToString(), testEnv.operatorKey.ToString());
                AssertThat(info.defaultFreezeStatus).IsNotNull();
                AssertThat(info.defaultFreezeStatus).IsFalse();
                AssertThat(info.defaultKycStatus).IsNotNull();
                AssertThat(info.defaultKycStatus).IsFalse();
            }
        }

        virtual void CannotUpdateImmutableToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTreasuryAccountId(testEnv.operatorId).SetFreezeDefault(false).Execute(testEnv.client);
                var tokenId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenName("aaaa").SetTokenSymbol("A").Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
            }
        }

        virtual void CanUpdateFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
            }
        }

        virtual void CanUpdateNonFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
            }
        }

        virtual void CanUpdateImmutableFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetMetadataKey(metadataKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
            }
        }

        virtual void CanUpdateImmutableNonFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
            }
        }

        virtual void CannotUpdateFungibleTokenMetadataWhenItsNotSet()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMemo("abc").Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMemoUpdate.metadata, initialTokenMetadata);
            }
        }

        virtual void CannotUpdateNonFungibleTokenMetadataWhenItsNotSet()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMemo("abc").Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterMemoUpdate.metadata, initialTokenMetadata);
            }
        }

        virtual void CanEraseFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetDecimals(3).SetInitialSupply(1000000).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(emptyTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.metadata, emptyTokenMetadata);
            }
        }

        virtual void CanEraseNonFungibleTokenMetadata()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoAfterCreation = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(emptyTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.metadata, emptyTokenMetadata);
            }
        }

        virtual void CannotUpdateFungibleTokenMetadataWhenTransactionIsNotSignedWithMetadataKey()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetTreasuryAccountId(testEnv.operatorId).SetDecimals(3).SetInitialSupply(1000000).SetAdminKey(adminKey).SetMetadataKey(metadataKey).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateNonFungibleTokenMetadataWhenTransactionIsNotSignedWithMetadataKey()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateFungibleTokenMetadataWhenMetadataKeyNotSet()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetTreasuryAccountId(testEnv.operatorId).SetDecimals(3).SetInitialSupply(1000000).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
            }
        }

        virtual void CannotUpdateNonFungibleTokenMetadataWhenMetadataKeyNotSet()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetSupplyKey(testEnv.operatorKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
            }
        }

        virtual void CanMakeTokenImmutableWhenUpdatingKeysToEmptyKeyListSigningWithAdminKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Make a token immutable by removing all of its keys when updating them to an empty KeyList,
                // signing with an Admin Key, and setting the key verification mode to NO_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(emptyKeyList).SetKycKey(emptyKeyList).SetFreezeKey(emptyKeyList).SetPauseKey(emptyKeyList).SetSupplyKey(emptyKeyList).SetFeeScheduleKey(emptyKeyList).SetMetadataKey(emptyKeyList).SetAdminKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                AssertThat(tokenInfoAfterUpdate.adminKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.wipeKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.kycKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.freezeKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.pauseKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.supplyKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.feeScheduleKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.metadataKey).IsNull();
            }
        }

        virtual void CanRemoveAllLowerPrivilegeKeysWhenUpdatingKeysToEmptyKeyListSigningWithAdminKeyWithKeyVerificationSetToFullValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Remove all of token’s lower-privilege keys when updating them to an empty KeyList,
                // signing with an Admin Key, and setting the key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(emptyKeyList).SetKycKey(emptyKeyList).SetFreezeKey(emptyKeyList).SetPauseKey(emptyKeyList).SetSupplyKey(emptyKeyList).SetFeeScheduleKey(emptyKeyList).SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                AssertThat(tokenInfoAfterUpdate.wipeKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.kycKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.freezeKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.pauseKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.supplyKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.feeScheduleKey).IsNull();
                AssertThat(tokenInfoAfterUpdate.metadataKey).IsNull();
            }
        }

        virtual void CanUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithAdminKeyWithKeyVerificationSetToFullValidationAndThenRevertPreviousKeys()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.e., all-zeros key),
                // signing with an Admin Key, and setting the key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKycKey(PublicKey.UnusableKey()).SetFreezeKey(PublicKey.UnusableKey()).SetPauseKey(PublicKey.UnusableKey()).SetSupplyKey(PublicKey.UnusableKey()).SetFeeScheduleKey(PublicKey.UnusableKey()).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), PublicKey.UnusableKey().ToString());

                // Set all lower-privilege keys back by signing with an Admin Key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterRevert = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterRevert.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterRevert.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());
            }
        }

        virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningWithAdminKeyAndNewLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an Admin Key and new respective
                // lower-privilege key,
                // and setting key verification mode to FULL_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Sign(newWipeKey).Sign(newKycKey).Sign(newFreezeKey).Sign(newPauseKey).Sign(newSupplyKey).Sign(newFeeScheduleKey).Sign(newMetadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        virtual void CannotMakeTokenImmutableWhenUpdatingKeysToEmptyKeyListSigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Make the token immutable when updating all of its keys to an empty KeyList
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from an Admin Key (implicitly with an operator key),
                // and setting the key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetAdminKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotMakeTokenImmutableWhenUpdatingKeysToUnusableKeySigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Make the token immutable when updating all of its keys to an unusable key (i.e. all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from an Admin Key (implicitly with an operator key),
                // and setting the key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetAdminKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateAdminKeyToUnusableKeySigningWithAdminKeyWithKeyVerificationSetToNoValidation()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {

                // Admin and supply keys
                var adminKey = PrivateKey.GenerateED25519();
                var supplyKey = PrivateKey.GenerateED25519();

                // Create a non-fungible token
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(adminKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());

                // Update the Admin Key to an unusable key (i.e., all-zeros key),
                // signing with an Admin Key, and setting the key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetAdminKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(adminKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CanUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.e., all-zeros key),
                // when signing with a respective lower-privilege key,
                // and setting the key verification mode to NO_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKycKey(PublicKey.UnusableKey()).SetFreezeKey(PublicKey.UnusableKey()).SetPauseKey(PublicKey.UnusableKey()).SetSupplyKey(PublicKey.UnusableKey()).SetFeeScheduleKey(PublicKey.UnusableKey()).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), PublicKey.UnusableKey().ToString());
            }
        }

        virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningWithOldLowerPrivilegeKeyAndNewLowerPrivilegeKeyWithKeyVerificationSetToFulValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Sign(newWipeKey).Sign(kycKey).Sign(newKycKey).Sign(freezeKey).Sign(newFreezeKey).Sign(pauseKey).Sign(newPauseKey).Sign(supplyKey).Sign(newSupplyKey).Sign(feeScheduleKey).Sign(newFeeScheduleKey).Sign(metadataKey).Sign(newMetadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        virtual void CanUpdateAllLowerPrivilegeKeysWhenSigningOnlyWithOldLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenInfoAfterUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
            }
        }

        virtual void CannotRemoveAllLowerPrivilegeKeysWhenUpdatingKeysToEmptyKeyListSigningWithRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());
                var emptyKeyList = new KeyList();

                // Remove all of token’s lower-privilege keys
                // when updating them to an empty KeyList (trying to remove keys one by one to check all errors),
                // signing with a respective lower-privilege key,
                // and setting the key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(kycKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(freezeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(pauseKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(feeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.TOKEN_IS_IMMUTABLE.ToString());
            }
        }

        virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithDifferentKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.e. all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with a key that is different from a respective lower-privilege key (implicitly with an operator
                // key),
                // and setting the key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.e., all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing ONLY with an old respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(kycKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(freezeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(pauseKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(feeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateAllLowerPrivilegeKeysToUnusableKeyWhenSigningWithOldRespectiveLowerPrivilegeKeyAndNewRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys to an unusable key (i.e., all-zeros key)
                // (trying to remove keys one by one to check all errors),
                // signing with an old respective lower-privilege key and new respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Sign(newWipeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(kycKey).Sign(newKycKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(freezeKey).Sign(newFreezeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(pauseKey).Sign(newPauseKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(supplyKey).Sign(newSupplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(feeScheduleKey).Sign(newFeeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(metadataKey).Sign(newMetadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateAllLowerPrivilegeKeysWhenSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToFullValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys
                // (trying to update keys one by one to check all errors),
                // signing ONLY with an old respective lower-privilege key,
                // and setting the key verification mode to FULL_VALIDATION
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(newWipeKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(newKycKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(kycKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(newFreezeKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(freezeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(newPauseKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(pauseKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(newSupplyKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(newFeeScheduleKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(feeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
                AssertThatExceptionOfType(typeof(ReceiptStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(newMetadataKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        virtual void CannotUpdateAllLowerPrivilegeKeysWhenUpdatingKeysToStructurallyInvalidKeysSigningOnlyWithOldRespectiveLowerPrivilegeKeyWithKeyVerificationSetToNoValidation()
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfoBeforeUpdate = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // invalid ecdsa key
                var ecdsaKey = PublicKey.FromBytesECDSA(new byte[33]);

                // update all of token’s lower-privilege keys
                // to a structurally invalid key (trying to update keys one by one to check all errors),
                // signing with an old respective lower-privilege
                // and setting key verification mode to NO_VALIDATION
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetWipeKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(wipeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_WIPE_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetKycKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(kycKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_KYC_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFreezeKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(freezeKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_FREEZE_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetPauseKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(pauseKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_PAUSE_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetSupplyKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SUPPLY_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetFeeScheduleKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(feeScheduleKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_CUSTOM_FEE_SCHEDULE_KEY.ToString());
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new TokenUpdateTransaction().SetTokenId(tokenId).SetMetadataKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_METADATA_KEY.ToString());
            }
        }
    }
}