// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;

using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class CreateAccountWithAliasExample
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
        public static void CreateAccountWithAlias(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Create an ECDSA private key.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            Console.WriteLine("\n--- Creating account with ECDSA key and derived alias ---");
            Console.WriteLine("ECDSA private key: " + privateKey);
            /// <summary>
            /// Step 2:
            /// Extract the ECDSA public key and generate EVM address.
            /// </summary>
            PublicKey publicKey = privateKey.GetPublicKey();
            EvmAddress evmAddress = publicKey.ToEvmAddress();
            Console.WriteLine("ECDSA public key: " + publicKey);
            Console.WriteLine("EVM address: " + evmAddress);
            /// <summary>
            /// Step 3:
            /// Create an account creation transaction with the key as an alias.
            /// Extract accountId from Transaction's receipt
            /// </summary>
            AccountId accountId = new AccountCreateTransaction().SetKeyWithAlias(privateKey).FreezeWith(client).Sign(privateKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 4:
            /// Query the account information to verify details.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Created account ID: " + accountId);
            Console.WriteLine("Account key: " + info.Key);
            Console.WriteLine("Initial EVM address: " + evmAddress + " is the same as " + info.ContractAccountId);
        }

        public static void CreateAccountWithBothKeys(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Generate separate ED25519 and ECDSA private keys.
            /// </summary>
            PrivateKey ed25519Key = PrivateKey.GenerateED25519();
            PrivateKey ecdsaKey = PrivateKey.GenerateECDSA();
            Console.WriteLine("\n--- Creating account with ED25519 account key and ECDSA alias key ---");
            Console.WriteLine("ED25519 key: " + ed25519Key);
            Console.WriteLine("ECDSA key: " + ecdsaKey);
            /// <summary>
            /// Step 2:
            /// Derive the EVM address from the ECDSA public key.
            /// </summary>
            EvmAddress evmAddress = ecdsaKey.GetPublicKey().ToEvmAddress();
            Console.WriteLine("EVM address: " + evmAddress);
            /// <summary>
            /// Step 3:
            /// Create an account creation transaction with both keys.
            /// It is required that transaction is signed with both keys
            /// Extract accountId from Transaction's receipt
            /// </summary>
            AccountId accountId = new AccountCreateTransaction().SetKeyWithAlias(ed25519Key, ecdsaKey).FreezeWith(client).Sign(ed25519Key).Sign(ecdsaKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 4:
            /// Query the account information to verify details.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Created account ID: " + accountId);
            Console.WriteLine("Account's key: " + info.Key + " is the same as " + ed25519Key.GetPublicKey());
            Console.WriteLine("Initial EVM address: " + evmAddress + " is the same as " + info.ContractAccountId);
        }

        public static void CreateAccountWithoutAlias(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Create a new ECDSA private key.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            Console.WriteLine("\n--- Creating account without alias ---");
            Console.WriteLine("ECDSA key: " + privateKey);
            /// <summary>
            /// Step 2:
            /// Create an account creation transaction without an alias.
            /// Extract accountId from Transaction's receipt
            /// </summary>
            AccountId accountId = new AccountCreateTransaction().SetKeyWithoutAlias(privateKey).FreezeWith(client).Sign(privateKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 3:
            /// Query the account information to verify details.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Created account ID: " + accountId);
            Console.WriteLine("Account's key: " + info.Key + " is the same as " + privateKey.GetPublicKey());
            Console.WriteLine("Account has no alias: " + IsZeroAddress(Hex.Decode(info.ContractAccountId)));
        }

        public static void CreateAccountWithPublicKeyAlias(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Create an ECDSA private key and derive the public key.
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            PublicKey publicKey = privateKey.GetPublicKey();
            Console.WriteLine("\n--- Creating account with public ECDSA key alias ---");
            Console.WriteLine("ECDSA private key: " + privateKey);
            Console.WriteLine("ECDSA public key: " + publicKey);
            /// <summary>
            /// Step 2:
            /// Generate the EVM address from the public key.
            /// </summary>
            EvmAddress evmAddress = publicKey.ToEvmAddress();
            Console.WriteLine("EVM address: " + evmAddress);
            /// <summary>
            /// Step 3:
            /// Create an account with the public key as an alias.
            /// The transaction must be signed with the corresponding private key.
            /// </summary>
            AccountId accountId = new AccountCreateTransaction().SetKeyWithAlias(publicKey).FreezeWith(client).Sign(privateKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 4:
            /// Query the account information to verify details.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Created account ID: " + accountId);
            Console.WriteLine("Account key: " + info.Key);
            Console.WriteLine("Initial EVM address: " + evmAddress + " is the same as " + info.ContractAccountId);
        }

        public static void CreateAccountWithSeparatePublicKeyAlias(Client client)
        {
            /// <summary>
            /// Step 1:
            /// Generate ED25519 account key and ECDSA key pair for alias.
            /// </summary>
            PrivateKey accountKey = PrivateKey.GenerateED25519();
            PrivateKey aliasPrivateKey = PrivateKey.GenerateECDSA();
            PublicKey aliasPublicKey = aliasPrivateKey.GetPublicKey();
            Console.WriteLine("\n--- Creating account with ED25519 key and separate ECDSA public key alias ---");
            Console.WriteLine("Account key (ED25519): " + accountKey);
            Console.WriteLine("Alias private key (ECDSA): " + aliasPrivateKey);
            Console.WriteLine("Alias public key (ECDSA): " + aliasPublicKey);
            /// <summary>
            /// Step 2:
            /// Derive the EVM address from the ECDSA public key.
            /// </summary>
            EvmAddress evmAddress = aliasPublicKey.ToEvmAddress();
            Console.WriteLine("EVM address: " + evmAddress);
            /// <summary>
            /// Step 3:
            /// Create an account with separate keys.
            /// The transaction must be signed with both the account key and the alias key.
            /// </summary>
            AccountId accountId = new AccountCreateTransaction().SetKeyWithAlias(accountKey, aliasPublicKey).FreezeWith(client).Sign(accountKey).Sign(aliasPrivateKey).Execute(client).GetReceipt(client).AccountId;
            /// <summary>
            /// Step 4:
            /// Query the account information to verify details.
            /// </summary>
            AccountInfo info = new AccountInfoQuery { AccountId = accountId }.Execute(client);
            Console.WriteLine("Created account ID: " + accountId);
            Console.WriteLine("Account's key: " + info.Key + " is the same as " + accountKey.GetPublicKey());
            Console.WriteLine("Initial EVM address: " + evmAddress + " is the same as " + info.ContractAccountId);
        }

        private static bool IsZeroAddress(byte[] address)
        {

            // Check if the first 12 bytes of the address are all zero
            for (int i = 0; i < 12; i++)
            {
                if (address[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Example Start!");
            /// <summary>
            /// Step 0:
            /// Create and configure SDK Client.
            /// </summary>
            Client client = CreateClient();
            /// <summary>
            /// Step 1:
            /// Demonstrate different account creation methods.
            /// </summary>
            CreateAccountWithAlias(client);
            CreateAccountWithBothKeys(client);
            CreateAccountWithoutAlias(client);
            CreateAccountWithPublicKeyAlias(client);
            CreateAccountWithSeparatePublicKeyAlias(client);
            /// <summary>
            /// Clean up:
            /// </summary>
            client.Dispose();
            Console.WriteLine("Example Complete!");
        }

        public static Client CreateClient()
        {
            /// <summary>
            /// Step 1:
            /// Create a client for the specified network.
            /// </summary>
            Client client = ClientHelper.ForName(HEDERA_NETWORK, _client =>
            {
                // All generated transactions will be paid by this account and signed by this key.
                _client.OperatorSet(OPERATOR_ID, OPERATOR_KEY);
                // Attach logger to the SDK Client.
                //_client.Logger = new Logger(Enum.Parse<LogLevel>(SDK_LOG_LEVEL));
            });
            return client;
        }
    }
}