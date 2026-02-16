// SPDX-License-Identifier: Apache-2.0
using Org.Mockito.Mockito;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.SDK.Logger
{
    class LoggerTest
    {
        private Logger logger;
        private org.slf4j.Logger internalLogger;
        public virtual void Setup()
        {
            internalLogger = Mock(typeof(org.slf4j.Logger));
            logger = new Logger(LogLevel.TRACE);
            logger.SetLogger(internalLogger);
        }

        public virtual void LogsTrace()
        {
            logger.Trace("log");
            Verify(internalLogger, Times(1)).Trace(Any(), Any(typeof(Object[])));
        }

        public virtual void DoesNotLogTraceIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Trace("log");
            Verify(internalLogger, Times(0)).Trace(Any(), Any(typeof(Object[])));
        }

        public virtual void LogsDebug()
        {
            logger.Debug("log");
            Verify(internalLogger, Times(1)).Debug(Any(), Any(typeof(Object[])));
        }

        public virtual void DoesNotLogDebugIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Debug("log");
            Verify(internalLogger, Times(0)).Debug(Any(), Any(typeof(Object[])));
        }

        public virtual void LogsInfo()
        {
            logger.Info("log");
            Verify(internalLogger, Times(1)).Info(Any(), Any(typeof(Object[])));
        }

        public virtual void DoesNotLogInfoIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Info("log");
            Verify(internalLogger, Times(0)).Info(Any(), Any(typeof(Object[])));
        }

        public virtual void LogsWarn()
        {
            logger.Warn("log");
            Verify(internalLogger, Times(1)).Warn(Any(), Any(typeof(Object[])));
        }

        public virtual void DoesNotLogWarnIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Warn("log");
            Verify(internalLogger, Times(0)).Warn(Any(), Any(typeof(Object[])));
        }

        public virtual void LogsError()
        {
            logger.Error("log");
            Verify(internalLogger, Times(1)).Error(Any(), Any(typeof(Object[])));
        }

        public virtual void DoesNotLogErrorIfSilent()
        {
            logger.SetSilent(true);
            logger.Error("log");
            Verify(internalLogger, Times(0)).Error(Any(), Any(typeof(Object[])));
        }

        public virtual void LogsWhenUnsilenced()
        {
            logger.SetSilent(true);
            logger.Error("log");
            logger.SetSilent(false);
            logger.Warn("log");
            Verify(internalLogger, Times(0)).Error(Any(), Any(typeof(Object[])));
            Verify(internalLogger, Times(1)).Warn(Any(), Any(typeof(Object[])));
        }
    }
}