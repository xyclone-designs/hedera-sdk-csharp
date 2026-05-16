// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to send large message to the private HCS topic and how to subscribe to the topic to receive it.
    /// </summary>
    public class ConsensusPubSubChunkedExample
    {
        private static readonly CountdownEvent LARGE_MESSAGE_LATCH = new CountdownEvent(1);
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
            Console.WriteLine("Consensus Service Submit Large Message And Subscribe Example Start!");
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
            /// Generate ED25519 key pair (Submit Key to use with the topic).
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...");
            PrivateKey submitPrivateKey = PrivateKey.GenerateED25519();
            PublicKey submitPublicKey = submitPrivateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create new HCS topic.
            /// </summary>
            Console.WriteLine("Creating new topic...");
            TopicId hederaTopicID = new TopicCreateTransaction { TopicMemo = "hedera-sdk-java/ConsensusPubSubChunkedExample", AdminKey = operatorPublicKey, SubmitKey = submitPublicKey }.Execute(client).GetReceipt(client).TopicId;

            Console.WriteLine("Created new topic with ID: " + hederaTopicID);
            /// <summary>
            /// Step 3:
            /// Wait 10 seconds (to ensure data propagated to mirror nodes).
            /// </summary>
            Console.WriteLine("Wait 5 seconds (to ensure data propagated to mirror nodes) ...");
            Thread.Sleep(5000);
            /// <summary>
            /// Step 4:
            /// Subscribe to messages on the topic, printing out the received message and metadata as it is published by the
            /// Hedera mirror node.
            /// </summary>
            Console.WriteLine("Setting up a mirror client...");
            new TopicMessageQuery { TopicId = hederaTopicID }.Subscribe(client, (topicMessage) =>
            {
                Console.WriteLine("Topic message received!" + " | Time: " + topicMessage.ConsensusTimestamp + " | Sequence No.: " + topicMessage.SequenceNumber + " | Size: " + topicMessage.Contents.Length + " bytes.");
                LARGE_MESSAGE_LATCH.Wait();
            });
            /// <summary>
            /// Step 5:
            /// Send large message to the topic created previously.
            /// </summary>

            // Get a large file to send.
            string largeMessage = ReadResources("util/large_message.txt");

            // Prepare a message send transaction that requires a submit key from "somewhere else".
            TopicMessageSubmitTransaction topicMessageSubmitTx = new TopicMessageSubmitTransaction 
            {
                MaxChunks = 15,
                TopicId = hederaTopicID,
                Message = ByteString.CopyFromUtf8(largeMessage),

            }.SignWithOperator(client);

            // Serialize to bytes, so we can be signed "somewhere else" by the submit key.
            byte[] transactionBytes = topicMessageSubmitTx.ToBytes();

            // Now pretend we sent those bytes across the network.
            // Parse them into a transaction, so we can sign as the submit key.
            topicMessageSubmitTx = Transaction.FromBytes<TopicMessageSubmitTransaction>(transactionBytes);

            // View out the message size from the parsed transaction.
            // This can be useful to display what we are about to sign.
            long transactionMessageSize = topicMessageSubmitTx.Message.Length;
            Console.WriteLine("Preparing to submit a message to the created topic (size of the message: " + transactionMessageSize + " bytes)...");

            // Sign with that Submit Key.
            topicMessageSubmitTx.Sign(submitPrivateKey);

            // Now actually submit the transaction and get the receipt to ensure there were no errors.
            topicMessageSubmitTx.Execute(client).GetReceipt(client);

            // Wait 60 seconds to receive the message. Fail if not received.
            bool largeMessageReceived = LARGE_MESSAGE_LATCH.Wait(TimeSpan.FromSeconds(60));
            /// <summary>
            /// Clean up:
            /// Delete created topic.
            /// </summary>
            new TopicDeleteTransaction { TopicId = hederaTopicID }.Execute(client).GetReceipt(client);
            client.Dispose();

            // Fail if message wasn't received.
            if (!largeMessageReceived)
            {
                throw new TimeoutException("Large topic message was not received! (Fail)");
            }

            Console.WriteLine("Consensus Service Submit Large Message And Subscribe Example Complete!");
        }

        private static string ReadResources(string filename)
        {
            InputStream inputStream = typeof(ConsensusPubSubChunkedExample).GetResourceAsStream(filename);
            StringBuilder bigContents = new ();
            try
            {
                using (BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream), UTF_8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        bigContents.Append(line).Append("\n");
                    }
                }
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }

            return bigContents.ToString();
        }
    }
}