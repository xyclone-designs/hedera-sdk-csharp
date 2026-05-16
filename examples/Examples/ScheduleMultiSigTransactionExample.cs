// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to schedule a transaction with a multi-sig account.
    /// </summary>
    public class ScheduleMultiSigTransactionExample
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
            Console.WriteLine("Scheduled Transaction Multi-Sig Example Start!");
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
            /// Generate three ED25519 private keys.
            /// </summary>
            Console.WriteLine("Generating ED25519 private keys...");
            PrivateKey privateKey1 = PrivateKey.GenerateED25519();
            PublicKey publicKey1 = privateKey1.GetPublicKey();
            PrivateKey privateKey2 = PrivateKey.GenerateED25519();
            PublicKey publicKey2 = privateKey2.GetPublicKey();
            PrivateKey privateKey3 = PrivateKey.GenerateED25519();
            PublicKey publicKey3 = privateKey3.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create a Key List from keys generated in previous step.
            ///
            /// This key will be used as the new account's key.
            /// The reason we want to use a `KeyList` is to simulate a multi-party system where
            /// multiple keys are required to sign.
            /// </summary>
            Console.WriteLine("Creating a Key List...");
            KeyList keyList = KeyList.Of(null, publicKey1, publicKey2, publicKey3);
            Console.WriteLine("Created a Key List: " + keyList);
            /// <summary>
            /// Step 3:
            /// Create a new account with a Key List created in a previous step.
            /// </summary>
            Console.WriteLine("Creating new account...");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(2)
            }
            .SetNodeAccountIds([new AccountId(0, 0, 3)])
            .SetKeyWithoutAlias(keyList)
            .Execute(client);

            // This will wait for the receipt to become available.
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            AccountId accountId = accountCreateTxReceipt.AccountId;
            Console.WriteLine("Created new account with ID: " + accountId);
            /// <summary>
            /// Step 4:
            /// Create a new scheduled transaction for transferring Hbars.
            /// </summary>

            // Generate a TransactionId. This id is used to query the inner scheduled transaction
            // after we expect it to have been executed.
            TransactionId transactionId = TransactionId.Generate(OPERATOR_ID);
            Console.WriteLine("Generated `TransactionId` for a scheduled transaction: " + transactionId);

            // Create a transfer transaction with 2/3 signatures.
            Console.WriteLine("Creating a token transfer transaction...");
            TransferTransaction transferTx = new TransferTransaction().AddHbarTransfer(accountId, Hbar.From(1).Negated()).AddHbarTransfer(OPERATOR_ID, Hbar.From(1));

            // Schedule the transaction.
            Console.WriteLine("Scheduling the token transfer transaction...");
            ScheduleCreateTransaction scheduled = transferTx.Schedule(_ =>
            {
                _.PayerAccountId = OPERATOR_ID;
                _.AdminKey = operatorPublicKey;
            })
            .FreezeWith(client)
            .Sign(privateKey2);
            accountCreateTxReceipt = scheduled.Execute(client).GetReceipt(client);

            // Get the schedule ID from the receipt.
            ScheduleId scheduleId = accountCreateTxReceipt.ScheduleId;
            Console.WriteLine("Schedule ID: " + scheduleId);
            /// <summary>
            /// Step 5:
            /// Get the schedule info to see if signatories is populated with 2/3 signatures.
            /// </summary>
            ScheduleInfo scheduleInfo_BeforeLastSignature = new ScheduleInfoQuery
            {
                NodeAccountIds = { accountCreateTxResponse.NodeId },
                ScheduleId = scheduleId

            }.Execute(client);
            Console.WriteLine("Schedule info: " + scheduleInfo_BeforeLastSignature);
            transferTx = (TransferTransaction)scheduleInfo_BeforeLastSignature.GetScheduledTransaction();
            Dictionary<AccountId, Hbar> transfers = transferTx.GetHbarTransfers();

            // Make sure the transfer transaction is what we expect.
            if (transfers.Count != 2)
            {
                throw new Exception("More transfers than expected! (Fail)");
            }

            if (!transfers[accountId].Equals(Hbar.From(1).Negated()))
            {
                throw new Exception("Transfer for " + accountId + " is not what is expected " + transfers[accountId]);
            }

            if (!transfers[OPERATOR_ID].Equals(Hbar.From(1)))
            {
                throw new Exception("Transfer for " + OPERATOR_ID + " is not what is expected " + transfers[OPERATOR_ID]);
            }

            Console.WriteLine("Sending schedule sign transaction...");
            /// <summary>
            /// Step 6:
            /// Send this last signature to Hedera.
            ///
            /// This last signature should mean the transaction executes since all 3 signatures have been provided.
            /// </summary>
            Console.WriteLine("Appending private key #3 signature to a schedule transaction..." + "(This last signature should mean the transaction executes since all 3 signatures have been provided)");
            TransactionReceipt scheduleSignTxReceipt = new ScheduleSignTransaction
            {
                NodeAccountIds = { accountCreateTxResponse.NodeId },
                ScheduleId = scheduleId,
            }
            .FreezeWith(client)
            .Sign(privateKey3)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("A transaction that appends signature to a schedule transaction (private key #3) " + "was complete with status: " + scheduleSignTxReceipt.Status);
            /// <summary>
            /// Step 7:
            /// Query the schedule info again.
            /// </summary>
            ScheduleInfo scheduleInfo_AfterAllSigned = new ScheduleInfoQuery
            {
                NodeAccountIds = { accountCreateTxResponse.NodeId },
                ScheduleId = scheduleId,

            }.Execute(client);
            Console.WriteLine("Schedule info: " + scheduleInfo_AfterAllSigned);
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
            .Sign(privateKey3)
            .Execute(client);
            client.Dispose();
            Console.WriteLine("Scheduled Transaction Multi-Sig Example Complete!");
        }
    }
}