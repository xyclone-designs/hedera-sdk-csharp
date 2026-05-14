// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class LoggerFunctionalitiesExample
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
        public static void Main(string[] args)
        {
            Console.WriteLine("Logger Functionalities Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK);

            // All generated transactions will be paid by this account and signed by this key.
            _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            /// <summary>
            /// Step 1:
            /// Instantiate debug- and info-level loggers.
            /// </summary>
            var debugLogger = new Logger(LogLevel.DEBUG);
            var infoLogger = new Logger(LogLevel.INFO);
            /// <summary>
            /// Step 2:
            /// Attach debug logger to the SDK Client.
            /// </summary>
            //_client.Logger = new Logger(debugLogger);
            /// <summary>
            /// Step 3:
            /// Generate ED25519 private and public keys.
            /// </summary>
            var privateKey = PrivateKey.GenerateED25519();
            var publicKey = privateKey.GetPublicKey();
            /// <summary>
            /// Step 4:
            /// "Create" account.
            /// </summary>
            Console.WriteLine("\"Creating\" new account...");
            var aliasAccountId = publicKey.ToAccountId(0, 0);
            var operatorPublicKey = OPERATOR_KEY.GetPublicKey();
            /// <summary>
            /// Step 4:
            /// Transfer 10 tinybars from operator's account to newly created account to init it on Hedera network.
            /// </summary>
            Console.WriteLine("Transferring Hbar to the the new account...");
            new TransferTransaction().AddHbarTransfer(OPERATOR_ID, Hbar.From(1).Negated()).AddHbarTransfer(aliasAccountId, Hbar.From(1)).SetTransactionMemo("").Execute(client);
            /// <summary>
            /// Step 5:
            /// Create a topic with attached info logger.
            /// </summary>
            Console.WriteLine("Creating new topic...(with attached info logger).");
            TopicId hederaTopicId = new TopicCreateTransaction { Logger = infoLogger, TopicMemo = "Hedera topic", AdminKey = operatorPublicKey }.Execute(client).GetReceipt(client).TopicId;
            hederaTopicId;
            /// <summary>
            /// Step 6:
            /// Set the level of the infoLogger from info to error.
            /// </summary>
            infoLogger.SetLevel(LogLevel.ERROR);
            /// <summary>
            /// Step 7:
            /// Create a topic with attached info logger.
            ///
            /// This should not display any logs because currently there are no warn logs predefined in the SDK.
            /// </summary>
            Console.WriteLine("Creating new topic...(with attached info logger).");
            var logisticsTopicId = new TopicCreateTransaction { Logger = infoLogger, TopicMemo = "Logistics topic", AdminKey = operatorPublicKey }.Execute(client).GetReceipt(client).TopicId;
            logisticsTopicId;
            /// <summary>
            /// Step 8:
            /// Silence the debugLogger - no logs should be shown.
            ///
            /// This can also be achieved by calling .setLevel(LogLevel.Silent).
            /// </summary>
            debugLogger.SetSilent(true);
            /// <summary>
            /// Step 9:
            /// Create a topic with attached debug logger.
            /// This should not display any logs because logger was silenced.
            /// </summary>
            Console.WriteLine("Creating new topic...(with attached debug logger).");
            var supplyChainTopicId = new TopicCreateTransaction { Logger = debugLogger, TopicMemo = "Supply chain topic", AdminKey = operatorPublicKey }.Execute(client).GetReceipt(client).TopicId;
            supplyChainTopicId;
            /// <summary>
            /// Step 10:
            /// Unsilence the debugLogger - applies back the old log level before silencing.
            /// </summary>
            debugLogger.SetSilent(false);
            /// <summary>
            /// Step 11:
            /// Create a topic with attached debug logger.
            ///
            /// Should produce logs.
            /// </summary>
            Console.WriteLine("Creating new topic...(with attached debug logger).");
            var chatTopicId = new TopicCreateTransaction { Logger = debugLogger, TopicMemo = "Chat topic", AdminKey = operatorPublicKey }.Execute(client).GetReceipt(client).TopicId;
            chatTopicId;
            /// <summary>
            /// Clean up:
            /// Delete created topics.
            /// </summary>
            new TopicDeleteTransaction { TopicId = hederaTopicId }.Execute(client).GetReceipt(client);
            new TopicDeleteTransaction { TopicId = logisticsTopicId }.Execute(client).GetReceipt(client);
            new TopicDeleteTransaction { TopicId = supplyChainTopicId }.Execute(client).GetReceipt(client);
            new TopicDeleteTransaction { TopicId = chatTopicId }.Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Logger Functionalities Example Complete!");
        }
    }
}