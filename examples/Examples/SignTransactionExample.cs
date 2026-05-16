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
    /// How to sign a transaction with a multi-sig account.
    /// </summary>
    public class SignTransactionExample
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
            Console.WriteLine("Sign Transaction Example Start!");
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
            /// Generate ED25519 key pairs.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey privateKey1 = PrivateKey.GenerateED25519();
            PublicKey publicKey1 = privateKey1.GetPublicKey();
            PrivateKey privateKey2 = PrivateKey.GenerateED25519();
            PublicKey publicKey2 = privateKey2.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create a Key List from keys generated in previous step.
            /// </summary>
            Console.WriteLine("Creating a Key List...");
            KeyList keylist = [publicKey1, publicKey2];
            Console.WriteLine("Created a Key List: " + keylist);
            /// <summary>
            /// Step 3:
            /// Create a new account with a Key List created in a previous step.
            /// </summary>
            Console.WriteLine("Creating new account...");
            TransactionResponse createAccountTxResponse = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(2)
            }
            .SetKeyWithoutAlias(keylist)
            .Execute(client);
            TransactionReceipt createAccountTxReceipt = createAccountTxResponse.GetReceipt(client);
            var accountId = createAccountTxReceipt.AccountId;
            Console.WriteLine("Created new account with ID: " + accountId);
            /// <summary>
            /// Step 4:
            /// Create a transfer transaction and freeze it with a client.
            /// </summary>
            Console.WriteLine("Creating a transfer transaction...");
            TransferTransaction transferTx = new TransferTransaction()
                .SetNodeAccountIds([new AccountId(0, 0, 3)])
                .AddHbarTransfer(createAccountTxReceipt.AccountId, Hbar.From(1).Negated())
                .AddHbarTransfer(new AccountId(0, 0, 3), Hbar.From(1))
            .FreezeWith(client);
            /// <summary>
            /// Step 5:
            /// Sign the transfer transaction with all respective keys (from a Key List).
            /// </summary>
            Console.WriteLine("Signing the transfer transaction...");
            transferTx.SignWithOperator(client);
            privateKey1.SignTransaction(transferTx);
            privateKey2.SignTransaction(transferTx);
            /// <summary>
            /// Step 6:
            /// Execute the transfer transaction and output its status.
            /// </summary>
            Console.WriteLine("Executing the transfer transaction...");
            TransactionResponse transferTxResponse = transferTx.Execute(client);
            TransactionReceipt transferTxReceipt = transferTxResponse.GetReceipt(client);
            Console.WriteLine("The transfer transaction was complete with status: " + transferTxReceipt.Status);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = accountId,
                TransferAccountId = OPERATOR_ID,

            }.FreezeWith(client).Sign(privateKey1).Sign(privateKey2).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Sign Transaction Example Complete!");
        }
    }
}