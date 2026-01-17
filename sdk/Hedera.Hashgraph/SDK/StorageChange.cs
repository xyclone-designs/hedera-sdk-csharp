using System;
using System.Numerics;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * @deprecated - User mirror nodes for contract traceability instead
	 *
	 * A storage slot change description.
	 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/smart-contracts/contractcalllocal#storagechange">Hedera Documentation</a>
	 */
	[Obsolete]
	public class StorageChange
	{
		/**
		 * Constructor.
		 *
		 * @param slot                      the storage slot charged
		 * @param valueRead                 the value read
		 * @param valueWritten              the value written
		 */
		StorageChange(BigInteger slot, BigInteger valueRead, BigInteger? valueWritten)
		{
			Slot = slot;
			ValueRead = valueRead;
			ValueWritten = valueWritten;
		}

		// /**
		//  * Create a storage charge from a protobuf.
		//  *
		//  * @param storageChangeProto        the protobuf
		//  * @return                          the new storage charge object
		//  */
		// static StorageChange FromProtobuf(Proto.StorageChange storageChangeProto) {
		//     return new StorageChange(
		//         new BigInteger(storageChangeProto.getSlot().ToByteArray()),
		//         new BigInteger(storageChangeProto.getValueRead().ToByteArray()),
		//         storageChangeProto.hasValueWritten() ? (
		//             storageChangeProto.getValueWritten().getValue().size() == 0 ?
		//                 BigInteger.ZERO :
		//                 new BigInteger(storageChangeProto.getValueWritten().getValue().ToByteArray())
		//         ) : null
		//     );
		// }
		//
		// /**
		//  * Create a storage charge from a byte array.
		//  *
		//  * @param bytes                     the byte array
		//  * @return                          the new storage charge object
		//  * @       when there is an issue with the protobuf
		//  */
		// public static StorageChange FromBytes(byte[] bytes)  {
		//     return FromProtobuf(Proto.StorageChange.Parser.ParseFrom(bytes));
		// }
		//
		// /**
		//  * Create the byte array.
		//  *
		//  * @return                          the byte array representation
		//  */
		// Proto.StorageChange ToProtobuf() {
		//     var builder = Proto.StorageChange.newBuilder()
		//         .setSlot(ByteString.copyFrom(slot.ToByteArray()))
		//         .setValueRead(ByteString.copyFrom(valueRead.ToByteArray()));
		//     if (valueWritten != null) {
		//         if (valueWritten.equals(BigInteger.ZERO)) {
		//             builder.setValueWritten(BytesValue.newBuilder().setValue(ByteString.EMPTY).build());
		//         } else {
		//
		// builder.setValueWritten(BytesValue.newBuilder().setValue(ByteString.copyFrom(valueWritten.ToByteArray())).build());
		//         }
		//     }
		//     return builder.build();
		// }
		//
		// /**
		//  * Create the byte array.
		//  *
		//  * @return                          the byte array representation
		//  */
		// public byte[] ToBytes() {
		//     return ToProtobuf().ToByteArray();
		// }

		/**
		 * The storage slot changed. Up to 32 bytes, big-endian, zero bytes left trimmed
		 */
		public BigInteger Slot { get; }

		/**
		 * The value read from the storage slot. Up to 32 bytes, big-endian, zero
		 * bytes left trimmed. Because of the way SSTORE operations are charged
		 * the slot is always read before being written to
		 */
		public BigInteger ValueRead { get; }

		/**
		 * The new value written to the slot. Up to 32 bytes, big-endian, zero
		 * bytes left trimmed. If a value of zero is written the valueWritten
		 * will be present but the inner value will be absent. If a value was
		 * read and not written this value will not be present.
		 */
		public BigInteger? ValueWritten { get; }
	}
}