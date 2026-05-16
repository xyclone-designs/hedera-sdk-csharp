// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to update NFTs' metadata (HIP-657).
    /// </summary>
    public class UpdateNftsMetadataExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Update Nfts Metadata Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                // All generated transactions will be paid by this account and signed by this key.
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                // Attach logger to the SDK Client.
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });
            PublicKey operatorKeyPublic = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pair (Metadata Key).
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...(metadata key).");
            PrivateKey metadataPrivateKey = PrivateKey.GenerateED25519();
            PublicKey metadataPublicKey = metadataPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// The beginning of the first example (mutable token's metadata).
            ///
            /// Create a non-fungible token (NFT) with the metadata key field set.
            /// </summary>
            Console.WriteLine("The beginning of the first example (mutable token's metadata).");
            byte[] initialMetadata = [1];
            Console.WriteLine("Creating mutable NFT with the metadata key field set...");
            var mutableNftCreateTx = new TokenCreateTransaction
            {
                TokenName = "HIP-657 Mutable NFT",
                TokenSymbol = "HIP657MNFT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = OPERATOR_ID,
                AdminKey = operatorKeyPublic,
                SupplyKey = operatorKeyPublic,
                MetadataKey = metadataPublicKey,

            }.FreezeWith(client);
            var mutableNftCreateTxResponse = mutableNftCreateTx.Sign(OPERATOR_KEY).Execute(client);
            var mutableNftCreateTxReceipt = mutableNftCreateTxResponse.GetReceipt(client);

            // Get the token ID of the token that was created.
            var mutableNftId = mutableNftCreateTxReceipt.TokenId;
            Console.WriteLine("Created mutable NFT with token ID: " + mutableNftId);
            /// <summary>
            /// Step 3:
            /// Query for the mutable token information stored in consensus node state to see that the Metadata Key is set.
            /// </summary>
            var mutableNftInfo = new TokenInfoQuery { TokenId = mutableNftId }.Execute(client);
            Console.WriteLine("Mutable NFT metadata key: " + mutableNftInfo.MetadataKey);
            /// <summary>
            /// Step 4:
            /// Mint the first NFT and set the initial metadata for the NFT.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            var mutableNftMintTx = new TokenMintTransaction
            {
                Metadata = [initialMetadata],
                TokenId = mutableNftId
            };
            foreach (var metadata in mutableNftMintTx.Metadata)
                Console.WriteLine("Setting metadata: " + string.Join("; ", metadata));
            var mutableNftMintTxResponse = mutableNftMintTx.Execute(client);

            // Get receipt for mint token transaction.
            var mutableNftMintTxReceipt = mutableNftMintTxResponse.GetReceipt(client);
            Console.WriteLine("Mint transaction was complete with status: " + mutableNftMintTxReceipt.Status);
            var mutableNftSerials = mutableNftMintTxReceipt.Serials;

            // Check that metadata on the NFT was set correctly.
            GetMetadataList(client, mutableNftId, mutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("Metadata after mint: " + string.Join("; ", metadata));
            });
            /// <summary>
            /// Step 5:
            /// Create an account to send the NFT to.
            /// </summary>
            Console.WriteLine("Creating Alice's account...");
            var aliceAccountCreateTx = new AccountCreateTransaction 
            { 
                MaxAutomaticTokenAssociations = 10
                
            }.SetKeyWithoutAlias(operatorKeyPublic).Execute(client);
            var aliceAccountId = aliceAccountCreateTx.GetReceipt(client).AccountId;
            Console.WriteLine("Created Alice's account with ID: " + aliceAccountId);
            /// <summary>
            /// Step 6:
            /// Transfer the NFT to the new account.
            /// </summary>
            Console.WriteLine("Transferring the NFT to Alice's account...");
            new TransferTransaction().AddNftTransfer(mutableNftId.Nft(mutableNftSerials[0]), OPERATOR_ID, aliceAccountId).Execute(client);
            /// <summary>
            /// Step 7:
            /// Update NFTs' metadata.
            /// </summary>
            byte[] updatedMetadata = new byte[]
            {
                1,
                2
            };
            Console.WriteLine("Updating NFTs' metadata...");
            var tokenUpdateNftsTx = new TokenUpdateNftsTransaction 
            {
                TokenId = mutableNftId,
                Serials = [.. mutableNftSerials],
                Metadata = updatedMetadata,
            
            }.FreezeWith(client);
            Console.WriteLine("Updated NFTs' metadata: " + string.Join("; ", tokenUpdateNftsTx.Metadata));
            var tokenUpdateNftsTxResponse = tokenUpdateNftsTx.Sign(metadataPrivateKey).Execute(client);

            // Get receipt for update NFTs metadata transaction.
            var tokenUpdateNftsTxReceipt = tokenUpdateNftsTxResponse.GetReceipt(client);
            Console.WriteLine("Token update nfts metadata transaction was complete with status: " + tokenUpdateNftsTxReceipt.Status);

            // Check that metadata for the NFT was updated correctly.
            GetMetadataList(client, mutableNftId, mutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("NFTs' metadata after update: " + string.Join("; ", metadata));
            });
            /// <summary>
            /// Step 8:
            /// The beginning of the second example (immutable token's metadata).
            ///
            /// Create a non-fungible token (NFT) with the metadata key field set.
            /// </summary>
            Console.WriteLine("The beginning of the second example (immutable token's metadata).");
            Console.WriteLine("Creating immutable NFT with the metadata key field set...");
            var immutableNftCreateTx = new TokenCreateTransaction
            {
                TokenName = "HIP-657 Immutable NFT",
                TokenSymbol = "HIP657IMMNFT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = OPERATOR_ID,
                SupplyKey = operatorKeyPublic,
                MetadataKey = metadataPublicKey,

            }.FreezeWith(client);
            var immutableNftCreateTxResponse = immutableNftCreateTx.Sign(OPERATOR_KEY).Execute(client);
            var immutableNftCreateTxReceipt = immutableNftCreateTxResponse.GetReceipt(client);

            // Get the token ID of the token that was created.
            var immutableNftId = immutableNftCreateTxReceipt.TokenId;
            Console.WriteLine("Created immutable NFT with token ID: " + immutableNftId);
            /// <summary>
            /// Step 9:
            /// Query for the mutable token information stored in consensus node state to see that the metadata key is set.
            /// </summary>
            var immutableNftInfo = new TokenInfoQuery { TokenId = immutableNftId }.Execute(client);
            Console.WriteLine("Immutable NFT metadata key: " + immutableNftInfo.MetadataKey);
            /// <summary>
            /// Step 10:
            /// Mint the first NFT and set the initial metadata for the NFT.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            var immutableNftMintTx = new TokenMintTransaction
            {
                Metadata = [initialMetadata],
                TokenId = immutableNftId
            };

            foreach (var metadata in immutableNftMintTx.Metadata)
                Console.WriteLine("Setting metadata: " + string.Join("; ", metadata));
            var immutableNftMintTxResponse = immutableNftMintTx.Execute(client);

            // Get receipt for mint token transaction.
            var immutableNftMintTxReceipt = immutableNftMintTxResponse.GetReceipt(client);
            Console.WriteLine("Mint transaction was complete with status: " + immutableNftMintTxReceipt.Status);
            var immutableNftSerials = immutableNftMintTxReceipt.Serials;

            // Check that metadata on the NFT was set correctly.
            GetMetadataList(client, immutableNftId, immutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("Metadata after mint: " + string.Join("; ", metadata));
            });
            /// <summary>
            /// Step 11:
            /// Create an account to send the NFT to.
            /// </summary>
            Console.WriteLine("Creating Bob's account...");
            var bobAccountCreateTx = new AccountCreateTransaction { MaxAutomaticTokenAssociations = 10 }.SetKeyWithoutAlias(operatorKeyPublic).Execute(client);
            var bobAccountId = bobAccountCreateTx.GetReceipt(client).AccountId;

            Console.WriteLine("Created Bob's account with ID: " + bobAccountId);
            /// <summary>
            /// Step 12:
            /// Transfer the NFT to the new account.
            /// </summary>
            Console.WriteLine("Transferring the NFT to Bob's account...");
            new TransferTransaction().AddNftTransfer(immutableNftId.Nft(immutableNftSerials[0]), OPERATOR_ID, bobAccountId).Execute(client);
            /// <summary>
            /// Step 13:
            /// Update NFTs' metadata.
            /// </summary>
            Console.WriteLine("Updating NFTs' metadata...");
            var immutableNftUpdateNftsTx = new TokenUpdateNftsTransaction
            {
                TokenId = immutableNftId,
                Serials = [immutableNftSerials],
                Metadata = updatedMetadata,
            
            }.FreezeWith(client);
            Console.WriteLine("Updated NFTs' metadata: " + string.Join("; ", immutableNftUpdateNftsTx.Metadata));
            var immutableNftUpdateNftsTxResponse = immutableNftUpdateNftsTx.Sign(metadataPrivateKey).Execute(client);

            // Get receipt for update NFTs metadata transaction.
            var immutableNftUpdateNftsTxReceipt = immutableNftUpdateNftsTxResponse.GetReceipt(client);
            Console.WriteLine("Token update nfts metadata transaction was complete with status: " + immutableNftUpdateNftsTxReceipt.Status);

            // Check that metadata for the NFT was updated correctly.
            GetMetadataList(client, immutableNftId, immutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("NFTs' metadata after update: " + string.Join("; ", metadata));
            });
            /// <summary>
            /// Clean up:
            /// Delete created mutable token.
            /// </summary>
            new TokenDeleteTransaction { TokenId = mutableNftId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Update Nfts Metadata Example Complete!");
        }

        private static List<byte[]> GetMetadataList(Client client, TokenId tokenId, IList<long> nftSerials)
        {
            return [ .. nftSerials.SelectMany(_ =>
            {
                NftId nftid = new (tokenId, _);

                IList<TokenNftInfo> list = [];

                try
                {
                    list = new TokenNftInfoQuery { NftId = nftid }.Execute(client);
                }
                catch (Exception)
                {
                    // throw new Exception(e);
                }

                return list.Select(_ => _.Metadata);

            }) ];
        }
    }
}