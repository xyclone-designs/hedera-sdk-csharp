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
    class TokenNftInfoIntegrationTest
    {
        public virtual void CanQueryNftInfoByNftId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                byte[] metadata = new[]
                {
                    50
                };
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).AddMetadata(metadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftId = tokenId.Nft(mintReceipt.serials[0]);
                var nftInfos = new TokenNftInfoQuery().SetNftId(nftId).Execute(testEnv.client);
                Assert.Equal(nftInfos.Count, 1);
                Assert.Equal(nftInfos[0].nftId, nftId);
                Assert.Equal(nftInfos[0].accountId, testEnv.operatorId);
                Assert.Equal(nftInfos[0].metadata[0], (byte)50);
            }
        }

        public virtual void CannotQueryNftInfoByInvalidNftId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                byte[] metadata = new[]
                {
                    50
                };
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).AddMetadata(metadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftId = tokenId.Nft(mintReceipt.serials[0]);
                var invalidNftId = new NftId(nftId.tokenId, nftId.serial + 1);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenNftInfoQuery().SetNftId(invalidNftId).Execute(testEnv.client);
                }).WithMessageContaining(Status.INVALID_NFT_ID.ToString());
            }
        }

        public virtual void CannotQueryNftInfoByInvalidSerialNumber()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                byte[] metadata = new[]
                {
                    50
                };
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).AddMetadata(metadata).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftId = tokenId.Nft(mintReceipt.serials[0]);
                var invalidNftId = new NftId(nftId.tokenId, -1);
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new TokenNftInfoQuery().ByNftId(invalidNftId).Execute(testEnv.client);
                }).WithMessageContaining(Status.INVALID_TOKEN_NFT_SERIAL_NUMBER.ToString());
            }
        }

        public virtual void CanQueryNftInfoByAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                List<byte[]> metadatas = NftMetadataGenerator.Generate((byte)10);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(metadatas).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftInfos = new TokenNftInfoQuery().ByAccountId(testEnv.operatorId).SetEnd(10).Execute(testEnv.client);
                Assert.Equal(nftInfos.Count, 10);
                var serials = new List<long>(mintReceipt.serials);
                foreach (var info in nftInfos)
                {
                    Assert.Equal(info.nftId.tokenId, tokenId);
                    AssertThat(serials.Remove(info.nftId.serial)).IsTrue();
                    Assert.Equal(info.accountId, testEnv.operatorId);
                }
            }
        }

        public virtual void CanQueryNftInfoByTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetFreezeKey(testEnv.operatorKey).SetWipeKey(testEnv.operatorKey).SetKycKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetFreezeDefault(false).Execute(testEnv.client).GetReceipt(testEnv.client);
                var tokenId = Objects.RequireNonNull(createReceipt.tokenId);
                List<byte[]> metadatas = NftMetadataGenerator.Generate((byte)10);
                var mintReceipt = new TokenMintTransaction().SetTokenId(tokenId).SetMetadata(metadatas).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftInfos = new TokenNftInfoQuery().ByTokenId(tokenId).SetEnd(10).Execute(testEnv.client);
                Assert.Equal(nftInfos.Count, 10);
                var serials = new List<long>(mintReceipt.serials);
                foreach (var info in nftInfos)
                {
                    Assert.Equal(info.nftId.tokenId, tokenId);
                    AssertThat(serials.Remove(info.nftId.serial)).IsTrue();
                    Assert.Equal(info.accountId, testEnv.operatorId);
                }
            }
        }
    }
}