// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;

using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to create a file.
    /// </summary>
    public class CreateFileExample
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
            Console.WriteLine("Create File Example Start!");
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
            var operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Submit the file create transaction.
            /// </summary>

            // The file is required to be a byte array,
            // you can easily use the bytes of a file instead.
            string fileContents = "Hedera hashgraph is great!";
            Console.WriteLine("Creating new file...");
            TransactionResponse fileCreateTxResponse = new FileCreateTransaction
            {
                Keys = [operatorPublicKey],
                Contents = Encoding.UTF8.GetBytes(fileContents),
                MaxTransactionFee = Hbar.From(2),
            
            }.Execute(client);
            TransactionReceipt fileCreateTxReceipt = fileCreateTxResponse.GetReceipt(client);
            FileId newFileId = fileCreateTxReceipt.FileId;

            Console.WriteLine("Created new file with ID: " + newFileId);
            /// <summary>
            /// Clean up:
            /// Delete created file.
            /// </summary>
            new FileDeleteTransaction { FileId = newFileId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Create File Example Complete!");
        }
    }
}