// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to create a public HCS topic and submit a message to it.
    /// </summary>
    public class CreateTopicExample
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
            Console.WriteLine("Consensus Service Submit Message To The Public Topic Example Start!");
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
            /// Create new HCS topic.
            /// </summary>
            Console.WriteLine("Creating new topic...");
            TransactionResponse topicCreateTxResponse = new TopicCreateTransaction { AdminKey = operatorPublicKey }.Execute(client);
            TransactionReceipt topicCreateTxReceipt = topicCreateTxResponse.GetReceipt(client);
            TopicId topicId = topicCreateTxReceipt.TopicId;
            Console.WriteLine("Created new topic with ID: " + topicId);
            /// <summary>
            /// Step 2:
            /// Submit message to the topic created in previous step.
            /// </summary>
            Console.WriteLine("Publishing message to the topic...");
            TransactionResponse topicMessageSubmitTxResponse = new TopicMessageSubmitTransaction { TopicId = topicCreateTxReceipt.TopicId, Message = ByteString.CopyFromUtf8("Hello World") }.Execute(client);
            TransactionReceipt topicMessageSubmitTxReceipt = topicMessageSubmitTxResponse.GetReceipt(client);
            Console.WriteLine("Topic sequence number: " + topicMessageSubmitTxReceipt.TopicSequenceNumber);
            /// <summary>
            /// Clean up:
            /// Delete created topic.
            /// </summary>
            new TopicDeleteTransaction { TopicId = topicCreateTxReceipt.TopicId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Consensus Service Submit Message To The Public Topic Example Complete!");
        }
    }
}