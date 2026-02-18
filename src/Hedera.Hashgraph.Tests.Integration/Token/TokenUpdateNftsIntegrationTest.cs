// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Nfts;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TokenUpdateNftsIntegrationTest
    {
        public virtual void CanUpdateNFTMetadataOfEntireCollection()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);
                var updatedMetadata = new byte[]
                {
                    6,
                    9
                };
                var updatedMetadataList = NftMetadataGenerator.Generate(updatedMetadata, nftCount);

                // create a token with metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction
                {
					Metadata = initialMetadataList,
					TokenId = tokenId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.Serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // update metadata all minted NFTs
                new TokenUpdateNftsTransaction
                {
					TokenId = tokenId,
					Serials = nftSerials,
					Metadata = updatedMetadata,
				}
                .FreezeWith(testEnv.Client)
                .Sign(metadataKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check updated NFTs' metadata
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterUpdate.ToArray(), updatedMetadataList.ToArray());
            }
        }

        public virtual void CanUpdateNFTMetadataOfPartOfCollection()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);
                var updatedMetadata = new byte[]
                {
                    6,
                    9
                };
                var updatedMetadataList = NftMetadataGenerator.Generate(updatedMetadata, nftCount / 2);

                // create a token with metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction()
                    Metadata = initialMetadataList)
                    TokenId = tokenId).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.Serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // update metadata of the first two minted NFTs
                var nftSerialsToUpdate = nftSerials.SubList(0, nftCount / 2);
                new TokenUpdateNftsTransaction
                {
					TokenId = tokenId,
					Serials = nftSerialsToUpdate,
					Metadata = updatedMetadata,

				}.FreezeWith(testEnv.Client).Sign(metadataKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);

                // check updated NFTs' metadata
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.Client, tokenId, nftSerialsToUpdate);
                Assert.Equal(metadataListAfterUpdate.ToArray(), updatedMetadataList.ToArray());

                // check that remaining NFTs were not updated
                var nftSerialsSame = nftSerials.SubList(nftCount / 2, nftCount);
                List<byte[]> metadataList = GetMetadataList(testEnv.Client, tokenId, nftSerialsSame);
                Assert.Equal(metadataList.ToArray(), initialMetadataList.SubList(nftCount / 2, nftCount).ToArray());
            }
        }

        public virtual void CannotUpdateNFTMetadataWhenItsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);

                // create a token with metadata key
                var tokenId = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
				}                    
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction
                {
					Metadata = initialMetadataList,
					TokenId = tokenId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.Serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // run `TokenUpdateNftsTransaction` without `setMetadata`
                new TokenUpdateNftsTransaction
                {
					TokenId = tokenId,
					Serials = nftSerials,
				}                    
                .FreezeWith(testEnv.Client)
                .Sign(metadataKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check that NFTs' metadata was not updated
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterUpdate.ToArray(), initialMetadataList.ToArray());
            }
        }

        public virtual void CanEraseNFTsMetadata()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);
                var emptyMetadata = [];
                var emptyMetadataList = NftMetadataGenerator.Generate(emptyMetadata, nftCount);

                // create a token with metadata key
                var tokenId = new TokenCreateTransaction()
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					MetadataKey = metadataKey,
				}
                .Execute(testEnv.Client).GetReceipt(testEnv.Client).TokenId;

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction
                {
					Metadata = initialMetadataList,
					TokenId = tokenId,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.Serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // erase metadata all minted NFTs (update to an empty byte array)
                new TokenUpdateNftsTransaction
                {
					TokenId = tokenId,
					Serials = nftSerials,
					Metadata = emptyMetadata,
				}
                .FreezeWith(testEnv.Client)
                .Sign(metadataKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);

                // check that NFTs' metadata was erased
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.Client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterUpdate.ToArray(), emptyMetadataList.ToArray());
            }
        }

        public virtual void CannotUpdateNFTMetadataWhenTransactionIsNotSignedWithMetadataKey()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var supplyKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);
                var updatedMetadata = new byte[]
                {
                    6,
                    9
                };

                // create a token with a metadata key and check it
                var tokenId = new TokenCreateTransaction(
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TokenType = TokenType.NonFungibleUnique,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					SupplyKey = supplyKey,
					MetadataKey = metadataKey,
				}
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;
                var tokenInfo = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);

                Assert.Equal(tokenInfo.MetadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction()
                    Metadata = initialMetadataList)
                    TokenId = tokenId).FreezeWith(testEnv.Client).Sign(supplyKey).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var nftSerials = tokenMintTransactionReceipt.Serials;

                // update nfts without signing
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenUpdateNftsTransaction
                    {
						TokenId = tokenId,
						Serials = nftSerials,
						Metadata = updatedMetadata,
					}
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        public virtual void CannotUpdateNFTMetadataWhenMetadataKeyNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var supplyKey = PrivateKey.GenerateED25519();
                var metadataKey = PrivateKey.GenerateED25519();
                var nftCount = 4;
                var initialMetadataList = NftMetadataGenerator.Generate(new byte[] { 4, 2, 0 }, nftCount);
                var updatedMetadata = new byte[]
                {
                    6,
                    9
                };

                // create a token without a metadata key and check it
                var tokenId = new TokenCreateTransaction()
                {
                    TokenName = "ffff",
                    TokenSymbol = "F",
                    TokenType = TokenType.NonFungibleUnique,
                    TreasuryAccountId = testEnv.OperatorId,
                    AdminKey = testEnv.OperatorKey,
                    SupplyKey = supplyKey,
                }
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client).TokenId;

                var tokenInfo = new TokenInfoQuery
                {
					TokenId = tokenId

				}.Execute(testEnv.Client);
                Assert.Null(tokenInfo.MetadataKey);

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction
                {
					Metadata = initialMetadataList,
					TokenId = tokenId

				}
                .FreezeWith(testEnv.Client)
                .Sign(supplyKey)
                .Execute(testEnv.Client)
                .GetReceipt(testEnv.Client);
                var nftSerials = tokenMintTransactionReceipt.Serials;

                // check NFTs' metadata can't be updated when a metadata key is not set
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenUpdateNftsTransaction
                    {
						TokenId = tokenId,
						Serials = nftSerials,
						Metadata = updatedMetadata,
					}
                    .FreezeWith(testEnv.Client)
                    .Sign(metadataKey)
                    .Execute(testEnv.Client)
                    .GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        private List<byte[]> GetMetadataList(Client client, TokenId tokenId, IList<long> nftSerials)
        {
            return nftSerials.Stream().Map((serial) => new NftId(tokenId, serial)).FlatMap((nftId) =>
            {
                try
                {
                    return new TokenNftInfoQuery
                    {
						NftId = nftId

					}.Execute(client).Stream();
                }
                catch (Exception e)
                {
                    throw new Exception(e);
                }
            }).Map((tokenNftInfo) => tokenNftInfo.metadata).ToList();
        }
    }
}