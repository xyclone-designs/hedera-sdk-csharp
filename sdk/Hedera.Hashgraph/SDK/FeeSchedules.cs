namespace Hedera.Hashgraph.SDK
{
	/**
     * This contains two Fee Schedules with expiry timestamp.
     *
     * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/currentandnextfeeschedule">Hedera Documentation</a>
     */
    public class FeeSchedules 
    {
        public FeeSchedule? Current { get; set; }
		public FeeSchedule? Next { get; set; }

		/**
         * Constructor.
         */
		public FeeSchedules() {
            current = next = null;
        }

        /**
         * Create a fee schedules object from a protobuf.
         *
         * @param feeSchedules              the protobuf
         * @return                          the fee schedules object
         */
        public static FeeSchedules FromProtobuf(Proto.CurrentAndNextFeeSchedule feeSchedules) 
        {
            return new FeeSchedules
            {
                Current = FeeSchedule.Fr
            };


			return new FeeSchedules()
                    .setCurrent(
                            feeSchedules.hasCurrentFeeSchedule()
                                    ? FeeSchedule.FromProtobuf(feeSchedules.getCurrentFeeSchedule())
                                    : null)
                    .setNext(
                            feeSchedules.hasNextFeeSchedule()
                                    ? FeeSchedule.FromProtobuf(feeSchedules.getNextFeeSchedule())
                                    : null);
        }

        /**
         * Create a fee schedules object from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the fee schedules object
         * @       when there is an issue with the protobuf
         */
        public static FeeSchedules FromBytes(byte[] bytes)  {
            return FromProtobuf(
                    CurrentAndNextFeeSchedule.Parser.ParseFrom(bytes));
        }

        /**
         * Extract the current fee schedule.
         *
         * @return                          the current fee schedule
         */
        @Nullable
        public FeeSchedule getCurrent() {
            return current != null ? current.clone() : null;
        }

        /**
         * Assign the current fee schedule.
         *
         * @param current                   the fee schedule
         * @return {@code this}
         */
        public FeeSchedules setCurrent(@Nullable FeeSchedule current) {
            this.current = current != null ? current.clone() : null;
            return this;
        }

        /**
         * Extract the next fee schedule.
         *
         * @return                          the next fee schedule
         */
        @Nullable
        public FeeSchedule getNext() {
            return next != null ? next.clone() : null;
        }

        /**
         * Assign the next fee schedule.
         *
         * @param next                      the fee schedule
         * @return {@code this}
         */
        public FeeSchedules setNext(@Nullable FeeSchedule next) {
            this.next = next != null ? next.clone() : null;
            return this;
        }

        /**
         * Create the protobuf.
         *
         * @return                          protobuf representation
         */
        public Proto.CurrentAndNextFeeSchedule ToProtobuf() {
            var returnBuilder = CurrentAndNextFeeSchedule.newBuilder();
            if (current != null) {
                returnBuilder.setCurrentFeeSchedule(current.ToProtobuf());
            }
            if (next != null) {
                returnBuilder.setNextFeeSchedule(next.ToProtobuf());
            }
            return returnBuilder.build();
        }

        /**
         * Create the byte array.
         *
         * @return                          byte array representation
         */
        public byte[] ToBytes() {
            return ToProtobuf().ToByteArray();
        }
    }
}