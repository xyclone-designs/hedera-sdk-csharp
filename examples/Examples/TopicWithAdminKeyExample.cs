// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Linq;

namespace Hedera.Hashgraph.Examples
{
    public class TopicWithAdminKeyExample
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
            Console.WriteLine("Topic With Admin (Threshold) Key Example Start!");
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
            /// Generate the initial key pairs that are part of the Admin Key's Threshold Key.
            ///
            /// Three ED25519 keys part of a 2-of-3 threshold key.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pairs...");
            PrivateKey[] initialAdminPrivateKeys = [.. Enumerable.Repeat(PrivateKey.Generate(), 3)];
            PublicKey[] initialAdminPublicKeys = [.. initialAdminPrivateKeys.Select(_ => _.GetPublicKey())];
            /// <summary>
            /// Step 2:
            /// Create the Threshold Key.
            /// </summary>
            Console.WriteLine("Creating a Key List (threshold key)...");
            KeyList thresholdKey = KeyList.Of(2, initialAdminPublicKeys);
            Console.WriteLine("Created a Key List: " + thresholdKey);
            /// <summary>
            /// Step 3:
            /// Create the topic create transaction with Threshold Key.
            /// </summary>
            Console.WriteLine("Creating topic create transaction...");
            TopicCreateTransaction topicCreateTx = new TopicCreateTransaction
            {
                TopicMemo = "demo topic",
                AdminKey = thresholdKey,

            }.FreezeWith(client);
            /// <summary>
            /// Step 4:
            /// Sign the topic create transaction with 2 of 3 keys that are part of the Admin Key Threshold Key.
            /// </summary>
            foreach (var k in initialAdminPrivateKeys.Take(2))
            {
                Console.WriteLine($"Signing topic create transaction with key {k}");
                topicCreateTx.Sign(k);
            }
            /// <summary>
            /// Step 5:
            /// Execute the topic create transaction.
            /// </summary>
            TransactionResponse topicCreateTxResponse = topicCreateTx.Execute(client);
            TopicId hederaTopicId = topicCreateTxResponse.GetReceipt(client).TopicId;
            Console.WriteLine("Created new topic (" + hederaTopicId + ") with 2-of-3 threshold key as admin key.");
            /// <summary>
            /// Step 6:
            /// Generate the new key pairs that are part of the Admin Key's Threshold Key.
            ///
            /// Four ED25519 keys part of a 3-of-4 threshold key.
            /// </summary>
            Console.WriteLine("Generating new ED25519 key pairs...");
            PrivateKey[] newAdminKeys = [.. Enumerable.Repeat(PrivateKey.Generate(), 4)];
            PublicKey[] newAdminPublicKeys = [.. newAdminKeys.Select(_ => _.GetPublicKey())];
            /// <summary>
            /// Step 7:
            /// Create the new threshold key.
            /// </summary>
            Console.WriteLine("Creating new Key List (threshold key)...");
            KeyList newThresholdKey = KeyList.Of(3, newAdminPublicKeys);
            Console.WriteLine("Created new Key List: " + thresholdKey);
            /// <summary>
            /// Step 8:
            /// Create the topic update transaction with the new threshold key.
            /// </summary>s
            Console.WriteLine("Creating topic update transaction...");
            TopicUpdateTransaction topicUpdateTx = new TopicUpdateTransaction
            {
                TopicId = hederaTopicId,
                TopicMemo = "This topic will be updated",
                AdminKey = newThresholdKey,

            }.FreezeWith(client);
            /// <summary>
            /// Step 9:
            /// Sign the topic update transaction with the initial Admin Key.
            ///
            /// 2 of the 3 keys already part of the topic's Admin Key.
            /// </summary>
            foreach (var k in initialAdminPrivateKeys.Take(2))
            {
                Console.WriteLine("Signing topic update transaction with initial admin key " + k);
                topicUpdateTx.Sign(k);
            }
            /// <summary>
            /// Step 9:
            /// Sign the topic update transaction with the new Admin Key.
            /// 3 of 4 keys already part of the topic's Admin Key.
            /// </summary>
            foreach (var k in newAdminKeys.Take(3))
            {
                Console.WriteLine("Signing topic update transaction with new admin key " + k);
                topicUpdateTx.Sign(k);
            }
            /// <summary>
            /// Step 10:
            /// Execute the topic update transaction.
            /// </summary>
            TransactionResponse topicUpdateTxResponse = topicUpdateTx.Execute(client);

            // Retrieve results post-consensus.
            topicUpdateTxResponse.GetReceipt(client);
            Console.WriteLine("Updated topic (" + hederaTopicId + ") with 3-of-4 threshold key as admin key.");
            /// <summary>
            /// Step 11:
            /// Query the topic info and output it.
            /// </summary>
            TopicInfo hederaTopicInfo = new TopicInfoQuery { TopicId = hederaTopicId }.Execute(client);
            Console.WriteLine("Topic info: " + hederaTopicInfo);
            /// <summary>
            /// Clean up:
            /// Delete created topic.
            /// </summary>
            var topicDeleteTransaction = new TopicDeleteTransaction
            {
                TopicId = hederaTopicId

            }.FreezeWith(client);
            foreach (var k in newAdminKeys.Take(3)) topicDeleteTransaction.Sign(k);
            topicDeleteTransaction.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Topic With Admin (Threshold) Key Example Complete!");
        }
    }
}