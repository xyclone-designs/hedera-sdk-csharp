// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// A list of Keys that are required to sign in unison, with an optional threshold controlling how many Keys of
    /// the list are required.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/key">Hedera Documentation</a>
    /// </summary>
    public sealed class KeyList : Key, IList<Key>
    {
        public List<Key> Keys { get; init; } = [];

        /// <summary>
        /// List of Keys in the key.
        /// </summary>
        /// <param name="Keys">the key / key list</param>
        /// <returns>                         a list of the Keys</returns>
        public static KeyList Of(uint? threshold = null, params Key[] Keys)
        {
			return new KeyList
			{
				Threshold = threshold,
				Keys = [.. Keys]
			};
		}
        /// <summary>
        /// Create key list from protobuf.
        /// </summary>
        /// <param name="keyList">the key list</param>
        /// <param name="threshold">the minimum number of Keys that must sign</param>
        /// <returns>                         the key list</returns>
        public static KeyList FromProtobuf(Proto.KeyList keyList, uint? threshold)
        {
            return new KeyList
            {
                Threshold = threshold,
                Keys = [.. keyList.Keys.Select(_ => FromProtobufKey(_)).OfType<Key>()]
			};
        }

		public Key this[int index]
		{
			get => Keys[index];
			set => Keys[index] = value;
		}

		/// <summary>
		/// Get the threshold for the KeyList.
		/// The minimum number of Keys that must sign.
		/// </summary>
		public uint? Threshold { get; set; }
        public int Count { get => Keys.Count; }
        public bool IsReadOnly { get => false; }

		/// <summary>
		/// Convert into protobuf representation.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public Proto.KeyList ToProtobuf()
        {
			Proto.KeyList proto = new ();
			proto.Keys.AddRange(Keys.Select(_ => _.ToProtobufKey()));

			return proto;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Keys.GetHashCode(), (int?)Threshold ?? -1);
        }
		public override bool Equals(object? o)
		{
			if (this == o)
			{
				return true;
			}

			if (o is not KeyList)
			{
				return false;
			}

			KeyList keyList = (KeyList)o;

			if (keyList.Count != Count)
			{
				return false;
			}

			for (int i = 0; i < keyList.Count; i++)
			{
				if (!Equals(keyList.Keys[i].ToBytes(), Keys[i].ToBytes()))
				{
					return false;
				}
			}

			return true;
		}
		public override Proto.Key ToProtobufKey()
		{
			Proto.Key proto = new()
			{
				KeyList = new Proto.KeyList { }
			};

			proto.KeyList.Keys.AddRange(Keys.Select(_ => _.ToProtobufKey()));

			if (Threshold.HasValue)
			{
				proto.ThresholdKey = new Proto.ThresholdKey
				{
					Threshold = Threshold.Value,
					Keys = new Proto.KeyList { }
				};

				proto.ThresholdKey.Keys.Keys.AddRange(Keys.Select(_ => _.ToProtobufKey()));
			}

			return proto;
		}

        public int IndexOf(Key item)
        {
            return ((IList<Key>)Keys).IndexOf(item);
        }
        public void Insert(int index, Key item)
        {
            ((IList<Key>)Keys).Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            ((IList<Key>)Keys).RemoveAt(index);
        }
        public void Add(Key item)
        {
            ((ICollection<Key>)Keys).Add(item);
        }
        public void Clear()
        {
            ((ICollection<Key>)Keys).Clear();
        }
        public bool Contains(Key item)
        {
            return ((ICollection<Key>)Keys).Contains(item);
        }
        public void CopyTo(Key[] array, int arrayIndex)
        {
            ((ICollection<Key>)Keys).CopyTo(array, arrayIndex);
        }
        public bool Remove(Key item)
        {
            return ((ICollection<Key>)Keys).Remove(item);
        }

        public IEnumerator<Key> GetEnumerator()
        {
            return ((IEnumerable<Key>)Keys).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Keys).GetEnumerator();
        }
    }
}