// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="FeeComponents.cs.xml" path='docs/member[@name="T:FeeComponents"]/*' />
    public class FeeComponents : ICloneable
    {
        /// <include file="FeeComponents.cs.xml" path='docs/member[@name="M:FeeComponents.#ctor"]/*' />
        public FeeComponents() { }

		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="M:FeeComponents.FromBytes(System.Byte[])"]/*' />
		public static FeeComponents FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.FeeComponents.Parser.ParseFrom(bytes));
		}
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="M:FeeComponents.FromProtobuf(Proto.Services.FeeComponents)"]/*' />
		public static FeeComponents FromProtobuf(Proto.Services.FeeComponents feeComponents)
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

		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.Min"]/*' />
		public long Min { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.Max"]/*' />
		public long Max { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.Constant"]/*' />
		public long Constant { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.TransactionBandwidthByte"]/*' />
		public long TransactionBandwidthByte { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.TransactionVerification"]/*' />
		public long TransactionVerification { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.TransactionRamByteHour"]/*' />
		public long TransactionRamByteHour { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.TransactionStorageByteHour"]/*' />
		public long TransactionStorageByteHour { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.ContractTransactionGas"]/*' />
		public long ContractTransactionGas { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.TransferVolumeHbar"]/*' />
		public long TransferVolumeHbar { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.ResponseMemoryByte"]/*' />
		public long ResponseMemoryByte { get; set; }
		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="P:FeeComponents.ResponseDiskByte"]/*' />
		public long ResponseDiskByte { get; set; }

		/// <include file="FeeComponents.cs.xml" path='docs/member[@name="M:FeeComponents.ToProtobuf"]/*' />
		public virtual Proto.Services.FeeComponents ToProtobuf()
        {
            return new Proto.Services.FeeComponents
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

        /// <include file="FeeComponents.cs.xml" path='docs/member[@name="M:FeeComponents.ToBytes"]/*' />
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
