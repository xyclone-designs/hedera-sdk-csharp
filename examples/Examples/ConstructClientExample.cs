// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.Examples
{
    public class ConstructClientExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        /// <summary>
        /// Path to .json config file. See resources/client-config.json
        /// </summary>
        private static readonly string CONFIG_FILE = Environment.GetEnvironmentVariable("CONFIG_FILE");
        private static readonly string HEDERA_NETWORK = "testnet";
        public static void Main(string[] args)
        {
            Console.WriteLine("Construct Client Example Start!");
            /// <summary>
            /// Here's the simplest way to construct a client.
            /// These clients' networks are filled with default lists of nodes that are baked into the SDK.
            /// Their operators are not yet set, and trying to use them now will result in exceptions.
            /// </summary>
            Client testnetClient = Client.ForTestnet();
            Client previewnetClient = Client.ForPreviewnet();
            Client mainnetClient = Client.ForMainnet();
            /// <summary>
            /// We can also construct a client for testnet, previewnet or mainnet depending on the value of a
            /// network name string. If, for example, the input string equals "testnet", this client will be
            /// configured to connect to testnet.
            /// </summary>
            Client namedNetworkClient = Client.ForName(HEDERA_NETWORK);

            // Let's set the operator on testnetClient.
            // (The AccountId and PrivateKey here are fake, this is just an example.)
            testnetClient.OperatorSet(AccountId.FromString("0.0.3"), PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10"));

            // Let's create a client with a custom network.
            Dictionary<string, AccountId> customNetwork = [];
            customNetwork.Add("2.testnet.hedera.com:50211", new AccountId(0, 0, 5));
            customNetwork.Add("3.testnet.hedera.com:50211", new AccountId(0, 0, 6));
            Client customClient = Client.ForNetwork(customNetwork);
            /// <summary>
            /// Since our customClient's network is in this case a subset of testnet, we should set the
            /// network's name to testnet. If we don't do this, checksum validation won't work.
            /// See ValidateChecksumExample. You can use customClient.getNetworkName()
            /// to check the network name. If not set, it will return null.
            /// If you attempt to validate a checksum against a client whose networkName is not set,
            /// an IllegalStateException will be thrown.
            /// </summary>
            customClient.SetNetworkName(NetworkName.TESTNET);
            /// <summary>
            /// Let's generate a client from a config.json file.
            /// A config file may specify a network by name, or it may provide a custom network
            /// in the form of a list of nodes.
            /// The config file should specify the operator, so you can use a client constructed
            /// using fromConfigFile() immediately.
            /// </summary>
            if (CONFIG_FILE != null)
            {
                Client configClient = Client.FromConfigFile(CONFIG_FILE);
                configClient.Dispose();
            }


            // Always close a client when you're done with it.
            testnetClient.Dispose();
            previewnetClient.Dispose();
            mainnetClient.Dispose();
            namedNetworkClient.Dispose();
            customClient.Dispose();
            Console.WriteLine("Construct Client Example Complete!");
        }
    }
}