using System.Threading;

namespace System
{
    public sealed class AtomicReference<T>(T initialValue) where T : class
    {
        private T _value = initialValue;

        /// <summary>
        /// Returns the current value without memory fencing.
        /// Equivalent to Java's lazySet semantics if used carefully.
        /// </summary>
        public T Value
		{
			get => Get();
			set => Set(value);
		}

		/// <summary>
		/// Gets the current value (atomic read).
		/// </summary>
		public T Get()
        {
            return Volatile.Read(ref _value);
        }
		/// <summary>
		/// Atomically sets to newValue and returns the old value.
		/// </summary>
		public T GetAndSet(T newValue)
		{
			return Interlocked.Exchange(ref _value, newValue);
		}
		/// <summary>
		/// Sets the value (atomic write).
		/// </summary>
		public void Set(T newValue)
        {
            Volatile.Write(ref _value, newValue);
        }
        /// <summary>
        /// Atomically sets the value if it equals expected.
        /// Returns true if successful.
        /// </summary>
        public bool CompareAndSet(T expected, T update)
        {
            return Interlocked.CompareExchange(ref _value, update, expected) == expected;
        }
    }
}
