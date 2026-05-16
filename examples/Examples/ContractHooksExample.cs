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
    public class ContractHooksExample
    {
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        /// <summary>
        /// Operator's private key.
        /// </summary>
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "localhost";
        private static readonly string SDK_LOG_LEVEL = Environment.GetEnvironmentVariable("SDK_LOG_LEVEL") ?? "SILENT";
        public static void Main(string[] args)
        {
            Console.WriteLine("Contract Hooks Example Start!");
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
            /// Demonstrate creating a contract with hooks.
            /// Note: This may not work on all networks, so we'll show the concept
            /// and then demonstrate adding hooks to existing contracts.
            /// </summary>
            Console.WriteLine("\n=== Creating Contract with Hooks ===");
            ContractId contractWithHooksId = CreateContractWithHooks(client, contractId);
            /// <summary>
            /// Step 3:
            /// Demonstrate adding hooks to an existing contract.
            /// </summary>
            Console.WriteLine("\n=== Adding Hooks to Existing Contract ===");
            AddHooksToContract(client, contractId, contractWithHooksId);
            /// <summary>
            /// Step 4:
            /// Demonstrate hook deletion.
            /// </summary>
            Console.WriteLine("\n=== Deleting Hooks from Contract ===");
            DeleteHooksFromContract(client, contractWithHooksId);
            client.Dispose();
            Console.WriteLine("Contract Hooks Example Complete!");
        }

        private static ContractId CreateContractWithHooks(Client client, ContractId hookContractId)
        {
            Console.WriteLine("Creating contract with lambda EVM hook...");

            // Build a basic lambda EVM hook (no admin key, no storage updates) - like the integration test
            var lambdaHook = new EvmHook(hookContractId);
            var hookDetails = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 1, lambdaHook);
            var response = new ContractCreateTransaction
            {
                AdminKey = OPERATOR_KEY,
                Gas = 400000,
                BytecodeFileId = CreateBytecodeFile(client),
                HookCreationDetails_ = { hookDetails }
            
            }.Execute(client);
            var receipt = response.GetReceipt(client);
            ContractId contractId = receipt.ContractId;
            Console.WriteLine("Created contract with ID: " + contractId);
            Console.WriteLine("Successfully created contract with basic lambda hook!");
            return contractId;
        }

        /// <summary>
        /// Adds hooks to an existing contract.
        /// </summary>
        private static void AddHooksToContract(Client client, ContractId hookContractId, ContractId targetContractId)
        {
            Console.WriteLine("Adding hooks to existing contract...");
            Key adminKey = OPERATOR_KEY.GetPublicKey();

            // Hook 3: Basic lambda hook with no storage updates (using ID 3 to avoid conflict with existing hook 1)
            EvmHook basicHook = new EvmHook(hookContractId);
            HookCreationDetails hook3 = new HookCreationDetails(HookExtensionPoint.AccountAllowanceHook, 3, basicHook, adminKey);
            try
            {
                TransactionResponse contractUpdateResponse = new ContractUpdateTransaction 
                {
                    ContractId = targetContractId,
                    HookCreationDetails_ = { hook3 }

                }.FreezeWith(client).Sign(OPERATOR_KEY).Execute(client);
                contractUpdateResponse.GetReceipt(client);

                // Throws on failure; success if we reached here
                Console.WriteLine("Successfully added hooks to contract!");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute hook transaction: " + e.Message);
            }
        }

        /// <summary>
        /// Deletes hooks from a contract.
        /// </summary>
        private static void DeleteHooksFromContract(Client client, ContractId contractId)
        {
            Console.WriteLine("Deleting hooks from contract...");

            // Delete both hooks we created
            try
            {
                TransactionResponse deleteHookResponse = new ContractUpdateTransaction
                {
                    ContractId = contractId,
                    HookIdsToDelete = { 1, 3 }
                
                }.FreezeWith(client).Sign(OPERATOR_KEY).Execute(client);
                deleteHookResponse.GetReceipt(client);

                // Throws on failure; success if we reached here
                Console.WriteLine("Successfully deleted hooks with IDs: 1 and 3");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to execute hook deletion: " + e.Message);
            }
        }

        private static FileId CreateBytecodeFile(Client client)
        {
            string contractBytecodeHex = ContractHelper.GetBytecodeHex("contracts/hello_world/hello_world.json");
            var response = new FileCreateTransaction { Keys = [OPERATOR_KEY], Contents = Encoding.UTF8.GetBytes(contractBytecodeHex) }.Execute(client);
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