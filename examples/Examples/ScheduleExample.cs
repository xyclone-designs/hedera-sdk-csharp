// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to schedule a transaction.
    /// </summary>
    public class ScheduleExample
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
            Console.WriteLine("Schedule Transaction Example Start!");
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
            /// Generate ED25519 key pairs for accounts.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs for accounts...");
            PrivateKey privateKey1 = PrivateKey.GenerateED25519();
            PublicKey publicKey1 = privateKey1.GetPublicKey();
            PrivateKey privateKey2 = PrivateKey.GenerateED25519();
            PublicKey publicKey2 = privateKey2.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Create new account.
            /// </summary>
            Console.WriteLine("Creating new account...");
            AccountId accountId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1)
            }
            .SetKeyWithoutAlias(KeyList.Of(null, publicKey1, publicKey2))
            .Execute(client)
            .GetReceipt(client).AccountId;
            Console.WriteLine("Created new account with ID: " + accountId);
            /// <summary>
            /// Step 2:
            /// Schedule a transfer transaction.
            /// </summary>
            Console.WriteLine("Scheduling token transfer...");
            TransactionResponse transferTxResponse = new TransferTransaction()
                .AddHbarTransfer(accountId, Hbar.From(1).Negated())
                .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
                .Schedule(_ =>
                {
                    _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(24 / 60 / 60);
                    _.WaitForExpiry = true;

                }).Execute(client);
            Console.WriteLine("Scheduled transaction ID: " + transferTxResponse.TransactionId);
            ScheduleId scheduleId = transferTxResponse.GetReceipt(client).ScheduleId;
            Console.WriteLine("Schedule ID for the transaction above: " + scheduleId);
            TransactionRecord record = transferTxResponse.GetRecord(client);
            Console.WriteLine("Scheduled transaction record: " + record);
            /// <summary>
            /// Step 3:
            /// Sign the schedule transaction with the first key.
            /// </summary>
            Console.WriteLine("Appending private key #1 signature to a schedule transaction...");
            var scheduleSignTxReceiptFirstSignature = new ScheduleSignTransaction
            {
                ScheduleId = scheduleId 
            }
            .FreezeWith(client)
            .Sign(privateKey1)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("A transaction that appends signature to a schedule transaction (private key #1) " + "was complete with status: " + scheduleSignTxReceiptFirstSignature.Status);
            /// <summary>
            /// Step 4:
            /// Query the state of a schedule transaction.
            /// </summary>
            ScheduleInfo scheduleInfo = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Schedule info: " + scheduleInfo);
            /// <summary>
            /// Step 5:
            /// Sign the schedule transaction with the second key.
            /// </summary>
            Console.WriteLine("Appending private key #2 signature to a schedule transaction...");
            var scheduleSignTxReceiptSecondSignature = new ScheduleSignTransaction
            {
                ScheduleId = scheduleId 
            }
                .FreezeWith(client)
                .Sign(privateKey2)
                .Execute(client)
                .GetReceipt(client);
            Console.WriteLine("A transaction that appends signature to a schedule transaction (private key #2) " + "was complete with status: " + scheduleSignTxReceiptSecondSignature.Status);
            TransactionId transactionId = transferTxResponse.TransactionId;
            string validMirrorTransactionId = transactionId.AccountId.ToString() + "-" + transactionId.ValidStart.ToUnixTimeSeconds() + "-" + transactionId.ValidStart.Nanosecond;
            string mirrorNodeUrl = "https://" + HEDERA_NETWORK + ".mirrornode.hedera.com/api/v1/transactions/" + validMirrorTransactionId;
            Console.WriteLine("The following link should query the mirror node for the scheduled transaction: " + mirrorNodeUrl);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = accountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(privateKey1)
            .Sign(privateKey2)
            .Execute(client);
            client.Dispose();
            Console.WriteLine("Schedule Transaction Example Complete!");
        }
    }
}