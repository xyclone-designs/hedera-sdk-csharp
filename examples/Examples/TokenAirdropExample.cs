// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

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
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        private static readonly string HEDERA_NETWORK = Dotenv.Load().Get("HEDERA_NETWORK", "testnet");
        private static readonly string SDK_LOG_LEVEL = Dotenv.Load().Get("SDK_LOG_LEVEL", "SILENT");
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
            var alice = new AccountCreateTransaction().SetKeyWithoutAlias(privateKey1).SetInitialBalance(new Hbar(10)).SetMaxAutomaticTokenAssociations(-1).Execute(client).GetReceipt(client).AccountId;
            var privateKey2 = PrivateKey.GenerateECDSA();
            var bob = new AccountCreateTransaction { KeyWithoutAlias = privateKey2, MaxAutomaticTokenAssociations = 1 }.Execute(client).GetReceipt(client).AccountId;
            var privateKey3 = PrivateKey.GenerateECDSA();
            var carol = new AccountCreateTransaction { KeyWithoutAlias = privateKey3, MaxAutomaticTokenAssociations = 0 }.Execute(client).GetReceipt(client).AccountId;
            var treasuryKey = PrivateKey.GenerateECDSA();
            var treasuryAccount = new AccountCreateTransaction().SetKeyWithoutAlias(treasuryKey).SetInitialBalance(new Hbar(10)).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 2:
            /// Create FT and NFT and mint
            /// </summary>
            var tokenID = new TokenCreateTransaction().SetTokenName("Fungible Token").SetTokenSymbol("TFT").SetTokenMemo("Example memo").SetDecimals(3).SetInitialSupply(100).SetMaxSupply(100).SetTreasuryAccountId(treasuryAccount).SetSupplyType(TokenSupplyType.FINITE).SetAdminKey(client.GetOperatorPublicKey()).SetFreezeKey(client.GetOperatorPublicKey()).SetSupplyKey(client.GetOperatorPublicKey()).SetMetadataKey(client.GetOperatorPublicKey()).SetPauseKey(client.GetOperatorPublicKey()).FreezeWith(client).Sign(treasuryKey).Execute(client).GetReceipt(client).TokenId;
            var nftID = new TokenCreateTransaction().SetTokenName("Test NFT").SetTokenSymbol("TNFT").SetTokenType(TokenType.NON_FUNGIBLE_UNIQUE).SetTreasuryAccountId(treasuryAccount).SetSupplyType(TokenSupplyType.FINITE).SetMaxSupply(10).SetSupplyType(TokenSupplyType.FINITE).SetAdminKey(client.GetOperatorPublicKey()).SetFreezeKey(client.GetOperatorPublicKey()).SetSupplyKey(client.GetOperatorPublicKey()).SetMetadataKey(client.GetOperatorPublicKey()).SetPauseKey(client.GetOperatorPublicKey()).FreezeWith(client).Sign(treasuryKey).Execute(client).GetReceipt(client).TokenId;
            new TokenMintTransaction().SetTokenId(nftID).SetMetadata(GenerateNftMetadata((byte)3)).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 3:
            /// Airdrop fungible tokens to all 3 accounts
            /// </summary>
            Console.WriteLine("Airdropping fts");
            var txnRecord = new TokenAirdropTransaction().AddTokenTransfer(tokenID, alice, 10).AddTokenTransfer(tokenID, treasuryAccount, -10).AddTokenTransfer(tokenID, bob, 10).AddTokenTransfer(tokenID, treasuryAccount, -10).AddTokenTransfer(tokenID, carol, 10).AddTokenTransfer(tokenID, treasuryAccount, -10).FreezeWith(client).Sign(treasuryKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 4:
            /// Get the transaction record and see one pending airdrop (for carol)
            /// </summary>
            Console.WriteLine("Pending airdrops length: " + txnRecord.pendingAirdropRecords.Count);
            Console.WriteLine("Pending airdrops: " + txnRecord.pendingAirdropRecords[0]);
            /// <summary>
            /// Step 5:
            /// Query to verify alice and bob received the airdrops and carol did not
            /// </summary>
            var aliceBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            var bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            var carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Alice ft balance after airdrop: " + aliceBalance.tokens[tokenID]);
            Console.WriteLine("Bob ft balance after airdrop: " + bobBalance.tokens[tokenID]);
            Console.WriteLine("Carol ft balance after airdrop: " + carolBalance.tokens[tokenID]);
            /// <summary>
            /// Step 6:
            /// Claim the airdrop for carol
            /// </summary>
            Console.WriteLine("Claiming ft with carol");
            new TokenClaimAirdropTransaction().AddPendingAirdrop(txnRecord.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(client).Sign(privateKey3).Execute(client).GetReceipt(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol ft balance after claim: " + carolBalance.tokens[tokenID]);
            /// <summary>
            /// Step 7:
            /// Airdrop the NFTs to all three accounts
            /// </summary>
            Console.WriteLine("Airdropping nfts");
            txnRecord = new TokenAirdropTransaction().AddNftTransfer(nftID.Nft(1), treasuryAccount, alice).AddNftTransfer(nftID.Nft(2), treasuryAccount, bob).AddNftTransfer(nftID.Nft(3), treasuryAccount, carol).FreezeWith(client).Sign(treasuryKey).Execute(client).GetRecord(client);
            /// <summary>
            /// Step 8:
            /// Get the transaction record and verify two pending airdrops (for bob & carol)
            /// </summary>
            Console.WriteLine("Pending airdrops length: " + txnRecord.pendingAirdropRecords.Count);
            Console.WriteLine("Pending airdrops for Bob: " + txnRecord.pendingAirdropRecords[0]);
            Console.WriteLine("Pending airdrops for Carol: " + txnRecord.pendingAirdropRecords[1]);
            /// <summary>
            /// Step 9:
            /// Query to verify alice received the airdrop and bob and carol did not
            /// </summary>
            aliceBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Alice nft balance after airdrop: " + aliceBalance.tokens[nftID]);
            Console.WriteLine("Bob nft balance after airdrop: " + bobBalance.tokens[nftID]);
            Console.WriteLine("Carol nft balance after airdrop: " + carolBalance.tokens[nftID]);
            /// <summary>
            /// Step 10:
            /// Claim the airdrop for bob
            /// </summary>
            Console.WriteLine("Claiming nft with Bob");
            new TokenClaimAirdropTransaction().AddPendingAirdrop(txnRecord.pendingAirdropRecords[0].GetPendingAirdropId()).FreezeWith(client).Sign(privateKey2).Execute(client).GetReceipt(client);
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            Console.WriteLine("Bob nft balance after claim: " + bobBalance.tokens[nftID]);
            /// <summary>
            /// Step 11:
            /// Cancel the airdrop for carol
            /// </summary>
            Console.WriteLine("Canceling nft for Carol");
            new TokenCancelAirdropTransaction().AddPendingAirdrop(txnRecord.pendingAirdropRecords[1].GetPendingAirdropId()).FreezeWith(client).Sign(treasuryKey).Execute(client).GetReceipt(client);
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol nft balance after cancel: " + carolBalance.tokens[nftID]);
            /// <summary>
            /// Step 12:
            /// Reject the NFT for bob
            /// </summary>
            Console.WriteLine("Rejecting nft with Bob");
            new TokenRejectTransaction().SetOwnerId(bob).AddNftId(nftID.Nft(2)).FreezeWith(client).Sign(privateKey2).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 13:
            /// Query to verify bob no longer has the NFT
            /// </summary>
            bobBalance = new AccountBalanceQuery { AccountId = bob }.Execute(client);
            Console.WriteLine("Bob nft balance after reject: " + bobBalance.tokens[nftID]);
            /// <summary>
            /// Step 13:
            /// Query to verify the NFT was returned to the Treasury
            /// </summary>
            var treasuryBalance = new AccountBalanceQuery { AccountId = treasuryAccount }.Execute(client);
            Console.WriteLine("Treasury nft balance after reject: " + treasuryBalance.tokens[nftID]);
            /// <summary>
            /// Step 14:
            /// Reject the fungible tokens for Carol
            /// </summary>
            Console.WriteLine("Rejecting ft with Carol");
            new TokenRejectTransaction().SetOwnerId(carol).AddTokenId(tokenID).FreezeWith(client).Sign(privateKey3).Execute(client).GetReceipt(client);
            /// <summary>
            /// Step 14:
            /// Query to verify carol no longer has the fungible tokens
            /// </summary>
            carolBalance = new AccountBalanceQuery { AccountId = carol }.Execute(client);
            Console.WriteLine("Carol ft balance after reject: " + carolBalance.tokens[tokenID]);
            /// <summary>
            /// Step 15:
            /// Query to verify Treasury received the rejected fungible tokens
            /// </summary>
            treasuryBalance = new AccountBalanceQuery { AccountId = treasuryAccount }.Execute(client);
            Console.WriteLine("Treasury ft balance after reject: " + treasuryBalance.tokens[tokenID]);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Example Complete!");
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