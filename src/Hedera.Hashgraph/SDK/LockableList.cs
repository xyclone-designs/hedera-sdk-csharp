// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility struct for a lockable list type, ported from Java.
    /// </summary>
    public class LockableList<T> : IEnumerable<T>
	{
		private readonly List<T> _list;
		private readonly int _index;
		private readonly bool _locked;

		public LockableList()
		{
			_list = [];
			_index = 0;
			_locked = false;
		}
		public LockableList(List<T> list, int index = 0, bool locked = false)
		{
			_list = list ?? throw new ArgumentNullException(nameof(list));
			_index = index;
			_locked = locked;
		}

		private void RequireNotLocked()
		{
			if (_locked) throw new InvalidOperationException("Cannot modify a locked list");
		}

		public bool IsEmpty => _list.Count == 0;
		public bool IsLocked => _locked;
		public int Size => _list.Count;
		public int Index { get; private set; }

		public List<T> GetList() => _list;
		public LockableList<T> EnsureCapacity(int capacity)
		{
			_list.EnsureCapacity(capacity);
			return this;
		}
		public LockableList<T> SetList(IEnumerable<T> newList)
		{
			RequireNotLocked();
			return new LockableList<T>(new List<T>(newList), 0, _locked);
		}
		public LockableList<T> Add(params T[] elements)
		{
			RequireNotLocked();
			_list.AddRange(elements);
			return this;
		}
		public LockableList<T> AddAll(IEnumerable<T> elements)
		{
			RequireNotLocked();
			_list.AddRange(elements);
			return this;
		}
		public LockableList<T> Shuffle()
		{
			RequireNotLocked();
			var rng = new Random();
			int n = _list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = _list[k];
				_list[k] = _list[n];
				_list[n] = value;
			}
			return this;
		}
		public LockableList<T> Remove(T element)
		{
			RequireNotLocked();
			_list.Remove(element);
			return this;
		}

		public T GetCurrent() => Get(_index);
		public T GetNext()
		{
			// Note: Structs are value types; to persist the index change 
			// across calls, you would need to return the new struct state.
			return Get(_index);
		}
		public T Get(int index) => _list[index];

		public LockableList<T> Set(int index, T item)
		{
			RequireNotLocked();
			if (index == _list.Count) _list.Add(item);
			else _list[index] = item;
			return this;
		}
		public LockableList<T> SetLocked(bool locked) => new(_list, _index, locked);
		public LockableList<T> SetIndex(int index) => new(_list, index, _locked);
		public LockableList<T> Clear()
		{
			RequireNotLocked();
			_list.Clear();
			return this;
		}

		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

}