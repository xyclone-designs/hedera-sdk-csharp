// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to schedule identical transactions.
    /// </summary>
    public class ScheduleIdenticalTransactionExample
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
            Console.WriteLine("Schedule Identical Transaction Example Start!");
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
            /// Create key pairs, clients and accounts.
            /// </summary>
            PrivateKey[] privateKeys = new PrivateKey[3];
            PublicKey[] publicKeys = new PublicKey[3];
            Client[] clients = new Client[3];
            AccountId[] accounts = new AccountId[3];
            ScheduleId scheduleId = null;
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Generating ED25519 key pair...");
                PrivateKey newPrivateKey = PrivateKey.GenerateED25519();
                PublicKey newPublicKey = newPrivateKey.GetPublicKey();
                privateKeys[i] = newPrivateKey;
                publicKeys[i] = newPublicKey;
                Console.WriteLine("Key pair #" + (i + 1) + " | Private key: " + privateKeys[i]);
                Console.WriteLine("Key pair #" + (i + 1) + " | Public key: " + publicKeys[i]);
                Console.WriteLine("Creating new account...");
                TransactionResponse _accountCreateTxResponse = new AccountCreateTransaction { InitialBalance = Hbar.From(1) }.SetKeyWithoutAlias(newPublicKey).Execute(client);

                // Make sure the transaction succeeded.
                TransactionReceipt _accountCreateTxReceipt = _accountCreateTxResponse.GetReceipt(client);
                Client newClient = ClientHelper.ForName(HEDERA_NETWORK);
                newClient.OperatorSet(_accountCreateTxReceipt.AccountId, newPrivateKey);
                clients[i] = newClient;
                accounts[i] = _accountCreateTxReceipt.AccountId;
                Console.WriteLine("Created new account with ID: " + accounts[i]);
                Console.WriteLine("---");
            }

            /// <summary>
            /// Step 2:
            /// Create a threshold key with a threshold of 2 and length of 3 requires
            /// (at least 2 of 3 keys to sign anything modifying the account).
            /// </summary>
            Console.WriteLine("Creating a Key List..." + "(with threshold, it will require 2 of 3 keys we generated to sign on anything modifying this account).");
            KeyList thresholdKey = KeyList.Of(2, publicKeys);
            Console.WriteLine("Created a Key List: " + thresholdKey);
            /// <summary>
            /// Step 3:
            /// Create a new account with the Key List from previous step.
            /// </summary>

            // We are using all of these keys, so the scheduled transaction doesn't automatically go through.
            // It works perfectly fine with just one key.
            Console.WriteLine("Creating new account...(with the above Key List as an account key).");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction { InitialBalance = Hbar.From(10) }.SetKeyWithoutAlias(thresholdKey).Execute(client);

            // Make sure the transaction succeeded.
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            AccountId thresholdAccount = accountCreateTxReceipt.AccountId;
            Console.WriteLine("Created new account with ID: " + thresholdAccount);
            Console.WriteLine("\n---\n");
            /// <summary>
            /// Step 4:
            /// Each loopClient creates an identical transaction, sending 1 Hbar to each of the created accounts,
            /// sent from the threshold Account.
            /// </summary>
            foreach (Client loopClient in clients)
            {
                AccountId operatorId = loopClient.OperatorAccountId;
                Console.WriteLine("Creating transfer transaction...");
                TransferTransaction transferTx = new TransferTransaction();
                foreach (AccountId account in accounts)
                {
                    transferTx.AddHbarTransfer(account, Hbar.From(1));
                }

                transferTx.AddHbarTransfer(thresholdAccount, Hbar.From(3).Negated());
                Console.WriteLine("Scheduling created transfer transaction...");
                ScheduleCreateTransaction scheduledTx = new ScheduleCreateTransaction
                {
                    // TODO ScheduledTransactionBody = transferTx,
                    PayerAccountId = thresholdAccount
                };
                TransactionResponse scheduledTxResponse = scheduledTx.Execute(loopClient);
                Console.WriteLine("Executing scheduled transaction...");
                TransactionReceipt loopReceipt = new TransactionReceiptQuery
                {
                    TransactionId = scheduledTxResponse.TransactionId,
                    NodeAccountIds = [scheduledTxResponse.NodeId]

                }.Execute(loopClient);
                Console.WriteLine("Operator (ID: " + operatorId + ") | Schedule ID: " + loopReceipt.ScheduleId);

                // Save the schedule ID, so that it can be asserted for each loopClient submission.
                if (scheduleId == null)
                {
                    scheduleId = loopReceipt.ScheduleId;
                }

                if (!scheduleId.Equals(loopReceipt.ScheduleId))
                {
                    throw new Exception("Invalid generated schedule ID! Expected " + scheduleId + ", got " + loopReceipt.ScheduleId);
                }


                // If the status return by the receipt is related to already created, execute a schedule sign transaction.
                if (loopReceipt.Status == ResponseStatus.IdenticalScheduleAlreadyCreated)
                {
                    Console.WriteLine("Appending signature to a schedule transaction...");
                    TransactionResponse scheduleSignTxResponse = new ScheduleSignTransaction 
                    { 
                        ScheduleId = scheduleId,
                        // TODO ScheduleId = loopReceipt.ScheduleId
                        NodeAccountIds = [accountCreateTxResponse.NodeId],
                    }.Execute(loopClient);
                    TransactionReceipt scheduleSignTxReceipt = new TransactionReceiptQuery { TransactionId = scheduleSignTxResponse.TransactionId, }.Execute(client);
                    Console.WriteLine("A transaction that appends signature to a schedule transaction " + "was complete with status: " + scheduleSignTxReceipt.Status);
                    if (scheduleSignTxReceipt.Status != ResponseStatus.Success && scheduleSignTxReceipt.Status != ResponseStatus.ScheduleAlreadyExecuted)
                    {
                        throw new Exception("Bad status while getting receipt of schedule sign with operator " + operatorId + ": " + scheduleSignTxReceipt.Status);
                    }
                }

                Console.WriteLine("---");
            }

            Console.WriteLine("\n---\n");
            /// <summary>
            /// Step 5:
            /// Query the state of a schedule transaction.
            /// </summary>
            ScheduleInfo scheduleInfo = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Scheduled transaction info: " + scheduleInfo);
            /// <summary>
            /// Clean up:
            /// Delete created accounts and close created clients.
            /// </summary>
            AccountDeleteTransaction accountDeleteTx = new AccountDeleteTransaction 
            {
                AccountId = thresholdAccount,
                TransferAccountId = OPERATOR_ID,
            
            }.FreezeWith(client);
            for (int i = 0; i < 3; i++)
            {
                accountDeleteTx.Sign(privateKeys[i]);
                new AccountDeleteTransaction
                {
                    AccountId = accounts[i],
                    TransferAccountId = OPERATOR_ID,
                }
                    .FreezeWith(client)
                    .Sign(privateKeys[i])
                    .Execute(client)
                    .GetReceipt(client);
            }

            accountDeleteTx.Execute(client).GetReceipt(client);
            client.Dispose();
            foreach (Client loopClient in clients)
            {
                loopClient.Dispose();
            }

            Console.WriteLine("Schedule Identical Transaction Example Complete!");
        }
    }
}