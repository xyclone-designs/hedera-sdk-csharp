
namespace System.Collections.Generic
{
	public class ListGuarded<T> : IList<T> 
    {
		internal ListGuarded() : this(_ => { }) { }
        internal ListGuarded(Action<ListGuarded<T>> oninit)
		{
			OnRequireNotFrozen = () => { if (IsFrozen) throw new InvalidOperationException("Cannot operate on a frozen list"); }; 
			OnRequireNotLocked = () => { if (IsLocked) throw new InvalidOperationException("Cannot modify a locked list"); };

			oninit.Invoke(this);
		}

        private readonly List<T> list = [];

		public T this[int index] 
        {
            get
            {
                OnRequireNotFrozen?.Invoke();
				OnRequireNotLocked?.Invoke();
                return list[index];
            }
            set
            {
                OnRequireNotFrozen?.Invoke();
				OnRequireNotLocked?.Invoke();
				OnValidate?.Invoke(value);
				list[index] = value;
			}
        }

		public int Index { get; set; }
		public int Count { get => list.Count; }
        public bool IsReadOnly { get => false; }
		public bool IsFrozen { get; internal set; }
		public bool IsLocked { get; internal set; }
		public bool IsEmpty { get => list.Count == 0; }

		public T Current { get => list[Index]; }

		public Action OnRequireNotFrozen { get; internal set; }
        public Action OnRequireNotLocked { get; internal set; }
        public Action<T>? OnValidate { get; internal set; }

		public int Advance()
		{
			int index = Index;
			Index = (Index + 1) % list.Count;
			return index;
		}

		public void Add(T item)
        {
            OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
            OnValidate?.Invoke(item);

			list.Add(item);
        }
        public void Clear()
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			list.Clear();
        }
        public bool Contains(T item)
        {
            return list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }
        public void Insert(int index, T item)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			OnValidate?.Invoke(item);
			list.Insert(index, item);
        }
        public bool Remove(T item)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			return list.Remove(item);
        }
        public void RemoveAt(int index)
		{
			OnRequireNotFrozen?.Invoke();
			OnRequireNotLocked?.Invoke();
			list.RemoveAt(index);
        }

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)list).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }

		public IReadOnlyList<T> Read { get => list.AsReadOnly(); }

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

			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = rng.Next(i + 1);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}
		public int EnsureCapacity(int capacity)
		{
			return list.EnsureCapacity(capacity);
		}
	}
}
