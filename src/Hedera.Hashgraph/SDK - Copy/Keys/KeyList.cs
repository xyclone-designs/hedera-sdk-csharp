// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// A list of keys that are required to sign in unison, with an optional threshold controlling how many keys of
    /// the list are required.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/key">Hedera Documentation</a>
    /// </summary>
    public sealed class KeyList : Key, Collection<Key>
    {
        /// <summary>
        /// The list of keys.
        /// </summary>
        private readonly IList<Key> keys = new ();
        /// <summary>
        /// The minimum number of keys that must sign.
        /// </summary>
        public int threshold;
        /// <summary>
        /// Create a new key list where all keys that are added will be required to sign.
        /// </summary>
        public KeyList()
        {
            threshold = null;
        }

        /// <summary>
        /// Number of keys that need to sign.
        /// </summary>
        /// <param name="threshold">the minimum number of keys that must sign</param>
        private KeyList(int threshold)
        {
            threshold = threshold;
        }

        /// <summary>
        /// List of keys in the key.
        /// </summary>
        /// <param name="keys">the key / key list</param>
        /// <returns>                         a list of the keys</returns>
        public static KeyList Of(params Key[] keys)
        {
            var list = new KeyList();
            foreach (var key in keys)
            {
                list.Add(key);
            }

            return list;
        }

        /// <summary>
        /// Create a new key list where at least {@code threshold} keys must sign.
        /// </summary>
        /// <param name="threshold">the minimum number of keys that must sign</param>
        /// <returns>KeyList</returns>
        public static KeyList WithThreshold(int threshold)
        {
            return new KeyList(threshold);
        }

        /// <summary>
        /// Create key list from protobuf.
        /// </summary>
        /// <param name="keyList">the key list</param>
        /// <param name="threshold">the minimum number of keys that must sign</param>
        /// <returns>                         the key list</returns>
        static KeyList FromProtobuf(Proto.KeyList keyList, int threshold)
        {
            var keys = (threshold != null ? new KeyList(threshold) : new KeyList());
            for (var i = 0; i < keyList.KeysCount(); ++i)
            {
                keys.Add(Key.FromProtobufKey(keyList.Keys(i)));
            }

            return keys;
        }

        /// <summary>
        /// Get the threshold for the KeyList.
        /// </summary>
        /// <returns>int</returns>
        public int GetThreshold()
        {
            return threshold;
        }

        /// <summary>
        /// Set a threshold for the KeyList.
        /// </summary>
        /// <param name="threshold">the minimum number of keys that must sign</param>
        /// <returns>KeyList</returns>
        public KeyList SetThreshold(int threshold)
        {
            threshold = threshold;
            return this;
        }

        public override int Size()
        {
            return keys.Count;
        }

        public override bool IsEmpty()
        {
            return keys.IsEmpty();
        }

        public override bool Contains(object o)
        {
            return keys.Contains(o);
        }

        public override IEnumerator<Key> Iterator()
        {
            return keys.Iterator();
        }

        public override Object[] ToArray()
        {
            return keys.ToArray();
        }

        public override T[] ToArray<T>(T[] ts)
        {

            // noinspection unchecked,SuspiciousToArrayCall
            return (T[])keys.ToArray((Key[])ts);
        }

        public override bool Add(Key key)
        {
            return keys.Add(key);
        }

        public override bool Remove(object o)
        {
            return keys.Remove(o);
        }

        public override bool ContainsAll(Collection<TWildcardTodo> collection)
        {
            return keys.ContainsAll(collection);
        }

        public override bool AddAll(Collection<TWildcardTodoKey> collection)
        {
            return keys.AddAll(collection);
        }

        public override bool RemoveAll(Collection<TWildcardTodo> collection)
        {
            return keys.RemoveAll(collection);
        }

        public override bool RetainAll(Collection<TWildcardTodo> collection)
        {
            return keys.RetainAll(collection);
        }

        public override void Clear()
        {
            keys.Clear();
        }

        override Proto.Key ToProtobufKey()
        {
            var protoKeyList = Proto.KeyList.NewBuilder();
            foreach (var key in keys)
            {
                protoKeyList.AddKeys(key.ToProtobufKey());
            }

            if (threshold != null)
            {
                return Proto.Key.NewBuilder().SetThresholdKey(ThresholdKey.NewBuilder().SetThreshold(threshold).SetKeys(protoKeyList)).Build();
            }

            return Proto.Key.NewBuilder().SetKeyList(protoKeyList).Build();
        }

        /// <summary>
        /// Convert into protobuf representation.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        Proto.KeyList ToProtobuf()
        {
            var keyList = Proto.KeyList.NewBuilder();
            foreach (Key key in keys)
            {
                keyList.AddKeys(key.ToProtobufKey());
            }

            return keyList.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("threshold", threshold).Add("keys", keys).ToString();
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is KeyList))
            {
                return false;
            }

            KeyList keyList = (KeyList)o;
            if (keyList.Count != Size())
            {
                return false;
            }

            for (int i = 0; i < keyList.Count; i++)
            {
                if (!Equals(keyList.keys[i].ToBytes(), keys[i].ToBytes()))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(keys.GetHashCode(), threshold != null ? threshold : -1);
        }
    }
}