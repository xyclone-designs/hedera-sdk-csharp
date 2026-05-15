// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to get exchange rates info from the Hedera network.
    /// </summary>
    public class GetExchangeRatesExample
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
            Console.WriteLine("Get Exchange Rates Example Start!");
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
            /// Get contents of the file '0.0.112'. It is a system file, where exchange rate is stored.
            /// </summary>
            Console.WriteLine("Getting contents of the file `0.0.112`...");
            ByteString fileContentsByteString = new FileContentsQuery
            {
                FileId = FileId.FromString("0.0.112")

            }.Execute(client);
            /// <summary>
            /// Step 2:
            /// Parse file contents to an ExchangeRates object.
            /// </summary>
            byte[] fileContents = fileContentsByteString.ToByteArray();
            ExchangeRates exchangeRateSet = ExchangeRates.FromBytes(fileContents);
            /// <summary>
            /// Step 3:
            /// Print the info.
            /// </summary>
            Console.WriteLine("Current numerator: " + exchangeRateSet.CurrentRate.Cents);
            Console.WriteLine("Current denominator: " + exchangeRateSet.CurrentRate.Hbars);
            Console.WriteLine("Current expiration time: " + exchangeRateSet.CurrentRate.ExpirationTime.ToString());
            Console.WriteLine("Current Exchange Rate: " + exchangeRateSet.CurrentRate.ExchangeRateInCents);
            Console.WriteLine("Next numerator: " + exchangeRateSet.NextRate.Cents);
            Console.WriteLine("Next denominator: " + exchangeRateSet.NextRate.Hbars);
            Console.WriteLine("Next expiration time: " + exchangeRateSet.NextRate.ExpirationTime.ToString());
            Console.WriteLine("Next Exchange Rate: " + exchangeRateSet.NextRate.ExchangeRateInCents);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Get Exchange Rates Example Complete!");
        }
    }
}