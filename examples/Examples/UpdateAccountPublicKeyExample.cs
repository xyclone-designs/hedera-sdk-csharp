// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to update account's key.
    /// </summary>
    public class UpdateAccountPublicKeyExample
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
            Console.WriteLine("Update Account Public Key Example Start!");
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
                _client.DefaultMaxTransactionFee = Hbar.From(10);
            });
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pairs.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey privateKey1 = PrivateKey.GenerateED25519();
            PublicKey publicKey1 = privateKey1.GetPublicKey();
            PrivateKey privateKey2 = PrivateKey.GenerateED25519();
            PublicKey publicKey2 = privateKey2.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create a new account.
            /// </summary>
            Console.WriteLine("Creating new account...");
            TransactionResponse accountCreateTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1)
            }
            .SetKeyWithoutAlias(publicKey1)
            .Execute(client);
            AccountId accountId = accountCreateTxResponse.GetReceipt(client).AccountId;
            Console.WriteLine("Created new account with ID: " + accountId + " and public key: " + publicKey1);
            /// <summary>
            /// Step 2:
            /// Update account's key.
            /// </summary>
            Console.WriteLine("Updating public key of new account...(Setting key: " + publicKey2 + ").");
            TransactionResponse accountUpdateTxResponse = new AccountUpdateTransaction
            {
                AccountId = accountId,
                Key = publicKey2,

            }.FreezeWith(client).Sign(privateKey1).Sign(privateKey2).Execute(client);

            // (Important!) Wait for the transaction to complete by querying the receipt.
            accountUpdateTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 3:
            /// Get account info to confirm the key was changed.
            /// </summary>
            AccountInfo accountInfo = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("New account public key: " + accountInfo.Key);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = accountId,
                TransferAccountId = OPERATOR_ID,

            }.FreezeWith(client).Sign(privateKey2).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Update Account Public Key Example Complete!");
        }
    }
}