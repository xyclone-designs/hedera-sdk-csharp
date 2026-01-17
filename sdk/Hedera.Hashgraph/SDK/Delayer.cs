namespace Hedera.Hashgraph.SDK
{
	/**
	 * Utility class used internally by the sdk.
	 */
	sealed class Delayer
	{
		private static readonly Logger logger = LoggerFactory.getLogger(Delayer.class);

		private static readonly ScheduledExecutorService SCHEDULER = Executors.newSingleThreadScheduledExecutor(r -> {
			Thread t = new Thread(r);
			t.setDaemon(true);
			return t;
		});

		private static readonly Duration MIN_DELAY = Duration.ofMillis(500);

		/**
		 * Constructor.
		 */
		private Delayer() { }

		/**
		 * Set the delay backoff attempts.
		 *
		 * @param attempt                   the attempts
		 * @param executor                  the executor
		 * @return                          the updated future
		 */
		static Task delayBackOff(int attempt, Executor executor)
		{
			var interval = MIN_DELAY.multipliedBy(ThreadLocalRandom.current().nextLong(1L << attempt));

			return delayFor(interval.toMillis(), executor);
		}

		/**
		 * Set the delay backoff milliseconds.
		 *
		 * @param milliseconds              the milliseconds
		 * @param executor                  the executor
		 * @return                          the updated future
		 */
		static Task delayFor(long milliseconds, Executor executor)
		{
			logger.trace("waiting for {} seconds before trying again", (double)milliseconds / 1000.0);

			return Task.runAsync(()-> { }, delayedExecutor(milliseconds, TimeUnit.MILLISECONDS, executor));
		}

		private static Executor delayedExecutor(long delay, TimeUnit unit, Executor executor)
		{
			return r->SCHEDULER.schedule(()->executor.execute(r), delay, unit);
		}
	}
}