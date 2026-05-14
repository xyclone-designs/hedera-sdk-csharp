// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class ScheduleNetworkUpdateExample
    {
        /// <summary>
        /// Operator's account ID.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Dotenv.Load()["OPERATOR_ID"]);
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Dotenv.Load()["OPERATOR_KEY"]);
        public static void Main(string[] args)
        {
            Console.WriteLine("Network Update Period Example Start!");
            /// <summary>
            /// Step 1: Initialize the client.
            /// Note: By default, the first network address book update will be executed now
            /// and subsequent updates will occur every 24 hours.
            /// This is controlled by network update period, which defaults to 24 hours.
            /// </summary>
            Client client = ClientHelper.ForName("testnet");
            _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
            Duration networkUpdateDuration = client.GetNetworkUpdatePeriod();
            Console.WriteLine("The current default network update period is: " + networkUpdateDuration.ToMinutes() + " minutes or " + networkUpdateDuration.ToHours() + " hour.");
            /// <summary>
            /// Step 2: Change network update period to 1 hour
            /// </summary>
            Console.WriteLine("Changing network update period to 1 hour...");
            client.SetNetworkUpdatePeriod(Duration.OfHours(1));
            networkUpdateDuration = client.GetNetworkUpdatePeriod();
            Console.WriteLine("The current network update period is: " + networkUpdateDuration.ToMinutes() + " minutes or " + networkUpdateDuration.ToHours() + " hours.");
            /// <summary>
            /// Step 3: Create client without scheduling network update
            /// </summary>
            Console.WriteLine("Creating client without scheduling network update...");

            // Define network nodes
            Dictionary<string, AccountId> network = new HashMap();
            network.Add("35.237.200.180:50211", AccountId.FromString("0.0.3"));
            network.Add("35.186.191.247:50211", AccountId.FromString("0.0.4"));
            network.Add("35.192.2.25:50211", AccountId.FromString("0.0.5"));
            network.Add("35.199.161.108:50211", AccountId.FromString("0.0.6"));
            network.Add("35.203.82.240:50211", AccountId.FromString("0.0.7"));
            network.Add("35.236.5.219:50211", AccountId.FromString("0.0.8"));
            network.Add("35.197.192.225:50211", AccountId.FromString("0.0.9"));
            network.Add("35.242.233.154:50211", AccountId.FromString("0.0.10"));
            network.Add("35.240.118.96:50211", AccountId.FromString("0.0.11"));
            network.Add("35.204.86.32:50211", AccountId.FromString("0.0.12"));

            // network schedule update is not set for custom network
            Client clientWithoutScheduling = Client.ForNetwork(network);
            clientWithoutScheduling.SetOperator(OPERATOR_ID, OPERATOR_KEY);
            Duration newUpdateDuration = clientWithoutScheduling.GetNetworkUpdatePeriod();
            if (newUpdateDuration == null)
            {
                Console.WriteLine("Network updates are disabled for this client.");
            }
            else
            {
                Console.WriteLine("The current network update period is: " + newUpdateDuration.ToMinutes() + " minutes or " + newUpdateDuration.ToHours() + " hours.");
            }


            // Clean up
            client.Dispose();
            clientWithoutScheduling.Dispose();
            Console.WriteLine("Network Update Period Example Complete!");
        }
    }
}