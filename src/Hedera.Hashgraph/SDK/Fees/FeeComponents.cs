// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Utility class used internally by the sdk.
    /// </summary>
    public class FeeComponents : ICloneable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FeeComponents() { }

		/// <summary>
		/// Create a fee component object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the fee component object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static FeeComponents FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FeeComponents.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a fee components object from a protobuf.
		/// </summary>
		/// <param name="feeComponents">the protobuf</param>
		/// <returns>                         the fee component object</returns>
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

		/// <summary>
		/// A minimum, the calculated fee must be greater than this value
		/// </summary>
		public long Min { get; set; }
		/// <summary>
		/// A maximum, the calculated fee must be less than this value
		/// </summary>
		public long Max { get; set; }
		/// <summary>
		/// A constant contribution to the fee
		/// </summary>
		public long Constant { get; set; }
		/// <summary>
		/// The price of bandwidth consumed by a transaction, measured in bytes
		/// </summary>
		public long TransactionBandwidthByte { get; set; }
		/// <summary>
		/// The price per signature verification for a transaction
		/// </summary>
		public long TransactionVerification { get; set; }
		/// <summary>
		/// The price of RAM consumed by a transaction, measured in byte-hours
		/// </summary>
		public long TransactionRamByteHour { get; set; }
		/// <summary>
		/// The price of storage consumed by a transaction, measured in byte-hours
		/// </summary>
		public long TransactionStorageByteHour { get; set; }
		/// <summary>
		/// The price of computation for a smart contract transaction, measured in gas
		/// </summary>
		public long ContractTransactionGas { get; set; }
		/// <summary>
		/// The price per hbar transferred for a transfer
		/// </summary>
		public long TransferVolumeHbar { get; set; }
		/// <summary>
		/// The price of bandwidth for data retrieved from memory for a response, measured in bytes
		/// </summary>
		public long ResponseMemoryByte { get; set; }
		/// <summary>
		/// The price of bandwidth for data retrieved from disk for a response, measured in bytes
		/// </summary>
		public long ResponseDiskByte { get; set; }

		/// <summary>
		/// Convert fee component object to protobuf.
		/// </summary>
		/// <returns>                         the protobuf</returns>
		public virtual Proto.FeeComponents ToProtobuf()
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

        /// <summary>
        /// Convert fee component object to byte array.
        /// </summary>
        /// <returns>                         the byte array</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
        public virtual object Clone()
        {
            return new FeeComponents
			{
				Min = Min,
				Max = Max,
				Constant = Constant,
				TransactionBandwidthByte = TransactionBandwidthByte,
				TransactionVerification = TransactionVerification,
				TransactionRamByteHour = TransactionRamByteHour,
				TransactionStorageByteHour = TransactionStorageByteHour,
				ContractTransactionGas = ContractTransactionGas,
				TransferVolumeHbar = TransferVolumeHbar,
				ResponseMemoryByte = ResponseMemoryByte,
				ResponseDiskByte = ResponseDiskByte,
			};
		}
    }
}