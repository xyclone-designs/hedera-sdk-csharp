// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Java.Math;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ECDSAPublicKeyTest
    {
        virtual void VerifyTransaction()
        {
            var transaction = new TransferTransaction().SetNodeAccountIds(Collections.SingletonList(new AccountId(0, 0, 3))).SetTransactionId(TransactionId.Generate(new AccountId(0, 0, 4))).Freeze();
            var key = PrivateKey.FromStringECDSA("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
            key.SignTransaction(transaction);
            AssertThat(key.GetPublicKey().VerifyTransaction(transaction)).IsTrue();
        }

        virtual void KeyByteSerialization()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            byte[] key1Bytes = key1.ToBytes();
            PublicKey key2 = PublicKey.FromBytes(key1Bytes);
            byte[] key2Bytes = key2.ToBytes();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
        }

        virtual void KeyByteSerialization2()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            byte[] key1Bytes = key1.ToBytesRaw();
            PublicKey key2 = PublicKey.FromBytesECDSA(key1Bytes);
            byte[] key2Bytes = key2.ToBytesRaw();

            // cannot use PrivateKey.fromBytes() to parse raw ECDSA bytes
            // because they're indistinguishable from ED25519 raw bytes
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
        }

        virtual void KeyByteValidation()
        {
            byte[] invalidKeyECDSA = new byte[33];
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytes(invalidKeyECDSA));
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesECDSA(invalidKeyECDSA));
            byte[] invalidCompressedKey = new byte[]
            {
                0x00,
                (byte)0xca,
                (byte)0x35,
                0x4b,
                0x7c,
                (byte)0xf4,
                (byte)0x87,
                (byte)0xd1,
                (byte)0xbc,
                0x43,
                0x5a,
                0x25,
                0x66,
                0x77,
                0x09,
                (byte)0xc1,
                (byte)0xab,
                (byte)0x98,
                0x0c,
                0x11,
                0x4d,
                0x35,
                (byte)0x94,
                (byte)0xe6,
                0x25,
                (byte)0x9e,
                (byte)0x81,
                0x2e,
                0x6a,
                0x70,
                0x3d,
                0x4f,
                0x51
            };
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => PublicKey.FromBytesECDSA(invalidCompressedKey));
            byte[] malformedKey = new byte[]
            {
                0x00,
                0x01,
                0x02
            };
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => PublicKey.FromBytesECDSA(malformedKey));
            byte[] validCompressedKey = new byte[]
            {
                0x02,
                (byte)0xca,
                (byte)0x35,
                0x4b,
                0x7c,
                (byte)0xf4,
                (byte)0x87,
                (byte)0xd1,
                (byte)0xbc,
                0x43,
                0x5a,
                0x25,
                0x66,
                0x77,
                0x09,
                (byte)0xc1,
                (byte)0xab,
                (byte)0x98,
                0x0c,
                0x1f,
                0x4d,
                0x35,
                (byte)0x94,
                (byte)0xe6,
                0x25,
                (byte)0x9e,
                (byte)0x81,
                0x2e,
                0x6a,
                0x75,
                0x3d,
                0x4f,
                0x59
            };
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesECDSA(validCompressedKey));
            byte[] validDERKey = PrivateKey.GenerateECDSA().GetPublicKey().ToBytesDER();
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesECDSA(validDERKey));
        }

        virtual void KeyByteSerialization3()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            byte[] key1Bytes = key1.ToBytesDER();
            PublicKey key2 = PublicKey.FromBytesDER(key1Bytes);
            byte[] key2Bytes = key2.ToBytesDER();
            PublicKey key3 = PublicKey.FromBytes(key1Bytes);
            byte[] key3Bytes = key3.ToBytesDER();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
            Assert.Equal(key3Bytes, key1Bytes);
        }

        virtual void KeyByteSerializationThroughTransaction()
        {
            var senderAccount = AccountId.FromString("0.0.1337");
            var receiverAccount = AccountId.FromString("0.0.3");
            var transferAmount = Hbar.From(new BigDecimal("0.0001"), HbarUnit.HBAR);
            var privateKey = PrivateKey.GenerateECDSA();
            var client = Client.ForTestnet().SetOperator(senderAccount, privateKey);
            var tx = new TransferTransaction().AddHbarTransfer(senderAccount, transferAmount.Negated()).AddHbarTransfer(receiverAccount, transferAmount);
            tx.FreezeWith(client);
            tx.SignWithOperator(client);
            var bytes = tx.ToBytes();
            AssertThatNoException().IsThrownBy(() => Transaction.FromBytes(bytes));
            Assert.NotEmpty(tx.GetSignatures());
        }

        virtual void KeyStringSerialization()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            string key1Str = key1.ToString();
            PublicKey key2 = PublicKey.FromString(key1Str);
            string key2Str = key2.ToString();
            PublicKey key3 = PublicKey.FromString(key1Str);
            string key3Str = key3.ToString();
            Assert.Equal(key3.GetType(), typeof(PublicKeyECDSA));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void KeyStringSerialization2()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            string key1Str = key1.ToStringRaw();
            PublicKey key2 = PublicKey.FromStringECDSA(key1Str);
            string key2Str = key2.ToStringRaw();
            PublicKey key3 = PublicKey.FromStringECDSA(key2Str);
            string key3Str = key3.ToStringRaw();

            // cannot use PublicKey.fromString() to parse raw ECDSA string
            // because it's indistinguishable from ED25519 raw bytes
            Assert.Equal(key3.GetType(), typeof(PublicKeyECDSA));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void KeyStringSerialization3()
        {
            PublicKey key1 = PrivateKey.GenerateECDSA().GetPublicKey();
            string key1Str = key1.ToStringDER();
            PublicKey key2 = PublicKey.FromStringDER(key1Str);
            string key2Str = key2.ToStringDER();
            PublicKey key3 = PublicKey.FromString(key1Str);
            string key3Str = key3.ToStringDER();
            Assert.Equal(key3.GetType(), typeof(PublicKeyECDSA));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void KeyIsECDSA()
        {
            PublicKey key = PrivateKey.GenerateECDSA().GetPublicKey();
            AssertThat(key.IsECDSA()).IsTrue();
        }

        virtual void KeyIsNotEd25519()
        {
            PublicKey key = PrivateKey.GenerateECDSA().GetPublicKey();
            AssertThat(key.IsED25519()).IsFalse();
        }

        virtual void ToEvmAddress()
        {

            // Generated by https://www.rfctools.com/ethereum-address-test-tool/
            string privateKeyString = "DEBAE3CA62AB3157110DBA79C8DE26540DC320EE9BE73A77D70BA175643A3500";
            string expectedEvmAddress = "d8eb8db03c699faa3f47adcdcd2ae91773b10f8b";
            PrivateKey privateKey = PrivateKey.FromStringECDSA(privateKeyString);
            PublicKey key = privateKey.GetPublicKey();
            AssertThat(key.ToEvmAddress()).HasToString(expectedEvmAddress);
        }

        virtual void DERImportTestVectors()
        {

            // https://github.com/hashgraph/hedera-sdk-reference/issues/93#issue-1665972122
            var PUBLIC_KEY_DER1 = "302d300706052b8104000a032200028173079d2e996ef6b2d064fc82d5fc7094367211e28422bec50a2f75c365f5fd";
            var PUBLIC_KEY1 = "028173079d2e996ef6b2d064fc82d5fc7094367211e28422bec50a2f75c365f5fd";
            var PUBLIC_KEY_DER2 = "3036301006072a8648ce3d020106052b8104000a032200036843f5cb338bbb4cdb21b0da4ea739d910951d6e8a5f703d313efe31afe788f4";
            var PUBLIC_KEY2 = "036843f5cb338bbb4cdb21b0da4ea739d910951d6e8a5f703d313efe31afe788f4";
            var PUBLIC_KEY_DER3 = "3056301006072a8648ce3d020106052b8104000a03420004aaac1c3ac1bea0245b8e00ce1e2018f9eab61b6331fbef7266f2287750a6597795f855ddcad2377e22259d1fcb4e0f1d35e8f2056300c15070bcbfce3759cc9d";
            var PUBLIC_KEY3 = "03aaac1c3ac1bea0245b8e00ce1e2018f9eab61b6331fbef7266f2287750a65977";
            var ecdsaPublicKey1 = PublicKey.FromStringDER(PUBLIC_KEY_DER1);
            Assert.Equal(ecdsaPublicKey1.ToStringRaw(), PUBLIC_KEY1);
            var ecdsaPublicKey2 = PublicKey.FromStringDER(PUBLIC_KEY_DER2);
            Assert.Equal(ecdsaPublicKey2.ToStringRaw(), PUBLIC_KEY2);
            var ecdsaPublicKey3 = PublicKey.FromStringDER(PUBLIC_KEY_DER3);
            Assert.Equal(ecdsaPublicKey3.ToStringRaw(), PUBLIC_KEY3);
        }
    }
}