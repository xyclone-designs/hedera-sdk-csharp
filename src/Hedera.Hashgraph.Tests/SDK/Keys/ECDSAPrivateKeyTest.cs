// SPDX-License-Identifier: Apache-2.0
using System.Linq;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Utils;
using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Keys
{
    public class ECDSAPrivateKeyTest
    {
        private static readonly string TEST_VECTOR_PEM_PASSPHRASE = "asdasd123";
        public virtual void KeyGenerates()
        {
            PrivateKey key = PrivateKey.GenerateECDSA();
            Assert.NotNull(key);
            Assert.NotNull(key.ToBytes());
        }

        public virtual void KeySerialization()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            byte[] key1Bytes = key1.ToBytes();
            PrivateKey key2 = PrivateKey.FromBytes(key1Bytes);
            byte[] key2Bytes = key2.ToBytes();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
        }

        public virtual void KeySerialization2()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            byte[] key1Bytes = key1.ToBytesRaw();
            PrivateKey key2 = PrivateKey.FromBytesECDSA(key1Bytes);
            byte[] key2Bytes = key2.ToBytesRaw();

            // cannot use PrivateKey.fromBytes() to parse raw ECDSA bytes
            // because they're indistinguishable from ED25519 raw bytes
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
        }

        public virtual void KeySerialization3()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            byte[] key1Bytes = key1.ToBytesDER();
            PrivateKey key2 = PrivateKey.FromBytesDER(key1Bytes);
            byte[] key2Bytes = key2.ToBytesDER();
            PrivateKey key3 = PrivateKey.FromBytes(key1Bytes);
            byte[] key3Bytes = key3.ToBytesDER();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
            AssertThat(key3Bytes).ContainsExactly(key1Bytes);
        }

        public virtual void KeyStringification()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            string key1String = key1.ToString();
            PrivateKey key2 = PrivateKey.FromString(key1String);
            string key2String = key2.ToString();
            Assert.Equal(key2String, key1String);
        }

        public virtual void KeyStringification2()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            string key1String = key1.ToStringRaw();
            PrivateKey key2 = PrivateKey.FromStringECDSA(key1String);
            string key2String = key2.ToStringRaw();

            // cannot use PrivateKey.fromString() to parse raw ECDSA string
            // because it's indistinguishable from ED25519 raw string
            Assert.Equal(key2String, key1String);
        }

        public virtual void KeyStringification3()
        {
            PrivateKey key1 = PrivateKey.GenerateECDSA();
            string key1String = key1.ToStringDER();
            PrivateKey key2 = PrivateKey.FromStringDER(key1String);
            string key2String = key2.ToStringDER();
            PrivateKey key3 = PrivateKey.FromString(key1String);
            string key3String = key3.ToStringDER();
            Assert.Equal(key2String, key1String);
            Assert.Equal(key3String, key1String);
        }

        public virtual void KeyIsECDSA()
        {
            PrivateKey key = PrivateKey.GenerateECDSA();
            Assert.True(key.IsECDSA());
        }

        public virtual void KeyIsNotEd25519()
        {
            PrivateKey key = PrivateKey.GenerateECDSA();
            Assert.False(key.IsED25519());
        }

        // TODO: replace with HexFormat.of().parseHex when the required Java version is 17
        public static byte[] HexStringToByteArray(string s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte)((Character.Digit(s.ElementAt(i), 16) << 4) + Character.Digit(s.ElementAt(i + 1), 16));
            }

            return data;
        }

        public virtual void Slip10TestVector1()
        {

            // https://github.com/satoshilabs/slips/blob/master/slip-0010.md#test-vector-1-for-secp256k1
            string CHAIN_CODE1 = "873dff81c02f525623fd1fe5167eac3a55a049de3d314bb42ee227ffed37d508";
            string PRIVATE_KEY1 = "e8f32e723decf4051aefac8e2c93c9c5b214313817cdb01a1494b917c8436b35";
            string PUBLIC_KEY1 = "0339a36013301597daef41fbe593a02cc513d0b55527ec2df1050e2e8ff49c85c2";
            string CHAIN_CODE2 = "47fdacbd0f1097043b78c63c20c34ef4ed9a111d980047ad16282c7ae6236141";
            string PRIVATE_KEY2 = "edb2e14f9ee77d26dd93b4ecede8d16ed408ce149b6cd80b0715a2d911a0afea";
            string PUBLIC_KEY2 = "035a784662a4a20a65bf6aab9ae98a6c068a81c52e4b032c0fb5400c706cfccc56";
            string CHAIN_CODE3 = "2a7857631386ba23dacac34180dd1983734e444fdbf774041578e9b6adb37c19";
            string PRIVATE_KEY3 = "3c6cb8d0f6a264c91ea8b5030fadaa8e538b020f0a387421a12de9319dc93368";
            string PUBLIC_KEY3 = "03501e454bf00751f24b1b489aa925215d66af2234e3891c3b21a52bedb3cd711c";
            string CHAIN_CODE4 = "04466b9cc8e161e966409ca52986c584f07e9dc81f735db683c3ff6ec7b1503f";
            string PRIVATE_KEY4 = "cbce0d719ecf7431d88e6a89fa1483e02e35092af60c042b1df2ff59fa424dca";
            string PUBLIC_KEY4 = "0357bfe1e341d01c69fe5654309956cbea516822fba8a601743a012a7896ee8dc2";
            string CHAIN_CODE5 = "cfb71883f01676f587d023cc53a35bc7f88f724b1f8c2892ac1275ac822a3edd";
            string PRIVATE_KEY5 = "0f479245fb19a38a1954c5c7c0ebab2f9bdfd96a17563ef28a6a4b1a2a764ef4";
            string PUBLIC_KEY5 = "02e8445082a72f29b75ca48748a914df60622a609cacfce8ed0e35804560741d29";
            string CHAIN_CODE6 = "c783e67b921d2beb8f6b389cc646d7263b4145701dadd2161548a8b078e65e9e";
            string PRIVATE_KEY6 = "471b76e389e528d6de6d816857e012c5455051cad6660850e58372a6c3e6e7c8";
            string PUBLIC_KEY6 = "022a471424da5e657499d1ff51cb43c47481a03b1e77f951fe64cec9f5a48f7011";
            var seed = HexStringToByteArray("000102030405060708090a0b0c0d0e0f");

            // Chain m
            PrivateKey key1 = PrivateKey.FromSeedECDSAsecp256k1(seed);
            Assert.Equal(Hex.ToHexString(key1.GetChainCode().GetKey()), CHAIN_CODE1);
            Assert.Equal(key1.ToStringRaw(), PRIVATE_KEY1);
            AssertThat(key1.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY1);

            // Chain m/0'
            PrivateKey key2 = key1.Derive(Bip32Utils.ToHardenedIndex(0));
            Assert.Equal(Hex.ToHexString(key2.GetChainCode().GetKey()), CHAIN_CODE2);
            Assert.Equal(key2.ToStringRaw(), PRIVATE_KEY2);
            AssertThat(key2.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY2);

            // Chain m/0'/1
            PrivateKey key3 = key2.Derive(1);
            Assert.Equal(Hex.ToHexString(key3.GetChainCode().GetKey()), CHAIN_CODE3);
            Assert.Equal(key3.ToStringRaw(), PRIVATE_KEY3);
            AssertThat(key3.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY3);

            // Chain m/0'/1/2'
            PrivateKey key4 = key3.Derive(Bip32Utils.ToHardenedIndex(2));
            Assert.Equal(Hex.ToHexString(key4.GetChainCode().GetKey()), CHAIN_CODE4);
            Assert.Equal(key4.ToStringRaw(), PRIVATE_KEY4);
            AssertThat(key4.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY4);

            // Chain m/0'/1/2'/2
            PrivateKey key5 = key4.Derive(2);
            Assert.Equal(Hex.ToHexString(key5.GetChainCode().GetKey()), CHAIN_CODE5);
            Assert.Equal(key5.ToStringRaw(), PRIVATE_KEY5);
            AssertThat(key5.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY5);

            // Chain m/0'/1/2'/2/1000000000
            PrivateKey key6 = key5.Derive(1000000000);
            Assert.Equal(Hex.ToHexString(key6.GetChainCode().GetKey()), CHAIN_CODE6);
            Assert.Equal(key6.ToStringRaw(), PRIVATE_KEY6);
            AssertThat(key6.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY6);
        }

        public virtual void Slip10TestVector2()
        {

            // https://github.com/satoshilabs/slips/blob/master/slip-0010.md#test-vector-2-for-secp256k1
            string CHAIN_CODE1 = "60499f801b896d83179a4374aeb7822aaeaceaa0db1f85ee3e904c4defbd9689";
            string PRIVATE_KEY1 = "4b03d6fc340455b363f51020ad3ecca4f0850280cf436c70c727923f6db46c3e";
            string PUBLIC_KEY1 = "03cbcaa9c98c877a26977d00825c956a238e8dddfbd322cce4f74b0b5bd6ace4a7";
            string CHAIN_CODE2 = "f0909affaa7ee7abe5dd4e100598d4dc53cd709d5a5c2cac40e7412f232f7c9c";
            string PRIVATE_KEY2 = "abe74a98f6c7eabee0428f53798f0ab8aa1bd37873999041703c742f15ac7e1e";
            string PUBLIC_KEY2 = "02fc9e5af0ac8d9b3cecfe2a888e2117ba3d089d8585886c9c826b6b22a98d12ea";
            string CHAIN_CODE3 = "be17a268474a6bb9c61e1d720cf6215e2a88c5406c4aee7b38547f585c9a37d9";
            string PRIVATE_KEY3 = "877c779ad9687164e9c2f4f0f4ff0340814392330693ce95a58fe18fd52e6e93";
            string PUBLIC_KEY3 = "03c01e7425647bdefa82b12d9bad5e3e6865bee0502694b94ca58b666abc0a5c3b";
            string CHAIN_CODE4 = "f366f48f1ea9f2d1d3fe958c95ca84ea18e4c4ddb9366c336c927eb246fb38cb";
            string PRIVATE_KEY4 = "704addf544a06e5ee4bea37098463c23613da32020d604506da8c0518e1da4b7";
            string PUBLIC_KEY4 = "03a7d1d856deb74c508e05031f9895dab54626251b3806e16b4bd12e781a7df5b9";
            string CHAIN_CODE5 = "637807030d55d01f9a0cb3a7839515d796bd07706386a6eddf06cc29a65a0e29";
            string PRIVATE_KEY5 = "f1c7c871a54a804afe328b4c83a1c33b8e5ff48f5087273f04efa83b247d6a2d";
            string PUBLIC_KEY5 = "02d2b36900396c9282fa14628566582f206a5dd0bcc8d5e892611806cafb0301f0";
            string CHAIN_CODE6 = "9452b549be8cea3ecb7a84bec10dcfd94afe4d129ebfd3b3cb58eedf394ed271";
            string PRIVATE_KEY6 = "bb7d39bdb83ecf58f2fd82b6d918341cbef428661ef01ab97c28a4842125ac23";
            string PUBLIC_KEY6 = "024d902e1a2fc7a8755ab5b694c575fce742c48d9ff192e63df5193e4c7afe1f9c";
            var seed = HexStringToByteArray("fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542");

            // Chain m
            PrivateKey key1 = PrivateKey.FromSeedECDSAsecp256k1(seed);
            Assert.Equal(Hex.ToHexString(key1.GetChainCode().GetKey()), CHAIN_CODE1);
            Assert.Equal(key1.ToStringRaw(), PRIVATE_KEY1);
            AssertThat(key1.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY1);

            // Chain m/0
            PrivateKey key2 = key1.Derive(0);
            Assert.Equal(Hex.ToHexString(key2.GetChainCode().GetKey()), CHAIN_CODE2);
            Assert.Equal(key2.ToStringRaw(), PRIVATE_KEY2);
            AssertThat(key2.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY2);

            // Chain m/0/2147483647'
            PrivateKey key3 = key2.Derive(Bip32Utils.ToHardenedIndex(2147483647));
            Assert.Equal(Hex.ToHexString(key3.GetChainCode().GetKey()), CHAIN_CODE3);
            Assert.Equal(key3.ToStringRaw(), PRIVATE_KEY3);
            AssertThat(key3.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY3);

            // Chain m/0/2147483647'/1
            PrivateKey key4 = key3.Derive(1);
            Assert.Equal(Hex.ToHexString(key4.GetChainCode().GetKey()), CHAIN_CODE4);
            Assert.Equal(key4.ToStringRaw(), PRIVATE_KEY4);
            AssertThat(key4.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY4);

            // Chain m/0/2147483647'/1/2147483646'
            PrivateKey key5 = key4.Derive(Bip32Utils.ToHardenedIndex(2147483646));
            Assert.Equal(Hex.ToHexString(key5.GetChainCode().GetKey()), CHAIN_CODE5);
            Assert.Equal(key5.ToStringRaw(), PRIVATE_KEY5);
            AssertThat(key5.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY5);

            // Chain m/0/2147483647'/1/2147483646'/2
            PrivateKey key6 = key5.Derive(2);
            Assert.Equal(Hex.ToHexString(key6.GetChainCode().GetKey()), CHAIN_CODE6);
            Assert.Equal(key6.ToStringRaw(), PRIVATE_KEY6);
            AssertThat(key6.GetPublicKey().ToStringRaw()).IsSubstringOf(PUBLIC_KEY6);
        }

        public virtual void PEMImportTestVectors()
        {

            // https://github.com/hashgraph/hedera-sdk-reference/issues/93#issue-1665972122
            var PRIVATE_KEY_PEM1 = "-----BEGIN EC PRIVATE KEY-----\n" + "MHQCAQEEIG8I+jKi+iGVa7ttbfnlnML5AdvPugbgBWnseYjrle6qoAcGBSuBBAAK\n" + "oUQDQgAEqf5BmMeBzkU1Ra9UAbZJo3tytVOlb7erTc36LRLP20mOLU7+mFY+3Cfe\n" + "fAZgBtPXRAmDtRvYGODswAalW85GKA==\n" + "-----END EC PRIVATE KEY-----";
            var PRIVATE_KEY1 = "6f08fa32a2fa21956bbb6d6df9e59cc2f901dbcfba06e00569ec7988eb95eeaa";
            var PUBLIC_KEY1 = "02a9fe4198c781ce453545af5401b649a37b72b553a56fb7ab4dcdfa2d12cfdb49";
            var PRIVATE_KEY_PEM2 = "-----BEGIN EC PRIVATE KEY-----\n" + "MFQCAQEEIOHyhclwHbha3f281Kvd884rhBzltxGJxCZyaQCagH9joAcGBSuBBAAK\n" + "oSQDIgACREr6gFZa4K7hBP+bA25VdgQ+0ABFgM+g5RYw/W6T1Og=\n" + "-----END EC PRIVATE KEY-----";
            var PRIVATE_KEY2 = "e1f285c9701db85addfdbcd4abddf3ce2b841ce5b71189c4267269009a807f63";
            var PUBLIC_KEY2 = "02444afa80565ae0aee104ff9b036e5576043ed0004580cfa0e51630fd6e93d4e8";
            var PRIVATE_KEY_PEM3 = "-----BEGIN EC PRIVATE KEY-----\n" + "Proc-Type: 4,ENCRYPTED\n" + "DEK-Info: AES-128-CBC,0046A9EED8D16F0CAA66A197CE8BE8BD\n" + "\n" + "9VU9gReUmrn4XywjMx0F0A3oGzpHIksEXma72TCSdcxI7zHy0mtzuGq4Wd25O38s\n" + "H9c6kvhTPS1N/c6iNhx154B0HUoND8jvAvfxbGR/R87vpZJsOoKCmRxGqrxG8HER\n" + "FIHQ1jy16DrAbU95kDyLsiF1dy2vUY/HoqFZwxl/IVc=\n" + "-----END EC PRIVATE KEY-----";
            var PRIVATE_KEY3 = "cf49eb5206c1b0468854d6ea7b370590619625514f71ff93608a18465e4012ad";
            var PUBLIC_KEY3 = "025f0d14a7562d6319e5b8f91620d2ce9ad13d9abf21cfe9bd0a092c0f35bf1701";
            var PRIVATE_KEY_PEM4 = "-----BEGIN EC PRIVATE KEY-----\n" + "Proc-Type: 4,ENCRYPTED\n" + "DEK-Info: AES-128-CBC,4A9B3B987EC2EFFA405818327D14FFF7\n" + "\n" + "Wh756RkK5fn1Ke2denR1OYfqE9Kr4BXhgrEMTU/6o0SNhMULUhWGHrCWvmNeEQwp\n" + "ZVZYUxgYoTlJBeREzKAZithcvxIcTbQfLABo1NZbjA6YKqAqlGpM6owwL/f9e2ST\n" + "-----END EC PRIVATE KEY-----";
            var PRIVATE_KEY4 = "c0d3e16ba5a1abbeac4cd327a3c3c1cc10438431d0bac019054e573e67768bb5";
            var PUBLIC_KEY4 = "02065f736378134c53c7a2ee46f199fb93b9b32337be4e95660677046476995544";
            var ecdsaPrivateKey1 = PrivateKey.FromPem(PRIVATE_KEY_PEM1);
            Assert.Equal(ecdsaPrivateKey1.ToStringRaw(), PRIVATE_KEY1);
            Assert.Equal(ecdsaPrivateKey1.GetPublicKey().ToStringRaw(), PUBLIC_KEY1);
            var ecdsaPrivateKey2 = PrivateKey.FromPem(PRIVATE_KEY_PEM2);
            Assert.Equal(ecdsaPrivateKey2.ToStringRaw(), PRIVATE_KEY2);
            Assert.Equal(ecdsaPrivateKey2.GetPublicKey().ToStringRaw(), PUBLIC_KEY2);
            var ecdsaPrivateKey3 = PrivateKey.FromPem(PRIVATE_KEY_PEM3, TEST_VECTOR_PEM_PASSPHRASE);
            Assert.Equal(ecdsaPrivateKey3.ToStringRaw(), PRIVATE_KEY3);
            Assert.Equal(ecdsaPrivateKey3.GetPublicKey().ToStringRaw(), PUBLIC_KEY3);
            var ecdsaPrivateKey4 = PrivateKey.FromPem(PRIVATE_KEY_PEM4, TEST_VECTOR_PEM_PASSPHRASE);
            Assert.Equal(ecdsaPrivateKey4.ToStringRaw(), PRIVATE_KEY4);
            Assert.Equal(ecdsaPrivateKey4.GetPublicKey().ToStringRaw(), PUBLIC_KEY4);
        }

        public virtual void DERImportTestVectors()
        {

            // https://github.com/hashgraph/hedera-sdk-reference/issues/93#issue-1665972122
            var PRIVATE_KEY_DER1 = "3030020100300706052b8104000a042204208c2cdc9575fe67493443967d74958fd7808a3787fd3337e99cfeebbc7566b586";
            var PRIVATE_KEY1 = "8c2cdc9575fe67493443967d74958fd7808a3787fd3337e99cfeebbc7566b586";
            var PUBLIC_KEY1 = "028173079d2e996ef6b2d064fc82d5fc7094367211e28422bec50a2f75c365f5fd";
            var PRIVATE_KEY_DER2 = "30540201010420ac318ea8ff8d991ab2f16172b4738e74dc35a56681199cfb1c0cb2e7cb560ffda00706052b8104000aa124032200036843f5cb338bbb4cdb21b0da4ea739d910951d6e8a5f703d313efe31afe788f4";
            var PRIVATE_KEY2 = "ac318ea8ff8d991ab2f16172b4738e74dc35a56681199cfb1c0cb2e7cb560ffd";
            var PUBLIC_KEY2 = "036843f5cb338bbb4cdb21b0da4ea739d910951d6e8a5f703d313efe31afe788f4";
            var PRIVATE_KEY_DER3 = "307402010104208927647ad12b29646a1d051da8453462937bb2c813c6815cac6c0b720526ffc6a00706052b8104000aa14403420004aaac1c3ac1bea0245b8e00ce1e2018f9eab61b6331fbef7266f2287750a6597795f855ddcad2377e22259d1fcb4e0f1d35e8f2056300c15070bcbfce3759cc9d";
            var PRIVATE_KEY3 = "8927647ad12b29646a1d051da8453462937bb2c813c6815cac6c0b720526ffc6";
            var PUBLIC_KEY3 = "03aaac1c3ac1bea0245b8e00ce1e2018f9eab61b6331fbef7266f2287750a65977";
            var PRIVATE_KEY_DER4 = "302e0201010420a6170a6aa6389a5bd3a3a8f9375f57bd91aa7f7d8b8b46ce0b702e000a21a5fea00706052b8104000a";
            var PRIVATE_KEY4 = "a6170a6aa6389a5bd3a3a8f9375f57bd91aa7f7d8b8b46ce0b702e000a21a5fe";
            var PUBLIC_KEY4 = "03b69a75a5ddb1c0747e995d47555019e5d8a28003ab5202bd92f534361fb4ec8a";
            var ecdsaPrivateKey1 = PrivateKey.FromStringDER(PRIVATE_KEY_DER1);
            Assert.Equal(ecdsaPrivateKey1.ToStringRaw(), PRIVATE_KEY1);
            Assert.Equal(ecdsaPrivateKey1.GetPublicKey().ToStringRaw(), PUBLIC_KEY1);
            var ecdsaPrivateKey2 = PrivateKey.FromStringDER(PRIVATE_KEY_DER2);
            Assert.Equal(ecdsaPrivateKey2.ToStringRaw(), PRIVATE_KEY2);
            Assert.Equal(ecdsaPrivateKey2.GetPublicKey().ToStringRaw(), PUBLIC_KEY2);
            var ecdsaPrivateKey3 = PrivateKey.FromStringDER(PRIVATE_KEY_DER3);
            Assert.Equal(ecdsaPrivateKey3.ToStringRaw(), PRIVATE_KEY3);
            Assert.Equal(ecdsaPrivateKey3.GetPublicKey().ToStringRaw(), PUBLIC_KEY3);
            var ecdsaPrivateKey4 = PrivateKey.FromStringDER(PRIVATE_KEY_DER4);
            Assert.Equal(ecdsaPrivateKey4.ToStringRaw(), PRIVATE_KEY4);
            Assert.Equal(ecdsaPrivateKey4.GetPublicKey().ToStringRaw(), PUBLIC_KEY4);
        }
    }
}