// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    public class TokenRejectExample
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
            Console.WriteLine("Token Reject (HIP-904) Example Start!");
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
            PrivateKey treasuryAccountPrivateKey = PrivateKey.GenerateED25519();
            PublicKey treasuryAccountPublicKey = treasuryAccountPrivateKey.GetPublicKey();
            PrivateKey receiverAccountPrivateKey = PrivateKey.GenerateED25519();
            PublicKey receiverAccountPublicKey = receiverAccountPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create accounts for this example.
            /// </summary>
            Console.WriteLine("Creating treasury and receiver accounts...");

            // Create a treasury account.
            var treasuryAccountId = new AccountCreateTransaction { MaxAutomaticTokenAssociations = 100 }.SetKeyWithoutAlias(treasuryAccountPublicKey).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).AccountId;
            // Create a receiver account with unlimited max auto associations (-1).
            var receiverAccountId = new AccountCreateTransaction { MaxAutomaticTokenAssociations = -1 }.SetKeyWithoutAlias(receiverAccountPublicKey).FreezeWith(client).Sign(receiverAccountPrivateKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 3:
            /// Create tokens for this example.
            /// </summary>
            Console.WriteLine("Creating FT and NFT...");

            // Create a Fungible Token.
            uint FUNGIBLE_TOKEN_SUPPLY = 1000000;
            TokenId fungibleTokenId = new TokenCreateTransaction
            {
                TokenName = "HIP-904 FT",
                TokenSymbol = "HIP904FT",
                Decimals = 0,
                InitialSupply = FUNGIBLE_TOKEN_SUPPLY,
                MaxSupply = FUNGIBLE_TOKEN_SUPPLY,
                TreasuryAccountId = treasuryAccountId,
                TokenSupplyType = TokenSupplyType.Finite,
                AdminKey = treasuryAccountPublicKey

            }.FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).TokenId;
            
            // Create NFT.
            TokenId nftId = new TokenCreateTransaction
            {
                TokenName = "HIP-904 NFT",
                TokenSymbol = "HIP904NFT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = treasuryAccountId,
                TokenSupplyType = TokenSupplyType.Finite,
                MaxSupply = 3,
                AdminKey = treasuryAccountPublicKey,
                SupplyKey = treasuryAccountPublicKey,

            }.FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).TokenId;
            /// <summary>
            /// Step 4:
            /// Mint three NFTs.
            /// </summary>
            Console.WriteLine("Minting three NFTs...");
            var tokenMintTxReceipt = new TokenMintTransaction
            {
                TokenId = nftId,
                Metadata = GenerateNftMetadata(3)

            }.FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client);
            var nftSerials = tokenMintTxReceipt.Serials;
            /// <summary>
            /// Step 5:
            /// Transfer tokens to the receiver.
            /// </summary>
            Console.WriteLine("Transferring tokens to the receiver...");
            new TransferTransaction().AddTokenTransfer(fungibleTokenId, treasuryAccountId, -1000).AddTokenTransfer(fungibleTokenId, receiverAccountId, 1000).AddNftTransfer(nftId.Nft(nftSerials[0]), treasuryAccountId, receiverAccountId).AddNftTransfer(nftId.Nft(nftSerials[1]), treasuryAccountId, receiverAccountId).AddNftTransfer(nftId.Nft(nftSerials[2]), treasuryAccountId, receiverAccountId).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 6:
            /// Check receiver account balance.
            /// </summary>
            var receiverAccountBalance = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(client);
            if (receiverAccountBalance.Tokens[fungibleTokenId] == 1000)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.Tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to transfer Fungible Token to the receiver account!");
            }

            if (receiverAccountBalance.Tokens[nftId] == 3)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.Tokens[nftId] + " example NFTs.");
            }
            else
            {
                throw new Exception("Failed to transfer NFT to the receiver account!");
            }

            /// <summary>
            /// Step 7:
            /// Reject the fungible token.
            /// </summary>
            Console.WriteLine("Receiver rejects example fungible tokens...");
            new TokenRejectTransaction { OwnerId = receiverAccountId }
            .AddTokenId(fungibleTokenId)
            .FreezeWith(client)
            .Sign(receiverAccountPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 8:
            /// Execute the token reject flow -- reject NFTs.
            /// </summary>
            Console.WriteLine("Receiver rejects example NFTs...");
            new TokenRejectFlow
            {
                OwnerId = receiverAccountId,
                NftIds = [nftId.Nft(nftSerials[0]), nftId.Nft(nftSerials[1]), nftId.Nft(nftSerials[2])],
                FreezeWithClient = client,
                SignPrivateKey = receiverAccountPrivateKey
            }
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 9:
            /// Check receiver account balance after token reject.
            /// </summary>
            var receiverAccountBalance_AfterTokenReject = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(client);
            if (receiverAccountBalance_AfterTokenReject.Tokens[fungibleTokenId] == 0)
            {
                Console.WriteLine("Receiver account has (after rejecting tokens): " + receiverAccountBalance_AfterTokenReject.Tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to reject Fungible Token!");
            }

            if (receiverAccountBalance_AfterTokenReject.Tokens[nftId] == null)
            {
                Console.WriteLine("Receiver account has (after rejecting tokens): " + receiverAccountBalance_AfterTokenReject.Tokens[nftId] + " example NFTs.");
            }
            else
            {
                throw new Exception("Failed to reject NFT!");
            }

            /// <summary>
            /// Step 10:
            /// Check treasury account balance after token reject.
            /// </summary>
            var treasuryAccountBalance = new AccountBalanceQuery { AccountId = treasuryAccountId }.Execute(client);
            if (treasuryAccountBalance.Tokens[fungibleTokenId] == FUNGIBLE_TOKEN_SUPPLY)
            {
                Console.WriteLine("Treasury account has: " + treasuryAccountBalance.Tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to transfer Fungible Token to the treasury account during token rejection!");
            }

            if (treasuryAccountBalance.Tokens[nftId] == 3)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.Tokens[nftId] + " example NFTs.");
            }
            else
            {
                throw new Exception("Failed to transfer NFT to the treasury account during token rejection!");
            }

            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = treasuryAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(treasuryAccountPrivateKey)
            .Execute(client);
            new AccountDeleteTransaction
            {
                AccountId = receiverAccountId,
                TransferAccountId = OPERATOR_ID
            }
            .FreezeWith(client)
            .Sign(receiverAccountPrivateKey)
            .Execute(client);
            new TokenDeleteTransaction { TokenId = fungibleTokenId }
            .FreezeWith(client)
            .Sign(treasuryAccountPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            new TokenDeleteTransaction { TokenId = nftId }
            .FreezeWith(client)
            .Sign(treasuryAccountPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Token Reject (HIP-904) Example Complete!");
        }

        private static List<byte[]> GenerateNftMetadata(byte metadataCount)
        {
            List<byte[]> metadatas = [];
            for (byte i = 0; i < metadataCount; i++)
            {
                byte[] md = [i];
                metadatas.Add(md);
            }

            return metadatas;
        }
    }
}