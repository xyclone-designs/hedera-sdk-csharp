using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Utility exception class.
     */
    public class MaxAttemptsExceededException : InvalidOperationException
    {
        public MaxAttemptsExceededException(Exception? ex) : base("exceeded maximum attempts for request with last exception being", ex) { }
    }

}