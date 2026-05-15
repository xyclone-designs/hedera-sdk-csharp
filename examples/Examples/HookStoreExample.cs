// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class HookStoreExample
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
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "localhost";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Hook Store Example Start!");
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
            /// Set up prerequisites: Create hook contract and account with EVM hook.
            /// Note: This is not part of HookStoreTransaction itself, but required for the example.
            /// </summary>
            Console.WriteLine("Setting up prerequisites...");
            ContractId contractId = CreateContractId(client);
            AccountWithKey accountWithKey = CreateAccountWithLambdaHook(client, contractId);
            AccountId accountId = accountWithKey.AccountId;
            PrivateKey accountKey = accountWithKey.PrivateKey;
            /// <summary>
            /// Step 2:
            /// Demonstrate HookStoreTransaction - the core functionality.
            /// </summary>
            Console.WriteLine("\n=== HookStoreTransaction Example ===");

            // Create storage update (equivalent to TypeScript sample)
            byte[] storageKey = new byte[32];
            Arrays.Fill(storageKey, (byte)1);
            byte[] storageValue = new byte[32];
            Arrays.Fill(storageValue, (byte)200);
            EvmHookStorageUpdate storageUpdate = new EvmHookStorageSlot(storageKey, storageValue);

            // Create HookId for the existing hook (accountId with hook ID 1)
            HookId hookId = new HookId(new HookEntityId(accountId), 1);

            // Execute HookStoreTransaction (matches TypeScript pattern)
            TransactionResponse hookStoreResponse = new HookStoreTransaction().SetHookId(hookId).AddStorageUpdate(storageUpdate).FreezeWith(client).Sign(accountKey).Execute(client);
            hookStoreResponse.GetReceipt(client);
            Console.WriteLine("Successfully updated EVM hook storage!");
            client.Dispose();
            Console.WriteLine("Hook Store Example Complete!");
        }

        /// <summary>
        /// Simple class to hold both account ID and private key.
        /// </summary>
        private class AccountWithKey
        {
            readonly AccountId accountId;
            readonly PrivateKey privateKey;
            AccountWithKey(AccountId accountId, PrivateKey privateKey)
            {
                this.AccountId = accountId;
                this.privateKey = privateKey;
            }
        }

        private static AccountWithKey CreateAccountWithLambdaHook(Client client, ContractId contractId)
        {
            Console.WriteLine("Creating account with EVM hook");

            // Create EVM hook with initial storage updates
            EvmHook evmHook = new EvmHook(contractId);

            // Create hook creation details
            Key adminKey = OPERATOR_KEY.GetPublicKey();
            HookCreationDetails hookDetails = new HookCreationDetails(HookExtensionPoint.ACCOUNT_ALLOWANCE_HOOK, 1, evmHook, adminKey);

            // Create account with lambda hook
            PrivateKey accountKey = PrivateKey.GenerateED25519();
            PublicKey accountPublicKey = accountKey.GetPublicKey();
            try
            {
                TransactionResponse accountCreateResponse = new AccountCreateTransaction().SetKeyWithoutAlias(accountPublicKey).SetInitialBalance(Hbar.From(1)).AddHook(hookDetails).FreezeWith(client).Sign(accountKey).Execute(client);
                TransactionReceipt accountCreateReceipt = accountCreateResponse.GetReceipt(client);
                AccountId accountId = accountCreateReceipt.AccountId;
                accountId;
                Console.WriteLine("Created account with ID: " + accountId);
                Console.WriteLine("Successfully created account with EVM hook and initial storage!");
                return new AccountWithKey(accountId, accountKey);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute account creation with hook: " + e.Message);
                throw e;
            }
        }

        private static FileId CreateBytecodeFile(Client client)
        {
            string contractBytecodeHex = ContractHelper.GetBytecodeHex("contracts/hiero_hook/hiero_hook.json");
            var response = new FileCreateTransaction().SetKeys(OPERATOR_KEY).SetContents(contractBytecodeHex.GetBytes(StandardCharsets.UTF_8)).SetMaxTransactionFee(Hbar.From(2)).Execute(client);
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