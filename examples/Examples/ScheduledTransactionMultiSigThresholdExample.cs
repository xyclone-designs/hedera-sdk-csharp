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
    /// How to schedule a transaction with a multi-sig account with a threshold.
    /// </summary>
    public class ScheduledTransactionMultiSigThresholdExample
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
            Console.WriteLine("Scheduled Transaction Multi-Sig With Threshold Example Start!");
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
            /// Generate four ED25519 key pairs.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey[] privateKeys = new PrivateKey[4];
            PublicKey[] publicKeys = new PublicKey[4];
            for (int i = 0; i < 4; i++)
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                privateKeys[i] = key;
                publicKeys[i] = key.GetPublicKey();
                Console.WriteLine("Key pair #" + (i + 1) + " | Private key: " + privateKeys[i]);
                Console.WriteLine("Key pair #" + (i + 1) + " | Public key: " + publicKeys[i]);
            }

            /// <summary>
            /// Step 2:
            /// Create a Key List with threshold
            /// (require 3 of 4 keys we generated to sign on anything modifying this account).
            /// </summary>
            Console.WriteLine("Creating a Key List..." + "(with threshold, it will require 3 of 4 keys we generated to sign on anything modifying this account).");
            KeyList thresholdKey = KeyList.Of(3, publicKeys);
            Console.WriteLine("Created a Key List: " + thresholdKey);
            /// <summary>
            /// Step 3:
            /// Create a new account with a Key List from previous step.
            /// </summary>
            Console.WriteLine("Creating new account...(with the above Key List as an account key).");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1),
                AccountMemo = "3-of-4 multi-sig account"
            }
            .SetKeyWithoutAlias(thresholdKey)
            .Execute(client);

            // This will wait for the receipt to become available.
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            AccountId multiSigAccountId = accountCreateTxReceipt.AccountId;
            Console.WriteLine("Created new account with ID: " + multiSigAccountId);
            /// <summary>
            /// Step 4:
            /// Check the balance of the newly created account.
            /// </summary>
            AccountBalance accountBalance = new AccountBalanceQuery { AccountId = multiSigAccountId }.Execute(client);
            Console.WriteLine("Balance of a newly created account with ID " + multiSigAccountId + ": " + accountBalance.Hbars.ToTinybars() + " tinybar.");
            /// <summary>
            /// Step 5:
            /// Schedule crypto transfer from multi-sig account to operator account.
            /// </summary>
            Console.WriteLine("Scheduling crypto transfer from multi-sig account to operator account...");
            TransactionResponse transferTxScheduled = new TransferTransaction()
                .AddHbarTransfer(multiSigAccountId, Hbar.From(1).Negated())
                .AddHbarTransfer(client.OperatorAccountId, Hbar.From(1))
            .Schedule()
            .FreezeWith(client)
            .Sign(privateKeys[0])
            .Execute(client);
            TransactionReceipt transferTxScheduledReceipt = transferTxScheduled.GetReceipt(client);
            Console.WriteLine("Schedule status: " + transferTxScheduledReceipt.Status);
            ScheduleId scheduleId = transferTxScheduledReceipt.ScheduleId;
            Console.WriteLine("Schedule ID: " + scheduleId);
            TransactionId scheduledTxId = transferTxScheduledReceipt.ScheduledTransactionId;
            Console.WriteLine("Scheduled transaction ID: " + scheduledTxId);

            // Add second signature.
            TransactionResponse scheduleSignTxResponseSecondSignature = new ScheduleSignTransaction
            { 
                ScheduleId = scheduleId
            
            }.FreezeWith(client).Sign(privateKeys[1]).Execute(client);
            TransactionReceipt scheduleSignTxReceiptSecondSignature = scheduleSignTxResponseSecondSignature.GetReceipt(client);
            Console.WriteLine("A transaction that appends signature to a schedule transaction (private key #2) " + "was complete with status: " + scheduleSignTxReceiptSecondSignature.Status);

            // Add third signature.
            TransactionResponse scheduleSignTxResponseThirdSignature = new ScheduleSignTransaction
            {
                ScheduleId = scheduleId
            
            }.FreezeWith(client).Sign(privateKeys[2]).Execute(client);
            TransactionReceipt scheduleSignTxReceiptThirdSignature = scheduleSignTxResponseThirdSignature.GetReceipt(client);
            Console.WriteLine("A transaction that appends signature to a schedule transaction (private key #3) " + "was complete with status: " + scheduleSignTxReceiptThirdSignature.Status);
            /// <summary>
            /// Step 6:
            /// Query schedule.
            /// </summary>
            ScheduleInfo scheduleInfo = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Schedule info: " + scheduleInfo);
            /// <summary>
            /// Step 7:
            /// Query triggered scheduled transaction.
            /// </summary>
            TransactionRecord recordScheduledTx = new TransactionRecordQuery { TransactionId = scheduledTxId }.Execute(client);
            Console.WriteLine("Triggered scheduled transaction info: " + recordScheduledTx);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = multiSigAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(privateKeys[0])
            .Sign(privateKeys[1])
            .Sign(privateKeys[2])
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Scheduled Transaction Multi-Sig With Threshold Example Complete!");
        }
    }
}