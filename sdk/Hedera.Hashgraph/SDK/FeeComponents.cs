using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Utility class used internally by the sdk.
     */
    public class FeeComponents : ICloneable 
    {
        /**
         * Constructor.
         */
        public FeeComponents() {}

		/**
         * A minimum, the calculated fee must be greater than this value
         */
		public long Min { get; set; }
		/**
         * A maximum, the calculated fee must be less than this value
         */
		public long Max { get; set; }
		/**
         * A constant contribution to the fee
         */
		public long Constant { get; set; }
		/**
         * The price of bandwidth consumed by a transaction, measured in bytes
         */
		public long TransactionBandwidthByte { get; set; }
		/**
         * The price per signature verification for a transaction
         */
		public long TransactionVerification { get; set; }
		/**
         * The price of RAM consumed by a transaction, measured in byte-hours
         */
		public long TransactionRamByteHour { get; set; }
		/**
         * The price of storage consumed by a transaction, measured in byte-hours
         */
		public long TransactionStorageByteHour { get; set; }
		/**
         * The price of computation for a smart contract transaction, measured in gas
         */
		public long ContractTransactionGas { get; set; }
		/**
         * The price per hbar transferred for a transfer
         */
		public long TransferVolumeHbar { get; set; }
		/**
         * The price of bandwidth for data retrieved from memory for a response, measured in bytes
         */
		public long ResponseMemoryByte { get; set; }
		/**
         * The price of bandwidth for data retrieved from disk for a response, measured in bytes
         */
		public long ResponseDiskByte { get; set; }

		public object Clone()
		{
			return (FeeComponents)MemberwiseClone();
		}
		/**
         * Convert fee component object to byte array.
         *
         * @return                          the byte array
         */
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/**
         * Convert fee component object to protobuf.
         *
         * @return                          the protobuf
         */
		public Proto.FeeComponents ToProtobuf()
		{
			return new Proto.FeeComponents
			{
				Min = Min,
				Max = Max,
				Constant = Constant,
				Bpt = TransactionBandwidthByte,
				Vpt = TransactionVerification,
				Rbh = TransactionRamByteHour,
				Sbh = TransactionStorageByteHour,
				Gas = ContractTransactionGas,
				Tv = TransferVolumeHbar,
				Bpr = ResponseMemoryByte,
				Sbpr = ResponseDiskByte,
			};
		}

		/**
         * Create a fee component object from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the fee component object
         * @       when there is an issue with the protobuf
         */
		public static FeeComponents FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FeeComponents.Parser.ParseFrom(bytes));
		}
		/**
         * Create a fee components object from a protobuf.
         *
         * @param feeComponents             the protobuf
         * @return                          the fee component object
         */
		public static FeeComponents FromProtobuf(Proto.FeeComponents feeComponents)
		{
			return new FeeComponents
			{
				Min = feeComponents.Min,
				Max = feeComponents.Max,
				Constant = feeComponents.Constant,
				TransactionBandwidthByte = feeComponents.Bpt,
				TransactionVerification = feeComponents.Vpt,
				TransactionRamByteHour = feeComponents.Rbh,
				TransactionStorageByteHour = feeComponents.Sbh,
				ContractTransactionGas = feeComponents.Gas,
				TransferVolumeHbar = feeComponents.Tv,
				ResponseMemoryByte = feeComponents.Bpr,
				ResponseDiskByte = feeComponents.Sbpr,
			};
		}
	}
}