// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class InitializeClientWithMirrorNetworkExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        /// <summary>
        /// Operator's account ID. Used to sign and pay for operations on Hedera.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            /// </summary>
            Client client = Client.ForMirrorNetwork(["testnet.mirrornode.hedera.com:443"], 0, 0);

            // All generated transactions will be paid by this account and signed by this key.
            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);

            // Attach logger to the SDK Client.
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            /// <summary>
            /// Step 1:
            /// Generate ED25519 key pair.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...");
            PrivateKey privateKey = PrivateKey.GenerateED25519();
            /// <summary>
            /// Step 2:
            /// Create account
            /// </summary>
            AccountId aliceId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(5)
            }
                .SetKeyWithoutAlias(privateKey)
                .Execute(client)
                .GetReceipt(client).AccountId;
            Console.WriteLine("Alice's account ID: " + aliceId);
        }
    }
}