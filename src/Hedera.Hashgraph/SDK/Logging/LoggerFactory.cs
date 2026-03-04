// SPDX-License-Identifier: Apache-2.0

using System;

namespace Hedera.Hashgraph.SDK.Logging
{
    /// <include file="LoggerFactory.cs.xml" path='docs/member[@name="T:LoggerFactory"]/*' />
    public static class LoggerFactory
    {
        public static Logger GetLogger(Type type) { return new Logger(LogLevel.Debug); }
    }
}