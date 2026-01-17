using Google.Protobuf;
using Hedera.Hashgraph.Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;

namespace Hedera.Hashgraph.SDK
{
    /**
     * The fee schedule for a specific hedera functionality and the time period this fee schedule will expire.
     *
     * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/feeschedule">Hedera Documentation</a>
     */
    public class FeeSchedule : ICloneable
    {
		/**
         * Constructor.
         */
		public FeeSchedule() { }

		public DateTimeOffset? ExpirationTime { get; set; }
		public List<TransactionFeeSchedule> TransactionFeeSchedules 
        {
            set => TransactionFeeSchedulesRead = new ReadOnlyCollection<TransactionFeeSchedule>(value);
        }
		public ReadOnlyCollection<TransactionFeeSchedule> TransactionFeeSchedulesRead { get; private set; }

		/**
         * Create a fee schedule from a protobuf.
         *
         * @param feeSchedule               the protobuf
         * @return                          the fee schedule
         */
		public static FeeSchedule FromProtobuf(Proto.FeeSchedule feeSchedule) 
        {
            FeeSchedule returnFeeSchedule = new FeeSchedule()
                    .setExpirationTime(
                            feeSchedule.hasExpiryTime()
                                    ? DateTimeOffsetConverter.FromProtobuf(feeSchedule.getExpiryTime())
                                    : null);
            for (var transactionFeeSchedule : feeSchedule.getTransactionFeeScheduleList()) {
                returnFeeSchedule.AddTransactionFeeSchedule(TransactionFeeSchedule.FromProtobuf(transactionFeeSchedule));
            }
            return returnFeeSchedule;
        }
        /**
         * Create a fee schedule from byte array.
         *
         * @param bytes                     the bye array
         * @return                          the fee schedule
         * @       when there is an issue with the protobuf
         */
        public static FeeSchedule FromBytes(byte[] bytes) 
        {
            return FromProtobuf(Proto.FeeSchedule.Parser.ParseFrom(bytes));
        }


		public object Clone()
		{
			FeeSchedule clone = (FeeSchedule)base.clone();
			clone.transactionFeeSchedules = cloneTransactionFeeSchedules(transactionFeeSchedules);
		}
		/**
         * Create the byte array.
         *
         * @return                          a byte array representation
         */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/**
         * Convert to a protobuf.
         *
         * @return                          the protobuf
         */
		public Proto.FeeSchedule ToProtobuf() {
            var returnBuilder = Proto.FeeSchedule.newBuilder();
            if (expirationTime != null) {
                returnBuilder.setExpiryTime(DateTimeOffsetConverter.toSecondsProtobuf(expirationTime));
            }
            for (TransactionFeeSchedule tFeeSchedule : getTransactionFeeSchedules()) {
                returnBuilder.AddTransactionFeeSchedule(tFeeSchedule.ToProtobuf());
            }
            return returnBuilder.build();
        }        
    }
}