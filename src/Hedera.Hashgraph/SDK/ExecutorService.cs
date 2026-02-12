using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hedera.Hashgraph.SDK
{
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
						try { action(); }
						catch
						{
							// Match Java executor behavior (swallow task exceptions)
						}
				})
				{
					IsBackground = true, // daemon=true equivalent
					Name = $"hedera-sdk-{threadIndex}"
				};

				_threads[i].Start();
			}
		}

		public void Execute(Action action)
		{
			// Unbounded queue like LinkedBlockingQueue
			// CallerRunsPolicy equivalent if adding fails
			if (!_queue.TryAdd(action))
			{
				action();
			}
		}

		public void Dispose()
		{
			_queue.CompleteAdding();
		}
	}

}
