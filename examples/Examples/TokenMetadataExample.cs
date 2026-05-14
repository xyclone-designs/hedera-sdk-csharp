// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class TokenMetadataExample
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
            byte[] initialTokenMetadata = new byte[]
            {
                1,
                1,
                1,
                1,
                1
            };
            Console.WriteLine("Creating mutable Fungible Token using the Hedera Token Service...");
            var mutableFungibleTokenId = new TokenCreateTransaction().SetTokenName("HIP-646 Mutable FT").SetTokenSymbol("HIP646MFT").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetTreasuryAccountId(OPERATOR_ID).SetDecimals(3).SetInitialSupply(1000000).SetAdminKey(adminPublicKey).FreezeWith(client).Sign(adminPrivateKey).Execute(client).GetReceipt(client).TokenId;
            mutableFungibleTokenId;
            Console.WriteLine("Created mutable Fungible Token with ID: " + mutableFungibleTokenId);
            /// <summary>
            /// Step 3:
            /// Query and output mutable Fungible Token info after its creation.
            /// </summary>
            var mutableFungibleTokenInfo_AfterCreation = new TokenInfoQuery { TokenId = mutableFungibleTokenId }.Execute(client);

            // Check that metadata was set correctly.
            if (Arrays.Equals(mutableFungibleTokenInfo_AfterCreation.metadata, initialTokenMetadata))
            {
                Console.WriteLine("Mutable Fungible Token metadata after creation: " + Arrays.ToString(mutableFungibleTokenInfo_AfterCreation.metadata));
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
            new TokenUpdateTransaction().SetTokenId(mutableFungibleTokenId).SetTokenMetadata(updatedTokenMetadata).FreezeWith(client).Sign(adminPrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Query and output mutable Fungible Token info after its metadata was updated.
            /// </summary>
            var mutableFungibleTokenInfo_AfterMetadataUpdate = new TokenInfoQuery { TokenId = mutableFungibleTokenId }.Execute(client);

            // Check that metadata was updated correctly.
            if (Arrays.Equals(mutableFungibleTokenInfo_AfterMetadataUpdate.metadata, updatedTokenMetadata))
            {
                Console.WriteLine("Mutable Fungible Token metadata after update: " + Arrays.ToString(mutableFungibleTokenInfo_AfterMetadataUpdate.metadata));
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
            var immutableFungibleTokenId = new TokenCreateTransaction().SetTokenName("HIP-646 Immutable FT").SetTokenSymbol("HIP646IMMFT").SetTokenMetadata(initialTokenMetadata).SetTokenType(TokenType.FUNGIBLE_COMMON).SetTreasuryAccountId(OPERATOR_ID).SetMetadataKey(metadataPublicKey).SetDecimals(3).SetInitialSupply(1000000).Execute(client).GetReceipt(client).TokenId;
            immutableFungibleTokenId;
            Console.WriteLine("Created an immutable Fungible Token with ID: " + immutableFungibleTokenId);
            /// <summary>
            /// Step 7:
            /// Query and output immutable Fungible Token info after its creation.
            /// </summary>
            var immutableFungibleTokenTokenInfo_AfterCreation = new TokenInfoQuery { TokenId = immutableFungibleTokenId }.Execute(client);

            // Check that metadata was set correctly.
            if (Arrays.Equals(immutableFungibleTokenTokenInfo_AfterCreation.metadata, initialTokenMetadata))
            {
                Console.WriteLine("Immutable Fungible Token metadata after creation: " + Arrays.ToString(immutableFungibleTokenTokenInfo_AfterCreation.metadata));
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
            new TokenUpdateTransaction().SetTokenId(immutableFungibleTokenId).SetTokenMetadata(updatedTokenMetadata).FreezeWith(client).Sign(metadataPrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Query and output immutable Fungible Token info after its metadata was updated.
            /// </summary>
            var immutableFungibleTokenInfo_AfterMetadataUpdate = new TokenInfoQuery { TokenId = immutableFungibleTokenId }.Execute(client);

            // Check that metadata was updated correctly.
            if (Arrays.Equals(immutableFungibleTokenInfo_AfterMetadataUpdate.metadata, updatedTokenMetadata))
            {
                Console.WriteLine("Immutable Fungible Token metadata after update: " + Arrays.ToString(immutableFungibleTokenInfo_AfterMetadataUpdate.metadata));
            }
            else
            {
                throw new Exception("Immutable Fungible Token metadata was not updated correctly! (Fail)");
            }

            /// <summary>
            /// Clean up:
            /// Delete created mutable token.
            /// </summary>
            new TokenDeleteTransaction().SetTokenId(mutableFungibleTokenId).FreezeWith(client).Sign(adminPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Token Metadata (HIP-646 and HIP-765) Example Complete!");
        }
    }
}