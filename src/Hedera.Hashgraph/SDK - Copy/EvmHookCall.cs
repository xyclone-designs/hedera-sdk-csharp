// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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
        private readonly byte[] data;
        private readonly long gasLimit;
        /// <summary>
        /// Create a new EvmHookCall.
        /// </summary>
        /// <param name="data">the call data to pass to the hook via the IHieroHook.HookContext#data field</param>
        /// <param name="gasLimit">the gas limit to use for the hook execution</param>
        public EvmHookCall(byte[] data, long gasLimit)
        {
            data = Objects.RequireNonNull(data, "data cannot be null");
            gasLimit = gasLimit;
        }

        /// <summary>
        /// Get the call data for this hook call.
        /// </summary>
        /// <returns>the call data as a byte array</returns>
        public virtual byte[] GetData()
        {
            return data.Clone(); // Return a copy to prevent external modification
        }

        /// <summary>
        /// Get the gas limit for this hook call.
        /// </summary>
        /// <returns>the gas limit</returns>
        public virtual long GetGasLimit()
        {
            return gasLimit;
        }

        /// <summary>
        /// Convert this EvmHookCall to a protobuf message.
        /// </summary>
        /// <returns>the protobuf EvmHookCall</returns>
        virtual Proto.EvmHookCall ToProtobuf()
        {
            return Proto.EvmHookCall.NewBuilder().SetData(com.google.protobuf.ByteString.CopyFrom(data)).SetGasLimit(gasLimit).Build();
        }

        /// <summary>
        /// Create an EvmHookCall from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf EvmHookCall</param>
        /// <returns>a new EvmHookCall instance</returns>
        static EvmHookCall FromProtobuf(Proto.EvmHookCall proto)
        {
            return new EvmHookCall(proto.GetData().ToByteArray(), proto.GetGasLimit());
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            EvmHookCall that = (EvmHookCall)o;
            return gasLimit == that.gasLimit && java.util.Equals(data, that.data);
        }

        public override int GetHashCode()
        {
            int result = java.util.HashCode.Combine(data);
            result = 31 * result + Long.GetHashCode(gasLimit);
            return result;
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("data", data).Add("gasLimit", gasLimit).ToString();
        }
    }
}