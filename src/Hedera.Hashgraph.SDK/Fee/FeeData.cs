// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Fee
{
    /// <include file="FeeData.cs.xml" path='docs/member[@name="T:FeeData"]/*' />
    public class FeeData : ICloneable
    {
		/// <include file="FeeData.cs.xml" path='docs/member[@name="M:FeeData.FromProtobuf(Proto.Services.FeeData)"]/*' />
		public static FeeData FromProtobuf(Proto.Services.FeeData feeData)
        {
            return new FeeData
            {
                NodeData = feeData.Nodedata is not null ? FeeComponents.FromProtobuf(feeData.Nodedata) : null,
                NetworkData = feeData.Networkdata is not null ? FeeComponents.FromProtobuf(feeData.Networkdata) : null,
                ServiceData = feeData.Nodedata is not null ? FeeComponents.FromProtobuf(feeData.Servicedata) : null,
                Type = (FeeDataType)feeData.SubType,
            };
        }

        /// <include file="FeeData.cs.xml" path='docs/member[@name="M:FeeData.FromBytes(System.Byte[])"]/*' />
        public static FeeData FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.FeeData.Parser.ParseFrom(bytes));
        }

		public virtual FeeComponents? NodeData { get; set; }
		public virtual FeeComponents? NetworkData { get; set; }
		public virtual FeeComponents? ServiceData { get; set; }
		public virtual FeeDataType Type { get; set; } = FeeDataType.Default;


		/// <include file="FeeData.cs.xml" path='docs/member[@name="M:FeeData.ToProtobuf"]/*' />
		public virtual Proto.Services.FeeData ToProtobuf()
        {
            Proto.Services.FeeData protobuf = new()
            {
				SubType = (Proto.Services.SubType)Type
			};

            if (NodeData is not null)
                protobuf.Nodedata = NodeData.ToProtobuf();

            if (NetworkData is not null)
                protobuf.Networkdata = NetworkData.ToProtobuf();

            if (ServiceData is not null)
                protobuf.Servicedata = ServiceData.ToProtobuf();

            return protobuf;
        }

        /// <include file="FeeData.cs.xml" path='docs/member[@name="M:FeeData.ToBytes"]/*' />
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
