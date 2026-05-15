// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Schedule;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to long term schedule transactions (HIP-423).
    /// </summary>
    public class LongTermScheduledTransactionExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        /// <summary>
        /// Operator's account ID. Used to sign and pay for operations on Hedera.
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
            Console.WriteLine("Long Term Scheduled Transaction Example Start!");
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
            /// Create key pairs
            /// </summary>
            var privateKey1 = PrivateKey.GenerateED25519();
            var publicKey1 = privateKey1.GetPublicKey();
            var privateKey2 = PrivateKey.GenerateED25519();
            Console.WriteLine("Creating a Key List..." + "(with threshold, it will require 2 of 2 keys we generated to sign on anything modifying this account).");
            KeyList thresholdKey = KeyList.Of(2, privateKey1, privateKey2);
            Console.WriteLine("Created a Key List: " + thresholdKey);
            /// <summary>
            /// Step 2:
            /// Create the account
            /// </summary>
            Console.WriteLine("Creating new account...(with the above Key List as an account key).");
            var alice = new AccountCreateTransaction { InitialBalance = new Hbar(2) }
                .SetKeyWithoutAlias(thresholdKey)
                .Execute(client)
                .GetReceipt(client).AccountId;
            Console.WriteLine("Created new account with ID: " + alice);
            /// <summary>
            /// Step 3:
            /// Schedule a transfer transaction of 1 Hbar from the created account to the
            /// operator account with an expirationTime of
            /// 24 hours in the future and waitForExpiry=false
            /// </summary>
            Console.WriteLine("Creating new scheduled transaction with 1 day expiry");
            TransferTransaction transfer = new TransferTransaction().AddHbarTransfer(alice, new Hbar(1).Negated()).AddHbarTransfer(client.OperatorAccountId, new Hbar(1));
            int oneDayInSecs = 86400;
            var scheduleId = transfer.Schedule(_ =>
            {
                _.WaitForExpiry = false;
                _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(oneDayInSecs);
            })
            .Execute(client)
            .GetReceipt(client).ScheduleId;
            /// <summary>
            /// Step 4:
            /// Sign the transaction with one key and verify the transaction is not executed
            /// </summary>
            Console.WriteLine("Signing the new scheduled transaction with 1 key");
            new ScheduleSignTransaction { ScheduleId = scheduleId }
            .FreezeWith(client)
            .Sign(privateKey1)
            .Execute(client)
            .GetReceipt(client);
            ScheduleInfo info = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Scheduled transaction is not yet executed. Executed at: " + info.ExecutedAt);
            /// <summary>
            /// Step 5:
            /// Sign the transaction with the other key and verify the transaction executes successfully
            /// </summary>
            var accountBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            Console.WriteLine("Alice's account balance before schedule transfer: " + accountBalance.Hbars);
            Console.WriteLine("Signing the new scheduled transaction with the 2nd key");
            new ScheduleSignTransaction { ScheduleId = scheduleId }
            .FreezeWith(client)
            .Sign(privateKey2)
            .Execute(client)
            .GetReceipt(client);
            accountBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            Console.WriteLine("Alice's account balance after schedule transfer: " + accountBalance.Hbars);
            info = new ScheduleInfoQuery { ScheduleId = scheduleId }.Execute(client);
            Console.WriteLine("Scheduled transaction is executed. Executed at: " + info.ExecutedAt);
            /// <summary>
            /// Step 6:
            /// Schedule another transfer transaction of 1 Hbar from the account to the operator account
            /// with an expirationTime of 10 seconds in the future and waitForExpiry=true .
            /// </summary>
            Console.WriteLine("Creating new scheduled transaction with 10 seconds expiry");
            transfer = new TransferTransaction().AddHbarTransfer(alice, new Hbar(1).Negated()).AddHbarTransfer(client.OperatorAccountId, new Hbar(1));
            var scheduleId2 = transfer.Schedule(_ =>
            {
                _.WaitForExpiry = true;
                _.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(10);
            })
            .Execute(client)
            .GetReceipt(client).ScheduleId;
            long startTime = DateTime.Now.Millisecond;
            long elapsedTime = 0;
            /// <summary>
            /// Step 7:
            /// Sign the transaction with one key and verify the transaction is not executed
            /// </summary>
            Console.WriteLine("Signing the new scheduled transaction with 1 key");
            new ScheduleSignTransaction
            {
                ScheduleId = scheduleId2
            }
            .FreezeWith(client)
            .Sign(privateKey1)
            .Execute(client)
            .GetReceipt(client);
            info = new ScheduleInfoQuery { ScheduleId = scheduleId2 }.Execute(client);
            Console.WriteLine("Scheduled transaction is not yet executed. Executed at: " + info.ExecutedAt);
            /// <summary>
            /// Step 8:
            /// Update the account’s key to be only the one key
            /// that has already signed the scheduled transfer.
            /// </summary>
            Console.WriteLine("Updating Alice's key to be the 1st key");
            new AccountUpdateTransaction
            {
                AccountId = alice,
                Key = publicKey1,
            }
            .FreezeWith(client)
            .Sign(privateKey1)
            .Sign(privateKey2)
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 9:
            /// Verify that the transfer successfully executes roughly at the time of its expiration.
            /// </summary>
            accountBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            Console.WriteLine("Alice's account balance before schedule transfer: " + accountBalance.Hbars);
            while (elapsedTime < 10 / 1000)
            {
                elapsedTime = DateTime.Now.Millisecond - startTime;
                Console.WriteLine("Elapsed time: {0:000}.1f seconds\r", elapsedTime / 1000);
                Thread.Sleep(100); // Pause briefly to reduce CPU usage
            }

            accountBalance = new AccountBalanceQuery { AccountId = alice }.Execute(client);
            Console.WriteLine("Alice's account balance after schedule transfer: " + accountBalance.Hbars);
            Console.WriteLine("Long Term Scheduled Transaction Example Complete!");
        }
    }
}