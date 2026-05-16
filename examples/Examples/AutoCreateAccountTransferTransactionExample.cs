// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class AutoCreateAccountTransferTransactionExample
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
            Console.WriteLine("Auto Create Account Via Transfer Transaction (HIP-583) Example Start!");
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
            /// Generate ECSDA private key.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            /// <summary>
            /// Step 2:
            /// Extract ECDSA public key.
            /// </summary>
            PublicKey publicKey = privateKey.GetPublicKey();
            /// <summary>
            /// Step 3:
            /// Extract Ethereum public address.
            /// </summary>
            EvmAddress evmAddress = publicKey.ToEvmAddress();
            Console.WriteLine("EVM address of the new account: " + evmAddress);
            /// <summary>
            /// Step 4:
            /// Use the TransferTransaction.
            /// - populate the FromAddress with the sender Hedera account ID;
            /// - populate the ToAddress with Ethereum public address.
            ///
            /// Note: Can transfer from public address to public address in the TransferTransaction for complete accounts.
            /// Transfers from hollow accounts will not work because the hollow account does not have a public key
            /// assigned to authorize transfers out of the account.
            /// </summary>
            TransferTransaction transferTx = new TransferTransaction()
                .AddHbarTransfer(OPERATOR_ID, Hbar.From(1).Negated())
                .AddHbarTransfer(AccountId.FromEvmAddress(evmAddress, 0, 0), Hbar.From(1))
            .FreezeWith(client);
            /// <summary>
            /// Step 5:
            /// Sign and execute the TransferTransaction transaction using existing Hedera account
            /// and key paying for the transaction fee.
            /// </summary>
            Console.WriteLine("Transferring Hbar to the the new account...");
            TransactionResponse transferTxResponse = transferTx.Execute(client);
            /// <summary>
            /// Step 6:
            /// Get the new account ID ask for the child receipts or child records for the parent transaction ID of the TransferTransaction
            /// (the AccountCreateTransaction is executed as a child transaction triggered by the TransferTransaction).
            /// </summary>
            TransactionReceipt transferTxReceipt = new TransactionReceiptQuery
            {
                TransactionId = transferTxResponse.TransactionId,
                IncludeChildren = true,

            }.Execute(client);
            AccountId aliceAccountId = transferTxReceipt.Children[0].AccountId;
            
            Console.WriteLine("The \"normal\" account ID of the given alias: " + aliceAccountId);
            /// <summary>
            /// Step 7:
            /// Get the AccountInfo and verify the account is a hollow account with the supplied public address (may need to verify with mirror node API).
            ///
            /// The Hedera Account that was created has a public address the user specified in the TransferTransaction ToAddress:
            ///  - will not have a public key at this stage;
            ///  - cannot do anything besides receive Hbar or tokens;
            ///  - the alias property of the account does not have the public address;
            ///  - referred to as a hollow account.
            /// </summary>
            AccountInfo aliceAccountInfo_BeforeEnhancing = new AccountInfoQuery { AccountId = aliceAccountId }.Execute(client);
            if (((KeyList)aliceAccountInfo_BeforeEnhancing.Key).Count == 0)
            {
                Console.WriteLine("The newly created account is a hollow account! (Success)");
            }
            else
            {
                throw new Exception("The newly created account is not a hollow account! (Fail)");
            }

            /// <summary>
            /// Step 8:
            /// Create a HAPI transaction and assign the new hollow account as the transaction fee payer.
            ///
            /// Sign with the private key that corresponds to the public key on the hollow account
            /// (to enhance the hollow account to have a public key the hollow account needs to be specified as a transaction fee payer in a HAPI transaction).
            /// </summary>
            Console.WriteLine("Creating new topic...");
            TransactionReceipt topicCreateTxReceipt = new TopicCreateTransaction
            {
                AdminKey = publicKey,
                TransactionId = TransactionId.Generate(aliceAccountId),
                TopicMemo = "Memo",
            }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            Console.WriteLine("Created new topic with ID: " + topicCreateTxReceipt.TopicId);
            /// <summary>
            /// Step 9:
            /// Get the account info and return public key to show its complete account.
            /// </summary>
            AccountInfo aliceAccountInfo_AfterEnhancing = new AccountInfoQuery { AccountId = aliceAccountId }.Execute(client);
            Console.WriteLine("The public key of the newly created and now complete account: " + aliceAccountInfo_AfterEnhancing.Key);
            /// <summary>
            /// Clean up:
            /// Delete created account and topic.
            /// </summary>
            new AccountDeleteTransaction
            {
                TransferAccountId = OPERATOR_ID,
                AccountId = aliceAccountId,
            }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            new TopicDeleteTransaction { TopicId = topicCreateTxReceipt.TopicId }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Auto Create Account Via Transfer Transaction (HIP-583) Example Complete!");
        }
    }
}