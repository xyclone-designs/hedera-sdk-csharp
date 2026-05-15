// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to get information about Hedera account.
    /// </summary>
    public class GetAccountInfoExample
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
            Console.WriteLine("Get Account Info Example Start!");
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
            /// Execute AccountBalanceQuery and output operator's account info.
            /// </summary>
            AccountInfo operatorsAccountInfo = new AccountInfoQuery
            {
                AccountId = OPERATOR_ID,
                MaxQueryPayment = Hbar.From(1),

            }.Execute(client);
            Console.WriteLine("Operator's account public key: " + operatorsAccountInfo.Key);
            Console.WriteLine("Operator's account require receiver signature: " + operatorsAccountInfo.IsReceiverSigRequired);
            Console.WriteLine("Operator's account expiration time: " + operatorsAccountInfo.ExpirationTime);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Get Account Info Example Complete!");
        }
    }
}