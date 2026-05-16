// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    public class TokenAirdropExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        /// <summary>
        /// Operator's account ID. Used to sign and pay for operations on Hedera.
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
            Console.WriteLine("Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure SDK Client.
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
            /// Create 4 accounts
            /// </summary>
            var privateKey1 = PrivateKey.GenerateECDSA();
            var alice = new AccountCreateTransaction
            {
                InitialBalance = new Hbar(10),
                MaxAutomaticTokenAssociations = -1,
            }
            .SetKeyWithoutAlias(privateKey1)                
            .Execute(client).GetReceipt(client).AccountId;
            var privateKey2 = PrivateKey.GenerateECDSA();
            var bob = new AccountCreateTransaction { MaxAutomaticTokenAssociations = 1 }.SetKeyWithoutAlias(privateKey2).Execute(client).GetReceipt(client).AccountId;
            var privateKey3 = PrivateKey.GenerateECDSA();
            var carol = new AccountCreateTransaction { MaxAutomaticTokenAssociations = 0 }.SetKeyWithoutAlias(privateKey3).Execute(client).GetReceipt(client).AccountId;
            var treasuryKey = PrivateKey.GenerateECDSA();
            var treasuryAccount = new AccountCreateTransaction { InitialBalance = new Hbar(10) }
            .SetKeyWithoutAlias(treasuryKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            /// <summary>
            /// Step 2:
            /// Create FT and NFT and mint
            /// </summary>
            var tokenID = new TokenCreateTransaction
            {
                TokenName = "Fungible Token",
                TokenSymbol = "TNFT",
                TokenMemo = "Example memo",
                Decimals = 3,
                InitialSupply = 100,
                MaxSupply = 100,
                TreasuryAccountId = treasuryAccount,
                AdminKey = client.OperatorPublicKey,
                FreezeKey = client.OperatorPublicKey,
                SupplyKey = client.OperatorPublicKey,
                MetadataKey = client.OperatorPublicKey,
                PauseKey = client.OperatorPublicKey,
            }
            .FreezeWith(client)
            .Sign(treasuryKey)
            .Execute(client)
            .GetReceipt(client).TokenId;
            var nftID = new TokenCreateTransaction
            {
                TokenName = "Test NFT",
                TokenSymbol = "TNFT",
                TokenType = TokenType.NonFungibleUnique,
                TreasuryAccountId = treasuryAccount,
                MaxSupply = 10,
                TokenSupplyType = TokenSupplyType.Finite,
                AdminKey = client.OperatorPublicKey,
                FreezeKey = client.OperatorPublicKey,
                SupplyKey = client.OperatorPublicKey,
                MetadataKey = client.OperatorPublicKey,
                PauseKey = client.OperatorPublicKey,
            }
            .FreezeWith(client)
            .Sign(treasuryKey)
            .Execute(client)
            .GetReceipt(client).TokenId;
            new TokenMintTransaction
            {
                TokenId = nftID,
                Metadata = GenerateNftMetadata((byte)3),
            }
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 3:
            /// Airdrop fungible tokens to all 3 accounts
            /// </summary>
            Console.WriteLine("Airdropping fts");
            var txnRecord = new TokenAirdropTransaction()
                
                .AddTokenTransfer(tokenID, alice, 10)
                
                .AddTokenTransfer(tokenID, treasuryAccount, -10)
                
                .AddTokenTransfer(tokenID, bob, 10)
                
                .AddTokenTransfer(tokenID, treasuryAccount, -10)
                
                .AddTokenTransfer(tokenID, carol, 10)
                
                .AddTokenTransfer(tokenID, treasuryAccount, -10).FreezeWith(client).Sign(treasuryKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 4:
            /// Get the transaction record and see one pending airdrop (for carol)
            /// </summary>
            Console.WriteLine("Pending airdrops length: " + txnRecord.PendingAirdropRecords.Count);
            Console.WriteLine("Pending airdrops: " + txnRecord.PendingAirdropRecords[0]);
            /// <summary>
            /// Step 5:
            /// Query to verify alice and bob received the airdrops and carol did not
            /// </summary>
            var aliceBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            var bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            var carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Alice ft balance after airdrop: " + aliceBalance.Tokens[tokenID]);
            Console.WriteLine("Bob ft balance after airdrop: " + bobBalance.Tokens[tokenID]);
            Console.WriteLine("Carol ft balance after airdrop: " + carolBalance.Tokens[tokenID]);
            /// <summary>
            /// Step 6:
            /// Claim the airdrop for carol
            /// </summary>
            Console.WriteLine("Claiming ft with carol");
            new TokenClaimAirdropTransaction
            {
                PendingAirdropIds = { txnRecord.PendingAirdropRecords[0].PendingAirdropId }

            }.FreezeWith(client).Sign(privateKey3).Execute(client).GetReceipt(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol ft balance after claim: " + carolBalance.Tokens[tokenID]);
            /// <summary>
            /// Step 7:
            /// Airdrop the NFTs to all three accounts
            /// </summary>
            Console.WriteLine("Airdropping nfts");
            txnRecord = new TokenAirdropTransaction()
                .AddNftTransfer(nftID.Nft(1), treasuryAccount, alice)
                .AddNftTransfer(nftID.Nft(2), treasuryAccount, bob)
                .AddNftTransfer(nftID.Nft(3), treasuryAccount, carol).FreezeWith(client).Sign(treasuryKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 8:
            /// Get the transaction record and verify two pending airdrops (for bob & carol)
            /// </summary>
            Console.WriteLine("Pending airdrops length: " + txnRecord.PendingAirdropRecords.Count);
            Console.WriteLine("Pending airdrops for Bob: " + txnRecord.PendingAirdropRecords[0]);
            Console.WriteLine("Pending airdrops for Carol: " + txnRecord.PendingAirdropRecords[1]);
            /// <summary>
            /// Step 9:
            /// Query to verify alice received the airdrop and bob and carol did not
            /// </summary>
            aliceBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Alice nft balance after airdrop: " + aliceBalance.Tokens[nftID]);
            Console.WriteLine("Bob nft balance after airdrop: " + bobBalance.Tokens[nftID]);
            Console.WriteLine("Carol nft balance after airdrop: " + carolBalance.Tokens[nftID]);
            /// <summary>
            /// Step 10:
            /// Claim the airdrop for bob
            /// </summary>
            Console.WriteLine("Claiming nft with Bob");
            new TokenClaimAirdropTransaction 
            { 
                PendingAirdropIds = { txnRecord.PendingAirdropRecords[0].PendingAirdropId } 
            
            }.FreezeWith(client).Sign(privateKey2).Execute(client).GetReceipt(client);
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            Console.WriteLine("Bob nft balance after claim: " + bobBalance.Tokens[nftID]);
            /// <summary>
            /// Step 11:
            /// Cancel the airdrop for carol
            /// </summary>
            Console.WriteLine("Canceling nft for Carol");
            new TokenCancelAirdropTransaction 
            { 
                PendingAirdropIds = { txnRecord.PendingAirdropRecords[1].PendingAirdropId } 
            
            }.FreezeWith(client).Sign(treasuryKey).Execute(client).GetReceipt(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol nft balance after cancel: " + carolBalance.Tokens[nftID]);
            /// <summary>
            /// Step 12:
            /// Reject the NFT for bob
            /// </summary>
            Console.WriteLine("Rejecting nft with Bob");
            new TokenRejectTransaction { OwnerId = bob }
            .AddNftId(nftID.Nft(2)).FreezeWith(client).Sign(privateKey2).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 13:
            /// Query to verify bob no longer has the NFT
            /// </summary>
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            Console.WriteLine("Bob nft balance after reject: " + bobBalance.Tokens[nftID]);
            /// <summary>
            /// Step 13:
            /// Query to verify the NFT was returned to the Treasury
            /// </summary>
            var treasuryBalance = new AccountBalanceQuery { AccountId = treasuryAccount }.Execute(client);
            Console.WriteLine("Treasury nft balance after reject: " + treasuryBalance.Tokens[nftID]);
            /// <summary>
            /// Step 14:
            /// Reject the fungible tokens for Carol
            /// </summary>
            Console.WriteLine("Rejecting ft with Carol");
            new TokenRejectTransaction { OwnerId = carol }
            .AddTokenId(tokenID).FreezeWith(client).Sign(privateKey3).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 14:
            /// Query to verify carol no longer has the fungible tokens
            /// </summary>
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol ft balance after reject: " + carolBalance.Tokens[tokenID]);
            /// <summary>
            /// Step 15:
            /// Query to verify Treasury received the rejected fungible tokens
            /// </summary>
            treasuryBalance = new AccountBalanceQuery { AccountId = treasuryAccount }.Execute(client);
            Console.WriteLine("Treasury ft balance after reject: " + treasuryBalance.Tokens[tokenID]);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Example Complete!");
        }

        private static List<byte[]> GenerateNftMetadata(byte metadataCount)
        {
            List<byte[]> metadatas = [];
            for (byte i = 0; i < metadataCount; i++)
            {
                byte[] md = [i];
                metadatas
                    .Add(md);
            }

            return metadatas;
        }
    }
}