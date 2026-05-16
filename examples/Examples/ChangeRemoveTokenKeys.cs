// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class ChangeRemoveTokenKeys
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
            Console.WriteLine("Change Or Remove Existing Keys From A Token (HIP-540) Example Start!");
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
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey adminPrivateKey = PrivateKey.GenerateED25519();
            PublicKey adminPublicKey = adminPrivateKey.GetPublicKey();
            PrivateKey supplyPrivateKey = PrivateKey.GenerateED25519();
            PublicKey supplyPublicKey = supplyPrivateKey.GetPublicKey();
            PrivateKey newSupplyPrivateKey = PrivateKey.GenerateED25519();
            PublicKey newSupplyPublicKey = newSupplyPrivateKey.GetPublicKey();
            PrivateKey wipePrivateKey = PrivateKey.GenerateED25519();
            PublicKey wipePublicKey = wipePrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create NFT and check its keys.
            /// </summary>
            Console.WriteLine("Creating NFT using the Hedera Token Service...");
            var nftTokenId = new TokenCreateTransaction
            {
                TokenName = "HIP-540 NFT",
                TokenSymbol = "HIP540NFT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = OPERATOR_ID,
                AdminKey = adminPublicKey,
                WipeKey = wipePublicKey,
                SupplyKey = supplyPublicKey,
            }
            .FreezeWith(client)
            .Sign(adminPrivateKey)
            .Execute(client)
            .GetReceipt(client).TokenId;
            var nftInfoBefore = new TokenInfoQuery { TokenId = nftTokenId }.Execute(client);
            if (nftInfoBefore.AdminKey != null && nftInfoBefore.SupplyKey != null && nftInfoBefore.WipeKey != null)
            {
                Console.WriteLine("Admin public key in the newly created token: " + nftInfoBefore.AdminKey);
                Console.WriteLine("Supply public key in the newly created token: " + nftInfoBefore.SupplyKey);
                Console.WriteLine("Wipe public key in the newly created token: " + nftInfoBefore.WipeKey);
            }
            else
            {
                throw new Exception("The required keys are not set correctly! (Fail)");
            }

            /// <summary>
            /// Step 3:
            /// Remove Wipe Key from a token (by updating it to an empty Key List) and check that its removed.
            /// </summary>
            Console.WriteLine("Removing the Wipe Key...(updating to an empty Key List).");

            // This HIP introduces ability to remove lower-privilege keys
            // (Wipe, KYC, Freeze, Pause, Supply, Fee Schedule, Metadata) from a Token
            // using an update with the empty KeyList.
            var emptyKeyList = new KeyList();
            new TokenUpdateTransaction
            {
                TokenId = nftTokenId,
                WipeKey = emptyKeyList,
                TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
            }
            .FreezeWith(client)
            .Sign(adminPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            var nftInfoAfterWipeKeyRemoval = new TokenInfoQuery { TokenId = nftTokenId }.Execute(client);
            if (nftInfoAfterWipeKeyRemoval.WipeKey == null)
            {
                Console.WriteLine("Token Wipe Public Key (after removal): " + nftInfoAfterWipeKeyRemoval.WipeKey);
            }
            else
            {
                throw new Exception("Token Wipe Key was not removed after removal operation! (Fail)");
            }

            /// <summary>
            /// Step 4:
            /// Remove Admin Key from a token (by updating it to an empty Key List) and check that its removed.
            /// </summary>
            Console.WriteLine("Removing the Admin Key...(updating to an empty Key List).");
            new TokenUpdateTransaction
            {
                TokenId = nftTokenId,
                AdminKey = emptyKeyList,
                TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
            }
            .FreezeWith(client)
            .Sign(adminPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            var nftInfoAfterAdminKeyRemoval = new TokenInfoQuery { TokenId = nftTokenId }.Execute(client);
            if (nftInfoAfterAdminKeyRemoval.AdminKey == null)
            {
                Console.WriteLine("Token Admin Public Key (after removal): " + nftInfoAfterAdminKeyRemoval.AdminKey);
            }
            else
            {
                throw new Exception("Token Admin Key was not removed after removal operation! (Fail)");
            }

            /// <summary>
            /// Step 5:
            /// Update Supply Key and check that its updated.
            /// </summary>
            Console.WriteLine("Updating the Supply Key...(to the new key).");
            new TokenUpdateTransaction
            {
                TokenId = nftTokenId,
                SupplyKey = newSupplyPublicKey,
                TokenKeyVerificationMode = TokenKeyValidation.FullValidation,
            }
            .FreezeWith(client)
            .Sign(supplyPrivateKey)
            .Sign(newSupplyPrivateKey)
            .Execute(client).GetReceipt(client);
            var nftInfoAfterSupplyKeyUpdate = new TokenInfoQuery { TokenId = nftTokenId }.Execute(client);
            if (nftInfoAfterSupplyKeyUpdate.SupplyKey.Equals(newSupplyPublicKey))
            {
                Console.WriteLine("Token Supply Public Key (after update): " + nftInfoAfterSupplyKeyUpdate.SupplyKey);
            }
            else
            {
                throw new Exception("Token Supply Key was not updated correctly! (Fail)");
            }

            /// <summary>
            /// Step 6:
            /// Remove Supply Key (update to the unusable key).
            /// </summary>
            Console.WriteLine("Removing the Supply Key...(updating to the unusable key).");
            new TokenUpdateTransaction
            {
                TokenId = nftTokenId,
                SupplyKey = PublicKey.UnusableKey(),
                TokenKeyVerificationMode = TokenKeyValidation.NoValidation,
            }
            .FreezeWith(client)
            .Sign(newSupplyPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            var nftInfoAfterSupplyKeyRemoval = new TokenInfoQuery { TokenId = nftTokenId }.Execute(client);
            var supplyKeyAfterRemoval = (PublicKey)nftInfoAfterSupplyKeyRemoval.SupplyKey;
            if (supplyKeyAfterRemoval.Equals(PublicKey.UnusableKey()))
            {
                Console.WriteLine("Token Supply Public Key (after removal): " + supplyKeyAfterRemoval.ToStringRaw());
            }
            else
            {
                throw new Exception("Token Supply key was not removed after removal operation! (Fail)");
            }

            /// <summary>
            /// Clean up:
            /// Can't delete a token as it is immutable.
            /// </summary>
            client.Dispose();
            Console.WriteLine("Change Or Remove Existing Keys From A Token (HIP-540) Example Complete!");
        }
    }
}