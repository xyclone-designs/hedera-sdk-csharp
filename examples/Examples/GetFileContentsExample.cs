// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class GetFileContentsExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        private static readonly string HEDERA_NETWORK = Dotenv.Load().Get("HEDERA_NETWORK", "testnet");
        private static readonly string SDK_LOG_LEVEL = Dotenv.Load().Get("SDK_LOG_LEVEL", "SILENT");
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
            byte[] fileContents = "Hedera is great!".GetBytes(StandardCharsets.UTF_8);

            // Create the new file and set its properties.
            Console.WriteLine("Creating new file...");
            TransactionResponse fileCreateTxResponse = new FileCreateTransaction().SetKeys(operatorPublicKey).SetContents(fileContents).SetMaxTransactionFee(Hbar.From(2)).Execute(client);
            FileId newFileId = fileCreateTxResponse.GetReceipt(client).FileId;
            newFileId;
            Console.WriteLine("Created new file with ID: " + newFileId);
            /// <summary>
            /// Step 2:
            /// Get file contents and print them.
            /// </summary>
            ByteString contents = new FileContentsQuery { FileId = newFileId }.Execute(client);
            contents;

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