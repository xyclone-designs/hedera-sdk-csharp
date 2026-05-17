using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
	public class ListGuarded<T> : IEnumerable<T>
	{
		public ListGuarded() : this(_ => { }) { }
		public ListGuarded(params T[] values) : this(_ => { })
		{
            _list = new ListInternal<T>(this);
            _list.AddRange(values);
        }
		public ListGuarded(IEnumerable<T> values) : this(_ => { })
		{
			_list = new ListInternal<T>(this);
			_list.AddRange(values);
        }
		public ListGuarded(Action<ListGuarded<T>> oninit)
		{
			OnRequireNotFrozen = () => { if (IsFrozen) throw new InvalidOperationException("Cannot operate on a frozen list"); };
			OnRequireNotLocked = () => { if (IsLocked) throw new InvalidOperationException("Cannot modify a locked list"); };

            _list = new ListInternal<T>(this);

            oninit.Invoke(this);
        }

		private ListInternal<T> _list;

        public T this[int index]
		{
			get => _list[index];
			set
			{
				OnRequireNotFrozen?.Invoke();
				OnRequireNotLocked?.Invoke();
				OnValidate?.Invoke(value);
				_list[index] = value;
			}
		}

		public int Index { get; set; }
		public int Count { get => _list.Count; }
		public bool IsReadOnly { get => false; }
		public bool IsFrozen { get; internal set; }
		public bool IsLocked { get; internal set; }
		public bool IsEmpty { get => _list.Count == 0; }

		public T Current { get => _list[Index]; }

        public IReadOnlyList<T> Read { get => _list.AsReadOnly(); }

        public Action OnRequireNotFrozen { get; internal set; }
		public Action OnRequireNotLocked { get; internal set; }
		public Action<T>? OnValidate { get; internal set; }

        public ListGuarded<T> Operate(Action<List<T>> list)
        {
            OnRequireNotFrozen?.Invoke();
            OnRequireNotLocked?.Invoke();

            list.Invoke(_list);

            if (OnValidate is not null)
                foreach (T item in _list)
                    OnValidate?.Invoke(item);

            _list = new ListInternal<T>(this, (List<T>)_list);

            return this;
        }
        public ListGuarded<T> Operate(Func<List<T>, IEnumerable<T>> list)
		{
            OnRequireNotFrozen?.Invoke();
            OnRequireNotLocked?.Invoke();

            List<T> items = [.. list.Invoke((List<T>)_list)];

            if (OnValidate is not null)
                foreach (T item in items)
                    OnValidate?.Invoke(item);

            _list = new ListInternal<T>(this, items);

            return this;
		}

        public void Clear()
        {
            _list.Clear();
        }
        public void Shuffle()
        {
            OnRequireNotFrozen?.Invoke();
            OnRequireNotLocked?.Invoke();

            var rng = Random.Shared;

            for (int i = _list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_list[i], _list[j]) = (_list[j], _list[i]);
            }
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public int Advance()
		{
			int index = Index;
			Index = (Index + 1) % _list.Count;
			return index;
		}
        public int EnsureCapacity(int capacity)
        {
            return _list.EnsureCapacity(capacity);
        }

        public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_list).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        public static implicit operator ListGuarded<T>(T item) => new(item);
        public static implicit operator ListGuarded<T>(T[] array) => new(array);
		public static implicit operator ListGuarded<T>(List<T> list) => new (list);
		public static implicit operator T[](ListGuarded<T> list) => [.. list];
		public static implicit operator List<T>(ListGuarded<T> list) => [.. list];

		private class ListInternal<TT> : List<TT>
		{
			public ListInternal(ListGuarded<TT> parent) : this(parent, []) { }
			public ListInternal(ListGuarded<TT> parent, params TT[] values) : this (parent, values as IEnumerable<TT>) { }
			public ListInternal(ListGuarded<TT> parent, IEnumerable<TT> values)
			{
				Parent = parent;

                AddRange(values);
			}

            public ListGuarded<TT> Parent { get; } 

            public new void Add(TT item)
            {
                Parent.OnRequireNotFrozen?.Invoke();
                Parent.OnRequireNotLocked?.Invoke();
                Parent.OnValidate?.Invoke(item);

                base.Add(item);
            }
            public new void Clear()
            {
                Parent.OnRequireNotFrozen?.Invoke();
                Parent.OnRequireNotLocked?.Invoke();
                
				base.Clear();
            }
            public new void CopyTo(TT[] array, int arrayIndex)
            {
                base.CopyTo(array, arrayIndex);
            }
            public new int IndexOf(TT item)
            {
                return base.IndexOf(item);
            }
            public new void Insert(int index, TT item)
            {
                Parent.OnRequireNotFrozen?.Invoke();
                Parent.OnRequireNotLocked?.Invoke();
                Parent.OnValidate?.Invoke(item);

                base.Insert(index, item);
            }
            public new bool Remove(TT item)
            {
                Parent.OnRequireNotFrozen?.Invoke();
                Parent.OnRequireNotLocked?.Invoke();
                
				return base.Remove(item);
            }
            public new void RemoveAt(int index)
            {
                Parent.OnRequireNotFrozen?.Invoke();
                Parent.OnRequireNotLocked?.Invoke();
                
				base.RemoveAt(index);
            }
        }
    }
}