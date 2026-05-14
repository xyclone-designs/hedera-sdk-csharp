// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using System;

namespace Hedera.Hashgraph.Examples
{
    public class AccountCreationWaysExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Account Creation Ways Example Start!");
            /// <summary>
            /// Account ID:
            /// shard.realm.number format, i.e. 0.0.10 with the corresponding 0x000000000000000000000000000000000000000a ethereum address
            /// </summary>
            AccountId hederaFormat = AccountId.FromString("0.0.10");
            Console.WriteLine("Account ID: " + hederaFormat);
            Console.WriteLine("Account \"0.0.10\" corresponding long-zero address: " + hederaFormat.ToEvmAddress());
            /// <summary>
            /// Hedera Long-Form Account ID:
            /// 0.0.aliasPublicKey, i.e. 0.0.CIQNOWUYAGBLCCVX2VF75U6JMQDTUDXBOLZ5VJRDEWXQEGTI64DVCGQ
            /// </summary>
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            PublicKey publicKey = privateKey.GetPublicKey();

            // Assuming that the target shard and realm are known.
            // For now, they are virtually always 0.
            AccountId aliasAccountId = publicKey.ToAccountId(0, 0);
            Console.WriteLine("Hedera long-form account ID: " + aliasAccountId.ToString());
            /// <summary>
            /// Hedera Account Long-Zero address:
            /// 0x000000000000000000000000000000000000000a (for accountId 0.0.10)
            /// </summary>
            AccountId longZeroAddress = AccountId.FromString("0x000000000000000000000000000000000000000a");
            Console.WriteLine("Hedera account long-zero address: " + longZeroAddress);
            /// <summary>
            /// Ethereum Account Address or public-address:
            /// 0xb794f5ea0ba39494ce839613fffba74279579268
            /// </summary>
            AccountId evmAddress = AccountId.FromString("0xb794f5ea0ba39494ce839613fffba74279579268");
            Console.WriteLine("Ethereum account address or public address: " + evmAddress);
            Console.WriteLine("Account Creation Ways Example Complete!");
        }
    }
}