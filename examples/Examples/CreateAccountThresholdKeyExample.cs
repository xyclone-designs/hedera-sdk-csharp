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
    /// How to create a Hedera account with threshold key.
    /// </summary>
    public class CreateAccountThresholdKeyExample
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
            Console.WriteLine("Create Account With Threshold Key Example Start!");
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
            /// Generate three new Ed25519 private, public key pairs.
            ///
            /// You do not need the private keys to create the Threshold Key List,
            /// you only need the public keys, and if you're doing things correctly,
            /// you probably shouldn't have these private keys.
            /// </summary>
            PrivateKey[] privateKeys = new PrivateKey[3];
            PublicKey[] publicKeys = new PublicKey[3];
            for (int i = 0; i < 3; i++)
            {
                PrivateKey key = PrivateKey.GenerateED25519();
                privateKeys[i] = key;
                publicKeys[i] = key.GetPublicKey();
            }

            Console.WriteLine("Generating public keys...");
            foreach (Key publicKey in publicKeys)
            {
                Console.WriteLine("Generated public key: " + publicKey);
            }

            /// <summary>
            /// Step 2:
            /// Create a Key List.
            ///
            /// Require 2 of the 3 keys we generated to sign on anything modifying this account.
            /// </summary>
            KeyList thresholdKey = KeyList.Of(2, publicKeys);
            /// <summary>
            /// Step 2:
            /// Create a new account setting a Key List from a previous step as an account's key.
            /// </summary>
            Console.WriteLine("Creating new account...");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1),
            }
                .SetKeyWithoutAlias(thresholdKey)
                .Execute(client);
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            AccountId newAccountId = accountCreateTxReceipt.AccountId;
            Console.WriteLine("Created account with ID: " + newAccountId);
            /// <summary>
            /// Step 2:
            /// Create a transfer transaction from a newly created account to demonstrate the signing process (threshold).
            /// </summary>
            Console.WriteLine("Transferring 1 Hbar from a newly created account...");
            TransactionResponse transferTxResponse = new TransferTransaction().AddHbarTransfer(newAccountId, Hbar.From(1).Negated()).AddHbarTransfer(new AccountId(0, 0, 3), Hbar.From(1)).FreezeWith(client).Sign(privateKeys[0]).Sign(privateKeys[1]).Execute(client);

            // (Important!) Wait for the transfer to reach the consensus.
            transferTxResponse.GetReceipt(client);
            Hbar accountBalanceAfterTransfer = new AccountBalanceQuery { AccountId = newAccountId }.Execute(client).Hbars;
            Console.WriteLine("New account's Hbar balance after transfer: " + accountBalanceAfterTransfer);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                TransferAccountId = OPERATOR_ID,
                AccountId = newAccountId,

            }.FreezeWith(client).Sign(privateKeys[0]).Sign(privateKeys[1]).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Create Account With Threshold Key Example Complete!");
        }
    }
}