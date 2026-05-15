// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Consensus;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

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
            PrivateKey[] initialAdminPrivateKeys = new PrivateKey[3];
            PublicKey[] initialAdminPublicKeys = new PublicKey[3];
            Arrays.SetAll(initialAdminPrivateKeys, (i) => PrivateKey.Generate());
            Arrays.SetAll(initialAdminPublicKeys, (i) => initialAdminPrivateKeys[i].GetPublicKey());
            /// <summary>
            /// Step 2:
            /// Create the Threshold Key.
            /// </summary>
            Console.WriteLine("Creating a Key List (threshold key)...");
            KeyList thresholdKey = KeyList.WithThreshold(2);
            Collections.AddAll(thresholdKey, initialAdminPublicKeys);
            Console.WriteLine("Created a Key List: " + thresholdKey);
            /// <summary>
            /// Step 3:
            /// Create the topic create transaction with Threshold Key.
            /// </summary>
            Console.WriteLine("Creating topic create transaction...");
            Transaction<TWildcardTodo> topicCreateTx = new TopicCreateTransaction().SetTopicMemo("demo topic").SetAdminKey(thresholdKey).FreezeWith(client);
            /// <summary>
            /// Step 4:
            /// Sign the topic create transaction with 2 of 3 keys that are part of the Admin Key Threshold Key.
            /// </summary>
            Arrays.Stream(initialAdminPrivateKeys, 0, 2).ForEach((k) =>
            {
                Console.WriteLine("Signing topic create transaction with key " + k);
                topicCreateTx.Sign(k);
            });
            /// <summary>
            /// Step 5:
            /// Execute the topic create transaction.
            /// </summary>
            TransactionResponse topicCreateTxResponse = topicCreateTx.Execute(client);
            TopicId hederaTopicId = topicCreateTxResponse.GetReceipt(client).TopicId;
            hederaTopicId;
            Console.WriteLine("Created new topic (" + hederaTopicId + ") with 2-of-3 threshold key as admin key.");
            /// <summary>
            /// Step 6:
            /// Generate the new key pairs that are part of the Admin Key's Threshold Key.
            ///
            /// Four ED25519 keys part of a 3-of-4 threshold key.
            /// </summary>
            Console.WriteLine("Generating new ED25519 key pairs...");
            PrivateKey[] newAdminKeys = new PrivateKey[4];
            PublicKey[] newAdminPublicKeys = new PublicKey[4];
            Arrays.SetAll(newAdminKeys, (i) => PrivateKey.Generate());
            Arrays.SetAll(newAdminPublicKeys, (i) => newAdminKeys[i].GetPublicKey());
            /// <summary>
            /// Step 7:
            /// Create the new threshold key.
            /// </summary>
            Console.WriteLine("Creating new Key List (threshold key)...");
            KeyList newThresholdKey = KeyList.WithThreshold(3);
            Collections.AddAll(newThresholdKey, newAdminPublicKeys);
            Console.WriteLine("Created new Key List: " + thresholdKey);
            /// <summary>
            /// Step 8:
            /// Create the topic update transaction with the new threshold key.
            /// </summary>s
            Console.WriteLine("Creating topic update transaction...");
            Transaction<TWildcardTodo> topicUpdateTx = new TopicUpdateTransaction().SetTopicId(hederaTopicId).SetTopicMemo("This topic will be updated").SetAdminKey(newThresholdKey).FreezeWith(client);
            /// <summary>
            /// Step 9:
            /// Sign the topic update transaction with the initial Admin Key.
            ///
            /// 2 of the 3 keys already part of the topic's Admin Key.
            /// </summary>
            Arrays.Stream(initialAdminPrivateKeys, 0, 2).ForEach((k) =>
            {
                Console.WriteLine("Signing topic update transaction with initial admin key " + k);
                topicUpdateTx.Sign(k);
            });
            /// <summary>
            /// Step 9:
            /// Sign the topic update transaction with the new Admin Key.
            /// 3 of 4 keys already part of the topic's Admin Key.
            /// </summary>
            Arrays.Stream(newAdminKeys, 0, 3).ForEach((k) =>
            {
                Console.WriteLine("Signing topic update transaction with new admin key " + k);
                topicUpdateTx.Sign(k);
            });
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
            var topicDeleteTransaction = new TopicDeleteTransaction().SetTopicId(hederaTopicId).FreezeWith(client);
            Arrays.Stream(newAdminKeys, 0, 3).ForEach((k) =>
            {
                topicDeleteTransaction.Sign(k);
            });
            topicDeleteTransaction.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Topic With Admin (Threshold) Key Example Complete!");
        }
    }
}