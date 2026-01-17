using System;
using System.Collections;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
    /**
     * Internal utility class for a new lockable list type.
     *
     * @param <T>                           the lockable list type
     */
    internal class LockableList<T> : IList<T>
    {
        private readonly List<T> List = [];
        private int Index = 0;
        private int IndexAdvance()
        {
			int index = Index;
			Index = (index + 1) % List.Count;
			return index;
		}
        private void LockedRequired()
		{
			if (Locked) throw new InvalidOperationException("Cannot modify a locked list");
		}


		public bool Locked { get; private set; }
		public int Count => ((ICollection<T>)List).Count;
		public bool IsReadOnly => ((ICollection<T>)List).IsReadOnly;

		public T Current { get => this[Index]; }
        public T Next { get => this[IndexAdvance()]; }
		public T this[int index] { get => ((IList<T>)List)[index]; set => ((IList<T>)List)[index] = value; }


        public void Add(T item)
		{
			LockedRequired();
			((ICollection<T>)List).Add(item);
        }
        public void Clear()
		{
			LockedRequired();
			((ICollection<T>)List).Clear();
        }
        public bool Contains(T item)
        {
            return ((ICollection<T>)List).Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            LockedRequired();
            ((ICollection<T>)List).CopyTo(array, arrayIndex);
        }
        public int IndexOf(T item)
        {
            return ((IList<T>)List).IndexOf(item);
        }
        public void Insert(int index, T item)
		{
			LockedRequired();
			((IList<T>)List).Insert(index, item);
        }
        public bool Remove(T item)
		{
			LockedRequired();
			return ((ICollection<T>)List).Remove(item);
        }
        public void RemoveAt(int index)
        {
            LockedRequired();
			((IList<T>)List).RemoveAt(index);
        }

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)List).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)List).GetEnumerator();
        }
    }
}