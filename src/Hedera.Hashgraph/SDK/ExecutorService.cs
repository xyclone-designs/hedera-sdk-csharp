using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/// <include file="ExecutorService.cs.xml" path='docs/member[@name="T:ExecutorService"]/*' />
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

		/// <include file="ExecutorService.cs.xml" path='docs/member[@name="M:ExecutorService.Execute(System.Action)"]/*' />
		public void Execute(Action action)
		{
			if (!_queue.TryAdd(action))
			{
				action(); // caller runs policy
			}
		}
		/// <include file="ExecutorService.cs.xml" path='docs/member[@name="M:ExecutorService.ForceShutdown"]/*' />
		public void ForceShutdown()
		{
			while (_queue.TryTake(out _)) { }
			_queue.CompleteAdding();
		}
		/// <include file="ExecutorService.cs.xml" path='docs/member[@name="M:ExecutorService.Submit(System.Action)"]/*' />
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

		/// <include file="ExecutorService.cs.xml" path='docs/member[@name="M:ExecutorService.Dispose"]/*' />
		public void Dispose()
		{
			_queue.CompleteAdding();
		}
	}
}