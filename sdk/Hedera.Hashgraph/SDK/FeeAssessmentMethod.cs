namespace Hedera.Hashgraph.SDK
{
	/**
     * Enum for the fee assessment method.
     * <p>
     * The terminology here (exclusive vs inclusive) is borrowed from tax assessment.
     */
    public enum FeeAssessmentMethod 
    {
		/**
         * If Alice is paying Bob, and an <b>inclusive</b> fractional fee is collected to be sent to Charlie,
         * the amount Alice declares she will pay in the transfer transaction <b>includes</b> the fee amount.
         * <p>
         * In other words, Bob receives the amount that Alice intended to send, minus the fee.
         */
		Inclusive = 0,

		/**
         * If Alice is paying Bob, and an <b>exclusive</b> fractional fee is collected to be sent to Charlie,
         * the amount Alice declares she will pay in the transfer transaction <b>does not</b> include the fee amount.
         * <p>
         * In other words, Alice is charged the fee <b>in addition to</b> the amount she intended to send to Bob.
         */
		Exclusive = 1,
    }
}