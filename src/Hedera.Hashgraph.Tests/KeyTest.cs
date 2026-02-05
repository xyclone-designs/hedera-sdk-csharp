// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.Key;
using Java.Nio.Charset.StandardCharsets;
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Java.Math;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class KeyTest
    {
        virtual void SignatureVerified()
        {
            var message = "Hello, World".GetBytes(UTF_8);
            var privateKey = PrivateKey.GenerateED25519();
            var publicKey = privateKey.GetPublicKey();
            var signature = privateKey.Sign(message);
            Assert.Equal(signature.Length, 64);
            AssertThat(publicKey.Verify(message, signature)).IsTrue();
        }

        virtual void SignatureVerifiedECDSA()
        {
            var message = "Hello, World".GetBytes(UTF_8);
            var privateKey = PrivateKey.GenerateECDSA();
            var publicKey = privateKey.GetPublicKey();
            var signature = privateKey.Sign(message);
            Assert.Equal(signature.Length, 64);
            AssertThat(publicKey.Verify(message, signature)).IsTrue();

            // muck with the signature a little and make sure it breaks
            signature[5] += 1;
            AssertThat(publicKey.Verify(message, signature)).IsFalse();
        }

        virtual void CalculateRecoveryIdECDSA()
        {
            var message = "Hello, World".GetBytes(UTF_8);
            var privateKey = PrivateKey.GenerateECDSA();
            var signature = privateKey.Sign(message);

            // wrap in signature object
            byte[] r = new byte[32];
            System.Arraycopy(signature, 0, r, 0, 32);
            byte[] s = new byte[32];
            System.Arraycopy(signature, 32, s, 0, 32);
            var recId = ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, message);
            AssertThat(recId).IsBetween(0, 1);
        }

        virtual void FailToCalculateRecoveryIdWithIllegalInputDataECDSA()
        {

            // create signature
            var message = "Hello, World".GetBytes(UTF_8);
            var privateKey = PrivateKey.GenerateECDSA();
            var signature = privateKey.Sign(message);
            byte[] r = new byte[32];
            System.Arraycopy(signature, 0, r, 0, 32);
            byte[] s = new byte[32];
            System.Arraycopy(signature, 32, s, 0, 32);

            // recover public key with recId > 1
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => Crypto.RecoverPublicKeyECDSAFromSignature(2, BigInteger.ONE, BigInteger.ONE, Crypto.CalcKeccak256(message)));

            // recover public key with negative 'r' or 's'
            AssertThatExceptionOfType(typeof(ArgumentException)).IsThrownBy(() => Crypto.RecoverPublicKeyECDSAFromSignature(0, BigInteger.ValueOf(-1), BigInteger.ONE, Crypto.CalcKeccak256(message)));

            // calculate recId with wrong message
            var wrongMessage = "Hello".GetBytes(UTF_8);
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, wrongMessage));
        }

        virtual void FromProtoKeyEd25519()
        {
            var keyBytes = Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011");
            var protoKey = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(keyBytes)).Build();
            var cut = PublicKey.FromProtobufKey(protoKey);
            Assert.Equal(cut.GetType(), typeof(PublicKeyED25519));
            AssertThat(cut.ToBytes()).ContainsExactly(keyBytes);
        }

        virtual void FromProtoKeyECDSA()
        {
            var keyProtobufBytes = Hex.Decode("3a21034e0441201f2bf9c7d9873c2a9dc3fd451f64b7c05e17e4d781d916e3a11dfd99");
            var protoKey = Key.ParseFrom(keyProtobufBytes);
            var cut = PublicKey.FromProtobufKey(protoKey);
            Assert.Equal(cut.GetType(), typeof(PublicKeyECDSA));
            AssertThat(((PublicKey)cut).ToProtobufKey().ToByteArray()).ContainsExactly(keyProtobufBytes);
        }

        virtual void FromProtoKeyKeyList()
        {

            // given
            var keyBytes = new byte[]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = KeyList.NewBuilder();
            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.AddKeys(Key.NewBuilder().SetEd25519(ByteString.CopyFrom(kb)));
            }

            var protoKey = Key.NewBuilder().SetKeyList(protoKeyList).Build();

            // when
            var cut = com.hedera.hashgraph.sdk.Key.FromProtobufKey(protoKey);

            // then
            Assert.Equal(cut.GetType(), typeof(com.hedera.hashgraph.sdk.KeyList));
            var keyList = (com.hedera.hashgraph.sdk.KeyList)cut;
            var actual = keyList.ToProtobufKey().GetKeyList();
            Assert.Equal(actual.GetKeysCount(), 2);
            AssertThat(actual.GetKeys(0).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[0]);
            AssertThat(actual.GetKeys(1).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[1]);
        }

        virtual void FromProtoKeyThresholdKey()
        {

            // given
            var keyBytes = new byte[]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = KeyList.NewBuilder();
            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.AddKeys(Key.NewBuilder().SetEd25519(ByteString.CopyFrom(kb)));
            }

            var protoThresholdKey = ThresholdKey.NewBuilder().SetThreshold(1).SetKeys(protoKeyList);
            var protoKey = Key.NewBuilder().SetThresholdKey(protoThresholdKey).Build();

            // when
            var cut = com.hedera.hashgraph.sdk.Key.FromProtobufKey(protoKey);

            // then
            Assert.Equal(cut.GetType(), typeof(com.hedera.hashgraph.sdk.KeyList));
            var thresholdKey = (com.hedera.hashgraph.sdk.KeyList)cut;
            var actual = thresholdKey.ToProtobufKey().GetThresholdKey();
            Assert.Equal(actual.GetThreshold(), 1);
            Assert.Equal(actual.GetKeys().GetKeysCount(), 2);
            AssertThat(actual.GetKeys().GetKeys(0).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[0]);
            AssertThat(actual.GetKeys().GetKeys(1).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[1]);
        }

        virtual void ThrowsUnsupportedKey()
        {
            byte[] keyBytes = new[]
            {
                0,
                1,
                2
            };
            var protoKey = Key.NewBuilder().SetRSA3072(ByteString.CopyFrom(keyBytes)).Build();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => com.hedera.hashgraph.sdk.Key.FromProtobufKey(protoKey));
        }

        virtual void KeyEquals()
        {
            var key1 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            var key2 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            Assert.Equal(key2.ToString(), key1.ToString());
            Assert.Equal(key2.GetPublicKey(), key1.GetPublicKey());
            AssertThat(key1.GetPublicKey()).IsNotEqualTo("random string");
        }

        virtual void KeyHash()
        {
            var key = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            AssertThatNoException().IsThrownBy(key.GetHashCode());
        }

        virtual void KeyListMethods()
        {
            var key1 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            var key2 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11");
            var key3 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12");
            var keyList = com.hedera.hashgraph.sdk.KeyList.WithThreshold(1);
            keyList.Add(key1);
            keyList.AddAll(List.Of(key2, key3));
            AssertThat(keyList.IsEmpty()).IsFalse();
            Assert.Equal(keyList.Count, 3);
            AssertThat(keyList).Contains(key1);
            AssertThat(keyList).Contains(key2);
            AssertThat(keyList).Contains(key3);
            var arr = keyList.ToArray();
            Assert.Equal(arr[0], key1);
            Assert.Equal(arr[1], key2);
            Assert.Equal(arr[2], key3);
            arr = new com.hedera.hashgraph.sdk.Key[]
            {
                null,
                null,
                null
            };
            keyList.ToArray(arr);
            Assert.Equal(arr[0], key1);
            Assert.Equal(arr[1], key2);
            Assert.Equal(arr[2], key3);
            keyList.Remove(key2);
            Assert.Equal(keyList.Count, 2);
            keyList.Clear();
            keyList.AddAll(List.Of(key1, key2, key3));
            Assert.Equal(keyList.Count, 3);
            keyList.RetainAll(List.Of(key2, key3));
            Assert.Equal(keyList.Count, 2);
            AssertThat(keyList).ContainsAll(List.Of(key2, key3));
            keyList.RemoveAll(List.Of(key2, key3));
            Assert.Empty(keyList);
        }

        virtual void FromBytesEd25519()
        {
            var keyBytes = Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011");
            var protoKey = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(keyBytes)).Build();
            var bytes = protoKey.ToByteArray();
            var cut = FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(PublicKeyED25519));
            AssertThat(cut.ToBytes()).ContainsExactly(keyBytes);
        }

        virtual void FromBytesECDSA()
        {
            var keyBytes = Hex.Decode("3a21034e0441201f2bf9c7d9873c2a9dc3fd451f64b7c05e17e4d781d916e3a11dfd99");
            var cut = FromBytes(keyBytes);
            Assert.Equal(cut.GetType(), typeof(PublicKeyECDSA));
            AssertThat(cut.ToProtobufKey().ToByteArray()).ContainsExactly(keyBytes);
        }

        virtual void FromBytesKeyList()
        {
            var keyBytes = new byte[]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = KeyList.NewBuilder();
            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.AddKeys(Key.NewBuilder().SetEd25519(ByteString.CopyFrom(kb)));
            }

            var protoKey = Key.NewBuilder().SetKeyList(protoKeyList).Build();
            var bytes = protoKey.ToByteArray();
            var cut = FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(com.hedera.hashgraph.sdk.KeyList));
            var keyList = (com.hedera.hashgraph.sdk.KeyList)cut;
            var actual = keyList.ToProtobufKey().GetKeyList();
            Assert.Equal(actual.GetKeysCount(), 2);
            AssertThat(actual.GetKeys(0).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[0]);
            AssertThat(actual.GetKeys(1).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[1]);
        }

        virtual void FromBytesThresholdKey()
        {
            var keyBytes = new byte[]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = KeyList.NewBuilder();
            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.AddKeys(Key.NewBuilder().SetEd25519(ByteString.CopyFrom(kb)));
            }

            var protoThresholdKey = ThresholdKey.NewBuilder().SetThreshold(1).SetKeys(protoKeyList);
            var protoKey = Key.NewBuilder().SetThresholdKey(protoThresholdKey).Build();
            var bytes = protoKey.ToByteArray();
            var cut = FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(com.hedera.hashgraph.sdk.KeyList));
            var thresholdKey = (com.hedera.hashgraph.sdk.KeyList)cut;
            var actual = thresholdKey.ToProtobufKey().GetThresholdKey();
            Assert.Equal(actual.GetThreshold(), 1);
            Assert.Equal(actual.GetKeys().GetKeysCount(), 2);
            AssertThat(actual.GetKeys().GetKeys(0).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[0]);
            AssertThat(actual.GetKeys().GetKeys(1).GetEd25519().ToByteArray()).ContainsExactly(keyBytes[1]);
        }

        virtual void ThrowsUnsupportedKeyFromBytes()
        {
            byte[] keyBytes = new[]
            {
                0,
                1,
                2
            };
            var protoKey = Key.NewBuilder().SetRSA3072(ByteString.CopyFrom(keyBytes)).Build();
            var bytes = protoKey.ToByteArray();
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => FromBytes(bytes));
        }
    }
}