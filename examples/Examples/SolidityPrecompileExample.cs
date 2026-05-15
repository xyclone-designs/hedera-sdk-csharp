// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Logging;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;
using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// This example just instantiates the solidity contract
    /// defined in resources/org/hiero/sdk/java/examples/contracts/precompile/PrecompileExample.sol, which has been
    /// compiled into resources/org/hiero/sdk/java/examples/contracts/precompile/PrecompileExample.json.
    /// 
    /// You should go look at that PrecompileExample.sol file, because that's where the meat of this example is.
    /// 
    /// This example uses the ContractHelper class (defined in ./ContractHelper.java) to declutter things.
    /// 
    /// When this example spits out a raw response code,
    /// you can look it up here: https://github.com/hashgraph/hedera-protobufs/blob/main/services/response_code.proto
    /// </summary>
    public class SolidityPrecompileExample
    {
        // see `.env.sample` in the repository root for how to specify these values
        // or set environment variables with the same names
        private static readonly AccountId OPERATOR_ID = AccountId.FromString(Environment.GetEnvironmentVariable("OPERATOR_ID"));
        private static readonly PrivateKey OPERATOR_KEY = PrivateKey.FromString(Environment.GetEnvironmentVariable("OPERATOR_KEY"));
        // HEDERA_NETWORK defaults to testnet if not specified in dotenv
        private static readonly string HEDERA_NETWORK = Environment.GetEnvironmentVariable("HEDERA_NETWORK") ?? "testnet";
        private SolidityPrecompileExample()
        {
        }

        public static void Main(string[] args)
        {
            Client client = ClientHelper.ForName(HEDERA_NETWORK);

            // Defaults the operator account ID and key such that all generated transactions will be paid for
            // by this account and be signed by this key
            client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);

            // Create a new account to use as the operator for subsequent transactions
            PrivateKey newOperatorPrivateKey = PrivateKey.GenerateED25519();
            PublicKey newOperatorPublicKey = newOperatorPrivateKey.GetPublicKey();
            AccountId newOperatorAccountId = new AccountCreateTransaction { InitialBalance = Hbar.FromTinybars(1000000), }
            .SetKeyWithoutAlias(newOperatorPublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;

            // Set the new account as the operator
            client.OperatorSet(newOperatorAccountId, newOperatorPrivateKey);
            PrivateKey alicePrivateKey = PrivateKey.GenerateED25519();
            PublicKey alicePublicKey = alicePrivateKey.GetPublicKey();
            AccountId aliceAccountId = new AccountCreateTransaction { InitialBalance = Hbar.FromTinybars(1000), }
            .SetKeyWithoutAlias(alicePublicKey)
            .Execute(client)
            .GetReceipt(client).AccountId;

            // Instantiate ContractHelper
            ContractHelper contractHelper = new ContractHelper("contracts/precompile/PrecompileExample.json", new ContractFunctionParameters().AddAddress(OPERATOR_ID.ToEvmAddress()).AddAddress(aliceAccountId.ToEvmAddress()), client);

            // Update the signer to have contractId KeyList (this is by security requirement)
            new AccountUpdateTransaction
            {
                AccountId = OPERATOR_ID,
                Key = KeyList.Of(1, OPERATOR_KEY.GetPublicKey(), contractHelper.ContractId),
            }
            .Execute(client)
            .GetReceipt(client);

            // Update the Alice account to have contractId KeyList (this is by security requirement)
            new AccountUpdateTransaction
            {
                AccountId = aliceAccountId,
                Key = KeyList.Of(1, alicePublicKey, contractHelper.ContractId),
            }
            .FreezeWith(client)
            .Sign(alicePrivateKey)
            .Execute(client)
            .GetReceipt(client);
            Action<string> additionalLogic = (tokenAddress) =>
            {
                try
                {
                    var tokenUpdateTransactionReceipt = new TokenUpdateTransaction
                    {
                        TokenId = TokenId.FromEvmAddress(0, 0, tokenAddress),
                        AdminKey = KeyList.Of(1, OPERATOR_KEY.GetPublicKey(), contractHelper.ContractId),
                        SupplyKey = KeyList.Of(1, OPERATOR_KEY.GetPublicKey(), contractHelper.ContractId),
                    }
                    .FreezeWith(client)
                    .Sign(alicePrivateKey)
                    .Execute(client)
                    .GetReceipt(client);
                    Console.WriteLine("Status of Token Update Transaction: " + tokenUpdateTransactionReceipt.Status);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message, e);
                }
            };

            // Configure steps in ContractHelper
            contractHelper
            .SetResultValidatorForStep(0, (contractFunctionResult) =>
            {
                Console.WriteLine("getPseudoRandomSeed() returned " + string.Join("; ", contractFunctionResult.GetBytes32(0)));

                return true;
            })
            .SetPayableAmountForStep(1, Hbar.From(20)).AddSignerForStep(3, alicePrivateKey).AddSignerForStep(5, alicePrivateKey)
            .SetParameterSupplierForStep(11, () =>
            {
                return new ContractFunctionParameters().AddBytes(alicePublicKey.ToBytesRaw());
            })
            .SetPayableAmountForStep(11, Hbar.From(40)).AddSignerForStep(11, alicePrivateKey)
            .SetStepLogic(11, additionalLogic).AddSignerForStep(12, alicePrivateKey)
            .SetParameterSupplierForStep(12, () =>
            {
                return new ContractFunctionParameters().AddBytesArray([[0x01b], [0x02b], [0x03b]]);

            }).AddSignerForStep(13, alicePrivateKey).AddSignerForStep(16, alicePrivateKey);

            // step 0 tests pseudo random number generator (PRNG)
            // step 1 creates a fungible token
            // step 2 mints it
            // step 3 associates Alice with it
            // step 4 transfers it to Alice.
            // step 5 approves an allowance of the fungible token with operator as the owner and Alice as the spender [NOT
            // WORKING]
            // steps 6 - 10 test misc functions on the fungible token (see PrecompileExample.sol for details).
            // step 11 creates an NFT token with a custom fee, and with the admin and supply set to Alice's key
            // step 12 mints some NFTs
            // step 13 associates Alice with the NFT token
            // step 14 transfers some NFTs to Alice
            // step 15 approves an NFT allowance with operator as the owner and Alice as the spender [NOT WORKING]
            // step 16 burn some NFTs
            contractHelper.ExecuteSteps(0, 16, client);
            Console.WriteLine("All steps completed with valid results.");
        }
    }
}