// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class KeyListTest
    {
        private static readonly PublicKey mTestPublicKey1 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10").GetPublicKey();
        private static readonly PublicKey mTestPublicKey2 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey mTestPublicKey3 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        virtual void FromProtobuf()
        {

            // Given
            var protoKey1 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey1.ToBytes())).Build();
            var protoKey3 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey2.ToBytes())).Build();
            var protoKey2 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey3.ToBytes())).Build();
            var protoKeyList = com.hedera.hashgraph.sdk.proto.KeyList.NewBuilder().AddAllKeys(List.Of(protoKey1, protoKey2, protoKey3)).Build();

            // When
            var keyList = KeyList.FromProtobuf(protoKeyList, 3);

            // Then
            AssertTrue(keyList.Contains(mTestPublicKey1));
            AssertTrue(keyList.Contains(mTestPublicKey2));
            AssertTrue(keyList.Contains(mTestPublicKey3));
        }

        virtual void OfKeys()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // Then
            AssertTrue(keyList.Contains(mTestPublicKey1));
            AssertTrue(keyList.Contains(mTestPublicKey2));
            AssertTrue(keyList.Contains(mTestPublicKey3));
        }

        virtual void ToProtobufKey()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            var protoKey = keyList.ToProtobufKey();

            // Then
            Assert.Equal(protoKey.GetKeyList().GetKeysCount(), 3);
            Assert.Equal(protoKey.GetKeyList().GetKeys(0).GetEd25519().ToByteArray(), mTestPublicKey1.ToBytesRaw());
            Assert.Equal(protoKey.GetKeyList().GetKeys(1).GetEd25519().ToByteArray(), mTestPublicKey2.ToBytesRaw());
            Assert.Equal(protoKey.GetKeyList().GetKeys(2).GetEd25519().ToByteArray(), mTestPublicKey3.ToBytesRaw());
        }

        virtual void ToProtobuf()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            var protoKeyList = keyList.ToProtobuf();

            // Then
            Assert.Equal(protoKeyList.GetKeysCount(), 3);
            Assert.Equal(protoKeyList.GetKeys(0).GetEd25519().ToByteArray(), mTestPublicKey1.ToBytesRaw());
            Assert.Equal(protoKeyList.GetKeys(1).GetEd25519().ToByteArray(), mTestPublicKey2.ToBytesRaw());
            Assert.Equal(protoKeyList.GetKeys(2).GetEd25519().ToByteArray(), mTestPublicKey3.ToBytesRaw());
        }

        virtual void Size()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);
            var emptyKeyList = new KeyList();

            // Then
            AssertThat(keyList).HasSize(3);
            Assert.Empty(emptyKeyList);
        }

        virtual void Contains()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);
            var emptyKeyList = new KeyList();

            // Then
            AssertTrue(keyList.Contains(mTestPublicKey1));
            AssertTrue(keyList.Contains(mTestPublicKey2));
            AssertTrue(keyList.Contains(mTestPublicKey3));
            Assert.False(emptyKeyList.Contains(mTestPublicKey1));
            Assert.False(emptyKeyList.Contains(mTestPublicKey2));
            Assert.False(emptyKeyList.Contains(mTestPublicKey3));
        }

        virtual void Add()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2);

            // When
            keyList.Add(mTestPublicKey3);

            // Then
            AssertThat(keyList).HasSize(3);
            AssertTrue(keyList.Contains(mTestPublicKey3));
        }

        virtual void Remove()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            keyList.Remove(mTestPublicKey1);

            // Then
            AssertThat(keyList).HasSize(2);
            Assert.False(keyList.Contains(mTestPublicKey1));
            AssertTrue(keyList.Contains(mTestPublicKey2));
            AssertTrue(keyList.Contains(mTestPublicKey3));
        }

        virtual void Clear()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            keyList.Clear();

            // Then
            AssertTrue(keyList.IsEmpty());
        }
    }
}