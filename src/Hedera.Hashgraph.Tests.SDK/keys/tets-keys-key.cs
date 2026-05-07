// SPDX-License-Identifier: Apache-2.0
using System;
using System.Linq;
using System.Text;

using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptography;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.Tests.SDK.Keys
{
    public class KeyTest
    {
        [Fact]
        public virtual void SignatureVerified()
        {
			var message = Encoding.UTF8.GetBytes("Hello, World");
			var privateKey = PrivateKey.GenerateED25519();
            var publicKey = privateKey.GetPublicKey();
            var signature = privateKey.Sign(message);
            Assert.Equal(signature.Length, 64);
            Assert.True(publicKey.Verify(message, signature));
        }
        [Fact]
        public virtual void SignatureVerifiedECDSA()
        {
            var message = Encoding.UTF8.GetBytes("Hello, World");
            var privateKey = PrivateKey.GenerateECDSA();
            var publicKey = privateKey.GetPublicKey();
            var signature = privateKey.Sign(message);
            Assert.Equal(signature.Length, 64);
            Assert.True(publicKey.Verify(message, signature));

            // muck with the signature a little and make sure it breaks
            signature[5] += 1;
            Assert.False(publicKey.Verify(message, signature));
        }
        [Fact]
        public virtual void CalculateRecoveryIdECDSA()
        {
			var message = Encoding.UTF8.GetBytes("Hello, World");
			var privateKey = PrivateKey.GenerateECDSA();
            var signature = privateKey.Sign(message);

            // wrap in signature object
            byte[] r = new byte[32];
            Array.Copy(signature, 0, r, 0, 32);
            byte[] s = new byte[32];
            Array.Copy(signature, 32, s, 0, 32);
            var recId = ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, message);
            Assert.True(recId >= 0 && recId <= 1);
        }
        [Fact]
        public virtual void FailToCalculateRecoveryIdWithIllegalInputDataECDSA()
        {

			// create signature
			var message = Encoding.UTF8.GetBytes("Hello, World");
			var privateKey = PrivateKey.GenerateECDSA();
            var signature = privateKey.Sign(message);
            byte[] r = new byte[32];
            Array.Copy(signature, 0, r, 0, 32);
            byte[] s = new byte[32];
            Array.Copy(signature, 32, s, 0, 32);

            // recover public key with recId > 1
            Assert.Throws<ArgumentException>(() => Crypto.RecoverPublicKeyECDSAFromSignature(2, BigInteger.One, BigInteger.One, Crypto.CalcKeccak256(message)));

            // recover public key with negative 'r' or 's'
            Assert.Throws<ArgumentException>(() => Crypto.RecoverPublicKeyECDSAFromSignature(0, BigInteger.ValueOf(-1), BigInteger.One, Crypto.CalcKeccak256(message)));

            // calculate recId with wrong message
            var wrongMessage = Encoding.UTF8.GetBytes("Hello");
			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ((PrivateKeyECDSA)privateKey).GetRecoveryId(r, s, wrongMessage));
        }
        [Fact]
        public virtual void FromProtoKeyEd25519()
        {
            var keyBytes = Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011");
            var protoKey = new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(keyBytes) };
            var cut = PublicKey.FromProtobufKey(protoKey);
            Assert.IsType<PublicKeyED25519>(cut);
            Assert.Same(cut.ToBytes(), keyBytes);
        }
        [Fact]
        public virtual void FromProtoKeyECDSA()
        {
            var keyProtobufBytes = Hex.Decode("3a21034e0441201f2bf9c7d9873c2a9dc3fd451f64b7c05e17e4d781d916e3a11dfd99");
            var protoKey = Proto.Services.Key.Parser.ParseFrom(keyProtobufBytes);
            var cut = PublicKey.FromProtobufKey(protoKey);
            Assert.IsType<PublicKeyECDSA>(cut);
            Assert.Same(((PublicKey)cut).ToProtobufKey().ToByteArray(), keyProtobufBytes);
        }
        [Fact]
        public virtual void FromProtoKeyKeyList()
        {
            // given
            var keyBytes = new byte[][]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = new Proto.Services.KeyList();
            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.Keys.Add(new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(kb) });
            }

            var protoKey = new Proto.Services.Key { KeyList = protoKeyList };

            // when
            var cut = Key.FromProtobufKey(protoKey);

            // then
            Assert.Equal(cut.GetType(), typeof(KeyList));
            var keyList = (KeyList)cut;
            var actual = keyList.ToProtobufKey().KeyList;
            Assert.Equal(actual.Keys.Count, 2);
            Assert.Same(actual.Keys[0].Ed25519.ToByteArray(), keyBytes[0]);
            Assert.Same(actual.Keys[1].Ed25519.ToByteArray(), keyBytes[1]);
        }
        [Fact]
        public virtual void FromProtoKeyThresholdKey()
        {
            // given
            var keyBytes = new byte[][]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = new Proto.Services.KeyList();

            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.Keys.Add(new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(kb) });
            }

            var protoThresholdKey = new Proto.Services.ThresholdKey { Threshold = 1, Keys = protoKeyList };
            var protoKey = new Proto.Services.Key { ThresholdKey = protoThresholdKey };

            // when
            var cut = Key.FromProtobufKey(protoKey);

            // then
            Assert.Equal(cut.GetType(), typeof(KeyList));
            var thresholdKey = (KeyList)cut;
            var actual = thresholdKey.ToProtobufKey().ThresholdKey;
            Assert.Equal(actual.Threshold, (uint)1);
            Assert.Equal(actual.Keys.Keys.Count, 2);
            Assert.Same(actual.Keys.Keys[0].Ed25519.ToByteArray(), keyBytes[0]);
            Assert.Same(actual.Keys.Keys[1].Ed25519.ToByteArray(), keyBytes[1]);
        }
        [Fact]
        public virtual void ThrowsUnsupportedKey()
        {
            byte[] keyBytes = new[]
            {
                (byte)0,
                (byte)1,
                (byte)2
            };
            var protoKey = new Proto.Services.Key { RSA3072 = ByteString.CopyFrom(keyBytes) };
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Key.FromProtobufKey(protoKey));
        }
        [Fact]
        public virtual void KeyEquals()
        {
            var key1 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            var key2 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            Assert.Equal(key2.ToString(), key1.ToString());
            Assert.Equal(key2.GetPublicKey(), key1.GetPublicKey());
            Assert.NotEqual(key1.GetPublicKey().ToString(), "random string");
            Assert.NotEqual(key1.GetPublicKey().ToStringDER(), "random string");
            Assert.NotEqual(key1.GetPublicKey().ToStringRaw(), "random string");
        }
        [Fact]
        public virtual void KeyHash()
        {
            var key = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            var _ = key.GetHashCode();
        }
        [Fact]
        public virtual void KeyListMethods()
        {
            var key1 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
            var key2 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11");
            var key3 = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12");
            var keyList = new KeyList { Threshold = 1 };
            keyList.Add(key1);
            keyList.Add(key2);
            keyList.Add(key3);
            Assert.False(keyList.Count == 0);
            Assert.Equal(keyList.Count, 3);
            Assert.True(keyList.Contains(key1));
            Assert.True(keyList.Contains(key2));
            Assert.True(keyList.Contains(key3));
            var arr = keyList.ToArray();
            Assert.Equal(arr[0], key1);
            Assert.Equal(arr[1], key2);
            Assert.Equal(arr[2], key3);
            arr = new Key[]
            {
                null,
                null,
                null
            };
            keyList.CopyTo(arr, 0);
            Assert.Equal(arr[0], key1);
            Assert.Equal(arr[1], key2);
            Assert.Equal(arr[2], key3);
            keyList.Remove(key2);
            Assert.Equal(keyList.Count, 2);
            keyList.Clear();
            keyList.Add(key1);
            keyList.Add(key2);
            keyList.Add(key3);
            Assert.Equal(keyList.Count, 3);
            keyList = [key2, key3];
            Assert.Equal(keyList.Count, 2);
            Assert.Equal(keyList.Keys, [key2, key3]);
            keyList.Remove(key2);
            keyList.Remove(key3);
            Assert.Empty(keyList);
        }
        [Fact]
        public virtual void FromBytesEd25519()
        {
            var keyBytes = Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011");
            var protoKey = new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(keyBytes) };
            var bytes = protoKey.ToByteArray();
            var cut = PublicKey.FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(PublicKeyED25519));
            Assert.Same(cut.ToBytes(), keyBytes);
        }
        [Fact]
        public virtual void FromBytesECDSA()
        {
            var keyBytes = Hex.Decode("3a21034e0441201f2bf9c7d9873c2a9dc3fd451f64b7c05e17e4d781d916e3a11dfd99");
            var cut = PublicKey.FromBytes(keyBytes);
            Assert.Equal(cut.GetType(), typeof(PublicKeyECDSA));
            Assert.Same(cut.ToProtobufKey().ToByteArray(), keyBytes);
        }
        [Fact]
        public virtual void FromBytesKeyList()
        {
            var keyBytes = new byte[][]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = new Proto.Services.KeyList();

            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.Keys.Add(new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(kb) });
            }

            var protoKey = new Proto.Services.Key { KeyList = protoKeyList };
            var bytes = protoKey.ToByteArray();
            var cut = KeyList.FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(KeyList));
            var keyList = (KeyList)cut;
            var actual = keyList.ToProtobufKey().KeyList;
            Assert.Equal(actual.Keys.Count, 2);
            Assert.Same(actual.Keys[0].Ed25519.ToByteArray(), keyBytes[0]);
            Assert.Same(actual.Keys[1].Ed25519.ToByteArray(), keyBytes[1]);
        }
        [Fact]
        public virtual void FromBytesThresholdKey()
        {
            var keyBytes = new byte[][]
            {
                Hex.Decode("0011223344556677889900112233445566778899001122334455667788990011"),
                Hex.Decode("aa11223344556677889900112233445566778899001122334455667788990011")
            };
            var protoKeyList = new Proto.Services.KeyList();

            foreach (byte[] kb in keyBytes)
            {
                protoKeyList.Keys.Add(new Proto.Services.Key { Ed25519 = ByteString.CopyFrom(kb) });
            }

            var protoThresholdKey =  new Proto.Services.ThresholdKey { Threshold = 1, Keys = new Proto.Services.KeyList { } };
            protoThresholdKey.Keys.Keys.AddRange(protoKeyList.Keys);

            var protoKey = new Proto.Services.Key { ThresholdKey = protoThresholdKey };
            var bytes = protoKey.ToByteArray();
            var cut = KeyList.FromBytes(bytes);
            Assert.Equal(cut.GetType(), typeof(KeyList));
            var thresholdKey = (KeyList)cut;
            var actual = thresholdKey.ToProtobufKey().ThresholdKey;
            Assert.Equal(actual.Threshold, (uint)1);
            Assert.Equal(actual.Keys.Keys.Count, 2);
            Assert.Same(actual.Keys.Keys[0].Ed25519.ToByteArray(), keyBytes[0]);
            Assert.Same(actual.Keys.Keys[1].Ed25519.ToByteArray(), keyBytes[1]);
        }
        [Fact]
        public virtual void ThrowsUnsupportedKeyFromBytes()
        {
            byte[] keyBytes = [0, 1, 2];
            var protoKey = new Proto.Services.Key
            {
				RSA3072 = ByteString.CopyFrom(keyBytes)
			};
            var bytes = protoKey.ToByteArray();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Key.FromBytes(bytes));
        }
    }
}