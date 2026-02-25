using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <summary>
	/// Provides a delay mechanism for executing actions after a given time.
	/// Uses the provided ExecutorService for scheduling.
	/// </summary>
	public class Delayer
	{
		private readonly ExecutorService _executor;

		public Delayer(ExecutorService executor)
		{
			_executor = executor ?? throw new ArgumentNullException(nameof(executor));
		}

		/// <summary>
		/// Schedules an action to run after the specified delay.
		/// </summary>
		/// <param name="delay">Time to wait before executing the action.</param>
		/// <param name="action">Action to execute.</param>
		public Task DelayAsync(TimeSpan delay)
		{
			return _executor.Submit(async () =>
			{
				await Task.Delay(delay).ConfigureAwait(false);
			});
		}
		
		/// <summary>
		/// Schedules an action to run after the specified delay.
		/// </summary>
		/// <param name="delay">Time to wait before executing the action.</param>
		/// <param name="action">Action to execute.</param>
		public Task DelayAsync(TimeSpan delay, Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			return _executor.Submit(async () =>
			{
				await Task.Delay(delay).ConfigureAwait(false);
				action();
			});
		}

		/// <summary>
		/// Schedules a function returning a value after a delay.
		/// </summary>
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