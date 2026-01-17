namespace Hedera.Hashgraph.SDK
{
	/**
     * Enum for the execution states.
     */
    public enum ExecutionState 
    {
        /**
         * Indicates that the execution was successful
         */
        Success,
        /**
         * Indicates that the call was successful but the operation did not complete. Retry with same/new node
         */
        Retry,
		/**
         * Indicates that the receiver was bad node. Retry with new node
         */
		ServerError,
        /**
         * Indicates that the request was incorrect
         */
        RequestError
    }
}