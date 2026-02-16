// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Keys
{
    class KeyListTest
    {
        private static readonly PublicKey mTestPublicKey1 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10").GetPublicKey();
        private static readonly PublicKey mTestPublicKey2 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e11").GetPublicKey();
        private static readonly PublicKey mTestPublicKey3 = PrivateKey.FromStringED25519("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e12").GetPublicKey();
        public virtual void FromProtobuf()
        {

            // Given
            var protoKey1 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey1.ToBytes())).Build();
            var protoKey3 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey2.ToBytes())).Build();
            var protoKey2 = Key.NewBuilder().SetEd25519(ByteString.CopyFrom(mTestPublicKey3.ToBytes())).Build();
            var protoKeyList = Proto.KeyList.NewBuilder().AddAllKeys(List.Of(protoKey1, protoKey2, protoKey3)).Build();

            // When
            var keyList = KeyList.FromProtobuf(protoKeyList, 3);

            // Then
            Assert.True(keyList.Contains(mTestPublicKey1));
            Assert.True(keyList.Contains(mTestPublicKey2));
            Assert.True(keyList.Contains(mTestPublicKey3));
        }

        public virtual void OfKeys()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // Then
            Assert.True(keyList.Contains(mTestPublicKey1));
            Assert.True(keyList.Contains(mTestPublicKey2));
            Assert.True(keyList.Contains(mTestPublicKey3));
        }

        public virtual void ToProtobufKey()
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

        public virtual void ToProtobuf()
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

        public virtual void Size()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);
            var emptyKeyList = new KeyList();

            // Then
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.Empty(emptyKeyList);
        }

        public virtual void Contains()
        {

            // Given / When
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);
            var emptyKeyList = new KeyList();

            // Then
            Assert.True(keyList.Contains(mTestPublicKey1));
            Assert.True(keyList.Contains(mTestPublicKey2));
            Assert.True(keyList.Contains(mTestPublicKey3));
            Assert.False(emptyKeyList.Contains(mTestPublicKey1));
            Assert.False(emptyKeyList.Contains(mTestPublicKey2));
            Assert.False(emptyKeyList.Contains(mTestPublicKey3));
        }

        public virtual void Add()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2);

            // When
            keyList.Add(mTestPublicKey3);

            // Then
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.True(keyList.Contains(mTestPublicKey3));
        }

        public virtual void Remove()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            keyList.Remove(mTestPublicKey1);

            // Then
            Assert.Equal(2, tx.GetHbarTransfers().Count);
            Assert.False(keyList.Contains(mTestPublicKey1));
            Assert.True(keyList.Contains(mTestPublicKey2));
            Assert.True(keyList.Contains(mTestPublicKey3));
        }

        public virtual void Clear()
        {

            // Given
            var keyList = KeyList.Of(mTestPublicKey1, mTestPublicKey2, mTestPublicKey3);

            // When
            keyList.Clear();

            // Then
            Assert.True(keyList.Count == 0);
        }
    }
}