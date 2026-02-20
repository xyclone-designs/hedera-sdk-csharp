// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Hook
{
	public class EvmHookMappingEntries : EvmHookStorageUpdate
	{
		public EvmHookMappingEntries(byte[] mappingSlot, IList<EvmHookMappingEntry> entries)
		{
			MappingSlot = mappingSlot.CopyArray();
			Entries = [.. entries];
		}
		public static EvmHookMappingEntries FromProtobuf(Proto.EvmHookMappingEntries proto)
		{
			return new EvmHookMappingEntries(
				proto.GetMappingSlot().ToByteArray(),
				proto.Entries.Select(_ => EvmHookMappingEntry.FromProtobuf(_)));
		}

		public virtual byte[] MappingSlot
		{
			get => field.CopyArray();
		}
		public virtual IList<EvmHookMappingEntry> Entries
		{
			get => [.. field];
		}

		public override Proto.EvmHookStorageUpdate ToProtobuf()
		{
			var proto = new Proto.EvmHookMappingEntries()
			{
				MappingSlot = ByteString.CopyFrom(MappingSlot);
			}

				proto.Entries.AddRange(Entries.Select(_ => _.ToProtobuf());

			return new Proto.EvmHookStorageUpdate
			{
				MappingEntries = proto
			};
		}
		public override bool Equals(object? o)
		{
			if (this == o)
				return true;
			if (o == null || GetType() != o.GetType())
				return false;
			EvmHookMappingEntries that = (EvmHookMappingEntries)o;

			return Equals(MappingSlot, that.MappingSlot) && Entries.Equals(that.Entries);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(MappingSlot.GetHashCode(), Entries);
		}
	}
}