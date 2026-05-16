// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Logging;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class ValidateChecksumExample
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
            Console.WriteLine("Validate Checksum Example Start!");
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
            /// Read an input and validate the checksum (manual).
            /// </summary>
            Console.WriteLine("An example of manual checksum validation:");
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter an account ID with checksum: ");
                    string? inputString = Console.ReadLine();

                    // Throws IllegalArgumentException if incorrectly formatted.
                    AccountId accountId = AccountId.FromString(inputString);
                    Console.WriteLine("The account ID with no checksum is: " + accountId.ToString());
                    Console.WriteLine("The account ID with the correct checksum is: " + accountId.ToStringWithChecksum(client));
                    if (accountId.Checksum == null)
                    {
                        Console.WriteLine("You must enter a checksum.");
                        continue;
                    }

                    Console.WriteLine("The checksum entered was: " + accountId.Checksum);

                    // Throws BadEntityIdException if checksum is incorrect.
                    accountId.ValidateChecksum(client);
                    AccountBalance accountBalance = new AccountBalanceQuery { AccountId = accountId }.Execute(client);
                    Console.WriteLine("Account Balance: " + accountBalance);

                    // Exit the loop.
                    break;
                }
                catch (ArgumentException exc)
                {
                    Console.WriteLine(exc.Message);
                }
                catch (BadEntityIdException exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine("You entered " + exc.Shard + "." + exc.Realm + "." + exc.Num + "-" + exc.PresentChecksum + ", the expected checksum was " + exc.ExpectedChecksum);
                }
            }

            /// <summary>
            /// Step 2:
            /// Read an input and validate the checksum (auto).
            ///
            /// It is also possible to perform automatic checksum validation.
            ///
            /// Automatic checksum validation is disabled by default, but it can be enabled with
            /// client.setAutoValidateChecksums(true). You can check whether automatic checksum
            /// validation is enabled with client.isAutoValidateChecksumsEnabled().
            ///
            /// When this feature is enabled, the execute() method of a transaction or query
            /// will automatically check the validity of checksums on any IDs in the
            /// transaction or query.  It will throw an IllegalArgumentException if an
            /// invalid checksum is encountered.
            /// </summary>
            Console.WriteLine("An example of automatic checksum validation:");
            client.AutoValidateChecksums = true;
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter an account ID with checksum: ");
                    AccountId accountId = AccountId.FromString(Console.ReadLine());
                    if (accountId.Checksum == null)
                    {
                        Console.WriteLine("You must enter a checksum.");
                        continue;
                    }

                    AccountBalance accountBalance = new AccountBalanceQuery { AccountId = accountId }.Execute(client);
                    Console.WriteLine("Account Balance: " + accountBalance);

                    // Exit the loop.
                    break;
                }
                catch (ArgumentException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Validate Checksum Example Complete!");
        }
    }
}