// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Networking;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to get the network address book and then inspect node public keys, etc.
    /// </summary>
    public class GetAddressBookExample
    {
        /// <summary>
        /// See .env.sample in the examples folder root for how to specify values below
        /// or set environment variables with the same names.
        /// </summary>
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Get Address Book Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure the SDK Client.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK);

            // Attach logger to the SDK Client.
            //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL)));
            /// <summary>
            /// Step 1:
            /// Fetch the address book.
            /// Note: from Feb 25 2022 you can now fetch the address book for free from a mirror node with AddressBookQuery.
            /// </summary>
            Console.WriteLine("Getting address book for " + HEDERA_NETWORK + "...");
            NodeAddressBook addressBook = new AddressBookQuery { FileId = FileId.ADDRESS_BOOK }.Execute(client);
            Console.WriteLine("Address book for " + HEDERA_NETWORK + ": " + addressBook);
        }
    }
}