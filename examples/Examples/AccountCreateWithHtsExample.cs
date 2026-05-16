// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class AccountCreateWithHtsExample
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
            Console.WriteLine("Account Auto-Creation Via HTS Assets (HIP-542) Example Start!");
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
                // Set the maximum fee to be paid for transactions executed by this client.
                _client.DefaultMaxTransactionFee = Hbar.From(10);
            });

            /// <summary>
            /// Step 1:
            /// Generate ECDSA keys pairs.
            /// </summary>
            PublicKey operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            Console.WriteLine("Generating ECDSA key pairs...");
            PrivateKey supplyPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey supplyPublicKey = supplyPrivateKey.GetPublicKey();
            PrivateKey freezePrivateKey = PrivateKey.GenerateECDSA();
            PublicKey freezePublicKey = freezePrivateKey.GetPublicKey();
            PrivateKey wipePrivateKey = PrivateKey.GenerateECDSA();
            PublicKey wipePublicKey = wipePrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// The beginning of the first example (with NFT).
            ///
            /// Create NFT using the Hedera Token Service.
            /// </summary>
            Console.WriteLine("The beginning of the first example (with NFT)...");

            // IPFS content identifiers for the NFT metadata.
            string[] CIDs = new string[]
            {
                "QmNPCiNA3Dsu3K5FxDPMG5Q3fZRwVTg14EXA92uqEeSRXn",
                "QmZ4dgAgt8owvnULxnKxNe8YqpavtVCXmc1Lt2XajFpJs9",
                "QmPzY5GxevjyfMUF5vEAjtyRoigzWp47MiKAtLBduLMC1T",
                "Qmd3kGgSrAwwSrhesYcY7K54f3qD7MDo38r7Po2dChtQx5",
                "QmWgkKz3ozgqtnvbCLeh7EaR1H8u5Sshx3ZJzxkcrT3jbw"
            };
            Console.WriteLine("Creating NFT using the Hedera Token Service...");
            TokenCreateTransaction nftCreateTx = new TokenCreateTransaction
            {
                TokenName = "HIP-542 NFT",
                TokenSymbol = "HIP542NFT",
                TokenType = TokenType.NonFungibleUnique,
                Decimals = 0,
                InitialSupply = 0,
                MaxSupply = CIDs.Length,
                TreasuryAccountId = OPERATOR_ID,
                TokenSupplyType = TokenSupplyType.Finite,
                AdminKey = operatorPublicKey,
                FreezeKey = freezePublicKey,
                WipeKey = wipePublicKey,
                SupplyKey = supplyPublicKey,

            }.FreezeWith(client);

            // Sign the transaction with the operator key.
            TokenCreateTransaction nftCreateTxSigned = nftCreateTx.Sign(OPERATOR_KEY);

            // Submit the transaction to the Hedera network.
            TransactionResponse nftCreateTxResponse = nftCreateTxSigned.Execute(client);

            // Get transaction receipt information.
            TransactionReceipt nftCreateTxReceipt = nftCreateTxResponse.GetReceipt(client);
            TokenId nftTokenId = nftCreateTxReceipt.TokenId;

            Console.WriteLine("Created NFT with token ID: " + nftTokenId);
            /// <summary>
            /// Step 3:
            /// Mint NFTs.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            TransactionReceipt[] nftMintTxReceipts = new TransactionReceipt[CIDs.Length];
            for (int i = 0; i < CIDs.Length; i++)
            {
                byte[] nftMetadata = Encoding.UTF8.GetBytes(CIDs[i]);
                TokenMintTransaction nftMintTx = new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = [nftMetadata]
                
                }.FreezeWith(client);
                TokenMintTransaction nftMintTxSigned = nftMintTx.Sign(supplyPrivateKey);
                TransactionResponse nftMintTxResponse = nftMintTxSigned.Execute(client);
                nftMintTxReceipts[i] = nftMintTxResponse.GetReceipt(client);
                Console.WriteLine("Minted NFT (token ID: " + nftTokenId + ") with serial: " + nftMintTxReceipts[i].Serials[0]);
            }

            long exampleNftId = nftMintTxReceipts[0].Serials[0];
            /// <summary>
            /// Step 4:
            /// Create an ECDSA public key alias.
            /// </summary>
            PrivateKey alicePrivateKey = PrivateKey.GenerateECDSA();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            Console.WriteLine("\"Creating\" Alice's account...");

            // Assuming that the target shard and realm are known.
            // For now they are virtually always 0 and 0.
            AccountId aliceAliasAccountId = alicePublicKey.ToAccountId(0, 0);
            Console.WriteLine("Alice's account ID: " + aliceAliasAccountId);
            Console.WriteLine("Alice's alias key: " + aliceAliasAccountId.AliasKey);
            /// <summary>
            /// Step 5:
            /// Transfer the NFT to Alice's public key alias using the transfer transaction.
            /// </summary>
            Console.WriteLine("Transferring NFT to Alice's account...");
            TransferTransaction nftTransferTx = new TransferTransaction()
                .AddNftTransfer(nftTokenId.Nft(exampleNftId), OPERATOR_ID, aliceAliasAccountId)
                .FreezeWith(client);

            // Sign the transaction with the operator key.
            TransferTransaction nftTransferTxSigned = nftTransferTx.Sign(OPERATOR_KEY);

            // Submit the transaction to the Hedera network.
            TransactionResponse nftTransferTxResponse = nftTransferTxSigned.Execute(client);

            // Get transaction receipt information here.
            nftTransferTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 6:
            /// Get the new account ID from the child record.
            /// </summary>
            IList<TokenNftInfo> nftsInfo = new TokenNftInfoQuery
            {
                NftId = nftTokenId.Nft(exampleNftId)

            }.Execute(client);
            string nftOwnerAccountId_FromChildRecord = nftsInfo[0].AccountId.ToString();
            Console.WriteLine("Current owner account ID: " + nftOwnerAccountId_FromChildRecord);
            /// <summary>
            /// Step 7:
            /// Show the normal account ID of account which owns the NFT.
            /// </summary>
            string nftOwnerAccountId_FromQuery = new AccountInfoQuery { AccountId = aliceAliasAccountId }.Execute(client).AccountId.ToString();
            Console.WriteLine("The \"normal\" account ID of the given alias: " + nftOwnerAccountId_FromQuery);
            /// <summary>
            /// Step 8:
            /// Validate that account ID value from the child record is equal to normal account ID value from the query.
            /// </summary>
            if (nftOwnerAccountId_FromChildRecord.Equals(nftOwnerAccountId_FromQuery))
            {
                Console.WriteLine("The NFT owner account ID matches the account ID created with the HTS! (Success)");
            }
            else
            {
                throw new Exception("The two account IDs does not match! (Error)");
            }

            /// <summary>
            /// Step 9:
            /// The beginning of the second example (with Fungible Token).
            /// Create a fungible HTS token using the Hedera Token Service.
            /// </summary>
            Console.WriteLine("The beginning of the second example (with Fungible Token).");
            Console.WriteLine("Creating Fungible Token using the Hedera Token Service...");
            TokenCreateTransaction ftCreateTx = new TokenCreateTransaction
            {
                TokenName = "HIP-542 Fungible Token",
                TokenSymbol = "HIP542FT",
                InitialSupply = 10000,
                Decimals = 2,
                TokenType = TokenType.FungibleCommon,
                TreasuryAccountId = OPERATOR_ID,
                AutoRenewAccountId = OPERATOR_ID,
                AdminKey = operatorPublicKey,
                WipeKey = wipePrivateKey

            }.FreezeWith(client);

            // Sign the transaction with the operator key.
            TokenCreateTransaction ftCreateTxSigned = ftCreateTx.Sign(OPERATOR_KEY);

            // Submit the transaction to the Hedera network.
            TransactionResponse ftCreateResponse = ftCreateTxSigned.Execute(client);

            // Get transaction receipt information.
            TransactionReceipt ftCreateReceipt = ftCreateResponse.GetReceipt(client);
            TokenId fungibleTokenId = ftCreateReceipt.TokenId;
            Console.WriteLine("Created fungible token with ID: " + fungibleTokenId);
            /// <summary>
            /// Step 10:
            /// Create an ECDSA public key alias.
            /// </summary>
            PrivateKey bobPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            Console.WriteLine("\"Creating\" Bob's account...");

            // Assuming that the target shard and realm are known.
            // For now, they are virtually always 0 and 0.
            AccountId bobAliasAccountId = bobPublicKey.ToAccountId(0, 0);
            Console.WriteLine("Bob's account ID: " + bobAliasAccountId);
            Console.WriteLine("Bob's alias key: " + bobAliasAccountId.AliasKey);
            /// <summary>
            /// Step 11:
            /// Transfer the Fungible Token to the Bob's public key alias using the transfer transaction.
            /// </summary>
            Console.WriteLine("Transferring Fungible Token the Bob's account...");
            TransferTransaction tokenTransferTx = new TransferTransaction().AddTokenTransfer(fungibleTokenId, OPERATOR_ID, -10).AddTokenTransfer(fungibleTokenId, bobAliasAccountId, 10).FreezeWith(client);

            // Sign the transaction with the operator key.
            TransferTransaction tokenTransferTxSign = tokenTransferTx.Sign(OPERATOR_KEY);

            // Submit the transaction to the Hedera network.
            TransactionResponse tokenTransferSubmit = tokenTransferTxSign.Execute(client);

            // Get transaction receipt information.
            tokenTransferSubmit.GetReceipt(client);
            /// <summary>
            /// Step 12:
            /// Get the new account ID from the child record.
            /// </summary>
            string bobAccountInfo = new AccountInfoQuery { AccountId = bobAliasAccountId }.Execute(client).AccountId.ToString();
            Console.WriteLine("The \"normal\" account ID of the given alias: " + bobAccountInfo);
            /// <summary>
            /// Step 13:
            /// Show the normal account ID of account which owns the NFT.
            /// </summary>
            AccountBalance bobAccountBalances = new AccountBalanceQuery { AccountId = bobAliasAccountId }.Execute(client);
            /// <summary>
            /// Step 14:
            /// Validate token balance of newly created account.
            /// </summary>
            int bobFtBalance = (int)bobAccountBalances.Tokens[fungibleTokenId];
            if (bobFtBalance == 10)
            {
                Console.WriteLine("New account was created using HTS TransferTransaction! (Success)");
            }
            else
            {
                throw new Exception("Creating account with HTS using public key alias failed! (Error)");
            }

            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>
            AccountId nftOwnerAccountId = AccountId.FromString(nftOwnerAccountId_FromQuery);
            new TokenWipeTransaction
            {
                TokenId = nftTokenId,
                AccountId = nftOwnerAccountId,
                Serials = [exampleNftId]
            }
            .FreezeWith(client)
            .Sign(wipePrivateKey)
            .Execute(client)
            .GetReceipt(client);

            AccountId bobAccountId = AccountId.FromString(bobAccountInfo);
            Dictionary<TokenId, ulong> bobsTokens = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client).Tokens;
            new TokenWipeTransaction
            {
                TokenId = fungibleTokenId,
                Amount = bobsTokens[fungibleTokenId],
                AccountId = bobAccountId,
            }
            .FreezeWith(client)
            .Sign(wipePrivateKey)
            .Execute(client)
            .GetReceipt(client);

            new AccountDeleteTransaction
            {
                AccountId = nftOwnerAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(alicePrivateKey)
            .Execute(client)
            .GetReceipt(client);

            new AccountDeleteTransaction
            {
                AccountId = bobAccountId,
                TransferAccountId = OPERATOR_ID
            }
            .FreezeWith(client)
            .Sign(bobPrivateKey)
            .Execute(client)
            .GetReceipt(client);

            new TokenDeleteTransaction { TokenId = nftTokenId }
            .FreezeWith(client)
            .Sign(OPERATOR_KEY)
            .Execute(client)
            .GetReceipt(client);

            new TokenDeleteTransaction { TokenId = fungibleTokenId }
            .FreezeWith(client)
            .Sign(OPERATOR_KEY)
            .Execute(client)
            .GetReceipt(client);

            client.Dispose();
            Console.WriteLine("Account Auto-Creation Via HTS Assets (HIP-542) Example Complete!");
        }
    }
}