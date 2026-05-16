// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using System;

namespace Hedera.Hashgraph.Examples
{
    // Class access modifier should be default (simplicity and accessibility).
    public class ExampleTemplate
    {
        // UTIL VARIABLES BELOW
        private static readonly int TOTAL_MESSAGES = 5; // Example.
        // CONFIG VARIABLES BELOW
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
        // No constructor (for simplicity)
        // There should be only main method for simplicity
        // throws only Exception for simplicity
        public static void Main(string[] args)
        {
            Console.WriteLine("Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure SDK Client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                // All generated transactions will be paid by this account and signed by this key.
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                // Attach logger to the SDK Client.
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });

            // Steps with comments, for example:
            /// <summary>
            /// Step 1:
            /// Create an ECSDA private key.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            /// <summary>
            /// Step 2:
            /// Extract the ECDSA public key.
            /// </summary>
            PublicKey publicKey = privateKey.GetPublicKey();
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Example Complete!");
        }
    }
}