// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class TransferTransactionHooksExample
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
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "localhost";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Transfer Transaction Hooks Example Start!");
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
            /// Set up prerequisites: Use existing accounts and create tokens.
            /// Note: This is not part of TransferTransaction itself, but required for the example.
            /// </summary>
            Console.WriteLine("Setting up prerequisites...");

            // Use existing accounts (following TransferCryptoExample pattern)
            AccountId senderAccountId = OPERATOR_ID; // Operator is typically the sender
            AccountId receiverAccountId = AccountId.FromString("0.0.3");

            // Create a fungible token
            TokenId fungibleTokenId = CreateFungibleToken(client);

            // Create an NFT token and mint an NFT
            TokenId nftTokenId = CreateNftToken(client);
            NftId nftId = MintNft(client, nftTokenId);
            /// <summary>
            /// Step 2:
            /// Demonstrate TransferTransaction API with hooks (demonstration only).
            /// Note: This shows the API structure - actual execution requires hooks to exist on the network.
            /// </summary>
            Console.WriteLine("\n=== TransferTransaction with Hooks API Demonstration ===");

            // Create different hooks for different transfer types (for demonstration)
            Console.WriteLine("Creating hook call objects (demonstration)...");

            // HBAR transfer with pre-tx allowance hook
            FungibleHookCall hbarHook = new FungibleHookCall(1001, new EvmHookCall(new byte[] { 0x01, 0x02 }, 20000), FungibleHookType.PreTxAllowanceHook);

            // NFT sender hook (pre-hook)
            NftHookCall nftSenderHook = new NftHookCall(1002, new EvmHookCall(new byte[] { 0x03, 0x04 }, 20000), NftHookType.PreHookSender);

            // NFT receiver hook (pre-hook)
            NftHookCall nftReceiverHook = new NftHookCall(1003, new EvmHookCall(new byte[] { 0x05, 0x06 }, 20000), NftHookType.PreHookReceiver);

            // Fungible token transfer with pre-post allowance hook
            FungibleHookCall fungibleTokenHook = new FungibleHookCall(1004, new EvmHookCall(new byte[] { 0x07, 0x08 }, 20000), FungibleHookType.PrePostTxAllowanceHook);

            // Build TransferTransaction with hooks (demonstration)
            Console.WriteLine("Building TransferTransaction with hooks...");
            new TransferTransaction()
                .AddHbarTransferWithHook(senderAccountId, Hbar.From(-100), hbarHook)
                .AddHbarTransfer(receiverAccountId, Hbar.From(100))
                .AddNftTransferWithHook(nftId, senderAccountId, receiverAccountId, nftSenderHook, nftReceiverHook)
                .AddTokenTransferWithHook(fungibleTokenId, senderAccountId, -1000, fungibleTokenHook)
                .AddTokenTransfer(fungibleTokenId, receiverAccountId, 1000);
            Console.WriteLine("TransferTransaction built successfully with the following hook calls:");
            Console.WriteLine("  - HBAR transfer with pre-tx allowance hook (ID: 1001)");
            Console.WriteLine("  - NFT transfer with sender hook (ID: 1002) and receiver hook (ID: 1003)");
            Console.WriteLine("  - Fungible token transfer with pre-post allowance hook (ID: 1004)");

            // Demonstrate the API without executing (since hooks don't exist)
            Console.WriteLine("\nNote: This demonstrates the TransferTransaction API with hooks.");
            Console.WriteLine("To actually execute this transaction, the hooks (IDs 1001-1004) must exist on the network.");
            Console.WriteLine("The transaction would be executed with: transferTx.execute(client)");

            // Show a simple transfer without hooks that actually works
            Console.WriteLine("\n=== Executing Simple Transfer (without hooks) ===");
            try
            {
                TransactionResponse simpleTransferResponse = new TransferTransaction()
                    .AddHbarTransfer(senderAccountId, Hbar.From(-1))
                    .AddHbarTransfer(receiverAccountId, Hbar.From(1)).Execute(client);
                simpleTransferResponse.GetReceipt(client);
                Console.WriteLine("Successfully executed simple HBAR transfer!");
                Console.WriteLine("Transaction ID: " + simpleTransferResponse.TransactionId);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute simple transfer: " + e.Message);
            }

            client.Dispose();
            Console.WriteLine("Transfer Transaction Hooks Example Complete!");
        }

        /// <summary>
        /// Creates a fungible token for the example.
        /// </summary>
        private static TokenId CreateFungibleToken(Client client)
        {
            Console.WriteLine("Creating fungible token...");
            TransactionResponse tokenCreateResponse = new TokenCreateTransaction 
            {
                TokenName = "Example Fungible Token",
                TokenSymbol = "EFT",
                TokenType = TokenType.FungibleCommon,
                Decimals = 2,
                InitialSupply = 10000,
                TreasuryAccountId = OPERATOR_ID,
                AdminKey = OPERATOR_KEY,
                SupplyKey = OPERATOR_KEY,

            }.Execute(client);
            TransactionReceipt tokenCreateReceipt = tokenCreateResponse.GetReceipt(client);
            TokenId tokenId = tokenCreateReceipt.TokenId;
            Console.WriteLine("Created fungible token with ID: " + tokenId);
            return tokenId;
        }

        /// <summary>
        /// Creates an NFT token for the example.
        /// </summary>
        private static TokenId CreateNftToken(Client client)
        {
            Console.WriteLine("Creating NFT token...");
            TransactionResponse tokenCreateResponse = new TokenCreateTransaction
            {
                TokenName = "Example NFT Token",
                TokenSymbol = "ENT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = OPERATOR_ID,
                AdminKey = OPERATOR_KEY,
                SupplyKey = OPERATOR_KEY,

            }.Execute(client);
            TransactionReceipt tokenCreateReceipt = tokenCreateResponse.GetReceipt(client);
            TokenId tokenId = tokenCreateReceipt.TokenId;
            Console.WriteLine("Created NFT token with ID: " + tokenId);
            return tokenId;
        }

        /// <summary>
        /// Mints an NFT for the example.
        /// </summary>
        private static NftId MintNft(Client client, TokenId tokenId)
        {
            Console.WriteLine("Minting NFT...");

            // Create metadata for the NFT
            byte[] metadata = Encoding.UTF8.GetBytes("Example NFT Metadata");
            TransactionResponse mintResponse = new TokenMintTransaction
            {
                TokenId = tokenId,
                Metadata = [metadata],

            }.Execute(client);
            TransactionReceipt mintReceipt = mintResponse.GetReceipt(client);
            long serialNumber = mintReceipt.Serials[0];
            NftId nftId = new (tokenId, serialNumber);
            Console.WriteLine("Minted NFT with ID: " + nftId);
            return nftId;
        }
    }
}