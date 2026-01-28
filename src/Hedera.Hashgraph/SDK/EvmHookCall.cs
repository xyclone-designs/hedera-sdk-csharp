// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Specifies details of a call to an EVM hook.
    /// <p>
    /// This class represents the evm_hook_call field in the HookCall protobuf message.
    /// It contains the call data and gas limit for executing an EVM hook.
    /// </summary>
    public class EvmHookCall
    {
        /// <summary>
        /// Create a new EvmHookCall.
        /// </summary>
        /// <param name="data">the call data to pass to the hook via the IHieroHook.HookContext#data field</param>
        /// <param name="gasLimit">the gas limit to use for the hook execution</param>
        public EvmHookCall(byte[] data, ulong gasLimit)
        {
            Data = data;
            GasLimit = gasLimit;
        }

		/// <summary>
		/// Get the call data for this hook call.
		/// </summary>
		/// <returns>the call data as a byte array</returns>
		public virtual byte[] Data { get => (byte[])field.Clone(); } // Return a copy to prevent external modification
		/// <summary>
		/// Get the gas limit for this hook call.
		/// </summary>
		/// <returns>the gas limit</returns>
		public virtual ulong GasLimit { get => field; }

		/// <summary>
		/// Convert this EvmHookCall to a protobuf message.
		/// </summary>
		/// <returns>the protobuf EvmHookCall</returns>
		public virtual Proto.EvmHookCall ToProtobuf()
        {
            return new Proto.EvmHookCall
            {
				Data = ByteString.CopyFrom(Data),
				GasLimit = GasLimit,
			};
        }
        /// <summary>
        /// Create an EvmHookCall from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf EvmHookCall</param>
        /// <returns>a new EvmHookCall instance</returns>
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