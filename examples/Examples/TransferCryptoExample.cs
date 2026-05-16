// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to transfer Hbar between accounts.
    /// </summary>
    public class TransferCryptoExample
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
            Console.WriteLine("Transfer Crypto Example Start!");
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
            AccountId recipientId = AccountId.FromString("0.0.3");
            /// <summary>
            /// Step 1:
            /// Check Hbar balance of sender and recipient.
            /// </summary>
            Hbar senderBalanceBefore = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
            Hbar recipientBalanceBefore = new AccountBalanceQuery { AccountId = recipientId }.Execute(client).Hbars;
            Console.WriteLine("Sender (" + OPERATOR_ID + ") balance before transfer: " + senderBalanceBefore);
            Console.WriteLine("Recipient (" + recipientId + ") balance before transfer: " + recipientBalanceBefore);
            /// <summary>
            /// Step 2:
            /// Execute the transfer transaction to send Hbars from operator to the recipient.
            /// </summary>
            Console.WriteLine("Executing the transfer transaction...");
            Hbar transferAmount = Hbar.From(1);
            TransactionResponse transferTxResponse = new TransferTransaction { TransactionMemo = "Transfer example" }
            .AddHbarTransfer(OPERATOR_ID, transferAmount.Negated())
            .AddHbarTransfer(recipientId, transferAmount)
            .Execute(client);
            Console.WriteLine("Transaction info: " + transferTxResponse);
            TransactionRecord record = transferTxResponse.GetRecord(client);
            Console.WriteLine("Transferred " + transferAmount);
            Console.WriteLine("Transfer memo: " + record.TransactionMemo);
            /// <summary>
            /// Step 6:
            /// Check Hbar balance of the sender and recipient after transfer transaction was executed.
            /// </summary>
            Hbar senderBalanceAfter = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
            Hbar receiptBalanceAfter = new AccountBalanceQuery { AccountId = recipientId }.Execute(client).Hbars;
            Console.WriteLine("Sender (" + OPERATOR_ID + ") balance after transfer: " + senderBalanceAfter);
            Console.WriteLine("Recipient (" + recipientId + ") balance after transfer: " + receiptBalanceAfter);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Example complete!");
        }
    }
}