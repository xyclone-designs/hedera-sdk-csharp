using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;

namespace Hedera.Hashgraph.Tests.SDK
{
    public static class KeyTestDataFactory
    {
        // Commonly used ED25519 test keys
        public static PrivateKey ED25519_TEST_KEY => PrivateKey.FromString(
            "302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");

        public static PrivateKey ED25519_TEST_KEY_2 => PrivateKey.FromString(
            "302e020100300506032b657004220420e1aa7a4206b0c0d3c0a0e93a0b8cce7af1bb8f39c97732394482538e10abcd");

        // Commonly used ECDSA test keys
        public static PrivateKey ECDSA_TEST_KEY => PrivateKey.FromStringECDSA(
            "7f109a9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d6");

        public static PrivateKey ECDSA_TEST_KEY_2 => PrivateKey.FromStringECDSA(
            "8f219b9e3b0d8ecfba9cc23a3614433ce0fa7ddcc80f2a8f10b222179a5a80d7");

        // Create a pair of keys for testing
        public static (PrivateKey, PrivateKey) CreateKeyPair()
        {
            return (ED25519_TEST_KEY, ECDSA_TEST_KEY);
        }

        // Create a pair of different keys
        public static (PrivateKey, PrivateKey) CreateKeyPair2()
        {
            return (ED25519_TEST_KEY_2, ECDSA_TEST_KEY_2);
        }

        public static PrivateKey CreateED25519Key()
        {
            return PrivateKey.Generate();
        }

        public static PrivateKey CreateECDSAKey()
        {
            return PrivateKey.GenerateECDSA();
        }

        // Helper to create a KeyList from test keys
        public static KeyList CreateTestKeyList(params PrivateKey[] keys)
        {
            var keyList = new KeyList();
            foreach (var key in keys)
            {
                keyList.Add(key.GetPublicKey());
            }
            return keyList;
        }
    }
}
