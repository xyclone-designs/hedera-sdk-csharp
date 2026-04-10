// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="EvmHookCall.cs.xml" path='docs/member[@name="T:EvmHookCall"]/*' />
    public class EvmHookCall
    {
        /// <include file="EvmHookCall.cs.xml" path='docs/member[@name="M:EvmHookCall.#ctor(System.Byte[],System.UInt64)"]/*' />
        public EvmHookCall(byte[] data, ulong gasLimit)
        {
            Data = data;
            GasLimit = gasLimit;
        }

		/// <include file="EvmHookCall.cs.xml" path='docs/member[@name="M:EvmHookCall.Clone"]/*' />
		public virtual byte[] Data { get => (byte[])field.Clone(); } // Return a copy to prevent external modification
		/// <include file="EvmHookCall.cs.xml" path='docs/member[@name="P:EvmHookCall.GasLimit"]/*' />
		public virtual ulong GasLimit { get => field; }

		/// <include file="EvmHookCall.cs.xml" path='docs/member[@name="M:EvmHookCall.ToProtobuf"]/*' />
		public virtual Proto.EvmHookCall ToProtobuf()
        {
            return new Proto.EvmHookCall
            {
				Data = ByteString.CopyFrom(Data),
				GasLimit = GasLimit,
			};
        }
        /// <include file="EvmHookCall.cs.xml" path='docs/member[@name="M:EvmHookCall.FromProtobuf(Proto.EvmHookCall)"]/*' />
        public static EvmHookCall FromProtobuf(Proto.EvmHookCall proto)
        {
            return new EvmHookCall(proto.Data.ToByteArray(), proto.GasLimit);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;

            EvmHookCall that = (EvmHookCall)o;

            return GasLimit == that.GasLimit && Equals(Data, that.Data);
        }
        public override int GetHashCode()
        {
            int result = HashCode.Combine(Data);
            result = 31 * result + GasLimit.GetHashCode();
            return result;
        }
    }
}