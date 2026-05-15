// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;

using System;

namespace Hedera.Hashgraph.Examples
{
    /// <summary>
    /// How to generate ED25519 key with mnemonic phrase.
    /// </summary>
    public class GenerateKeyWithMnemonicExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Generate ED25519 Key With Mnemonic Phrase Example Start!");
            Console.WriteLine("Generating random 24-word mnemonic from the BIP-39 standard English word list...");
            Mnemonic mnemonic24 = Mnemonic.Generate24();
            Console.WriteLine("Generated 24-word mnemonic: " + mnemonic24);
            Console.WriteLine("Recovering an ED25519 private key from the 24-word mnemonic phrase above...");
            PrivateKey privateKey24 = mnemonic24.ToStandardEd25519PrivateKey("", 0);
            Console.WriteLine("Recovered ED25519 private key: " + privateKey24);
            Console.WriteLine("Deriving a public key from the above private key...");
            PublicKey publicKey24 = privateKey24.GetPublicKey();
            Console.WriteLine("Public key: " + publicKey24);
            Console.WriteLine("---");
            Console.WriteLine("Generating random 12-word mnemonic from the BIP-39 standard English word list...");
            Mnemonic mnemonic12 = Mnemonic.Generate12();
            Console.WriteLine("Generated 12-word mnemonic: " + mnemonic12);
            Console.WriteLine("Recovering an ED25519 private key from the 12-word mnemonic phrase above...");
            PrivateKey privateKey12 = mnemonic12.ToStandardEd25519PrivateKey("", 0);
            Console.WriteLine("Recovered ED25519 private key: " + privateKey12);
            Console.WriteLine("Deriving a public key from the above private key...");
            PublicKey publicKey12 = privateKey12.GetPublicKey();
            Console.WriteLine("Public key: " + publicKey12);
            Console.WriteLine("Generate ED25519 Key With Mnemonic Phrase Example Complete!");
        }
    }
}