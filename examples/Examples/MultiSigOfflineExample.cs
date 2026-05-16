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
    /// How to sign a transaction with multi-sig account.
    /// </summary>
    public class MultiSigOfflineExample
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
            Console.WriteLine("Multi Sig Offline Example Start!");
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
            Console.WriteLine("Generating ED25519 private and public keys for accounts...");
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            Console.WriteLine("Alice's ED25519 Private Key: " + alicePrivateKey);
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            Console.WriteLine("Alice's ED25519 Public Key: " + alicePublicKey);
            PrivateKey bobPrivateKey = PrivateKey.GenerateED25519();
            Console.WriteLine("Bob's ED25519 Private Key: " + bobPrivateKey);
            PublicKey bobPublicKey = bobPrivateKey.GetPublicKey();
            Console.WriteLine("Bob's ED25519 Public Key: " + bobPublicKey);
            /// <summary>
            /// Step 2:
            /// Create a Multi-sig account.
            /// </summary>
            Console.WriteLine("Creating new Key List..");
            KeyList keylist = KeyList.Of(null, alicePublicKey, bobPublicKey);
            Console.WriteLine("Created Key List: " + keylist);
            Console.WriteLine("Creating a new account...");
            TransactionResponse createAccountTxResponse = new AccountCreateTransaction { InitialBalance = Hbar.From(2) }.SetKeyWithoutAlias(keylist).Execute(client);
            TransactionReceipt createAccountTxReceipt = createAccountTxResponse.GetReceipt(client);
            var newAccountId = createAccountTxReceipt.AccountId;

            Console.WriteLine("Created new account with ID: " + newAccountId);
            /// <summary>
            /// Step 2:
            /// Create a transfer from new account to the account with ID '0.0.3'.
            /// </summary>
            Console.WriteLine("Transferring 1 Hbar from new account to the account with ID `0.0.3`...");
            TransferTransaction transferTx = new TransferTransaction().SetNodeAccountIds([new AccountId(0, 0, 3)])
                .AddHbarTransfer(createAccountTxReceipt.AccountId, Hbar.From(1).Negated())
                .AddHbarTransfer(new AccountId(0, 0, 3), Hbar.From(1))
            .FreezeWith(client);
            /// <summary>
            /// Step 3:
            /// Convert transaction to bytes to send to signatories.
            /// </summary>
            Console.WriteLine("Converting transaction to bytes to send to signatories...");
            byte[] transactionBytes = transferTx.ToBytes();
            Transaction<> transactionToExecute = Transaction.FromBytes(transactionBytes);
            /// <summary>
            /// Step 4:
            /// Ask users to sign and return signature.
            /// </summary>
            byte[] alicesSignature = alicePrivateKey.SignTransaction(Transaction.FromBytes(transactionBytes));
            Console.WriteLine("Alice signed the transaction. Signature: " + string.Format("; ", alicesSignature));
            byte[] bobsSignature = bobPrivateKey.SignTransaction(Transaction.FromBytes(transactionBytes));
            Console.WriteLine("Bob signed the transaction. Signature: " + string.Format("; ", bobsSignature));
            /// <summary>
            /// Step 5:
            /// Recreate the transaction from bytes.
            /// </summary>
            Console.WriteLine("Adding users' signatures to the transaction...");
            transactionToExecute.SignWithOperator(client);
            transactionToExecute.AddSignature(alicePrivateKey.GetPublicKey(), alicesSignature);
            transactionToExecute.AddSignature(bobPrivateKey.GetPublicKey(), bobsSignature);
            /// <summary>
            /// Step 6:
            /// Execute recreated transaction.
            /// </summary>
            Console.WriteLine("Executing transfer transaction...");
            TransactionResponse transferTxResponse = transactionToExecute.Execute(client);
            createAccountTxReceipt = transferTxResponse.GetReceipt(client);
            Console.WriteLine("Transfer transaction was complete with status: " + createAccountTxReceipt.Status);
            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = newAccountId,
                TransferAccountId = OPERATOR_ID

            }.FreezeWith(client).Sign(alicePrivateKey).Sign(bobPrivateKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Multi Sig Offline Example Complete!");
        }
    }
}