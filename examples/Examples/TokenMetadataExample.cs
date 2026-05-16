// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class TokenMetadataExample
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
            Console.WriteLine("Token Metadata (HIP-646 and HIP-765) Example Start!");
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
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pairs.
            /// </summary>
            PrivateKey adminPrivateKey = PrivateKey.GenerateED25519();
            PublicKey adminPublicKey = adminPrivateKey.GetPublicKey();
            PrivateKey metadataPrivateKey = PrivateKey.GenerateED25519();
            PublicKey metadataPublicKey = metadataPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// The beginning of the first example (mutable token's metadata).
            ///
            /// Create a mutable fungible token with a metadata, but without a Metadata Key.
            /// </summary>
            Console.WriteLine("The beginning of the first example (mutable token's metadata).");
            byte[] initialTokenMetadata = [1, 1, 1, 1, 1];
            Console.WriteLine("Creating mutable Fungible Token using the Hedera Token Service...");
            var mutableFungibleTokenId = new TokenCreateTransaction
            {
                TokenName = "HIP-646 Mutable FT",
                TokenSymbol = "HIP646MFT",
                TokenMetadata = initialTokenMetadata,
                TokenType = TokenType.FungibleCommon,
                TreasuryAccountId = OPERATOR_ID,
                Decimals = 3,
                InitialSupply = 1000000,
                AdminKey = adminPublicKey,
            }
            .FreezeWith(client)
            .Sign(adminPrivateKey)
            .Execute(client)
            .GetReceipt(client).TokenId;
            Console.WriteLine("Created mutable Fungible Token with ID: " + mutableFungibleTokenId);
            /// <summary>
            /// Step 3:
            /// Query and output mutable Fungible Token info after its creation.
            /// </summary>
            var mutableFungibleTokenInfo_AfterCreation = new TokenInfoQuery { TokenId = mutableFungibleTokenId }.Execute(client);

            // Check that metadata was set correctly.
            if (Equals(mutableFungibleTokenInfo_AfterCreation.Metadata, initialTokenMetadata))
            {
                Console.WriteLine("Mutable Fungible Token metadata after creation: " + string.Join("; ", mutableFungibleTokenInfo_AfterCreation.Metadata));
            }
            else
            {
                throw new Exception("Mutable Fungible Token metadata was not set correctly! (Fail)");
            }

            /// <summary>
            /// Step 4:
            /// Update mutable Fungible Token metadata.
            /// </summary>
            byte[] updatedTokenMetadata = new byte[]
            {
                2,
                2,
                2,
                2,
                2
            };
            Console.WriteLine("Updating mutable Fungible Token metadata...");
            new TokenUpdateTransaction
            {
                TokenId = mutableFungibleTokenId,
                TokenMetadata = updatedTokenMetadata,
            }
            .FreezeWith(client)
            .Sign(adminPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Query and output mutable Fungible Token info after its metadata was updated.
            /// </summary>
            var mutableFungibleTokenInfo_AfterMetadataUpdate = new TokenInfoQuery { TokenId = mutableFungibleTokenId }.Execute(client);

            // Check that metadata was updated correctly.
            if (Equals(mutableFungibleTokenInfo_AfterMetadataUpdate.Metadata, updatedTokenMetadata))
            {
                Console.WriteLine("Mutable Fungible Token metadata after update: " + string.Join("; ", mutableFungibleTokenInfo_AfterMetadataUpdate.Metadata));
            }
            else
            {
                throw new Exception("Mutable Fungible Token metadata was not updated correctly! (Fail)");
            }

            /// <summary>
            /// Step 6:
            /// The beginning of the second example (immutable token's metadata).
            ///
            /// Create an immutable Fungible Token with a metadata key and a metadata.
            /// </summary>
            Console.WriteLine("The beginning of the second example (immutable token's metadata).");
            Console.WriteLine("Creating immutable Fungible Token using the Hedera Token Service...");
            var immutableFungibleTokenId = new TokenCreateTransaction
            {
                TokenName = "HIP-646 Immutable FT",
                TokenSymbol = "HIP646IMMFT",
                TokenMetadata = initialTokenMetadata,
                TokenType = TokenType.FungibleCommon,
                TreasuryAccountId = OPERATOR_ID,
                MetadataKey = metadataPublicKey,
                Decimals = 3,
                InitialSupply = 1000000,
            }
            .Execute(client)
            .GetReceipt(client).TokenId;
            Console.WriteLine("Created an immutable Fungible Token with ID: " + immutableFungibleTokenId);
            /// <summary>
            /// Step 7:
            /// Query and output immutable Fungible Token info after its creation.
            /// </summary>
            var immutableFungibleTokenTokenInfo_AfterCreation = new TokenInfoQuery { TokenId = immutableFungibleTokenId }.Execute(client);

            // Check that metadata was set correctly.
            if (Equals(immutableFungibleTokenTokenInfo_AfterCreation.Metadata, initialTokenMetadata))
            {
                Console.WriteLine("Immutable Fungible Token metadata after creation: " + string.Join("; ", immutableFungibleTokenTokenInfo_AfterCreation.Metadata));
            }
            else
            {
                throw new Exception("Immutable Fungible Token metadata was not set correctly! (Fail)");
            }

            /// <summary>
            /// Step 8:
            /// Update immutable Fungible Token metadata.
            /// </summary>
            Console.WriteLine("Updating immutable Fungible Token metadata...");
            new TokenUpdateTransaction
            {
                TokenId = immutableFungibleTokenId,
                TokenMetadata = updatedTokenMetadata,
            }
            .FreezeWith(client)
            .Sign(metadataPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Query and output immutable Fungible Token info after its metadata was updated.
            /// </summary>
            var immutableFungibleTokenInfo_AfterMetadataUpdate = new TokenInfoQuery { TokenId = immutableFungibleTokenId }.Execute(client);

            // Check that metadata was updated correctly.
            if (Equals(immutableFungibleTokenInfo_AfterMetadataUpdate.Metadata, updatedTokenMetadata))
            {
                Console.WriteLine("Immutable Fungible Token metadata after update: " + string.Join("; ", immutableFungibleTokenInfo_AfterMetadataUpdate.Metadata));
            }
            else
            {
                throw new Exception("Immutable Fungible Token metadata was not updated correctly! (Fail)");
            }

            /// <summary>
            /// Clean up:
            /// Delete created mutable token.
            /// </summary>
            new TokenDeleteTransaction { TokenId = mutableFungibleTokenId }.FreezeWith(client).Sign(adminPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Token Metadata (HIP-646 and HIP-765) Example Complete!");
        }
    }
}