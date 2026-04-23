// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Cryptography
{
    /// <include file="KeyList.cs.xml" path='docs/member[@name="T:KeyList"]/*' />
    public sealed class KeyList : Key, IList<Key>
    {
        public List<Key> Keys { get; init; } = [];

        /// <include file="KeyList.cs.xml" path='docs/member[@name="M:KeyList.Of(System.UInt32,Key[])"]/*' />
        public static KeyList Of(uint? threshold = null, params Key[] Keys) 
        {
			return new KeyList
			{
				Threshold = threshold,
				Keys = [.. Keys]
			};
		}
        /// <include file="KeyList.cs.xml" path='docs/member[@name="M:KeyList.FromProtobuf(Proto.Services.KeyList,System.UInt32)"]/*' />
        public static KeyList FromProtobuf(Proto.Services.KeyList keyList, uint? threshold)
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

		/// <include file="KeyList.cs.xml" path='docs/member[@name="P:KeyList.Threshold"]/*' />
		public uint? Threshold { get; set; }
        public int Count { get => Keys.Count; }
        public bool IsReadOnly { get => false; }

		/// <include file="KeyList.cs.xml" path='docs/member[@name="M:KeyList.ToProtobuf"]/*' />
		public Proto.Services.KeyList ToProtobuf()
        {
			Proto.Services.KeyList proto = new ();
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
		public override Proto.Services.Key ToProtobufKey()
		{
			Proto.Services.Key proto = new()
			{
				KeyList = new Proto.Services.KeyList { }
			};

            proto.KeyList.Keys.AddRange(Keys.Select(_ => _.ToProtobufKey()));

			if (Threshold.HasValue)
			{
				proto.ThresholdKey = new Proto.Services.ThresholdKey
				{
					Threshold = Threshold.Value,
					Keys = new Proto.Services.KeyList { }
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
