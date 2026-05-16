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
    public class ScheduledTransferExample
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
            Console.WriteLine("Scheduled Transfer Transaction Example Start!");
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
            Console.WriteLine("In this example Alice's account ID would be equal to the Operator's account ID: " + client.OperatorAccountId);
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pair.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair for Bob's account...");
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create Bob's account with receiver signature property enabled.
            /// </summary>
            Console.WriteLine("Create Bob's account...(with receiver signature property enabled).");
            AccountId bobAccountId = new AccountCreateTransaction
            {
                ReceiverSigRequired = true,
                InitialBalance = Hbar.From(1)

            }.SetKeyWithoutAlias(bobPublicKey).FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client).AccountId;
            Console.WriteLine("Created Bob's account with ID: " + bobAccountId);
            /// <summary>
            /// Step 3:
            /// Check Bob's initial balance.
            /// </summary>
            AccountBalance bobsInitialBalance = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client);
            Console.WriteLine("Bob's initial account balance: " + bobsInitialBalance);
            /// <summary>
            /// Step 4:
            /// Create a transfer transaction which we will schedule.
            /// </summary>
            TransferTransaction transferTx = new TransferTransaction().AddHbarTransfer(client.OperatorAccountId, Hbar.From(1).Negated()).AddHbarTransfer(bobAccountId, Hbar.From(1));
            Console.WriteLine("Scheduling token transfer: " + transferTx);
            /// <summary>
            /// Step 5:
            /// Create a scheduled transaction from a transfer transaction.
            ///
            /// The payerAccountId is the account that will be charged the fee
            /// for executing the scheduled transaction if/when it is executed.
            /// That fee is separate from the fee that we will pay to execute the
            /// ScheduleCreateTransaction itself.
            ///
            /// To clarify: Alice pays a fee to execute the ScheduleCreateTransaction,
            /// which creates the scheduled transaction on the Hedera network.
            /// She specifies when creating the scheduled transaction that Bob will pay
            /// the fee for the scheduled transaction when it is executed.
            ///
            /// If payerAccountId is not specified, the account who creates the scheduled transaction
            /// will be charged for executing the scheduled transaction.
            /// </summary>
            ScheduleId scheduleId = new ScheduleCreateTransaction 
            { 
                // TODO ScheduledTransaction = transferTx, 
                PayerAccountId = bobAccountId 
            }.Execute(client).GetReceipt(client).ScheduleId;
            Console.WriteLine("Schedule ID for the transaction above: " + scheduleId);
            /// <summary>
            /// Step 6:
            /// Check Bob's balance -- it should be unchanged, because the transfer has been scheduled,
            /// but it hasn't been executed yet as it requires Bob's signature.
            /// </summary>
            AccountBalance bobsBalanceAfterSchedule = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client);
            Console.WriteLine("Bob's balance after scheduling the transfer (should be unchanged): " + bobsBalanceAfterSchedule);
            /// <summary>
            /// Step 7:
            /// Query the state of a schedule transaction.
            ///
            /// Once Alice has communicated the scheduleId to Bob, Bob can query for information about the
            /// scheduled transaction.
            /// </summary>
            ScheduleInfo scheduledTransactionInfo = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Scheduled transaction info: " + scheduledTransactionInfo);

            // getScheduledTransaction() will return an SDK Transaction object identical to the transaction
            // that was scheduled, which Bob can then inspect like a normal transaction.
            // We happen to know that this transaction is (or certainly ought to be) a TransferTransaction.
            TransferTransaction scheduledTransaction = 
                scheduledTransactionInfo.GetScheduledTransaction() as TransferTransaction ??
                throw new Exception("The scheduled transaction was not a transfer transaction! (Fail)");
            Console.WriteLine("The scheduled transfer transaction from Bob's POV: " + scheduledTransaction);

            /// <summary>
            /// Step 8:
            /// Appends Bob's signature to a schedule transaction, i.e. Bob signs the scheduled transaction.
            /// </summary>
            Console.WriteLine("Appending Bob's signature to a schedule transaction...");
            var scheduleSignTxReceipt = new ScheduleSignTransaction
            {
                ScheduleId = scheduleId

            }.FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            Console.WriteLine("A transaction that appends Bob's signature to a schedule transfer transaction " + "was complete with status: " + scheduleSignTxReceipt.Status);
            /// <summary>
            /// Step 9:
            /// Check Bob's account balance after signing the scheduled transaction.
            /// </summary>
            AccountBalance balanceAfterSigning = new AccountBalanceQuery { AccountId = bobAccountId }.Execute(client);
            Console.WriteLine("Bob's balance after signing the scheduled transaction: " + balanceAfterSigning);
            /// <summary>
            /// Step 10:
            /// Query the state of a schedule transaction.
            /// </summary>
            ScheduleInfo postTransactionInfo = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Scheduled transaction info (`executedAt` should no longer be `null`): " + postTransactionInfo);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                TransferAccountId = client.OperatorAccountId,
                AccountId = bobAccountId

            }.FreezeWith(client).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Scheduled Transfer Transaction Example Complete!");
        }
    }
}