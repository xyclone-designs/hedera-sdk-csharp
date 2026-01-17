using Hedera.Hashgraph.Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
 * A list of keys that are required to sign in unison, with an optional threshold controlling how many keys of
 * the list are required.
 *
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/key">Hedera Documentation</a>
 */
    public sealed class KeyList : Key, IList<Key> {
        /**
         * The list of keys.
         */
        private readonly List<Key> Keys = [];

        

        /**
         * Create a new key list where all keys that are added will be required to sign.
         */
        public KeyList() {
            Threshold = null;
        }
        /**
         * Number of keys that need to sign.
         *
         * @param threshold                 the minimum number of keys that must sign
         */
        private KeyList(int threshold) {
			Threshold = threshold;
        }

		/**
         * The minimum number of keys that must sign.
         */
		public int? Threshold { get; set; }
        

        /**
         * List of keys in the key.
         *
         * @param keys                      the key / key list
         * @return                          a list of the keys
         */
        public static KeyList Of(params Key[] keys) {
			KeyList _keys = [];

            foreach (Key key in keys)
				_keys.Add(key);

            return _keys;
        }
        /**
         * Create a new key list where at least {@code threshold} keys must sign.
         *
         * @param threshold the minimum number of keys that must sign
         * @return KeyList
         */
        public static KeyList WithThreshold(int threshold) {
            return new KeyList(threshold);
        }
        /**
         * Create key list from protobuf.
         *
         * @param keyList                   the key list
         * @param threshold                 the minimum number of keys that must sign
         * @return                          the key list
         */
        public static KeyList FromProtobuf(Proto.KeyList keyList, int? threshold) {
            var keys = (threshold != null ? new KeyList(threshold) : new KeyList());
            for (var i = 0; i < keyList.getKeysCount(); ++i) {
                keys.Add(Key.FromProtobufKey(keyList.getKeys(i)));
            }

            return keys;
        }


        /**
         * Convert into protobuf representation.
         *
         * @return                          the protobuf representation
         */
        public Proto.KeyList ToProtobuf() {
            var keyList = Proto.KeyList.newBuilder();

            for (Key key : keys) {
                keyList.AddKeys(key.ToProtobufKey());
            }

            return keyList.build();
        }

		public override int GetHashCode()
		{
			return HashCode.Combine(Keys.GetHashCode(), Threshold ?? -1);
		}
		public override bool Equals(object? obj) {
            if (this == obj) return true;
            if (obj?.GetType() != typeof(KeyList)) return false;

            KeyList keyList = (KeyList) obj;

            if (keyList.Count() != Count()) return false;

			for (int i = 0; i < keyList.size(); i++) {
                if (!Arrays.equals(keyList.keys.get(i).toBytes(), keys.get(i).toBytes())) {
                    return false;
                }
            }

            return true;
        }
		public override Proto.Key ToProtobufKey()
		{
            return new Proto.KeyList

			var protoKeyList = .newBuilder();
			for (var key : keys)
			{
				protoKeyList.AddKeys(key.ToProtobufKey());
			}

			if (threshold != null)
			{
				return Proto.Key.newBuilder()
						.setThresholdKey(
								ThresholdKey.newBuilder().setThreshold(threshold).setKeys(protoKeyList))
						.build();
			}

			return Proto.Key.newBuilder()
					.setKeyList(protoKeyList)
					.build();
		}


		public int Count
		{
			get => ((ICollection<Key>)Keys).Count;
		}
		public bool IsReadOnly
		{
			get => ((ICollection<Key>)Keys).IsReadOnly;
		}
		public Key this[int index]
		{
			get => ((IList<Key>)Keys)[index];
			set => ((IList<Key>)Keys)[index] = value;
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