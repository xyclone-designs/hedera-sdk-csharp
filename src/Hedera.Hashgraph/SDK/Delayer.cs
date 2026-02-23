// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	internal static class Delayer
	{
		private static readonly ILogger logger =
			LoggerFactory
				.Create(builder => builder.AddConsole())
				.CreateLogger("Delayer");

		// Equivalent to newSingleThreadScheduledExecutor(daemon=true)
		private static readonly TaskScheduler Scheduler = new SingleThreadTaskScheduler();
		private static readonly TaskFactory SchedulerFactory = new (Scheduler);

		private static readonly TimeSpan MIN_DELAY = TimeSpan.FromMilliseconds(500);

		#region Public API

		internal static Task DelayBackOff(int attempt, TaskFactory executor)
		{
			// 1L << attempt
			long maxMultiplier = 1L << Math.Min(attempt, 62);
			long multiplier = Random.Shared.NextInt64(maxMultiplier);
			var interval = TimeSpan.FromTicks(MIN_DELAY.Ticks * multiplier);

			return DelayFor((long)interval.TotalMilliseconds, executor);
		}
		internal static Task DelayFor(int milliseconds, TaskFactory executor)
		{
			return DelayFor((long)milliseconds, executor);
		}
		internal static Task DelayFor(long milliseconds, TaskFactory executor)
		{
			logger.LogTrace("waiting for {Seconds} seconds before trying again", milliseconds / 1000.0);

			return Task.Factory.StartNew(
				() => { },
				CancellationToken.None,
				TaskCreationOptions.None,
				DelayedExecutor(milliseconds, executor)
			);
		}
		internal static Task DelayFor(double milliseconds, TaskFactory executor)
		{
			return DelayFor((long)milliseconds, executor);
		}

		#endregion

		#region Private Helpers

		private static TaskScheduler DelayedExecutor(long delayMilliseconds, TaskFactory executor)
		{
			return new DelayedTaskScheduler(delayMilliseconds, executor);
		}

		#endregion

		#region Scheduler Implementations

		/// <summary>
		/// Equivalent to Java's single daemon scheduled executor.
		/// </summary>
		private sealed class SingleThreadTaskScheduler : TaskScheduler
		{
			private readonly Thread _thread;
			private readonly AutoResetEvent _signal = new(false);
			private readonly Queue<Task> _tasks = new();

			public SingleThreadTaskScheduler()
			{
				_thread = new Thread(Run)
				{
					IsBackground = true,
					Name = "Delayer-Scheduler"
				};
				_thread.Start();
			}

			protected override void QueueTask(Task task)
			{
				lock (_tasks)
				{
					_tasks.Enqueue(task);
				}
				_signal.Set();
			}

			private void Run()
			{
				while (true)
				{
					Task? task = null;

					lock (_tasks)
					{
						if (_tasks.Count > 0)
							task = _tasks.Dequeue();
					}

					if (task != null)
						TryExecuteTask(task);
					else
						_signal.WaitOne();
				}
			}

			protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

			protected override IEnumerable<Task>? GetScheduledTasks()
			{
				lock (_tasks)
				{
					return _tasks.ToArray();
				}
			}
		}

		/// <summary>
		/// Equivalent to Java's delayedExecutor(...)
		/// Schedules execution after delay, then forwards to provided executor.
		/// </summary>
		private sealed class DelayedTaskScheduler : TaskScheduler
		{
			private readonly long _delay;
			private readonly TaskFactory _executor;

			public DelayedTaskScheduler(long delayMilliseconds, TaskFactory executor)
			{
				_delay = delayMilliseconds;
				_executor = executor;
			}

			protected override void QueueTask(Task task)
			{
				SchedulerFactory.StartNew(async () =>
				{
					await Task.Delay((int)_delay)
						.ConfigureAwait(false);

					await _executor.StartNew(
						() => TryExecuteTask(task),
						CancellationToken.None,
						TaskCreationOptions.None,
						_executor.Scheduler);
				});
			}

			protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
			protected override IEnumerable<Task>? GetScheduledTasks() => null;
		}

		#endregion
	}
}