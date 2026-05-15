// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class StakingExample
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
            Console.WriteLine("Staking Example Start!");
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
            /// Generate ED25519 key pair.
            /// </summary>
            Console.WriteLine("Generating ED25519 key pair...");
            PrivateKey privateKey = PrivateKey.GenerateED25519();
            PublicKey publicKey = privateKey.GetPublicKey();
            /// <summary>
            /// Step 2:
            /// Create an account and stake to an account ID.
            ///
            /// In this case we're staking to account ID 3 which happens to be
            /// the account ID of node 0, we're only doing this as an example.
            /// If you really want to stake to node 0, you should use setStakedNodeId() instead.
            /// </summary>
            Console.WriteLine("Creating new account with staked account ID...");
            AccountId stakedAccountId = AccountId.FromString("0.0.3");
            AccountId newAccountId = new AccountCreateTransaction
            {
                InitialBalance = Hbar.From(1),
                StakedAccountId = stakedAccountId,
            }
            .SetKeyWithoutAlias(publicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;
            Console.WriteLine("Created new account with ID: " + newAccountId);

            // Show the required key used to sign the account update transaction to
            // stake the accounts Hbar i.e. the fee payer key and key to authorize
            // changes to the account should be different.
            Console.WriteLine("Key required to update staking information: " + publicKey);
            Console.WriteLine("Fee payer or operator key: " + client.OperatorPublicKey);
            /// <summary>
            /// Step 3:
            /// Query the account info, it should show the staked account ID to be 0.0.3.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = newAccountId }.Execute(client);
            if (info.StakingInfo.StakedAccountId.Equals(stakedAccountId))
            {
                Console.WriteLine("New account staking info: " + info.StakingInfo);
            }
            else
            {
                throw new Exception("Staked account ID was not set correctly! (Fail)");
            }

            /// <summary>
            /// Clean up:
            /// Delete created account.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = newAccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            client.Dispose();
            Console.WriteLine("Staking Example Complete!");
        }
    }
}