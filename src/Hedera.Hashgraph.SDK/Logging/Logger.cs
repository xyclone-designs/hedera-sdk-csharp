// SPDX-License-Identifier: Apache-2.0

using Grpc.Core.Logging;

namespace Hedera.Hashgraph.SDK.Logging
{
    /// <include file="Logger.cs.xml" path='docs/member[@name="T:Logger"]/*' />
    public class Logger
    {
        //private org.slf4j.Logger internalLogger;
        private ILogger internalLogger;
        private LogLevel currentLevel;
        private LogLevel previousLevel;
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.#ctor(LogLevel)"]/*' />
        public Logger(LogLevel level)
        {
            //internalLogger = LoggerFactory.GetLogger(GetType());
            currentLevel = level;
            previousLevel = level;
        }

		public virtual LogLevel Level
		{
			get => currentLevel;
		}

		/// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.SetLogger(ILogger)"]/*' />
		public virtual Logger SetLogger(ILogger logger)
        {
            internalLogger = logger;
            return this;
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.SetLevel(LogLevel)"]/*' />
        public virtual Logger SetLevel(LogLevel level)
        {
            previousLevel = currentLevel;
            currentLevel = level;
            return this;
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.SetSilent(System.Boolean)"]/*' />
        public virtual Logger SetSilent(bool silent)
        {
            if (silent)
            {
                currentLevel = LogLevel.Silent;
            }
            else
            {
                currentLevel = previousLevel;
            }

            return this;
        }

        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.Trace(System.String,System.Object[])"]/*' />
        public virtual void Trace(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.Trace))
            {
                // TODO
                internalLogger.Debug(message, arguments);
            }
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.Debug(System.String,System.Object[])"]/*' />
        public virtual void Debug(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.Debug))
            {
                internalLogger.Debug(message, arguments);
            }
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.Info(System.String,System.Object[])"]/*' />
        public virtual void Info(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.Info))
            {
                internalLogger.Info(message, arguments);
            }
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.Warn(System.String,System.Object[])"]/*' />
        public virtual void Warn(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.Warn))
            {
                internalLogger.Warning(message, arguments);
            }
        }
        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.Error(System.String,System.Object[])"]/*' />
        public virtual void Error(string message, params object[] arguments)
        {
            if (IsEnabledForLevel(LogLevel.Error))
            {
                internalLogger.Error(message, arguments);
            }
        }

        /// <include file="Logger.cs.xml" path='docs/member[@name="M:Logger.IsEnabledForLevel(LogLevel)"]/*' />
        public virtual bool IsEnabledForLevel(LogLevel level)
        {
            return (int)level >= (int)currentLevel;
        }
    }
}