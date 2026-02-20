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
        public virtual void CanUpdateToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeKey = testEnv.OperatorKey,WipeKey = testEnv.OperatorKey,KycKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,.SetPauseKey(testEnv.OperatorKey).SetMetadataKey(testEnv.OperatorKey)FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                var info = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "ffff");
                Assert.Equal(info.symbol, "F");
                Assert.Equal(info.decimals, 3);
                Assert.Equal(info.treasuryAccountId, testEnv.OperatorId);
                Assert.NotNull(info.adminKey);
                Assert.NotNull(info.freezeKey);
                Assert.NotNull(info.wipeKey);
                Assert.NotNull(info.kycKey);
                Assert.NotNull(info.supplyKey);
                Assert.Equal(info.adminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.freezeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.wipeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.kycKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.supplyKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.pauseKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.metadataKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.False(info.defaultFreezeStatus).IsNotNull();
                Assert.False(info.defaultKycStatus).IsNotNull();
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenName("aaaa").SetTokenSymbol("A").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                info = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(info.tokenId, tokenId);
                Assert.Equal(info.name, "aaaa");
                Assert.Equal(info.symbol, "A");
                Assert.Equal(info.decimals, 3);
                Assert.Equal(info.treasuryAccountId, testEnv.OperatorId);
                Assert.NotNull(info.adminKey);
                Assert.NotNull(info.freezeKey);
                Assert.NotNull(info.wipeKey);
                Assert.NotNull(info.kycKey);
                Assert.NotNull(info.supplyKey);
                Assert.Equal(info.adminKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.freezeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.wipeKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.kycKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.supplyKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.pauseKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.Equal(info.metadataKey.ToString(), testEnv.OperatorKey.ToString());
                Assert.NotNull(info.defaultFreezeStatus);
                Assert.False(info.defaultFreezeStatus);
                Assert.NotNull(info.defaultKycStatus);
                Assert.False(info.defaultKycStatus);
            }
        }

        public virtual void CannotUpdateImmutableToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",TreasuryAccountId = testEnv.OperatorId,FreezeDefault = false,.Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetTokenName("aaaa").SetTokenSymbol("A").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token's metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,.SetMetadataKey(metadataKey)FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,SupplyKey = testEnv.OperatorKey,.SetMetadataKey(metadataKey)FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);
                Assert.Equal(tokenInfoAfterCreation.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // update token's metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMetadataUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMetadataUpdate.metadata, updatedTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMemo("abc").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMemoUpdate.metadata, initialTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // update token, but don't update metadata
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMemo("abc").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterMemoUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterMemoUpdate.metadata, initialTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)Decimals = 3,InitialSupply = 1000000,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(emptyTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.metadata, emptyTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,AdminKey = testEnv.OperatorKey,SupplyKey = testEnv.OperatorKey,FreezeDefault = false,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoAfterCreation = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterCreation.metadata, initialTokenMetadata);

                // erase token metadata (update token with empty metadata)
                new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(emptyTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterSettingEmptyMetadata = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterSettingEmptyMetadata.metadata, emptyTokenMetadata);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)TreasuryAccountId = testEnv.OperatorId,Decimals = 3,InitialSupply = 1000000,.SetAdminKey(adminKey).SetMetadataKey(metadataKey).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey)SupplyKey = testEnv.OperatorKey,.SetMetadataKey(metadataKey).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON)TreasuryAccountId = testEnv.OperatorId,Decimals = 3,InitialSupply = 1000000,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction()TokenName = "ffff",TokenSymbol = "F",.SetTokenMetadata(initialTokenMetadata)TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,SupplyKey = testEnv.OperatorKey,.Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetTokenMetadata(updatedTokenMetadata).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(emptyKeyList).SetKycKey(emptyKeyList).SetFreezeKey(emptyKeyList).SetPauseKey(emptyKeyList).SetSupplyKey(emptyKeyList).SetFeeScheduleKey(emptyKeyList).SetMetadataKey(emptyKeyList).SetAdminKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Null(tokenInfoAfterUpdate.adminKey);
                Assert.Null(tokenInfoAfterUpdate.wipeKey);
                Assert.Null(tokenInfoAfterUpdate.kycKey);
                Assert.Null(tokenInfoAfterUpdate.freezeKey);
                Assert.Null(tokenInfoAfterUpdate.pauseKey);
                Assert.Null(tokenInfoAfterUpdate.supplyKey);
                Assert.Null(tokenInfoAfterUpdate.feeScheduleKey);
                Assert.Null(tokenInfoAfterUpdate.metadataKey);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(emptyKeyList).SetKycKey(emptyKeyList).SetFreezeKey(emptyKeyList).SetPauseKey(emptyKeyList).SetSupplyKey(emptyKeyList).SetFeeScheduleKey(emptyKeyList).SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Null(tokenInfoAfterUpdate.wipeKey);
                Assert.Null(tokenInfoAfterUpdate.kycKey);
                Assert.Null(tokenInfoAfterUpdate.freezeKey);
                Assert.Null(tokenInfoAfterUpdate.pauseKey);
                Assert.Null(tokenInfoAfterUpdate.supplyKey);
                Assert.Null(tokenInfoAfterUpdate.feeScheduleKey);
                Assert.Null(tokenInfoAfterUpdate.metadataKey);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKycKey(PublicKey.UnusableKey()).SetFreezeKey(PublicKey.UnusableKey()).SetPauseKey(PublicKey.UnusableKey()).SetSupplyKey(PublicKey.UnusableKey()).SetFeeScheduleKey(PublicKey.UnusableKey()).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), PublicKey.UnusableKey().ToString());

                // Set all lower-privilege keys back by signing with an Admin Key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterRevert = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Sign(newWipeKey).Sign(newKycKey).Sign(newFreezeKey).Sign(newPauseKey).Sign(newSupplyKey).Sign(newFeeScheduleKey).Sign(newMetadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetAdminKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetAdminKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetAdminKey(adminKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoBeforeUpdate.adminKey.ToString(), adminKey.GetPublicKey().ToString());

                // Update the Admin Key to an unusable key (i.e., all-zeros key),
                // signing with an Admin Key, and setting the key verification mode to NO_VALIDATION
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetAdminKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(adminKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKycKey(PublicKey.UnusableKey()).SetFreezeKey(PublicKey.UnusableKey()).SetPauseKey(PublicKey.UnusableKey()).SetSupplyKey(PublicKey.UnusableKey()).SetFeeScheduleKey(PublicKey.UnusableKey()).SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), PublicKey.UnusableKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), PublicKey.UnusableKey().ToString());
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Sign(newWipeKey).Sign(kycKey).Sign(newKycKey).Sign(freezeKey).Sign(newFreezeKey).Sign(pauseKey).Sign(newPauseKey).Sign(supplyKey).Sign(newSupplyKey).Sign(feeScheduleKey).Sign(newFeeScheduleKey).Sign(metadataKey).Sign(newMetadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoBeforeUpdate.wipeKey.ToString(), wipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.kycKey.ToString(), kycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.freezeKey.ToString(), freezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.pauseKey.ToString(), pauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.supplyKey.ToString(), supplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.feeScheduleKey.ToString(), feeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoBeforeUpdate.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // Update all of token’s lower-privilege keys when signing with an old respective lower-privilege key,
                // and setting key verification mode to NO_VALIDATION
                new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(newWipeKey.GetPublicKey()).SetKycKey(newKycKey.GetPublicKey()).SetFreezeKey(newFreezeKey.GetPublicKey()).SetPauseKey(newPauseKey.GetPublicKey()).SetSupplyKey(newSupplyKey.GetPublicKey()).SetFeeScheduleKey(newFeeScheduleKey.GetPublicKey()).SetMetadataKey(newMetadataKey.GetPublicKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Sign(kycKey).Sign(freezeKey).Sign(pauseKey).Sign(supplyKey).Sign(feeScheduleKey).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var tokenInfoAfterUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
                Assert.Equal(tokenInfoAfterUpdate.wipeKey.ToString(), newWipeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.kycKey.ToString(), newKycKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.freezeKey.ToString(), newFreezeKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.pauseKey.ToString(), newPauseKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.supplyKey.ToString(), newSupplyKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.feeScheduleKey.ToString(), newFeeScheduleKey.GetPublicKey().ToString());
                Assert.Equal(tokenInfoAfterUpdate.metadataKey.ToString(), newMetadataKey.GetPublicKey().ToString());
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(emptyKeyList).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.TOKEN_IS_IMMUTABLE.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Sign(newWipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(kycKey).Sign(newKycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(freezeKey).Sign(newFreezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(pauseKey).Sign(newPauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(supplyKey).Sign(newSupplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(feeScheduleKey).Sign(newFeeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(PublicKey.UnusableKey()).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(metadataKey).Sign(newMetadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(newWipeKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(newKycKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(newFreezeKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(newPauseKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(newSupplyKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(newFeeScheduleKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(newMetadataKey).SetKeyVerificationMode(TokenKeyValidation.FULL_VALIDATION).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);
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
                var tokenId = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT")TokenType = TokenType.NonFungibleUnique,TreasuryAccountId = testEnv.OperatorId,.SetWipeKey(wipeKey.GetPublicKey()).SetKycKey(kycKey.GetPublicKey()).SetFreezeKey(freezeKey.GetPublicKey()).SetPauseKey(pauseKey.GetPublicKey()).SetSupplyKey(supplyKey.GetPublicKey()).SetFeeScheduleKey(feeScheduleKey.GetPublicKey()).SetMetadataKey(metadataKey.GetPublicKey()).Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;
                var tokenInfoBeforeUpdate = new TokenInfoQuery()TokenId = tokenId,.Execute(testEnv.Client);
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
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetWipeKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(wipeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_WIPE_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetKycKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(kycKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_KYC_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFreezeKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(freezeKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_FREEZE_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetPauseKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(pauseKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_PAUSE_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetSupplyKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_SUPPLY_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetFeeScheduleKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(feeScheduleKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_CUSTOM_FEE_SCHEDULE_KEY.ToString(), exception.Message);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenUpdateTransaction()TokenId = tokenId,.SetMetadataKey(ecdsaKey).SetKeyVerificationMode(TokenKeyValidation.NO_VALIDATION).FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.INVALID_METADATA_KEY.ToString(), exception.Message);
            }
        }
    }
}