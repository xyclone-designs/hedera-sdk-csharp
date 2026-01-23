// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Logger
{
    /// <summary>
    /// </summary>
    public class Logger
    {
        private org.slf4j.Logger internalLogger;
        private LogLevel currentLevel;
        private LogLevel previousLevel;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level">the current log level</param>
        public Logger(LogLevel level)
        {
            internalLogger = LoggerFactory.GetLogger(GetType());
            currentLevel = level;
            previousLevel = level;
        }

        /// <summary>
        /// Set logger
        /// </summary>
        /// <param name="logger">the new logger</param>
        /// <returns>{@code this}</returns>
        public virtual Logger SetLogger(org.slf4j.Logger logger)
        {
            internalLogger = logger;
            return this;
        }

        public virtual LogLevel GetLevel()
        {
            return currentLevel;
        }

        /// <summary>
        /// Set log level
        /// </summary>
        /// <param name="level">the new level</param>
        /// <returns>{@code this}</returns>
        public virtual Logger SetLevel(LogLevel level)
        {
            previousLevel = currentLevel;
            currentLevel = level;
            return this;
        }

        /// <summary>
        /// Set silent mode on/off. If set to true, the logger will not display any log messages. This can also be achieved
        /// by calling .setLevel(LogLevel.Silent)`
        /// </summary>
        /// <param name="silent">should the logger be silent</param>
        /// <returns>{@code this}</returns>
        public virtual Logger SetSilent(bool silent)
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

        /// <summary>
        /// Log trace
        /// </summary>
        /// <param name="message">the message to be logged</param>
        /// <param name="arguments">the log arguments</param>
        public virtual void Trace(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.TRACE))
            {
                internalLogger.Trace(message, arguments);
            }
        }

        /// <summary>
        /// Log debug
        /// </summary>
        /// <param name="message">the message to be logged</param>
        /// <param name="arguments">the log arguments</param>
        public virtual void Debug(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.DEBUG))
            {
                internalLogger.Debug(message, arguments);
            }
        }

        /// <summary>
        /// Log info
        /// </summary>
        /// <param name="message">the message to be logged</param>
        /// <param name="arguments">the log arguments</param>
        public virtual void Info(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.INFO))
            {
                internalLogger.Info(message, arguments);
            }
        }

        /// <summary>
        /// Log warn
        /// </summary>
        /// <param name="message">the message to be logged</param>
        /// <param name="arguments">the log arguments</param>
        public virtual void Warn(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.WARN))
            {
                internalLogger.Warn(message, arguments);
            }
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="message">the message to be logged</param>
        /// <param name="arguments">the log arguments</param>
        public virtual void Error(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.ERROR))
            {
                internalLogger.Error(message, arguments);
            }
        }

        /// <summary>
        /// Returns whether this Logger is enabled for a given {@link LogLevel}.
        /// </summary>
        /// <param name="level">the log level</param>
        /// <returns>true if enabled, false otherwise.</returns>
        public virtual bool IsEnabledForLevel(LogLevel level)
        {
            return level.ToInt() >= currentLevel.ToInt();
        }
    }
}