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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // update metadata all minted NFTs
                new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerials).SetMetadata(updatedMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check updated NFTs' metadata
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.client, tokenId, nftSerials);
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // update metadata of the first two minted NFTs
                var nftSerialsToUpdate = nftSerials.SubList(0, nftCount / 2);
                new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerialsToUpdate).SetMetadata(updatedMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check updated NFTs' metadata
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.client, tokenId, nftSerialsToUpdate);
                Assert.Equal(metadataListAfterUpdate.ToArray(), updatedMetadataList.ToArray());

                // check that remaining NFTs were not updated
                var nftSerialsSame = nftSerials.SubList(nftCount / 2, nftCount);
                List<byte[]> metadataList = GetMetadataList(testEnv.client, tokenId, nftSerialsSame);
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // run `TokenUpdateNftsTransaction` without `setMetadata`
                new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerials).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that NFTs' metadata was not updated
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.client, tokenId, nftSerials);
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
                var emptyMetadata = new byte[]
                {
                };
                var emptyMetadataList = NftMetadataGenerator.Generate(emptyMetadata, nftCount);

                // create a token with metadata key
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(testEnv.operatorKey).SetMetadataKey(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that metadata was set correctly
                var nftSerials = tokenMintTransactionReceipt.serials;
                List<byte[]> metadataListAfterMint = GetMetadataList(testEnv.client, tokenId, nftSerials);
                Assert.Equal(metadataListAfterMint.ToArray(), initialMetadataList.ToArray());

                // erase metadata all minted NFTs (update to an empty byte array)
                new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerials).SetMetadata(emptyMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);

                // check that NFTs' metadata was erased
                List<byte[]> metadataListAfterUpdate = GetMetadataList(testEnv.client, tokenId, nftSerials);
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(supplyKey).SetMetadataKey(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                Assert.Equal(tokenInfo.metadataKey.ToString(), metadataKey.GetPublicKey().ToString());

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = tokenMintTransactionReceipt.serials;

                // update nfts without signing
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerials).SetMetadata(updatedMetadata).Execute(testEnv.client).GetReceipt(testEnv.client);
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
                var tokenId = Objects.RequireNonNull(new TokenCreateTransaction().SetTokenName("ffff").SetTokenSymbol("F").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(testEnv.operatorId).SetAdminKey(testEnv.operatorKey).SetSupplyKey(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client).tokenId);
                var tokenInfo = new TokenInfoQuery().SetTokenId(tokenId).Execute(testEnv.client);
                AssertThat(tokenInfo.metadataKey).IsNull();

                // mint tokens
                var tokenMintTransactionReceipt = new TokenMintTransaction().SetMetadata(initialMetadataList).SetTokenId(tokenId).FreezeWith(testEnv.client).Sign(supplyKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                var nftSerials = tokenMintTransactionReceipt.serials;

                // check NFTs' metadata can't be updated when a metadata key is not set
                Assert.Throws(typeof(ReceiptStatusException), () =>
                {
                    new TokenUpdateNftsTransaction().SetTokenId(tokenId).SetSerials(nftSerials).SetMetadata(updatedMetadata).FreezeWith(testEnv.client).Sign(metadataKey).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.INVALID_SIGNATURE.ToString());
            }
        }

        private List<byte[]> GetMetadataList(Client client, TokenId tokenId, IList<long> nftSerials)
        {
            return nftSerials.Stream().Map((serial) => new NftId(tokenId, serial)).FlatMap((nftId) =>
            {
                try
                {
                    return new TokenNftInfoQuery().SetNftId(nftId).Execute(client).Stream();
                }
                catch (Exception e)
                {
                    throw new Exception(e);
                }
            }).Map((tokenNftInfo) => tokenNftInfo.metadata).ToList();
        }
    }
}