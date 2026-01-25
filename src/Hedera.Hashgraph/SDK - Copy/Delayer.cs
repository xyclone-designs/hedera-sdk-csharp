// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.WellKnownTypes;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Utility class used internally by the sdk.
    /// </summary>
    sealed class Delayer
    {
        private static readonly Logger logger = LoggerFactory.GetLogger(typeof(Delayer));
        private static readonly ScheduledExecutorService SCHEDULER = Executors.NewSingleThreadScheduledExecutor((r) =>
        {
            Thread t = new Thread(r);
            t.SetDaemon(true);

            return t;
        });
        private static readonly Duration MIN_DELAY = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(500));
        /// <summary>
        /// Constructor.
        /// </summary>
        private Delayer() { }

        /// <summary>
        /// Set the delay backoff attempts.
        /// </summary>
        /// <param name="attempt">the attempts</param>
        /// <param name="executor">the executor</param>
        /// <returns>                         the updated future</returns>
        static Task DelayBackOff(int attempt, Executor executor)
        {
            var interval = MIN_DELAY.MultipliedBy(ThreadLocalRandom.Current().NextLong(1 << attempt));
            return DelayFor(interval.ToMillis(), executor);
        }

        /// <summary>
        /// Set the delay backoff milliseconds.
        /// </summary>
        /// <param name="milliseconds">the milliseconds</param>
        /// <param name="executor">the executor</param>
        /// <returns>                         the updated future</returns>
        static Task DelayFor(long milliseconds, Executor executor)
        {
            logger.Trace("waiting for {} seconds before trying again", (double)milliseconds / 1000);
            return Task.Run(() =>
            {
            }, DelayedExecutor(milliseconds, TimeUnit.MILLISECONDS, executor));
        }

        private static Executor DelayedExecutor(long delay, TimeUnit unit, Executor executor)
        {
            return (r) => SCHEDULER.Schedule(() => executor.Execute(r), delay, unit);
        }
    }
}