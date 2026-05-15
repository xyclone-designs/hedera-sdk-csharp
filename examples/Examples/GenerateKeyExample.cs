// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Ethereum;

using System;

namespace Hedera.Hashgraph.Examples
{
    public class GenerateKeyExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Generate ECDSA key pair and EVM address example start");
            Console.WriteLine("Generating an ECDSA (secp256k1) private key...");
            PrivateKey privateKey = PrivateKey.GenerateECDSA();
            Console.WriteLine("Private key: " + privateKey);
            Console.WriteLine("Deriving the public key from the private key");
            PublicKey publicKey = privateKey.GetPublicKey();
            Console.WriteLine("Public key: " + publicKey);
            Console.WriteLine("Deriving the EVM address (last 20 bytes of Keccak-256 of the uncompressed public key)");
            EvmAddress evmAddress = publicKey.ToEvmAddress();
            Console.WriteLine("EVM address: 0x" + evmAddress);
            Console.WriteLine("Generate ECDSA key pair and EVM address example complete");
        }
    }
}