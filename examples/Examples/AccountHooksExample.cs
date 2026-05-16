// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;

using System;
using System.Text;

namespace Hedera.Hashgraph.Examples
{
    public class AccountHooksExample
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
            Console.WriteLine("Account Hooks Example Start!");
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
            /// Create the hook contract.
            /// </summary>
            Console.WriteLine("Creating hook contract...");
            ContractId contractId = CreateContractId(client);
            Console.WriteLine("Hook contract created with ID: " + contractId);
            /// <summary>
            /// Step 2:
            /// Demonstrate creating an account with hooks.
            /// </summary>
            Console.WriteLine("\n=== Creating Account with Hooks ===");
            AccountWithKey accountWithKey = CreateAccountWithHooks(client, contractId);
            AccountId accountId = accountWithKey.AccountId;
            PrivateKey accountKey = accountWithKey.PrivateKey;
            /// <summary>
            /// Step 3:
            /// Demonstrate adding hooks to an existing account.
            /// </summary>
            Console.WriteLine("\n=== Adding Hooks to Existing Account ===");
            AddHooksToAccount(client, contractId, accountId, accountKey);
            /// <summary>
            /// Step 4:
            /// Demonstrate hook deletion.
            /// </summary>
            Console.WriteLine("\n=== Deleting Hooks from Account ===");
            DeleteHooksFromAccount(client, accountId, accountKey);
            client.Dispose();
            Console.WriteLine("Account Hooks Example Complete!");
        }

        /// <summary>
        /// Simple class to hold both account ID and private key.
        /// </summary>
        private class AccountWithKey
        {
            public readonly AccountId AccountId;
            public readonly PrivateKey PrivateKey;
            
            public AccountWithKey(AccountId accountId, PrivateKey privateKey)
            {
                AccountId = accountId;
                PrivateKey = privateKey;
            }
        }

        /// <summary>
        /// Creates an account with hooks from the start.
        /// </summary>
        private static AccountWithKey CreateAccountWithHooks(Client client, ContractId contractId)
        {
            Console.WriteLine("Creating account with lambda EVM hook...");
            EvmHook lambdaHook = new EvmHook(contractId);
            Key adminKey = OPERATOR_KEY.GetPublicKey();
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1002, lambdaHook, adminKey);
            PrivateKey accountKey = PrivateKey.GenerateED25519();
            PublicKey accountPublicKey = accountKey.GetPublicKey();
            try
            {
                TransactionResponse accountCreateResponse = new AccountCreateTransaction
                {
                    InitialBalance = Hbar.From(1),
                    HookCreationDetails = { hookDetails },

                }.SetKeyWithoutAlias(accountPublicKey).FreezeWith(client).Sign(accountKey).Execute(client);
                TransactionReceipt accountCreateReceipt = accountCreateResponse.GetReceipt(client);
                AccountId accountId = accountCreateReceipt.AccountId;

                Console.WriteLine("Created account with ID: " + accountId);
                Console.WriteLine("Successfully created account with lambda hook!");
                return new AccountWithKey(accountId, accountKey);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute account creation with hook: " + e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Adds hooks to an existing account.
        /// </summary>
        private static void AddHooksToAccount(Client client, ContractId contractId, AccountId accountId, PrivateKey accountKey)
        {
            Console.WriteLine("Adding hooks to existing account...");
            Key adminKey = OPERATOR_KEY.GetPublicKey();

            // Create basic lambda hooks with no storage updates
            EvmHook basicHook = new EvmHook(contractId);
            HookCreationDetails hook1 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, basicHook, adminKey);
            EvmHook basicHook2 = new EvmHook(contractId);
            HookCreationDetails hook2 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 2, basicHook2, adminKey);
            try
            {
                TransactionResponse accountUpdateResponse = new AccountUpdateTransaction
                {
                    AccountId = accountId,
                    HookCreationDetails =
                    [
                        hook1, hook2
                    ]
                }
                .FreezeWith(client)
                .Sign(accountKey)
                .Execute(client);
                TransactionReceipt accountUpdateReceipt = accountUpdateResponse.GetReceipt(client);

                // Throws on failure; success if we reached here
                Console.WriteLine("Successfully added hooks to account!");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute hook transaction: " + e.Message);
            }


            // Verify the hooks were added by querying account info
            AccountInfo accountInfo = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Account ID: " + accountInfo.AccountId);
            Console.WriteLine("Account balance: " + accountInfo.Balance);
        }

        /// <summary>
        /// Deletes hooks from an account.
        /// </summary>
        private static void DeleteHooksFromAccount(Client client, AccountId accountId, PrivateKey accountKey)
        {
            Console.WriteLine("Deleting hooks from account...");

            // Delete the basic hooks (no storage)
            try
            {
                TransactionResponse deleteHookResponse = new AccountUpdateTransaction
                {
                    AccountId = accountId,
                    HookIdsToDelete = [1, 2]
                }
                .FreezeWith(client)
                .Sign(accountKey)
                .Execute(client);
                deleteHookResponse.GetReceipt(client);

                // Throws on failure; success if we reached here
                Console.WriteLine("Successfully deleted hooks (IDs: 1, 2)");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute hook 1 deletion: " + e.Message);
            }
        }

        private static FileId CreateBytecodeFile(Client client)
        {
            string contractBytecodeHex = ContractHelper.GetBytecodeHex("contracts/hiero_hook/hiero_hook.json");
            var response = new FileCreateTransaction
            {
                Keys = [OPERATOR_KEY],
                Contents = Encoding.UTF8.GetBytes(contractBytecodeHex),
                MaxTransactionFee = Hbar.From(2)

            }.Execute(client);
            return response.GetReceipt(client).FileId;
        }

        private static ContractId CreateContractId(Client client)
        {
            var fileId = CreateBytecodeFile(client);
            var response = new ContractCreateTransaction { AdminKey = OPERATOR_KEY, Gas = 500000, BytecodeFileId = fileId }.Execute(client);
            var receipt = response.GetReceipt(client);
            return receipt.ContractId;
        }
    }
}