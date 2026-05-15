// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

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
            _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            AccountId accountId = AccountId.FromString("0.0.1999");
            string description = "Hedera™ cryptocurrency";
            string newDescription = "Hedera™ cryptocurrency - updated";

            // Set up IPv4 address
            Endpoint gossipEndpoint = new Endpoint();
            gossipEndpoint.SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x03 });

            // Set up service endpoint
            Endpoint serviceEndpoint = new Endpoint();
            serviceEndpoint.SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x03 });

            // Set up grpcWebProxyEndpoint address
            var grpcWebProxyEndpoint = new Endpoint().SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x05 }).SetPort(12345);

            // Generate admin key
            PrivateKey adminKey = PrivateKey.GenerateED25519();

            // Create node create transaction
            NodeCreateTransaction nodeCreateTransaction = new NodeCreateTransaction().SetAccountId(accountId).SetDescription(description).SetGossipCaCertificate("gossipCaCertificate".GetBytes()).SetServiceEndpoints([serviceEndpoint]).SetGossipEndpoints([gossipEndpoint]).SetGrpcWebProxyEndpoint(grpcWebProxyEndpoint).SetAdminKey(adminKey.GetPublicKey());
            try
            {
                nodeCreateTransaction.Execute(client).GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var grpcWebProxyEndpointUpdated = new Endpoint().SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x06 }).SetPort(123456);
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetNodeId(123).SetAccountId(accountId).SetDescription(newDescription).SetGossipCaCertificate("gossipCaCertificate".GetBytes()).SetServiceEndpoints([serviceEndpoint]).SetGossipEndpoints([gossipEndpoint]).SetDeclineReward(true).SetGrpcWebProxyEndpoint(grpcWebProxyEndpointUpdated).SetAdminKey(adminKey.GetPublicKey());
            try
            {
                nodeUpdateTransaction.Execute(client).GetReceipt(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var nodeDeleteTransaction = new NodeDeleteTransaction().SetNodeId(123);
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