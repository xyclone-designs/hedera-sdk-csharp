// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Transactions;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class AccountAliasExample
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
            Console.WriteLine("Account Alias Example (HIP-32) Start!");
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
            /// Create a couple of example Account Ids.
            ///
            /// Note that no queries or transactions have taken place yet.
            /// This account "creation" process is entirely local.
            ///
            /// AccountId.fromString() can construct an AccountId with an aliasKey.
            /// It expects a string of the form 0.0.123 in the case of a normal AccountId, or of the form
            /// 0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777
            /// in the case of an AccountId with aliasKey. Note the prefix of '0.0.' to indicate the shard and realm.
            ///
            /// If the shard and realm are known, you may use PublicKey.fromString().toAccountId() to construct the
            /// aliasKey AccountId.
            /// </summary>
            Console.WriteLine("\"Creating\" new account...");

            // Assuming that the target shard and realm are known.
            // For now, they are virtually always 0 and 0.
            AccountId aliasAccountId = publicKey.ToAccountId(0, 0);
            Console.WriteLine("New account ID: " + aliasAccountId);
            Console.WriteLine("Just the aliasKey: " + aliasAccountId.AliasKey);
            AccountId fromStringExample = AccountId.FromString("0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777");
            AccountId fromKeyStringExample = PublicKey.FromString("302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777").ToAccountId(0, 0);
            /// <summary>
            /// Step 3:
            /// Transfer Hbar to the new account.
            ///
            /// Transfer will actually create an actual Hedera account,
            /// deducting the creation fee from the amount transferred.
            /// </summary>
            Console.WriteLine("Transferring Hbar to the new account...");
            new TransferTransaction()
                .AddHbarTransfer(OPERATOR_ID, Hbar.From(1).Negated())
                .AddHbarTransfer(aliasAccountId, Hbar.From(1))
                .Execute(client)
                .GetReceipt(client);
            /// <summary>
            /// Step 4:
            /// Query and output info about the new account.
            ///
            /// Note that once an account exists in the ledger, it is assigned a normal AccountId, which can be retrieved
            /// via an AccountInfoQuery.
            ///
            /// Users may continue to refer to the account by its aliasKey AccountId, but they may also
            /// now refer to it by its normal AccountId
            /// </summary>
            AccountBalance newAccountBalance = new AccountBalanceQuery { AccountId = aliasAccountId }.Execute(client);
            Console.WriteLine("Balances of the new account: " + newAccountBalance);
            AccountInfo newAccountInfo = new AccountInfoQuery { AccountId = aliasAccountId }.Execute(client);

            Console.WriteLine("Info about the new account: " + newAccountInfo);
            Console.WriteLine("The normal account ID: " + newAccountInfo.AccountId);
            Console.WriteLine("The alias key: " + newAccountInfo.AliasKey);
            /// <summary>
            /// Clean up:
            /// Delete created account and close the client.
            /// </summary>
            new AccountDeleteTransaction
            {
                AccountId = newAccountInfo.AccountId,
                TransferAccountId = OPERATOR_ID,
            }
            .FreezeWith(client)
            .Sign(privateKey)
            .Execute(client)
            .GetReceipt(client);
            
            client.Dispose();

            Console.WriteLine("Account Alias Example (HIP-32) Complete!");
        }
    }
}