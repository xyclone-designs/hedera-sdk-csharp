// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    public class ConsensusPubSubWithSubmitKeyExample
    {
        private static readonly int TOTAL_MESSAGES = 5;
        private static readonly CountdownEvent MessagesLatch = new(TOTAL_MESSAGES);
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
            Console.WriteLine("Consensus Service Submit Message To The Private Topic And Subscribe Example Start!");
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
            PublicKey operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pair (Submit Key to use with the topic).
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...");
            PrivateKey submitPrivateKey = PrivateKey.GenerateED25519();
            PublicKey submitPublicKey = submitPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create new HCS topic with the key right above as the topic's Submit Key required to sign all future
            /// ConsensusMessageSubmitTransactions for that topic.
            /// </summary>
            Console.WriteLine("Creating new HCS topic...");
            TransactionResponse topicCreateTxResponse = new TopicCreateTransaction { TopicMemo = "HCS topic with Submit Key", AdminKey = operatorPublicKey, SubmitKey = submitPublicKey }.Execute(client);
            TopicId hederaTopicId = topicCreateTxResponse.GetReceipt(client).TopicId;
            Console.WriteLine("Created topic with ID: " + hederaTopicId + " and public ED25519 submit key: " + submitPrivateKey);
            /// <summary>
            /// Step 3:
            /// Wait 5 seconds (to ensure data propagated to mirror nodes).
            /// </summary>
            Console.WriteLine("Wait 5 seconds (to ensure data propagated to mirror nodes) ...");
            Thread.Sleep(5000);
            /// <summary>
            /// Step 4:
            /// Subscribe to messages on the topic, printing out the received message and metadata as it is published by the
            /// Hedera mirror node.
            /// </summary>
            Console.WriteLine("Setting up a mirror client...");
            new TopicMessageQuery
            {
                TopicId = hederaTopicId,
                StartTime = DateTimeOffset.FromUnixTimeMilliseconds(0)
            }
            .Subscribe(client, (resp) =>
            {
                string messageAsString = Encoding.UTF8.GetString(resp.Contents);
                Console.WriteLine("Topic message received!" + " | Time: " + resp.ConsensusTimestamp + " | Content: " + messageAsString);
                MessagesLatch.Signal();
            });
            /// <summary>
            /// Step 5:
            /// Publish a list of messages to a topic, signing each transaction with the topic's Submit Key.
            /// </summary>
            Random randomGenerator = new ();
            for (int i = 0; i <= TOTAL_MESSAGES; i++)
            {
                string message = "random message " + randomGenerator.NextInt64();
                Console.WriteLine("Publishing message to the topic: " + message);
                new TopicMessageSubmitTransaction
                {
                    TopicId = hederaTopicId,
                    Message = ByteString.CopyFromUtf8(message),
                }
                .FreezeWith(client)
                .Sign(submitPrivateKey)
                .Execute(client)
                .TransactionId
                .GetReceipt(client);
                Thread.Sleep(2000);
            }

            // Wait 60 seconds to receive all the messages. Fail if not received.
            bool allMessagesReceived = MessagesLatch.Wait(TimeSpan.FromSeconds(60));
            /// <summary>
            /// Clean up:
            /// Delete created topic.
            /// </summary>
            new TopicDeleteTransaction { TopicId = hederaTopicId }.Execute(client).GetReceipt(client);
            client.Dispose();

            // Fail if messages weren't received.
            if (!allMessagesReceived)
            {
                throw new TimeoutException("Not all topic messages were received! (Fail)");
            }

            Console.WriteLine("Consensus Service Submit Message To The Private Topic And Subscribe Example Complete!");
        }
    }
}