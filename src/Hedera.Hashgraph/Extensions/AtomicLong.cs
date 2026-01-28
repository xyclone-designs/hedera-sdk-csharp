using System.Threading;

namespace Hedera.Hashgraph.SDK
{
	// TODO if need to be done
	public class AtomicLong
	{
		private long _value;
		public long Get() => Interlocked.Read(ref _value);
		public void Set(long value) => Interlocked.Exchange(ref _value, value);
		public long IncrementAndGet() => Interlocked.Increment(ref _value);
		public bool CompareAndSet(long expect, long update)
		{
			// Returns the original value. If it matches 'expect', the update happened.
			return Interlocked.CompareExchange(ref _value, update, expect) == expect;
		}
	}
}