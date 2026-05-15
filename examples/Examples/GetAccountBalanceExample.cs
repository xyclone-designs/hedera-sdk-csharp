// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to get balance of a Hedera account.
    /// </summary>
    public class GetAccountBalanceExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Get Account Balance Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            ///
            /// Because AccountBalanceQuery is a free query, we can make it without setting an operator on the client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK);

            // Attach logger to the SDK Client.
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            /// <summary>
            /// Step 1:
            /// Execute AccountBalanceQuery and output operator's account balance.
            /// </summary>
            Hbar operatorsBalance = new AccountBalanceQuery { AccountId = OPERATOR_ID }.Execute(client).Hbars;
            Console.WriteLine("Operator's Hbar account balance: " + operatorsBalance);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Get Account Balance Example Complete!");
        }
    }
}