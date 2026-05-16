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
    /// <summary>
    /// How to append to already created file.
    /// </summary>
    public class FileAppendChunkedExample
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
            Console.WriteLine("Big File Append Example Start!");
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
            /// Step 2:
            /// Query file info to check its size after creation.
            /// </summary>
            FileInfo fileInfoAfterCreate = new FileInfoQuery { FileId = newFileId }.Execute(client);
            Console.WriteLine("Created file size after create (according to `FileInfoQuery`): " + fileInfoAfterCreate.Size + " bytes.");
            /// <summary>
            /// Step 3:
            /// Create new file contents that will be appended to a file.
            /// </summary>
            StringBuilder contents = new ();
            for (int i = 0; i <= 4096 / 9; i++)
            {
                contents.Append('1');
            }

            /// <summary>
            /// Step 4:
            /// Append new file contents to a file.
            /// </summary>
            Console.WriteLine("Appending new contents to the created file...");
            new FileAppendTransaction
            {
                NodeAccountIds = { fileCreateTxResponse.NodeId },
                FileId = newFileId,
                Contents = ByteString.CopyFromUtf8(contents.ToString()),
                MaxChunks = 40,
                MaxTransactionFee = Hbar.From(100),
            }
            .FreezeWith(client)
            .Execute(client)
            .GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Query file info to check its size after append.
            /// </summary>
            FileInfo fileInfoAfterAppend = new FileInfoQuery { FileId = newFileId }.Execute(client);
            if (fileInfoAfterCreate.Size < fileInfoAfterAppend.Size)
            {
                Console.WriteLine("File size after append (according to `FileInfoQuery`): " + fileInfoAfterAppend.Size + " bytes.");
            }
            else
            {
                throw new Exception("File append was unsuccessful! (Fail)");
            }

            /// <summary>
            /// Clean up:
            /// Delete created file.
            /// </summary>
            new FileDeleteTransaction { FileId = newFileId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Big File Append Example Complete!");
        }
    }
}