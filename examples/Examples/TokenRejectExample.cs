// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class TokenRejectExample
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
            var treasuryAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(treasuryAccountPublicKey).SetMaxAutomaticTokenAssociations(100).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).AccountId;
            treasuryAccountId;

            // Create a receiver account with unlimited max auto associations (-1).
            var receiverAccountId = new AccountCreateTransaction().SetKeyWithoutAlias(receiverAccountPublicKey).SetMaxAutomaticTokenAssociations(-1).FreezeWith(client).Sign(receiverAccountPrivateKey).Execute(client).GetReceipt(client).AccountId;
            receiverAccountId;
            /// <summary>
            /// Step 3:
            /// Create tokens for this example.
            /// </summary>
            Console.WriteLine("Creating FT and NFT...");

            // Create a Fungible Token.
            int FUNGIBLE_TOKEN_SUPPLY = 1000000;
            TokenId fungibleTokenId = new TokenCreateTransaction().SetTokenName("HIP-904 FT").SetTokenSymbol("HIP904FT").SetDecimals(0).SetInitialSupply(FUNGIBLE_TOKEN_SUPPLY).SetMaxSupply(FUNGIBLE_TOKEN_SUPPLY).SetTreasuryAccountId(treasuryAccountId).SetSupplyType(TokenSupplyType.FINITE).SetAdminKey(treasuryAccountPublicKey).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).TokenId;
            fungibleTokenId;

            // Create NFT.
            TokenId nftId = new TokenCreateTransaction().SetTokenName("HIP-904 NFT").SetTokenSymbol("HIP904NFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(treasuryAccountId).SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(3).SetAdminKey(treasuryAccountPublicKey).SetSupplyKey(treasuryAccountPublicKey).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client).TokenId;
            nftId;
            /// <summary>
            /// Step 4:
            /// Mint three NFTs.
            /// </summary>
            Console.WriteLine("Minting three NFTs...");
            var tokenMintTxReceipt = new TokenMintTransaction().SetTokenId(nftId).SetMetadata(GenerateNftMetadata((byte)3)).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client);
            var nftSerials = tokenMintTxReceipt.serials;
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
            if (receiverAccountBalance.tokens[fungibleTokenId] == 1000)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to transfer Fungible Token to the receiver account!");
            }

            if (receiverAccountBalance.tokens[nftId] == 3)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.tokens[nftId] + " example NFTs.");
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
            new TokenRejectTransaction().SetOwnerId(receiverAccountId).AddTokenId(fungibleTokenId).FreezeWith(client).Sign(receiverAccountPrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 8:
            /// Execute the token reject flow -- reject NFTs.
            /// </summary>
            Console.WriteLine("Receiver rejects example NFTs...");
            new TokenRejectFlow().SetOwnerId(receiverAccountId).SetNftIds(List.Of(nftId.Nft(nftSerials[0]), nftId.Nft(nftSerials[1]), nftId.Nft(nftSerials[2]))).FreezeWith(client).Sign(receiverAccountPrivateKey).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 9:
            /// Check receiver account balance after token reject.
            /// </summary>
            var receiverAccountBalance_AfterTokenReject = new AccountBalanceQuery { AccountId = receiverAccountId }.Execute(client);
            if (receiverAccountBalance_AfterTokenReject.tokens[fungibleTokenId] == 0)
            {
                Console.WriteLine("Receiver account has (after rejecting tokens): " + receiverAccountBalance_AfterTokenReject.tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to reject Fungible Token!");
            }

            if (receiverAccountBalance_AfterTokenReject.tokens[nftId] == null)
            {
                Console.WriteLine("Receiver account has (after rejecting tokens): " + receiverAccountBalance_AfterTokenReject.tokens[nftId] + " example NFTs.");
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
            if (treasuryAccountBalance.tokens[fungibleTokenId] == FUNGIBLE_TOKEN_SUPPLY)
            {
                Console.WriteLine("Treasury account has: " + treasuryAccountBalance.tokens[fungibleTokenId] + " example fungible tokens.");
            }
            else
            {
                throw new Exception("Failed to transfer Fungible Token to the treasury account during token rejection!");
            }

            if (treasuryAccountBalance.tokens[nftId] == 3)
            {
                Console.WriteLine("Receiver account has: " + receiverAccountBalance.tokens[nftId] + " example NFTs.");
            }
            else
            {
                throw new Exception("Failed to transfer NFT to the treasury account during token rejection!");
            }

            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>
            new AccountDeleteTransaction().SetAccountId(treasuryAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client);
            new AccountDeleteTransaction().SetAccountId(receiverAccountId).SetTransferAccountId(OPERATOR_ID).FreezeWith(client).Sign(receiverAccountPrivateKey).Execute(client);
            new TokenDeleteTransaction().SetTokenId(fungibleTokenId).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client);
            new TokenDeleteTransaction().SetTokenId(nftId).FreezeWith(client).Sign(treasuryAccountPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Token Reject (HIP-904) Example Complete!");
        }

        private static List<byte[]> GenerateNftMetadata(byte metadataCount)
        {
            List<byte[]> metadatas = new List();
            for (byte i = 0; i < metadataCount; i++)
            {
                byte[] md = new[]
                {
                    i
                };
                metadatas.Add(md);
            }

            return metadatas;
        }
    }
}