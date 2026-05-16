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
    /// How to serialize incomplete transaction, deserialize it, complete and execute (HIP-745).
    /// </summary>
    public class TransactionSerializationExample
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
            Console.WriteLine("Transaction Serialization (HIP-745) Example Start!");
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
            /// Check Hbar balance of sender and recipient.
            /// </summary>
            AccountId recipientId = AccountId.FromString("0.0.3");
            Hbar senderBalanceBefore = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
            Hbar recipientBalanceBefore = new AccountBalanceQuery { AccountId = recipientId }.Execute(client).Hbars;
            Console.WriteLine("Sender (" + OPERATOR_ID + ") balance before transfer: " + senderBalanceBefore);
            Console.WriteLine("Recipient (" + recipientId + ") balance before transfer: " + recipientBalanceBefore);
            /// <summary>
            /// Step 2:
            /// Create the transfer transaction with adding only Hbar transfer which credits the operator.
            /// </summary>
            Console.WriteLine("Creating the transfer transaction...");
            Hbar transferAmount = Hbar.From(1);
            var transferTx = new TransferTransaction().AddHbarTransfer(OPERATOR_ID, transferAmount.Negated());
            /// <summary>
            /// Step 3:
            /// Serialize the transfer transaction.
            /// </summary>
            Console.WriteLine("Serializing the transfer transaction...");
            var transactionBytes = transferTx.ToBytes();
            /// <summary>
            /// Step 4:
            /// Deserialize the transfer transaction.
            /// </summary>
            Console.WriteLine("Deserializing the transfer transaction...");
            TransferTransaction transferTxDeserialized = Transaction.FromBytes<TransferTransaction>(transactionBytes);
            /// <summary>
            /// Step 5:
            /// Complete the transfer transaction-- add Hbar transfer which debits Hbar to the recipient.
            /// And execute the transfer transaction.
            /// </summary>
            Console.WriteLine("Completing and executing the transfer transaction...");
            transferTxDeserialized.TransactionMemo = "HIP-745 example";
            var transferTxResponse = transferTxDeserialized
                .AddHbarTransfer(recipientId, transferAmount)
                .Execute(client);
            Console.WriteLine("Transaction info: " + transferTxResponse);
            TransactionRecord transferTxRecord = transferTxResponse.GetRecord(client);
            Console.WriteLine("Transferred " + transferAmount);
            Console.WriteLine("Transfer memo: " + transferTxRecord.TransactionMemo);
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