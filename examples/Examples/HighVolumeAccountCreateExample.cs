// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// Create a Hedera account using high-volume throttles.
    /// </summary>
    public class HighVolumeAccountCreateExample
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
            Console.WriteLine("High-Volume Account Create Example Start!");
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
            /// Generate ED25519 private and public key pair for the account.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateED25519();
            PublicKey publicKey = privateKey.GetPublicKey();
            Console.WriteLine("Future account private key: " + privateKey);
            Console.WriteLine("Future account public key: " + publicKey);
            /// <summary>
            /// Step 2:
            /// Create a new account using high-volume throttles and set a fee limit.
            /// </summary>
            Console.WriteLine("Creating new account with high-volume throttles...");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1),
                // TODO HighVolume = true,
                MaxTransactionFee = Hbar.From(5),
            }
            .SetKeyWithoutAlias(publicKey)
            .Execute(client);

            // This will wait for the receipt to become available.
            TransactionReceipt accountCreateTxReceipt = accountCreateTxResponse.GetReceipt(client);
            AccountId newAccountId = accountCreateTxReceipt.AccountId;

            Console.WriteLine("Created account with ID: " + newAccountId);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                TransferAccountId = OPERATOR_ID,
                AccountId = newAccountId,
            }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("High-Volume Account Create Example Complete!");
        }
    }
}