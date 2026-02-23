using System.Threading;

namespace System
{
    public sealed class AtomicStruct<T>(T initialValue) where T : struct
    {
        private T _value = initialValue;

        public T Value
		{
			get => Get();
			set => Set(value);
		}

        public T Get()
        {
            return Interlocked.Read(ref _value);
        }
		public T GetAndSet(T newValue)
		{
			return Interlocked.Exchange(ref _value, newValue);
		}
		public T IncrementAndGet()
		{
			return Interlocked.Increment(ref _value);
		}
		public void Set(T newValue)
        {
			Interlocked.Write(ref _value, newValue);
        }
        public bool CompareAndSet(T expected, T update)
        {
            return Interlocked.CompareExchange(ref _value, update, expected).Equals(expected);
        }
	}
}
