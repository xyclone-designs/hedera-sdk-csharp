using Google.Protobuf;

using System;

namespace Hedera.Hashgraph.SDK
{
	public sealed partial class ContractFunctionParameters
	{
		private sealed class Argument
		{
			public readonly string Type;
			public readonly bool IsDynamic;
			public readonly ByteString Value;

			public Argument(string type, ByteString value, bool isDynamic)
			{
				Type = type;

				if (!isDynamic && value.Length != 32)
					throw new ArgumentException("value argument that was not 32 bytes");

				Value = value;
				IsDynamic = isDynamic;
			}
		}
	}
}