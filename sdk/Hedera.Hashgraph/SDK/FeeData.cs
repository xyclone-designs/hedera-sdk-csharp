using Google.Protobuf;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
 * The total fees charged for a transaction. It contains three parts namely
 * node data, network data and service data.
 */
    public class FeeData : ICloneable 
    {
        public FeeDataType Type { get; set; } = FeeDataType.Default;

        /**
         * Constructor.
         */
        public FeeData() {}

		/**
         * Initialize fee data object from byte array.
         *
         * @param bytes                     the byte array
         * @return                          the fee data object
         * @       when there is an issue with the protobuf
         */
		public static FeeData FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FeeData.Parser.ParseFrom(bytes));
		}
		/**
         * Initialize fee data object from a protobuf.
         *
         * @param feeData                   the protobuf
         * @return                          the fee data object
         */
		public static FeeData FromProtobuf(Proto.FeeData feeData) {
            return new FeeData()
                    .setNodeData(feeData.hasNodedata() ? FeeComponents.FromProtobuf(feeData.getNodedata()) : null)
                    .setNetworkData(feeData.hasNetworkdata() ? FeeComponents.FromProtobuf(feeData.getNetworkdata()) : null)
                    .setServiceData(feeData.hasNodedata() ? FeeComponents.FromProtobuf(feeData.getServicedata()) : null)
                    .setType(FeeDataType.valueOf(feeData.getSubType()));
        }

		public FeeComponents? NodeData { get; set; }
		public FeeComponents? NetworkData { get; set; }
		public FeeComponents? ServiceData { get; set; }

		public object Clone()
		{
			return new FeeData
			{
				NodeData = NodeData?.Clone() as FeeComponents,
				NetworkData = NetworkData?.Clone() as FeeComponents,
				ServiceData = ServiceData?.Clone() as FeeComponents,
			};
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
         * Convert the fee data type into a protobuf.
         *
         * @return                          the protobuf
         */
		public Proto.FeeData ToProtobuf() 
        {
            return new Proto.FeeData
            {
                Nodedata = NodeData?.ToProtobuf(),
                Networkdata = NetworkData?.ToProtobuf(),
                Servicedata = ServiceData?.ToProtobuf(),
            };
        }
    }
}