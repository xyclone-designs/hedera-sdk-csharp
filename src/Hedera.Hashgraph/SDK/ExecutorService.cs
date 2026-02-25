using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <summary>
	/// ExecutorService replacement for Hedera SDK.
	/// Provides a fixed thread pool with unbounded queue and caller-runs policy.
	/// </summary>
	public sealed class ExecutorService : IDisposable
	{
		private readonly BlockingCollection<Action> _queue;
		private readonly Thread[] _threads;

		public ExecutorService()
		{
			int nThreads = Environment.ProcessorCount;

			_queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
			_threads = new Thread[nThreads];

			for (int i = 0; i < nThreads; i++)
			{
				int threadIndex = i;

				_threads[i] = new Thread(() =>
				{
					foreach (var action in _queue.GetConsumingEnumerable())
					{
						try { action(); }
						catch { /* swallow exceptions to match Java ExecutorService */ }
					}
				})
				{
					IsBackground = true,
					Name = $"hedera-sdk-{threadIndex}"
				};
				_threads[i].Start();
			}
		}

		/// <summary>
		/// Schedules an action for execution (caller-runs if queue is full).
		/// </summary>
		public void Execute(Action action)
		{
			if (!_queue.TryAdd(action))
			{
				action(); // caller runs policy
			}
		}
		/// <summary>
		/// Immediately clears remaining queued tasks and signals threads to exit.
		/// </summary>
		public void ForceShutdown()
		{
			while (_queue.TryTake(out _)) { }
			_queue.CompleteAdding();
		}
		/// <summary>
		/// Waits for all tasks to complete or timeout to elapse.
		/// </summary>
		/// <summary>
		/// Submits a task and returns a Task for async usage.
		/// </summary>
		public Task Submit(Action action)
		{
			var tcs = new TaskCompletionSource<bool>();
			Execute(() =>
			{
				try { action(); tcs.SetResult(true); }
				catch (Exception e) { tcs.SetException(e); }
			});
			return tcs.Task;
		}
		public bool WaitForTermination(TimeSpan timeout)
		{
			DateTime end = DateTime.UtcNow + timeout;
			foreach (var thread in _threads)
			{
				TimeSpan remaining = end - DateTime.UtcNow;
				if (remaining <= TimeSpan.Zero) return false;
				if (!thread.Join(remaining)) return false;
			}
			return true;
		}

		/// <summary>
		/// Gracefully shuts down the executor (stops accepting new tasks).
		/// </summary>
		public void Dispose()
		{
			_queue.CompleteAdding();
		}
	}
}