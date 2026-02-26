
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace System.Collections.Generic
{
	public class DictionaryFreezable<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        public DictionaryFreezable() { }

        private readonly Dictionary<TKey, TValue> dictionary = [];

        public Action? Frozen { get; internal set; }
        public Action<TKey, TValue>? Validate { get; internal set; }

        public ICollection<TKey> Keys => dictionary.Keys;
        public ICollection<TValue> Values => dictionary.Values;
		public IReadOnlyDictionary<TKey, TValue> Read => dictionary.AsReadOnly();
		public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;

        public TValue this[TKey key]
        {
            get
            {
				Frozen?.Invoke();
				return dictionary[key];
            }
            set
            {
				Frozen?.Invoke();
				Validate?.Invoke(key, value);

				dictionary[key] = value;
			} 
        }

        public void Add(TKey key, TValue value)
        {
			Frozen?.Invoke();
			Validate?.Invoke(key, value);

			dictionary.Add(key, value);
        }
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }
        public bool Remove(TKey key)
        {
			Frozen?.Invoke();

			return dictionary.Remove(key);
        }
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
			Frozen?.Invoke();
			Validate?.Invoke(item.Key, item.Value);

			((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(item);
        }
        public void Clear()
        {
			Frozen?.Invoke();

			((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
			Frozen?.Invoke();

			return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dictionary).GetEnumerator();
        }
    }
}
