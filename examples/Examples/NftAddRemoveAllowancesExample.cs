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
    /// <summary>
    /// How to grant another account the right to transfer hbar, fungible and non-fungible tokens from your account (HIP-336).
    /// </summary>
    public class NftAddRemoveAllowancesExample
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
            Console.WriteLine("Nft Add Remove Allowances (HIP-336) Example Start!");
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
            PublicKey operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// The beginning of the first example (approve/delete allowances for single serial numbers).
            /// Create NFT using the Hedera Token Service.
            /// </summary>
            Console.WriteLine("The beginning of the first example (approve/delete allowances for single serial numbers).");
            string[] CIDs = new[]
            {
                "QmNPCiNA3Dsu3K5FxDPMG5Q3fZRwVTg14EXA92uqEeSRXn",
                "QmZ4dgAgt8owvnULxnKxNe8YqpavtVCXmc1Lt2XajFpJs9",
                "QmPzY5GxevjyfMUF5vEAjtyRoigzWp47MiKAtLBduLMC1T"
            };
            Console.WriteLine("Creating NFT using the Hedera Token Service...");
            TransactionReceipt nftCreateTxReceipt = new TokenCreateTransaction
            {
                TokenName = "HIP-336 NFT1",
                TokenSymbol = "HIP336NFT1",
                TokenType = TokenType.NonFungibleUnique,
                Decimals = 0,
                InitialSupply = 0,
                MaxSupply = CIDs.Length,
                TreasuryAccountId = OPERATOR_ID,
                TokenSupplyType = TokenSupplyType.Finite,
                AdminKey = operatorPublicKey,
                SupplyKey = operatorPublicKey,
                WipeKey = operatorPublicKey,
            }
            .FreezeWith(client)
            .Execute(client)
            .GetReceipt(client);
            TokenId nftTokenId = nftCreateTxReceipt.TokenId;

            Console.WriteLine("Created NFT with token ID: " + nftTokenId);
            /// <summary>
            /// Step 2:
            /// Mint NFTs.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            IList<TransactionReceipt> nftMintTxReceipts = [];
            for (int i = 0; i < CIDs.Length; i++)
            {
                nftMintTxReceipts.Add(new TokenMintTransaction
                {
                    TokenId = nftTokenId,
                    Metadata = [Encoding.UTF8.GetBytes(CIDs[i])]
                }
                .FreezeWith(client)
                .Execute(client)
                .GetReceipt(client)
                );
                Console.WriteLine("Minted NFT (token ID: " + nftTokenId + ") with serial: " + nftMintTxReceipts[i].Serials[0]);
            }

            /// <summary>
            /// Step 3:
            /// Create spender and receiver accounts.
            /// </summary>
            Console.WriteLine("Creating spender and receiver accounts...");
            PrivateKey spenderPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey spenderPublicKey = spenderPrivateKey.GetPublicKey();
            AccountId spenderAccountId = new AccountCreateTransaction { InitialBalance = Hbar.From(2) }
            .SetKeyWithoutAlias(spenderPublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            
            Console.WriteLine("Created spender account with ID: " + spenderAccountId);
            PrivateKey receiverPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey receiverPublicKey = receiverPrivateKey.GetPublicKey();
            AccountId receiverAccountId = new AccountCreateTransaction { InitialBalance = Hbar.From(2) }
            .SetKeyWithoutAlias(receiverPublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            
            Console.WriteLine("Created receiver account with ID: " + receiverAccountId);
            /// <summary>
            /// Step 4:
            /// Associate spender and receiver accounts with the NFT.
            /// </summary>
            Console.WriteLine("Associating spender and receiver accounts with the NFT...");
            TransactionReceipt spenderAssociateReceipt = new TokenAssociateTransaction
            {
                AccountId = spenderAccountId,
                TokenIds = [nftTokenId],
            }
            .FreezeWith(client)
            .Sign(spenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Spender association transaction was complete with status: " + spenderAssociateReceipt.Status);
            TransactionReceipt receiverAssociateReceipt = new TokenAssociateTransaction
            {
                AccountId = receiverAccountId,
                TokenIds = [nftTokenId],
            }
            .FreezeWith(client)
            .Sign(receiverPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Receiver association transaction was complete with status: " + receiverAssociateReceipt.Status);
            /// <summary>
            /// Step 5:
            /// Approve NFT (serial '1' and '2') allowance for spender account.
            /// </summary>
            NftId nft1 = new NftId(nftTokenId, 1);
            NftId nft2 = new NftId(nftTokenId, 2);
            Console.WriteLine("Approving spender account allowance for NFT (serials #1 and #2)...");
            TransactionReceipt approveReceipt = new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(nft1, OPERATOR_ID, spenderAccountId).ApproveTokenNftAllowance(nft2, OPERATOR_ID, spenderAccountId).Execute(client).GetReceipt(client);
            Console.WriteLine("Approve spender allowance transaction was complete with status: " + approveReceipt.Status);
            /// <summary>
            /// Step 6:
            /// Send NFT with serial #1 from operator's to receiver account.
            /// This transaction should be executed on behalf of the spender and should end up with SUCCESS.
            /// </summary>

            // Generate TransactionId from spender's account id in order,
            // for the transaction to be executed on behalf of the spender.
            TransactionId onBehalfOfTransactionId = TransactionId.Generate(spenderAccountId);
            Console.WriteLine("Transferring NFT (serial #1) on behalf of the spender...");
            TransactionReceipt approvedSendReceipt = new TransferTransaction
            { 
                TransactionId = onBehalfOfTransactionId,
            }
            .AddApprovedNftTransfer(nft1, OPERATOR_ID, receiverAccountId)
            .FreezeWith(client)
            .Sign(spenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Transfer transaction was complete with status: " + approvedSendReceipt.Status);
            /// <summary>
            /// Step 7:
            /// Remove all NFT token allowances (for serial #2).
            /// </summary>
            Console.WriteLine("Removing all NFT token allowances (for serial #2)...");
            TransactionReceipt deleteAllowanceReceipt = new AccountAllowanceDeleteTransaction().DeleteAllTokenNftAllowances(nft2, OPERATOR_ID).Execute(client).GetReceipt(client);
            Console.WriteLine("Remove allowance transaction was complete with status: " + deleteAllowanceReceipt.Status);
            /// <summary>
            /// Step 8:
            /// Send NFT with serial #2 from operator's to receiver account.
            /// Spender does not have an allowance to send serial #2, should end up with SPENDER_DOES_NOT_HAVE_ALLOWANCE.
            /// </summary>
            TransactionId onBehalfOfTransactionId2 = TransactionId.Generate(spenderAccountId);
            try
            {
                Console.WriteLine("Transferring NFT (serial #2) on behalf of the spender...");
                new TransferTransaction
                {
                    TransactionId = onBehalfOfTransactionId2
                }
                .AddApprovedNftTransfer(nft2, OPERATOR_ID, receiverAccountId)
                .FreezeWith(client)
                .Sign(spenderPrivateKey)
                .Execute(client)
                .GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine("Transferring NFT (serial #2) was failed (as expected): " + e.Message);
            }

            Console.WriteLine("---");
            /// <summary>
            /// Step 9:
            /// The beginning of the second example (approve/delete allowances for ALL serial numbers at once).
            /// Create a fungible HTS token using the Hedera Token Service.
            /// </summary>
            Console.WriteLine("The beginning of the second example (approve/delete allowances for ALL serial numbers at once).");
            string[] CIDs2 = new[]
            {
                "QmNPCiNA3Dsu3K5FxDPMG5Q3fZRwVTg14EXA92uqEeSRXn",
                "QmZ4dgAgt8owvnULxnKxNe8YqpavtVCXmc1Lt2XajFpJs9",
                "QmPzY5GxevjyfMUF5vEAjtyRoigzWp47MiKAtLBduLMC1T"
            };
            Console.WriteLine("Creating NFT using the Hedera Token Service...");
            TransactionReceipt nftCreateReceipt2 = new TokenCreateTransaction
            {
                TokenName = "HIP336NFT2",
                TokenSymbol = "HIP336NFT2",
                TokenType = TokenType.NonFungibleUnique,
                Decimals = 0,
                InitialSupply = 0,
                MaxSupply = CIDs2.Length,
                TreasuryAccountId = OPERATOR_ID,
                TokenSupplyType = TokenSupplyType.Finite,
                AdminKey = operatorPublicKey,
                SupplyKey = operatorPublicKey,
                WipeKey = operatorPublicKey,
            }
            .FreezeWith(client)
            .Execute(client)
            .GetReceipt(client);
            TokenId nftTokenId2 = nftCreateReceipt2.TokenId;

            Console.WriteLine("Created NFT with token ID: " + nftTokenId2);
            /// <summary>
            /// Step 10:
            /// Mint NFTs.
            /// </summary>
            Console.WriteLine("Minting NFTs...");
            IList<TransactionReceipt> nftCollection2 = [];
            for (int i = 0; i < CIDs2.Length; i++)
            {
                nftCollection2.Add(new TokenMintTransaction
                {
                    TokenId = nftTokenId2,
                    Metadata = [ Encoding.UTF8.GetBytes(CIDs2[i]) ]
                }
                .FreezeWith(client)
                .Execute(client)
                .GetReceipt(client));
                Console.WriteLine("Minted NFT (token ID: " + nftTokenId2 + ") with serial: " + nftCollection2[i].Serials[0]);
            }

            /// <summary>
            /// Step 11:
            /// Create spender and receiver accounts.
            /// </summary>
            Console.WriteLine("Creating spender and receiver accounts...");
            PrivateKey delegatingSpenderPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey delegatingSpenderPublicKey2 = delegatingSpenderPrivateKey.GetPublicKey();
            AccountId delegatingSpenderAccountId = new AccountCreateTransaction { InitialBalance = Hbar.From(2) }
            .SetKeyWithoutAlias(delegatingSpenderPublicKey2)
            .Execute(client)
            .GetReceipt(client).AccountId;

            Console.WriteLine("Created spender account with ID: " + delegatingSpenderAccountId);
            PrivateKey receiverPrivateKey2 = PrivateKey.GenerateECDSA();
            PublicKey receiverPublicKey2 = receiverPrivateKey2.GetPublicKey();
            AccountId receiverAccountId2 = new AccountCreateTransaction { InitialBalance = Hbar.From(2) }
            .SetKeyWithoutAlias(receiverPublicKey2)
            .Execute(client)
            .GetReceipt(client).AccountId;

            Console.WriteLine("Created receiver account with ID: " + receiverAccountId2);
            /// <summary>
            /// Step 12:
            /// Associate spender and receiver accounts with the NFT.
            /// </summary>
            Console.WriteLine("Associating spender and receiver accounts with the NFT...");
            TransactionReceipt spenderAssociateReceipt2 = new TokenAssociateTransaction
            {
                AccountId = delegatingSpenderAccountId,
                TokenIds = [nftTokenId2],
            }
            .FreezeWith(client)
            .Sign(delegatingSpenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Spender association transaction was complete with status: " + spenderAssociateReceipt2.Status);
            TransactionReceipt receiverAssociateReceipt2 = new TokenAssociateTransaction
            {
                AccountId = receiverAccountId2,
                TokenIds = [nftTokenId2],
            }
            .FreezeWith(client)
            .Sign(receiverPrivateKey2)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Receiver association transaction was complete with status: " + receiverAssociateReceipt2.Status);
            /// <summary>
            /// Step 13:
            /// Approve NFT (all serials) allowance for spender account.
            /// </summary>
            NftId example2Nft1 = new NftId(nftTokenId2, 1);
            NftId example2Nft2 = new NftId(nftTokenId2, 2);
            NftId example2Nft3 = new NftId(nftTokenId2, 3);
            Console.WriteLine("Approving spender account allowance for NFT (all serials)...");
            TransactionReceipt approveReceipt2 = new AccountAllowanceApproveTransaction().ApproveTokenNftAllowanceAllSerials(nftTokenId2, OPERATOR_ID, delegatingSpenderAccountId).Execute(client).GetReceipt(client);
            Console.WriteLine("Approve spender allowance transaction was complete with status: " + approveReceipt2.Status);
            /// <summary>
            /// Step 14:
            /// Create delegate spender account.
            /// </summary>
            Console.WriteLine("Creating delegate spender account...");
            PrivateKey spenderPrivateKey2 = PrivateKey.GenerateECDSA();
            PublicKey spenderPublicKey2 = spenderPrivateKey2.GetPublicKey();
            AccountId spenderAccountId2 = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(2)
            }
                .SetKeyWithoutAlias(spenderPublicKey2)
            .Execute(client)
            .GetReceipt(client).AccountId;

            Console.WriteLine("Created delegate spender account with ID: : " + spenderAccountId2);
            /// <summary>
            /// Step 15:
            /// Give delegatingSpender allowance for NFT with serial #3 on behalf of spender account which has approveForAll rights.
            /// </summary>
            Console.WriteLine("Approving delegate spender account allowance for NFT (serial #3) on behalf of spender account which has `approveForAll` rights...");
            TransactionReceipt approveDelegateAllowanceReceipt = new AccountAllowanceApproveTransaction().ApproveTokenNftAllowance(example2Nft3, OPERATOR_ID, spenderAccountId2, delegatingSpenderAccountId)
            .FreezeWith(client)
            .Sign(delegatingSpenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Approve delegated spender allowance for serial 3 - status: " + approveDelegateAllowanceReceipt.Status);
            /// <summary>
            /// Step 16:
            /// Send NFT with serial #3 from operator's to receiver account.
            /// This transaction should be executed on behalf of the spenderAccountId2,
            /// which has an allowance to send serial #3, and should end up with SUCCESS.
            /// </summary>

            // Generate TransactionId from spender's account id in order,
            // for the transaction to be executed on behalf of the spender.
            TransactionId delegatedOnBehalfOfTxId = TransactionId.Generate(spenderAccountId2);
            TransactionReceipt delegatedSendTx = new TransferTransaction
            {
                TransactionId = delegatedOnBehalfOfTxId
            }
                .AddApprovedNftTransfer(example2Nft3, OPERATOR_ID, receiverAccountId2)
            .FreezeWith(client)
            .Sign(spenderPrivateKey2)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Transfer serial 3 on behalf of the delegated spender status:" + delegatedSendTx.Status);
            /// <summary>
            /// Step 17:
            /// Send NFT with serial #1 from operator's to receiver account.
            /// This transaction should be executed on behalf of the delegatingSpender,
            /// which has an allowance to send serial #1, and should end up with SUCCESS.
            /// </summary>

            // Generate TransactionId from spender's account id in order,
            // for the transaction to be executed on behalf of the spender.
            TransactionId onBehalfOfTransactionId3 = TransactionId.Generate(delegatingSpenderAccountId);
            TransactionReceipt approvedSendReceipt3 = new TransferTransaction
            {
                TransactionId = onBehalfOfTransactionId3
            }
                .AddApprovedNftTransfer(example2Nft1, OPERATOR_ID, receiverAccountId2)
            .FreezeWith(client)
            .Sign(delegatingSpenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Transfer serial 1 on behalf of the spender status:" + approvedSendReceipt3.Status);
            /// <summary>
            /// Step 18:
            /// Remove delegatingSpender allowance for all of NFT serials.
            /// </summary>
            TransactionReceipt deleteAllowanceReceipt2 = new AccountAllowanceApproveTransaction().DeleteTokenNftAllowanceAllSerials(nftTokenId2, OPERATOR_ID, delegatingSpenderAccountId).Execute(client).GetReceipt(client);
            Console.WriteLine("Remove spender's allowance for serial 2 - status: " + deleteAllowanceReceipt2.Status);
            /// <summary>
            /// Step 19:
            /// Send NFT with serial #2 from operator's to receiver account.
            /// Spender does not have an allowance to send serial #2, should end up with SPENDER_DOES_NOT_HAVE_ALLOWANCE.
            /// </summary>

            // Generate TransactionId from spender's account id in order,
            // for the transaction to be executed on behalf of the spender.
            TransactionId onBehalfOfTransactionId4 = TransactionId.Generate(delegatingSpenderAccountId);
            try
            {
                new TransferTransaction
                {
                    TransactionId = onBehalfOfTransactionId4
                }
                .AddApprovedNftTransfer(example2Nft2, OPERATOR_ID, receiverAccountId2)
                .FreezeWith(client)
                .Sign(delegatingSpenderPrivateKey)
                .Execute(client)
                .GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /// <summary>
            /// Clean up:
            /// Delete created accounts and tokens.
            /// </summary>
            new TokenWipeTransaction
            {
                TokenId = nftTokenId,
                AccountId = receiverAccountId,
                Serials = { 1 }
            }
            .FreezeWith(client)
            .Sign(OPERATOR_KEY)
            .Execute(client)
            .GetReceipt(client);
            new TokenWipeTransaction
            { 
                TokenId = nftTokenId2,
                AccountId = receiverAccountId2,
                Serials = { 1, 2 }
            }
            .FreezeWith(client)
            .Sign(OPERATOR_KEY)
            .Execute(client)
            .GetReceipt(client);
            new AccountDeleteTransaction
            { 
                AccountId = spenderAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(spenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            new AccountDeleteTransaction
            { 
                AccountId = receiverAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(receiverPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            new AccountDeleteTransaction
            { 
                AccountId = delegatingSpenderAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(delegatingSpenderPrivateKey)
            .Execute(client)
            .GetReceipt(client);
            new AccountDeleteTransaction
            {
                AccountId = receiverAccountId2,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(receiverPrivateKey2)
            .Execute(client)
            .GetReceipt(client);
            new TokenDeleteTransaction { TokenId = nftTokenId }.Execute(client).GetReceipt(client);
            new TokenDeleteTransaction { TokenId = nftTokenId2 }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Nft Add Remove Allowances (HIP-336) Example Complete!");
        }
    }
}