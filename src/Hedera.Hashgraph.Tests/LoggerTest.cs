// SPDX-License-Identifier: Apache-2.0
using Org.Mockito.Mockito;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk.Logger
{
    class LoggerTest
    {
        private Logger logger;
        private org.slf4j.Logger internalLogger;
        virtual void Setup()
        {
            internalLogger = Mock(typeof(org.slf4j.Logger));
            logger = new Logger(LogLevel.TRACE);
            logger.SetLogger(internalLogger);
        }

        virtual void LogsTrace()
        {
            logger.Trace("log");
            Verify(internalLogger, Times(1)).Trace(Any(), Any(typeof(Object[])));
        }

        virtual void DoesNotLogTraceIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Trace("log");
            Verify(internalLogger, Times(0)).Trace(Any(), Any(typeof(Object[])));
        }

        virtual void LogsDebug()
        {
            logger.Debug("log");
            Verify(internalLogger, Times(1)).Debug(Any(), Any(typeof(Object[])));
        }

        virtual void DoesNotLogDebugIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Debug("log");
            Verify(internalLogger, Times(0)).Debug(Any(), Any(typeof(Object[])));
        }

        virtual void LogsInfo()
        {
            logger.Info("log");
            Verify(internalLogger, Times(1)).Info(Any(), Any(typeof(Object[])));
        }

        virtual void DoesNotLogInfoIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Info("log");
            Verify(internalLogger, Times(0)).Info(Any(), Any(typeof(Object[])));
        }

        virtual void LogsWarn()
        {
            logger.Warn("log");
            Verify(internalLogger, Times(1)).Warn(Any(), Any(typeof(Object[])));
        }

        virtual void DoesNotLogWarnIfNotEnabled()
        {
            logger.SetLevel(LogLevel.ERROR);
            logger.Warn("log");
            Verify(internalLogger, Times(0)).Warn(Any(), Any(typeof(Object[])));
        }

        virtual void LogsError()
        {
            logger.Error("log");
            Verify(internalLogger, Times(1)).Error(Any(), Any(typeof(Object[])));
        }

        virtual void DoesNotLogErrorIfSilent()
        {
            logger.SetSilent(true);
            logger.Error("log");
            Verify(internalLogger, Times(0)).Error(Any(), Any(typeof(Object[])));
        }

        virtual void LogsWhenUnsilenced()
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