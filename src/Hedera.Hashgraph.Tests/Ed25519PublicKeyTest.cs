// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Java.Math;
using Java.Util;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class Ed25519PublicKeyTest
    {
        private static readonly string TEST_KEY_STR = "302a300506032b6570032100e0c8ec2758a5879ffac226a13c0c516b799e72e35141a0dd828f94d37988a4b7";
        private static readonly string TEST_KEY_STR_RAW = "e0c8ec2758a5879ffac226a13c0c516b799e72e35141a0dd828f94d37988a4b7";
        virtual void VerifyTransaction()
        {
            var transaction = new TransferTransaction().SetNodeAccountIds(Collections.SingletonList(new AccountId(0, 0, 3))).SetTransactionId(TransactionId.Generate(new AccountId(0, 0, 4))).Freeze();
            var key = PrivateKey.FromStringED25519("8776c6b831a1b61ac10dac0304a2843de4716f54b1919bb91a2685d0fe3f3048");
            key.SignTransaction(transaction);
            AssertThat(key.GetPublicKey().VerifyTransaction(transaction)).IsTrue();
        }

        virtual void KeyByteValidation()
        {
            byte[] invalidKeyED25519 = new byte[32];
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytes(invalidKeyED25519));
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesED25519(invalidKeyED25519));
            byte[] invalidKey = new byte[]
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
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => PublicKey.FromBytesED25519(invalidKey));
            byte[] malformedKey = new byte[]
            {
                0x00,
                0x01,
                0x02
            };
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => PublicKey.FromBytesED25519(malformedKey));
            byte[] validKey = PrivateKey.GenerateED25519().GetPublicKey().ToBytes();
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesED25519(validKey));
            byte[] validDERKey = PrivateKey.GenerateED25519().GetPublicKey().ToBytesDER();
            Assertions.AssertDoesNotThrow(() => PublicKey.FromBytesED25519(validDERKey));
        }

        virtual void KeyByteSerialization()
        {
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            byte[] key1Bytes = key1.ToBytes();
            PublicKey key2 = PublicKey.FromBytes(key1Bytes);
            byte[] key2Bytes = key2.ToBytes();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
        }

        virtual void KeyByteSerialization2()
        {
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            byte[] key1Bytes = key1.ToBytesRaw();
            PublicKey key2 = PublicKey.FromBytesED25519(key1Bytes);
            byte[] key2Bytes = key2.ToBytesRaw();
            PublicKey key3 = PublicKey.FromBytes(key1Bytes);
            byte[] key3Bytes = key3.ToBytesRaw();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
            AssertThat(key3Bytes).ContainsExactly(key1Bytes);
        }

        virtual void KeyByteSerialization3()
        {
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            byte[] key1Bytes = key1.ToBytesDER();
            PublicKey key2 = PublicKey.FromBytesDER(key1Bytes);
            byte[] key2Bytes = key2.ToBytesDER();
            PublicKey key3 = PublicKey.FromBytes(key1Bytes);
            byte[] key3Bytes = key3.ToBytesDER();
            AssertThat(key2Bytes).ContainsExactly(key1Bytes);
            AssertThat(key3Bytes).ContainsExactly(key1Bytes);
        }

        virtual void KeyByteSerializationThroughTransaction()
        {
            var senderAccount = AccountId.FromString("0.0.1337");
            var receiverAccount = AccountId.FromString("0.0.3");
            var transferAmount = Hbar.From(new BigDecimal("0.0001"), HbarUnit.HBAR);
            var privateKey = PrivateKey.GenerateED25519();
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
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            string key1Str = key1.ToString();
            PublicKey key2 = PublicKey.FromString(key1Str);
            string key2Str = key2.ToString();
            PublicKey key3 = PublicKey.FromString(key1Str);
            string key3Str = key3.ToString();
            Assert.Equal(key3.GetType(), typeof(PublicKeyED25519));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void KeyStringSerialization2()
        {
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            string key1Str = key1.ToStringRaw();
            PublicKey key2 = PublicKey.FromStringED25519(key1Str);
            string key2Str = key2.ToStringRaw();
            PublicKey key3 = PublicKey.FromString(key1Str);
            string key3Str = key3.ToStringRaw();
            Assert.Equal(key3.GetType(), typeof(PublicKeyED25519));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void KeyStringSerialization3()
        {
            PublicKey key1 = PrivateKey.GenerateED25519().GetPublicKey();
            string key1Str = key1.ToStringDER();
            PublicKey key2 = PublicKey.FromStringDER(key1Str);
            string key2Str = key2.ToStringDER();
            PublicKey key3 = PublicKey.FromString(key1Str);
            string key3Str = key3.ToStringDER();
            Assert.Equal(key3.GetType(), typeof(PublicKeyED25519));
            Assert.Equal(key2Str, key1Str);
            Assert.Equal(key3Str, key1Str);
        }

        virtual void ExternalKeyDeserialize(string keyStr)
        {
            PublicKey key = PublicKey.FromString(keyStr);
            AssertThat(key).IsNotNull();

            // the above are all the same key
            Assert.Equal(key.ToString(), TEST_KEY_STR);
            Assert.Equal(key.ToStringDER(), TEST_KEY_STR);
            Assert.Equal(key.ToStringRaw(), TEST_KEY_STR_RAW);
        }

        virtual void KeyToString()
        {
            PublicKey key = PublicKey.FromString(TEST_KEY_STR);
            AssertThat(key).IsNotNull();
            Assert.Equal(key.ToString(), TEST_KEY_STR);
        }

        virtual void KeyIsECDSA()
        {
            PublicKey key = PrivateKey.GenerateED25519().GetPublicKey();
            AssertThat(key.IsED25519()).IsTrue();
        }

        virtual void KeyIsNotEd25519()
        {
            PublicKey key = PrivateKey.GenerateED25519().GetPublicKey();
            AssertThat(key.IsECDSA()).IsFalse();
        }

        virtual void DERImportTestVectors()
        {

            // https://github.com/hashgraph/hedera-sdk-reference/issues/93#issue-1665972122
            var PUBLIC_KEY_DER1 = "302a300506032b65700321008ccd31b53d1835b467aac795dab19b274dd3b37e3daf12fcec6bc02bac87b53d";
            var PUBLIC_KEY1 = "8ccd31b53d1835b467aac795dab19b274dd3b37e3daf12fcec6bc02bac87b53d";
            var ed25519PublicKey1 = PublicKey.FromStringDER(PUBLIC_KEY_DER1);
            Assert.Equal(ed25519PublicKey1.ToStringRaw(), PUBLIC_KEY1);
        }
    }
}