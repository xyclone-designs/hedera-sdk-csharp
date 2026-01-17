using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Specifies details of a call to an EVM hook.
	 * <p>
	 * This class represents the evm_hook_call field in the HookCall protobuf message.
	 * It contains the call data and gas limit for executing an EVM hook.
	 */
	public class EvmHookCall
	{
		/**
		 * Create a new EvmHookCall.
		 *
		 * @param data the call data to pass to the hook via the IHieroHook.HookContext#data field
		 * @param gasLimit the gas limit to use for the hook execution
		 */
		public EvmHookCall(byte[] data, ulong? gasLimit)
		{
			Data = [];
			DataInternal = data;
			GasLimit = gasLimit;

			Array.Copy(DataInternal, Data, DataInternal.Length);
		}

		public ulong? GasLimit { get; }
		public byte[] Data { get; }
		private byte[] DataInternal { get; }

		/**
		 * Convert this EvmHookCall to a protobuf message.
		 *
		 * @return the protobuf EvmHookCall
		 */
		public Proto.EvmHookCall ToProtobuf()
		{
			Proto.EvmHookCall protobuf = new ()
			{
				Data = ByteString.CopyFrom(Data),
			};

			if (GasLimit.HasValue) protobuf.GasLimit = GasLimit.Value;

			return protobuf;
		}
		/**
		 * Create an EvmHookCall from a protobuf message.
		 *
		 * @param proto the protobuf EvmHookCall
		 * @return a new EvmHookCall instance
		 */
		public static EvmHookCall FromProtobuf(Proto.EvmHookCall proto)
		{
			return new EvmHookCall(proto.Data.ToByteArray(), proto.GasLimit);
		}

		public override int GetHashCode()
		{
			int result = Data.GetHashCode();
			result = 31 * result + GasLimit?.GetHashCode() ?? 0;
			return result;
		}
		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			EvmHookCall that = (EvmHookCall)obj;
			
			return GasLimit == that.GasLimit && Equals(Data, that.Data);
		}
	}
}