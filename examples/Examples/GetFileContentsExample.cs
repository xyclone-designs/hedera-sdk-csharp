// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;

using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class GetFileContentsExample
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
            Console.WriteLine("Get File Contents Example Start!");
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

            // Content to be stored in the file.
            byte[] fileContents = Encoding.UTF8.GetBytes("Hedera is great!");

            // Create the new file and set its properties.
            Console.WriteLine("Creating new file...");
            TransactionResponse fileCreateTxResponse = new FileCreateTransaction
            {
                Keys = [operatorPublicKey],
                Contents = fileContents,
                MaxTransactionFee = Hbar.From(2)

            }.Execute(client);
            FileId newFileId = fileCreateTxResponse.GetReceipt(client).FileId;
            Console.WriteLine("Created new file with ID: " + newFileId);
            /// <summary>
            /// Step 2:
            /// Get file contents and print them.
            /// </summary>
            ByteString contents = new FileContentsQuery { FileId = newFileId }.Execute(client);

            // Prints query results to console.
            Console.WriteLine("File contents: " + contents.ToStringUtf8());
            /// <summary>
            /// Clean up:
            /// Delete created file.
            /// </summary>
            new FileDeleteTransaction { FileId = newFileId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Get File Contents Example Complete!");
        }
    }
}