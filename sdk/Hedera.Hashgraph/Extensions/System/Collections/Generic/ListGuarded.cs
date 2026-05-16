
namespace System.Collections.Generic
{
    public class ListGuarded<T> : IList<T>
    {
		public ListGuarded() : this(_ => { }) { }
		public ListGuarded(params T[] values) : this(_ => { }) 
		{
			_list = [.. values];
		}
		public ListGuarded(IEnumerable<T> values) : this(_ => { }) 
		{
            _list = [.. values];
        }
        public ListGuarded(Action<ListGuarded<T>> oninit)
		{
			OnRequireNotFrozen = () => { if (IsFrozen) throw new InvalidOperationException("Cannot operate on a frozen list"); }; 
			OnRequireNotLocked = () => { if (IsLocked) throw new InvalidOperationException("Cannot modify a locked list"); };

			oninit.Invoke(this);
		}

        private readonly List<T> _list = [];

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

		public Action OnRequireNotFrozen { get; internal set; }
        public Action OnRequireNotLocked { get; internal set; }
        public Action<T>? OnValidate { get; internal set; }

		public int Advance()
		{
			int index = Index;
			Index = (Index + 1) % _list.Count;
			return index;
		}

		public void Add(T item)
        {
            OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
            OnValidate?.Invoke(item);

			_list.Add(item);
        }
        public void Clear()
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			_list.Clear();
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }
        public void Insert(int index, T item)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			OnValidate?.Invoke(item);
			_list.Insert(index, item);
        }
        public bool Remove(T item)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			return _list.Remove(item);
        }
        public void RemoveAt(int index)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			_list.RemoveAt(index);
        }

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_list).GetEnumerator();
		}
		public IReadOnlyList<T> Read { get => _list.AsReadOnly(); }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(params T[] values)
        {
            foreach (T value in values) Add(value);
		}
        public void AddRange(IEnumerable<T> values)
        {
            foreach (T value in values) Add(value);
		}
		public void ClearAndSet(params T[] values)
        {
            ClearAndSet(values as IEnumerable<T>);
        }
        public void ClearAndSet(IEnumerable<T> values)
		{
			Clear();
			foreach (T value in values) Add(value);
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
		public int EnsureCapacity(int capacity)
		{
			return _list.EnsureCapacity(capacity);
		}
    }
}