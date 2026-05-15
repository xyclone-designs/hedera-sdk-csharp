// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hedera.Hashgraph.Examples
{
    public class SpecificNodeExample
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
        private static readonly bool USE_TLS = !string.Equals("localhost", HEDERA_NETWORK, StringComparison.OrdinalIgnoreCase);
        public static void Main(string[] args)
        {
            Console.WriteLine("Specific Node Communication Example Start!");
            /// <summary>
            /// Method 1: Direct node specification
            /// Directly specify the node you want to communicate with.
            /// Optionally use TLS if supported by the network.
            /// </summary>
            Console.WriteLine("\nExample 1: Direct node specification" + (USE_TLS ? " with TLS" : ""));
            CommunicateWithSpecificNodeDirect();
            /// <summary>
            /// Method 2: Extract from network map
            /// Extract a specific node from the full network map.
            /// </summary>
            Console.WriteLine("\nExample 2: Extract from network map");
            CommunicateWithSpecificNodeFromNetworkMap();
            Console.WriteLine("\nSpecific Node Communication Example Complete!");
        }

        private static void CommunicateWithSpecificNodeDirect()
        {
            /// <summary>
            /// Step 1:
            /// First create a client with the standard network to get the address book
            /// which is needed for TLS
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK);
            /// <summary>
            /// Step 2:
            /// Configure TLS if supported by the network
            /// </summary>
            if (USE_TLS)
            {
                try
                {
                    client.TransportSecurity = true; 
                    client.VerifyCertificates = true;
                    Console.WriteLine("TLS security enabled for this connection");
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine("TLS setup was interrupted: " + e.Message);
                    Thread.CurrentThread.Interrupt(); // Restore the interrupted status
                    throw e; // Re-throw the exception to be handled by the caller
                }
            }
            else
            {
                Console.WriteLine("TLS security not enabled (not supported on localhost)");
            }

            /// <summary>
            /// Step 3:
            /// Set basic client configuration
            /// </summary>
            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            /// <summary>
            /// Step 4:
            /// Create a network map with only one specific node and update the client
            /// </summary>
            Dictionary<string, AccountId> networkMap = [];
            networkMap.Add("0.testnet.hedera.com:50211", new AccountId(3));
            client.Network_.SetNetwork(networkMap);
            /// <summary>
            /// Step 5:
            /// Set max node attempts to 1 to limit retries
            ///
            /// Note: This limits how many times the SDK will retry this node if it returns
            /// a bad gRPC status. The SDK will only use this one node because we've configured
            /// only one node in our network map above.
            /// </summary>
            client.MaxNodeAttempts = 1;
            /// <summary>
            /// Step 6:
            /// Get the node from the network for the ping operation
            /// </summary>
            var network = client.Network_;
            var nodes = new List<AccountId>(network.GetNetwork().Values);
            var node = nodes[0];
            /// <summary>
            /// Step 7:
            /// Ping the node to test connectivity
            /// </summary>
            Console.WriteLine("Pinging node: " + node);
            client.Ping(node);
            Console.WriteLine("Ping successful");
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
        }

        private static void CommunicateWithSpecificNodeFromNetworkMap()
        {
            /// <summary>
            /// Step 1:
            /// Initialize a standard client
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });
            /// <summary>
            /// Step 2:
            /// Get the full network map and extract a specific node
            /// </summary>
            var network = client.Network_;
            KeyValuePair<string, AccountId> firstNodeEntry = network.GetNetwork().First();
            string nodeAddress = firstNodeEntry.Key;
            AccountId nodeAccountId = firstNodeEntry.Value;
            Console.WriteLine("Selected node: " + nodeAddress + " (Account ID: " + nodeAccountId + ")");
            /// <summary>
            /// Step 3:
            /// Create a new map with only the specific node
            /// </summary>
            Dictionary<string, AccountId> specificNodeMap = new()
            {
                { nodeAddress, nodeAccountId }
            };
            /// <summary>
            /// Step 4:
            /// Update the client to use only the specific node
            /// </summary>
            client.Network_.SetNetwork(specificNodeMap);
            /// <summary>
            /// Step 5:
            /// Ping all nodes (which is now just the one specific node)
            /// </summary>
            client.PingAll();
            Console.WriteLine("Ping to specific node successful");
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
        }
    }
}