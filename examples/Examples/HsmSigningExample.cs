// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class HsmSigningExample
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
            Console.WriteLine("HSM Signing Example Start!");
            try
            {
                /// <summary>
                /// Step 0:
                /// Create and configure SDK Client.
                /// </summary>
                Client client = CreateClient();
                /// <summary>
                /// Step 1:
                /// Generate keys and create test accounts.
                /// </summary>
                AccountSetup accounts = SetupTestAccounts(client);
                /// <summary>
                /// Step 2:
                /// Demonstrate single node transaction signing.
                /// </summary>
                SingleNodeTransactionExample(client, accounts.SenderId, accounts.ReceiverId, accounts.SenderKey);
                /// <summary>
                /// Step 3:
                /// Demonstrate multi-node multi-chunk transaction signing.
                /// </summary>
                MultiNodeFileTransactionExample(client, accounts.SenderId, accounts.SenderKey);
                /// <summary>
                /// Clean up:
                /// </summary>
                client.Dispose();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Example failed: " + e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }

            Console.WriteLine("HSM Signing Example Complete!");
        }

        /// <summary>
        /// Container class for account setup results.
        /// </summary>
        private class AccountSetup
        {
            public AccountSetup(AccountId senderId, AccountId receiverId, PrivateKey senderKey, PrivateKey receiverKey)
            {
                SenderId = senderId;
                ReceiverId = receiverId;
                SenderKey = senderKey;
                ReceiverKey = receiverKey;
            }

            public readonly AccountId SenderId;
            public readonly AccountId ReceiverId;
            public readonly PrivateKey SenderKey;
            public readonly PrivateKey ReceiverKey;
        }

        private static AccountSetup SetupTestAccounts(Client client)
        {
            Console.WriteLine("\n--- Setting up test accounts ---");

            // Generate keys for sender and receiver
            PrivateKey senderKey = PrivateKey.GenerateED25519();
            PrivateKey receiverKey = PrivateKey.GenerateED25519();

            // Create sender account
            TransactionResponse senderAccountResponse = new AccountCreateTransaction { InitialBalance = Hbar.From(10) }
                
            .SetKeyWithoutAlias(senderKey.GetPublicKey())
                .Execute(client);
            TransactionReceipt senderAccountReceipt = senderAccountResponse.GetReceipt(client);
            AccountId senderId = senderAccountReceipt.AccountId;

            // Create receiver account
            TransactionResponse receiverAccountResponse = new AccountCreateTransaction { InitialBalance = Hbar.From(1) }
                
            .SetKeyWithoutAlias(receiverKey.GetPublicKey())
                .Execute(client);
            TransactionReceipt receiverAccountReceipt = receiverAccountResponse.GetReceipt(client);
            AccountId receiverId = receiverAccountReceipt.AccountId;
            Console.WriteLine("Created sender account: " + senderId);
            Console.WriteLine("Created receiver account: " + receiverId);

            return new AccountSetup(senderId, receiverId, senderKey, receiverKey);
        }

        private static void SingleNodeTransactionExample(Client client, AccountId senderId, AccountId receiverId, PrivateKey senderKey)
        {
            Console.WriteLine("\n--- Single Node Transaction Example ---");

            // Step 1 - Create and prepare transfer transaction
            // Get first node from network
            Dictionary<string, AccountId> network = client.Network_.GetNetwork();
            AccountId nodeAccountId = network.Values.First();

            // Create transfer transaction
            TransferTransaction transferTx = new TransferTransaction
            {
                TransactionId = TransactionId.Generate(senderId)
            }
            .AddHbarTransfer(senderId, Hbar.From(-1))
            .AddHbarTransfer(receiverId, Hbar.From(1))
                .SetNodeAccountIds([nodeAccountId])
                .FreezeWith(client);
            Console.WriteLine("Transaction frozen. Node IDs: " + transferTx.NodeAccountIds);

            // Step 2 - Get signable bytes and sign with HSM
            List<Transaction.SignableNodeTransactionBodyBytes> signableList = transferTx.GetSignableNodeBodyBytesList();
            Console.WriteLine("Got " + signableList.Count + " signable entries");

            // Sign with HSM for each entry
            for (int i = 0; i < signableList.Count; i++)
            {
                Transaction.SignableNodeTransactionBodyBytes signable = signableList[i];
                Console.WriteLine("Signing entry " + i + " for node " + signable.NodeID + " and transaction " + signable.TransactionID);
                byte[] signature = HsmSign(senderKey, signable.Body);
                transferTx = transferTx.AddSignature(senderKey.GetPublicKey(), signature, signable.TransactionID, signable.NodeID);
            }

            // Step 3 - Execute transaction and get receipt
            Console.WriteLine("Executing transaction...");
            TransactionResponse transferResponse = transferTx.Execute(client);
            TransactionReceipt transferReceipt = transferResponse.GetReceipt(client);
            Console.WriteLine("Single node transaction status: " + transferReceipt.Status);
        }

        private static void MultiNodeFileTransactionExample(Client client, AccountId senderId, PrivateKey senderKey)
        {
            Console.WriteLine("\n--- Multi-Node File Transaction Example ---");

            // Step 1 - Create initial file
            // Create smaller content for testing to avoid chunking issues
            string smallContents = "Test file content for HSM signing example.";

            // Create file transaction
            FileCreateTransaction fileCreateTx = new FileCreateTransaction
            {
                Keys = [senderKey.GetPublicKey()],
                Contents = Encoding.UTF8.GetBytes(smallContents),
                MaxTransactionFee = Hbar.From(5),
            }
            .FreezeWith(client)
            .Sign(senderKey);
            TransactionResponse fileCreateResponse = fileCreateTx.Execute(client);
            TransactionReceipt fileCreateReceipt = fileCreateResponse.GetReceipt(client);
            FileId fileId = fileCreateReceipt.FileId;
            Console.WriteLine("Created file with ID: " + fileId);

            // Step 2 - Prepare file append transaction (using smaller content to avoid chunking for now)
            string appendContent = "Additional content added via HSM signing.";
            FileAppendTransaction fileAppendTx = new FileAppendTransaction
            {
                FileId = fileId,
                Contents = ByteString.CopyFromUtf8(appendContent),
                MaxTransactionFee = Hbar.From(5),
                TransactionId = TransactionId.Generate(senderId),

            }.FreezeWith(client);
            Console.WriteLine("File append transaction frozen. Node IDs: " + fileAppendTx.NodeAccountIds);

            // Step 3 - Get signable bytes and sign with HSM for each node
            List<Transaction.SignableNodeTransactionBodyBytes> multiNodeSignableList = fileAppendTx.GetSignableNodeBodyBytesList();
            Console.WriteLine("Got " + multiNodeSignableList.Count + " signable entries for file append");

            // Sign with HSM for each entry
            for (int i = 0; i < multiNodeSignableList.Count; i++)
            {
                Transaction.SignableNodeTransactionBodyBytes signable = multiNodeSignableList[i];
                Console.WriteLine("Signing entry " + i + " for node " + signable.NodeID + " and transaction " + signable.TransactionID);
                byte[] signature = HsmSign(senderKey, signable.Body);
                fileAppendTx = fileAppendTx.AddSignature(senderKey.GetPublicKey(), signature, signable.TransactionID, signable.NodeID);
            }


            // Step 4 - Execute transaction and verify results
            Console.WriteLine("Executing file append transaction...");
            TransactionResponse fileAppendResponse = fileAppendTx.Execute(client);
            TransactionReceipt fileAppendReceipt = fileAppendResponse.GetReceipt(client);
            Console.WriteLine("Multi-node file append transaction status: " + fileAppendReceipt.Status);

            // Step 5 - Verify file contents
            byte[] contents = new FileContentsQuery { FileId = fileId }.Execute(client).ToByteArray();
            Console.WriteLine("File content length according to FileContentsQuery: " + contents.Length);
            Console.WriteLine("File contents: " + Encoding.UTF8.GetString(contents));
        }

        private static byte[] HsmSign(PrivateKey key, byte[] bodyBytes)
        {

            // This is a placeholder that simulates HSM signing
            // In a real HSM implementation, you would:
            // 1. Send bodyBytes to the HSM
            // 2. Use HSM APIs to sign with the stored private key
            // 3. Return the signature from the HSM
            return key.Sign(bodyBytes);
        }

        private static Client CreateClient()
        {
            /// <summary>
            /// Step 1:
            /// Create a client for the specified network.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                // All generated transactions will be paid by this account and signed by this key.
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                // Attach logger to the SDK Client.
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });
            return client;
        }
    }
}