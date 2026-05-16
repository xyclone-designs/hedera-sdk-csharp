// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class BatchTransactionExample
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
            Console.WriteLine("Batch Transaction Example Start!");
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
            Console.WriteLine("Showcasing manual batch transaction preparation");
            ExecuteBatchWithManualInnerTransactionFreeze(client);
            Console.WriteLine("Showcasing automatic batch transaction preparation using batchify");
            ExecuteBatchWithBatchify(client);
            client.Dispose();
            Console.WriteLine("Batch Transaction Example Complete!");
        }

        private static void ExecuteBatchWithManualInnerTransactionFreeze(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Create batch keys
            /// </summary>
            var batchKey1 = PrivateKey.GenerateECDSA();
            var batchKey2 = PrivateKey.GenerateECDSA();
            var batchKey3 = PrivateKey.GenerateECDSA();
            /// <summary>
            /// Step 2:
            /// Create 3 accounts and prepare transfers for batching
            /// </summary>
            Console.WriteLine("Creating three accounts and preparing batched transfers...");
            var aliceKey = PrivateKey.GenerateECDSA();
            var alice = new AccountCreateTransaction { InitialBalance = new Hbar(2) }
                .SetKeyWithoutAlias(aliceKey)
                .Execute(client)
                .GetReceipt(client).AccountId;
            var aliceBatchedTransfer = new TransferTransaction
            {
                BatchKey = batchKey1,
                TransactionId = TransactionId.Generate(alice)
            }
            .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
            .AddHbarTransfer(alice, Hbar.From(1).Negated())
            .FreezeWith(client)
            .Sign(aliceKey);
            Console.WriteLine("Created first account (Alice): " + alice);
            var bobKey = PrivateKey.GenerateECDSA();
            var bob = new AccountCreateTransaction { InitialBalance = new Hbar(2) }
                .SetKeyWithoutAlias(bobKey)
                .Execute(client)
                .GetReceipt(client).AccountId;
            var bobBatchedTransfer = new TransferTransaction
            {
                BatchKey = batchKey2,
                TransactionId = TransactionId.Generate(bob)
            }
            .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
            .AddHbarTransfer(bob, Hbar.From(1).Negated())
            .FreezeWith(client)
            .Sign(bobKey);
            Console.WriteLine("Created second account (Bob): " + bob);
            var carolKey = PrivateKey.GenerateECDSA();
            var carol = new AccountCreateTransaction { InitialBalance = new Hbar(2) }
                .SetKeyWithoutAlias(carolKey)
                .Execute(client)
                .GetReceipt(client).AccountId;
            var carolBatchedTransfer = new TransferTransaction 
            {
                BatchKey = batchKey3, 
                TransactionId = TransactionId.Generate(carol) 
            }
            .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
            .AddHbarTransfer(carol, Hbar.From(1).Negated())
            .FreezeWith(client)
            .Sign(carolKey);
            Console.WriteLine("Created third account (Carol): " + carol);
            /// <summary>
            /// Step 3:
            /// Get the balances in order to compare after the batch execution
            /// </summary>
            var aliceBalanceBefore = new AccountBalanceQuery 
            { 
                AccountId = alice 

            }.Execute(client);
            var bobBalanceBefore = new AccountBalanceQuery 
            { 
                AccountId = bob 

            }.Execute(client);
            var carolBalanceBefore = new AccountBalanceQuery 
            { 
                AccountId = carol 

            }.Execute(client);
            var operatorBalanceBefore = new AccountBalanceQuery
            {
                AccountId = client.OperatorAccountId

            }.Execute(client);
            /// <summary>
            /// Step 4:
            /// Execute the batch
            /// </summary>
            Console.WriteLine("Executing batch transaction...");
            var receipt = new BatchTransaction
            {
                InnerTransactions = [aliceBatchedTransfer, bobBatchedTransfer, carolBatchedTransfer]
            }
                .FreezeWith(client)
                .Sign(batchKey1)
                .Sign(batchKey2)
                .Sign(batchKey3)
                .Execute(client)
                .GetReceipt(client);
            Console.WriteLine("Batch transaction executed with status: " + receipt.Status);
            /// <summary>
            /// Step 5:
            /// Verify the new balances
            /// </summary>
            Console.WriteLine("Verifying the balances after batch execution...");
            var aliceBalanceAfter = new AccountBalanceQuery 
            { 
                AccountId = alice 

            }.Execute(client);
            var bobBalanceAfter = new AccountBalanceQuery 
            { 
                AccountId = bob 

            }.Execute(client);
            var carolBalanceAfter = new AccountBalanceQuery 
            { 
                AccountId = carol 

            }.Execute(client);
            var operatorBalanceAfter = new AccountBalanceQuery
            {
                AccountId = client.OperatorAccountId

            }.Execute(client);
            Console.WriteLine("Alice's initial balance: " + aliceBalanceBefore.Hbars + ", after: " + aliceBalanceAfter.Hbars);
            Console.WriteLine("Bob's initial balance: " + bobBalanceBefore.Hbars + ", after: " + bobBalanceAfter.Hbars);
            Console.WriteLine("Carol's initial balance: " + carolBalanceBefore.Hbars + ", after: " + carolBalanceAfter.Hbars);
            Console.WriteLine("Operator's initial balance: " + operatorBalanceBefore.Hbars + ", after: " + operatorBalanceAfter.Hbars);
        }

        private static void ExecuteBatchWithBatchify(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Create batch key
            /// </summary>
            var batchKey = PrivateKey.GenerateECDSA();
            /// <summary>
            /// Step 2:
            /// Create acccount - alice
            /// </summary>
            Console.WriteLine("Creating three accounts and preparing batched transfers...");
            var aliceKey = PrivateKey.GenerateECDSA();
            var alice = new AccountCreateTransaction { InitialBalance = new Hbar(2) }
                .SetKeyWithoutAlias(aliceKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            Console.WriteLine("Created Alice: " + alice);
            /// <summary>
            /// Step 3:
            /// Create client for alice
            /// </summary>
            var aliceClient = ClientHelper.ForName(HEDERA_NETWORK);
            aliceClient.OperatorSet(alice, aliceKey);
            /// <summary>
            /// Step 4:
            /// Batchify a transfer transaction
            /// </summary>
            var aliceBatchedTransfer = new TransferTransaction()
                .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
                .AddHbarTransfer(alice, Hbar.From(1).Negated()).Batchify(aliceClient, batchKey);
            /// <summary>
            /// Step 5:
            /// Get the balances in order to compare after the batch execution
            /// </summary>
            var aliceBalanceBefore = new AccountBalanceQuery 
            { 
                AccountId = alice 

            }.Execute(client);
            var operatorBalanceBefore = new AccountBalanceQuery
            {
                AccountId = client.OperatorAccountId

            }.Execute(client);
            /// <summary>
            /// Step 6:
            /// Execute the batch
            /// </summary>
            Console.WriteLine("Executing batch transaction...");
            var receipt = new BatchTransaction
            {
                InnerTransactions = [aliceBatchedTransfer],
            }
            .FreezeWith(client)
            .Sign(batchKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Batch transaction executed with status: " + receipt.Status);
            /// <summary>
            /// Step 7:
            /// Verify the new balances
            /// </summary>
            Console.WriteLine("Verifying the balances after batch execution...");
            var aliceBalanceAfter = new AccountBalanceQuery 
            { 
                AccountId = alice 

            }.Execute(client);
            var operatorBalanceAfter = new AccountBalanceQuery
            {
                AccountId = client.OperatorAccountId

            }.Execute(client);
            Console.WriteLine("Alice's initial balance: " + aliceBalanceBefore.Hbars + ", after: " + aliceBalanceAfter.Hbars);
            Console.WriteLine("Operator's initial balance: " + operatorBalanceBefore.Hbars + ", after: " + operatorBalanceAfter.Hbars);
        }
    }
}