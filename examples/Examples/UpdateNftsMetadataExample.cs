// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Collections.Generic;

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
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        private static readonly string HEDERA_NETWORK = Dotenv.Load().Get("HEDERA_NETWORK", "testnet");
        private static readonly string SDK_LOG_LEVEL = Dotenv.Load().Get("SDK_LOG_LEVEL", "SILENT");
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
            byte[] initialMetadata = new byte[]
            {
                1
            };
            Console.WriteLine("Creating mutable NFT with the metadata key field set...");
            var mutableNftCreateTx = new TokenCreateTransaction().SetTokenName("HIP-657 Mutable NFT").SetTokenSymbol("HIP657MNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(OPERATOR_ID).SetAdminKey(operatorKeyPublic).SetSupplyKey(operatorKeyPublic).SetMetadataKey(metadataPublicKey).FreezeWith(client);
            var mutableNftCreateTxResponse = mutableNftCreateTx.Sign(OPERATOR_KEY).Execute(client);
            var mutableNftCreateTxReceipt = mutableNftCreateTxResponse.GetReceipt(client);

            // Get the token ID of the token that was created.
            var mutableNftId = mutableNftCreateTxReceipt.TokenId;
            mutableNftId;
            Console.WriteLine("Created mutable NFT with token ID: " + mutableNftId);
            /// <summary>
            /// Step 3:
            /// Query for the mutable token information stored in consensus node state to see that the Metadata Key is set.
            /// </summary>
            var mutableNftInfo = new TokenInfoQuery { TokenId = mutableNftId }.Execute(client);
            Console.WriteLine("Mutable NFT metadata key: " + mutableNftInfo.metadataKey);
            /// <summary>
            /// Step 4:
            /// Mint the first NFT and set the initial metadata for the NFT.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            var mutableNftMintTx = new TokenMintTransaction().SetMetadata(List.Of(initialMetadata)).SetTokenId(mutableNftId);
            mutableNftMintTx.GetMetadata().ForEach((metadata) =>
            {
                Console.WriteLine("Setting metadata: " + Arrays.ToString(metadata));
            });
            var mutableNftMintTxResponse = mutableNftMintTx.Execute(client);

            // Get receipt for mint token transaction.
            var mutableNftMintTxReceipt = mutableNftMintTxResponse.GetReceipt(client);
            Console.WriteLine("Mint transaction was complete with status: " + mutableNftMintTxReceipt.Status);
            var mutableNftSerials = mutableNftMintTxReceipt.serials;

            // Check that metadata on the NFT was set correctly.
            GetMetadataList(client, mutableNftId, mutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("Metadata after mint: " + Arrays.ToString(metadata));
            });
            /// <summary>
            /// Step 5:
            /// Create an account to send the NFT to.
            /// </summary>
            Console.WriteLine("Creating Alice's account...");
            var aliceAccountCreateTx = new AccountCreateTransaction { KeyWithoutAlias = operatorKeyPublic, MaxAutomaticTokenAssociations = 10 }.Execute(client);
            var aliceAccountId = aliceAccountCreateTx.GetReceipt(client).AccountId;
            aliceAccountId;
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
            var tokenUpdateNftsTx = new TokenUpdateNftsTransaction().SetTokenId(mutableNftId).SetSerials(mutableNftSerials).SetMetadata(updatedMetadata).FreezeWith(client);
            Console.WriteLine("Updated NFTs' metadata: " + Arrays.ToString(tokenUpdateNftsTx.GetMetadata()));
            var tokenUpdateNftsTxResponse = tokenUpdateNftsTx.Sign(metadataPrivateKey).Execute(client);

            // Get receipt for update NFTs metadata transaction.
            var tokenUpdateNftsTxReceipt = tokenUpdateNftsTxResponse.GetReceipt(client);
            Console.WriteLine("Token update nfts metadata transaction was complete with status: " + tokenUpdateNftsTxReceipt.Status);

            // Check that metadata for the NFT was updated correctly.
            GetMetadataList(client, mutableNftId, mutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("NFTs' metadata after update: " + Arrays.ToString(metadata));
            });
            /// <summary>
            /// Step 8:
            /// The beginning of the second example (immutable token's metadata).
            ///
            /// Create a non-fungible token (NFT) with the metadata key field set.
            /// </summary>
            Console.WriteLine("The beginning of the second example (immutable token's metadata).");
            Console.WriteLine("Creating immutable NFT with the metadata key field set...");
            var immutableNftCreateTx = new TokenCreateTransaction().SetTokenName("HIP-657 Immutable NFT").SetTokenSymbol("HIP657IMMNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(OPERATOR_ID).SetSupplyKey(operatorKeyPublic).SetMetadataKey(metadataPublicKey).FreezeWith(client);
            var immutableNftCreateTxResponse = immutableNftCreateTx.Sign(OPERATOR_KEY).Execute(client);
            var immutableNftCreateTxReceipt = immutableNftCreateTxResponse.GetReceipt(client);

            // Get the token ID of the token that was created.
            var immutableNftId = immutableNftCreateTxReceipt.TokenId;
            immutableNftId;
            Console.WriteLine("Created immutable NFT with token ID: " + immutableNftId);
            /// <summary>
            /// Step 9:
            /// Query for the mutable token information stored in consensus node state to see that the metadata key is set.
            /// </summary>
            var immutableNftInfo = new TokenInfoQuery { TokenId = immutableNftId }.Execute(client);
            Console.WriteLine("Immutable NFT metadata key: " + immutableNftInfo.metadataKey);
            /// <summary>
            /// Step 10:
            /// Mint the first NFT and set the initial metadata for the NFT.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            var immutableNftMintTx = new TokenMintTransaction().SetMetadata(List.Of(initialMetadata)).SetTokenId(immutableNftId);
            immutableNftMintTx.GetMetadata().ForEach((metadata) =>
            {
                Console.WriteLine("Setting metadata: " + Arrays.ToString(metadata));
            });
            var immutableNftMintTxResponse = immutableNftMintTx.Execute(client);

            // Get receipt for mint token transaction.
            var immutableNftMintTxReceipt = immutableNftMintTxResponse.GetReceipt(client);
            Console.WriteLine("Mint transaction was complete with status: " + immutableNftMintTxReceipt.Status);
            var immutableNftSerials = immutableNftMintTxReceipt.serials;

            // Check that metadata on the NFT was set correctly.
            GetMetadataList(client, immutableNftId, immutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("Metadata after mint: " + Arrays.ToString(metadata));
            });
            /// <summary>
            /// Step 11:
            /// Create an account to send the NFT to.
            /// </summary>
            Console.WriteLine("Creating Bob's account...");
            var bobAccountCreateTx = new AccountCreateTransaction { KeyWithoutAlias = operatorKeyPublic, MaxAutomaticTokenAssociations = 10 }.Execute(client);
            var bobAccountId = bobAccountCreateTx.GetReceipt(client).AccountId;
            bobAccountId;
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
            var immutableNftUpdateNftsTx = new TokenUpdateNftsTransaction().SetTokenId(immutableNftId).SetSerials(immutableNftSerials).SetMetadata(updatedMetadata).FreezeWith(client);
            Console.WriteLine("Updated NFTs' metadata: " + Arrays.ToString(immutableNftUpdateNftsTx.GetMetadata()));
            var immutableNftUpdateNftsTxResponse = immutableNftUpdateNftsTx.Sign(metadataPrivateKey).Execute(client);

            // Get receipt for update NFTs metadata transaction.
            var immutableNftUpdateNftsTxReceipt = immutableNftUpdateNftsTxResponse.GetReceipt(client);
            Console.WriteLine("Token update nfts metadata transaction was complete with status: " + immutableNftUpdateNftsTxReceipt.Status);

            // Check that metadata for the NFT was updated correctly.
            GetMetadataList(client, immutableNftId, immutableNftSerials).ForEach((metadata) =>
            {
                Console.WriteLine("NFTs' metadata after update: " + Arrays.ToString(metadata));
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
            return nftSerials.Stream().Map((serial) => new NftId(tokenId, serial)).FlatMap((nftId) =>
            {
                try
                {
                    return new TokenNftInfoQuery { NftId = nftId }.Execute(client).Stream();
                }
                catch (Exception e)
                {
                    throw new Exception(e);
                }
            }).Map((tokenNftInfo) => tokenNftInfo.metadata).ToList();
        }
    }
}