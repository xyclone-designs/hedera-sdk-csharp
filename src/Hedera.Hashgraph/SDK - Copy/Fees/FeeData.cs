// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// The total fees charged for a transaction. It contains three parts namely
    /// node data, network data and service data.
    /// </summary>
    public class FeeData : ICloneable
    {
        /// <summary>
		/// Initialize fee data object from a protobuf.
		/// </summary>
		/// <param name="feeData">the protobuf</param>
		/// <returns>                         the fee data object</returns>
		public static FeeData FromProtobuf(Proto.FeeData feeData)
        {
            return new FeeData
            {
                NodeData = feeData.Nodedata is not null ? FeeComponents.FromProtobuf(feeData.Nodedata) : null,
                NetworkData = feeData.Networkdata is not null ? FeeComponents.FromProtobuf(feeData.Networkdata) : null,
                ServiceData = feeData.Nodedata is not null ? FeeComponents.FromProtobuf(feeData.Servicedata) : null,
                Type = (FeeDataType)feeData.SubType,
            };
        }

        /// <summary>
        /// Initialize fee data object from byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the fee data object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static FeeData FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.FeeData.Parser.ParseFrom(bytes));
        }

		public virtual FeeComponents? NodeData { get; set; }
		public virtual FeeComponents? NetworkData { get; set; }
		public virtual FeeComponents? ServiceData { get; set; }
		public virtual FeeDataType Type { get; set; } = FeeDataType.Default;


		/// <summary>
		/// Convert the fee data type into a protobuf.
		/// </summary>
		/// <returns>                         the protobuf</returns>
		public virtual Proto.FeeData ToProtobuf()
        {
            Proto.FeeData protobuf = new()
            {
				SubType = (Proto.SubType)Type
			};

            if (NodeData is not null)
                protobuf.Nodedata = NodeData.ToProtobuf();

            if (NetworkData is not null)
                protobuf.Networkdata = NetworkData.ToProtobuf();

            if (ServiceData is not null)
                protobuf.Servicedata = ServiceData.ToProtobuf();

            return protobuf;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        public virtual object Clone()
        {
            return new FeeData
            {
                NodeData = NodeData?.Clone() as FeeComponents,
                NetworkData = NetworkData?.Clone() as FeeComponents,
                ServiceData = ServiceData?.Clone() as FeeComponents,
            };
        }
    }
}