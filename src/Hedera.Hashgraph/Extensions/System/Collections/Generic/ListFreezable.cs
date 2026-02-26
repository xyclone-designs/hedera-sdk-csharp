
namespace System.Collections.Generic
{
	public class ListFreezable<T> : IList<T> 
    {
        public ListFreezable() { }

        private readonly List<T> list = [];

        public T this[int index] 
        {
            get
            {
                Frozen?.Invoke();
                return list[index];
            }
            set
            {
                Frozen?.Invoke();
				Validate?.Invoke(value);
				list[index] = value;
			}
        }

        public int Count { get => list.Count; }
        public bool IsReadOnly { get => false; }

        public Action? Frozen { get; internal set; }
        public Action<T>? Validate { get; internal set; }

        public void Add(T item)
        {
            Frozen?.Invoke();
            Validate?.Invoke(item);

			list.Add(item);
        }
        public void Clear()
		{
			Frozen?.Invoke();
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
			Frozen?.Invoke();
			Validate?.Invoke(item);
			list.Insert(index, item);
        }
        public bool Remove(T item)
		{
			Frozen?.Invoke();
			return list.Remove(item);
        }
        public void RemoveAt(int index)
		{
			Frozen?.Invoke();
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
    }
}
