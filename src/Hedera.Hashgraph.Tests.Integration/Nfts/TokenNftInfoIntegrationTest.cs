// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenNftInfoIntegrationTest
    {
        public virtual void CanQueryNftInfoByNftId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var tokenId = createReceipt.TokenId;
                byte[] metadata = [50];
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = tokenId, 
                    Metadata = metadata
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftId = tokenId.Nft(mintReceipt.Serials[0]);
                var nftInfos = new TokenNftInfoQuery{ NftId = nftId }.Execute(testEnv.Client);
                Assert.Equal(nftInfos.Count, 1);
                Assert.Equal(nftInfos[0].NftId, nftId);
                Assert.Equal(nftInfos[0].AccountId, testEnv.OperatorId);
                Assert.Equal(nftInfos[0].Metadata[0], (byte)50);
            }
        }

        public virtual void CannotQueryNftInfoByInvalidNftId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var tokenId = createReceipt.TokenId;
                byte[] metadata = [50];
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = tokenId,
                    Metadata = metadata
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftId = tokenId.Nft(mintReceipt.Serials[0]);
                var invalidNftId = new NftId(nftId.TokenId, nftId.Serial + 1);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenNftInfoQuery { NftId = invalidNftId }.Execute(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidNftId.ToString(), exception.Message);
            }
        }

        public virtual void CannotQueryNftInfoByInvalidSerialNumber()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var tokenId = createReceipt.TokenId;
                byte[] metadata = [50];
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = tokenId, 
                    Metadata = metadata
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftId = tokenId.Nft(mintReceipt.Serials[0]);
                var invalidNftId = new NftId(nftId.TokenId, -1);
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenNftInfoQuery { NftId = invalidNftId }.Execute(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidTokenNftSerialNumber.ToString(), exception.Message);
            }
        }

        public virtual void CanQueryNftInfoByAccountId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var tokenId = createReceipt.TokenId;
                List<byte[]> metadatas = NftMetadataGenerator.Generate((byte)10);
                var mintReceipt = new TokenMintTransaction
                {
                    TokenId = tokenId,
                    Metadata = metadatas,
                }
                .Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftInfos = new TokenNftInfoQuery
                {
					End = 10,
					AccountId = testEnv.OperatorId,

				}.Execute(testEnv.Client);
                Assert.Equal(nftInfos.Count, 10);
                var serials = new List<long>(mintReceipt.Serials);
                foreach (var info in nftInfos)
                {
                    Assert.Equal(info.NftId.TokenId, tokenId);
                    Assert.True(serials.Remove(info.NftId.Serial));
                    Assert.Equal(info.AccountId, testEnv.OperatorId);
                }
            }
        }

        public virtual void CanQueryNftInfoByTokenId()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var createReceipt = new TokenCreateTransaction
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
                    FreezeDefault = false
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                var tokenId = createReceipt.TokenId;
                List<byte[]> metadatas = NftMetadataGenerator.Generate((byte)10);
                var mintReceipt = new TokenMintTransaction
                {
					Metadata = metadatas,
					TokenId = tokenId
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftInfos = new TokenNftInfoQuery
                {
                    End = 10,
					TokenId = tokenId
				
                }.Execute(testEnv.Client);
                Assert.Equal(nftInfos.Count, 10);
                var serials = new List<long>(mintReceipt.Serials);
                foreach (var info in nftInfos)
                {
                    Assert.Equal(info.NftId.TokenId, tokenId);
                    Assert.True(serials.Remove(info.NftId.Serial));
                    Assert.Equal(info.AccountId, testEnv.OperatorId);
                }
            }
        }
    }
}