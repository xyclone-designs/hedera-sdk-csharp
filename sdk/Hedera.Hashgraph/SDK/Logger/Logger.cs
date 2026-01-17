namespace Hedera.Hashgraph.SDK
{
	public class Logger
	{
		private org.slf4j.Logger internalLogger;
		private LogLevel currentLevel;
		private LogLevel previousLevel;

		/**
		 * Constructor
		 *
		 * @param level the current log level
		 */
		public Logger(LogLevel level)
		{
			internalLogger = LoggerFactory.getLogger(getClass());
			currentLevel = level;
			previousLevel = level;
		}

		/**
		 * Set logger
		 *
		 * @param logger the new logger
		 * @return {@code this}
		 */
		public Logger setLogger(org.slf4j.Logger logger)
		{
			internalLogger = logger;
			return this;
		}

		public LogLevel getLevel()
		{
			return currentLevel;
		}

		/**
		 * Set log level
		 *
		 * @param level the new level
		 * @return {@code this}
		 */
		public Logger setLevel(LogLevel level)
		{
			previousLevel = currentLevel;
			currentLevel = level;
			return this;
		}

		/**
		 * Set silent mode on/off. If set to true, the logger will not display any log messages. This can also be achieved
		 * by calling .setLevel(LogLevel.Silent)`
		 *
		 * @param silent should the logger be silent
		 * @return {@code this}
		 */
		public Logger setSilent(bool silent)
		{
			if (silent)
			{
				currentLevel = LogLevel.SILENT;
			}
			else
			{
				currentLevel = previousLevel;
			}
			return this;
		}

		/**
		 * Log trace
		 *
		 * @param message   the message to be logged
		 * @param arguments the log arguments
		 */
		public void trace(string message, Object...arguments)
		{
			if (isEnabledForLevel(LogLevel.TRACE))
			{
				internalLogger.trace(message, arguments);
			}
		}

		/**
		 * Log debug
		 *
		 * @param message   the message to be logged
		 * @param arguments the log arguments
		 */
		public void debug(string message, Object...arguments)
		{
			if (isEnabledForLevel(LogLevel.DEBUG))
			{
				internalLogger.debug(message, arguments);
			}
		}

		/**
		 * Log info
		 *
		 * @param message   the message to be logged
		 * @param arguments the log arguments
		 */
		public void info(string message, Object...arguments)
		{
			if (isEnabledForLevel(LogLevel.INFO))
			{
				internalLogger.info(message, arguments);
			}
		}

		/**
		 * Log warn
		 *
		 * @param message   the message to be logged
		 * @param arguments the log arguments
		 */
		public void warn(string message, Object...arguments)
		{
			if (isEnabledForLevel(LogLevel.WARN))
			{
				internalLogger.warn(message, arguments);
			}
		}

		/**
		 * Log error
		 *
		 * @param message   the message to be logged
		 * @param arguments the log arguments
		 */
		public void error(string message, Object...arguments)
		{
			if (isEnabledForLevel(LogLevel.ERROR))
			{
				internalLogger.error(message, arguments);
			}
		}

		/**
		 * Returns whether this Logger is enabled for a given {@link LogLevel}.
		 *
		 * @param level the log level
		 * @return true if enabled, false otherwise.
		 */
		public bool isEnabledForLevel(LogLevel level)
		{
			return level.toInt() >= currentLevel.toInt();
		}
	}

}