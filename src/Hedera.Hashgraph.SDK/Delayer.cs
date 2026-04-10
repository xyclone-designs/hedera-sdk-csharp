using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <include file="Delayer.cs.xml" path='docs/member[@name="T:Delayer"]/*' />
	public class Delayer
	{
		private readonly ExecutorService _executor;

		public Delayer(ExecutorService executor)
		{
			_executor = executor ?? throw new ArgumentNullException(nameof(executor));
		}

		/// <include file="Delayer.cs.xml" path='docs/member[@name="M:Delayer.DelayAsync(System.TimeSpan)"]/*' />
		public Task DelayAsync(TimeSpan delay)
		{
			return _executor.Submit(async () =>
			{
				await Task.Delay(delay).ConfigureAwait(false);
			});
		}
		
		/// <include file="Delayer.cs.xml" path='docs/member[@name="M:Delayer.DelayAsync(System.TimeSpan,System.Action)"]/*' />
		public Task DelayAsync(TimeSpan delay, Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return _executor.Submit(async () =>
			{
				await Task.Delay(delay).ConfigureAwait(false);
				action();
			});
		}

		/// <include file="Delayer.cs.xml" path='docs/member[@name="M:Delayer.DelayAsync``1(System.TimeSpan,System.Func{``0})"]/*' />
		public Task<T> DelayAsync<T>(TimeSpan delay, Func<T> func)
		{
			if (func == null) throw new ArgumentNullException(nameof(func));

			var tcs = new TaskCompletionSource<T>();
			_executor.Submit(async () =>
			{
				try
				{
					await Task.Delay(delay).ConfigureAwait(false);
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});
			return tcs.Task;
		}
	}
}