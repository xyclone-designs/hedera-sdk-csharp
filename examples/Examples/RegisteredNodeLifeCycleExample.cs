// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;
using System;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    public class RegisteredNodeLifeCycleExample
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
            Console.WriteLine("Registered Node Lifecycle Example Start!");
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
            /// Generate an admin key pair and configure a BlockNodeServiceEndpoint
            /// for use in the RegisterNodeTransaction.
            /// </summary>
            PrivateKey adminKey = PrivateKey.GenerateED25519();
            BlockNodeServiceEndpoint initialEndpoint = new BlockNodeServiceEndpoint().SetIpAddress(new byte[] { 127, 0, 0, 1 }).SetPort(443).SetRequiresTls(true).SetEndpointApis(List.Of(BlockNodeApi.SUBSCRIBE_STREAM, BlockNodeApi.STATUS));
            /// <summary>
            /// Step 2:
            /// Create Registered Node.
            /// </summary>
            RegisteredNodeCreateTransaction registeredNodeCreateTx = new RegisteredNodeCreateTransaction().SetDescription("My Block Node").SetAdminKey(adminKey).AddServiceEndpoint(initialEndpoint).FreezeWith(client).Sign(adminKey);
            Console.WriteLine("Creating Registered Node...");
            TransactionResponse registeredNodeCreateTxResponse = registeredNodeCreateTx.Execute(client);
            TransactionReceipt registeredNodeCreateTxReceipt = registeredNodeCreateTxResponse.GetReceipt(client);
            if (registeredNodeCreateTxReceipt.registeredNodeId <= 0)
            {
                throw new Exception("RegisteredNodeCreate transaction receipt was missing registeredNodeId. (Fail)");
            }

            long registeredNodeId = registeredNodeCreateTxReceipt.registeredNodeId;
            /// <summary>
            /// Step 3:
            /// Execute a RegisteredNodeAddressBookQuery to verify the newly created
            /// registered node appears in the RegisteredNodeAddressBook.
            /// </summary>

            // Wait for mirror node to update
            Thread.Sleep(5000);
            RegisteredNodeAddressBookQuery addressBookQuery = new RegisteredNodeAddressBookQuery().SetRegisteredNodeId(registeredNodeId);
            Console.WriteLine("Executing RegisteredNodeQuery....");
            RegisteredNodeAddressBook addressBook = addressBookQuery.Execute(client);
            RegisteredNode registeredNode = addressBook.registeredNodes.GetFirst();
            Console.WriteLine("Successfully fetch the registered node, " + registeredNode);
            /// <summary>
            /// Step 4:
            /// Update the RegisteredNode with new Block Node endpoint.
            /// </summary>
            BlockNodeServiceEndpoint updateEndpoint = new BlockNodeServiceEndpoint().SetDomainName("block-node.example.com").SetPort(443).SetRequiresTls(true).AddEndpointApi(BlockNodeApi.STATUS);
            RegisteredNodeUpdateTransaction registeredNodeUpdateTx = new RegisteredNodeUpdateTransaction().SetRegisteredNodeId(registeredNodeId).SetDescription("My Updated Block Node").SetServiceEndpoints(List.Of(initialEndpoint, updateEndpoint)).FreezeWith(client).Sign(adminKey);
            Console.WriteLine("Updating Registered Node...");
            TransactionResponse registeredNodeUpdateTxResponse = registeredNodeUpdateTx.Execute(client);
            registeredNodeUpdateTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 5:
            /// Add the registeredNodeId as associatedRegisteredNodes to a Node.
            /// NOTE: This transaction must be signed by the consensus node's admin key.
            /// In this example, we assume the operator is the node admin.
            /// </summary>
            NodeUpdateTransaction associateTx = new NodeUpdateTransaction().SetNodeId(0).AddAssociatedRegisteredNode(registeredNodeId).FreezeWith(client);
            Console.WriteLine("Associating registered node " + registeredNodeId + " with consensus node...");
            TransactionResponse associateTxResponse = associateTx.Execute(client);
            associateTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 6:
            /// Remove the registeredNodeId as associatedRegisteredNodes from a Node.
            /// </summary>
            NodeUpdateTransaction disassociateTx = new NodeUpdateTransaction().SetNodeId(0).ClearAssociatedRegisteredNodes().FreezeWith(client);
            Console.WriteLine("Disassociating registered node " + registeredNodeId + " with consensus node...");
            TransactionResponse disassociatedTxResponse = disassociateTx.Execute(client);
            disassociatedTxResponse.GetReceipt(client);
            /// <summary>
            /// Step 7:
            /// Delete the Registered Node.
            /// </summary>
            Console.WriteLine("Deleting Registered Node...");
            new RegisteredNodeDeleteTransaction().SetRegisteredNodeId(registeredNodeCreateTxReceipt.registeredNodeId).FreezeWith(client).Sign(adminKey).Execute(client).GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Registered Node Lifecycle Example Complete!");
        }
    }
}