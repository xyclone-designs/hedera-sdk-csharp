// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// hip-869
    /// </summary>
    public class DynamicAddressBookExample
    {
        // see `.env.sample` in the repository root for how to specify these values
        // or set environment variables with the same names
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        // HEDERA_NETWORK defaults to testnet if not specified in dotenv
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        public static void Main(string[] args)
        {
            Client client = ClientHelper.ForName(HEDERA_NETWORK);

            // Defaults the operator account ID and key such that all generated transactions will be paid for
            // by this account and be signed by this key
            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            AccountId accountId = AccountId.FromString("0.0.1999");
            string description = "Hedera™ cryptocurrency";
            string newDescription = "Hedera™ cryptocurrency - updated";

            // Set up IPv4 address
            Endpoint gossipEndpoint = new Endpoint()
            {
                Address = [0x00, 0x01, 0x02, 0x03],
                Port = 123456
            };

            // Set up service endpoint
            Endpoint serviceEndpoint = new Endpoint()
            {
                Address = [0x00, 0x01, 0x02, 0x03],
                Port = 123456
            };

            // Set up grpcWebProxyEndpoint address
            var grpcWebProxyEndpoint = new Endpoint()
            {
                Address = [0x00, 0x01, 0x02, 0x05],
                Port = 123456
            };

            // Generate admin key
            PrivateKey adminKey = PrivateKey.GenerateED25519();

            // Create node create transaction
            NodeCreateTransaction nodeCreateTransaction = new NodeCreateTransaction
            {
                AccountId = accountId,
                Description = description,
                GossipCaCertificate = Encoding.UTF8.GetBytes("gossipCaCertificate"),
                ServiceEndpoints = [serviceEndpoint],
                GossipEndpoints = [gossipEndpoint],
                GrpcWebProxyEndpoint = grpcWebProxyEndpoint,
                AdminKey = adminKey.GetPublicKey()
            };
            try
            {
                nodeCreateTransaction.Execute(client).GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var grpcWebProxyEndpointUpdated = new Endpoint
            {
                Address = [0x00, 0x01, 0x02, 0x06],
                Port = 123456
            };
            var nodeUpdateTransaction = new NodeUpdateTransaction
            {
                NodeId = 123,
                AccountId = accountId,
                Description = newDescription,
                GossipCaCertificate = Encoding.UTF8.GetBytes("gossipCaCertificate"),
                ServiceEndpoints = [serviceEndpoint],
                GossipEndpoints = [gossipEndpoint],
                DeclineReward = true,
                GrpcWebProxyEndpoint = grpcWebProxyEndpointUpdated,
                AdminKey = adminKey.GetPublicKey()
            };
            try
            {
                nodeUpdateTransaction.Execute(client).GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var nodeDeleteTransaction = new NodeDeleteTransaction { NodeId = 123 };
            try
            {
                nodeDeleteTransaction.Execute(client).GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}