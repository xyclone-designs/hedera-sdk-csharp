// SPDX-License-Identifier: Apache-2.0
using System;
using System.Numerics;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated- User mirror nodes for contract traceability instead
    /// 
    /// A storage slot change description.
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/smart-contracts/contractcalllocal#storagechange">Hedera Documentation</a>
    /// </remarks>
    [Obsolete("Obsolete")]
    public class StorageChange
    {
        /// <summary>
        /// The storage slot changed. Up to 32 bytes, big-endian, zero bytes left trimmed
        /// </summary>
        public readonly BigInteger slot;
        /// <summary>
        /// The value read from the storage slot. Up to 32 bytes, big-endian, zero
        /// bytes left trimmed. Because of the way SSTORE operations are charged
        /// the slot is always read before being written to
        /// </summary>
        public readonly BigInteger valueRead;
        /// <summary>
        /// The new value written to the slot. Up to 32 bytes, big-endian, zero
        /// bytes left trimmed. If a value of zero is written the valueWritten
        /// will be present but the inner value will be absent. If a value was
        /// read and not written this value will not be present.
        /// </summary>
        public readonly BigInteger valueWritten;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="slot">the storage slot charged</param>
        /// <param name="valueRead">the value read</param>
        /// <param name="valueWritten">the value written</param>
        StorageChange(BigInteger slot, BigInteger valueRead, BigInteger valueWritten)
        {
            this.slot = slot;
            this.valueRead = valueRead;
            this.valueWritten = valueWritten;
        } // /**
        //  * Create a storage charge from a protobuf.
        //  *
        //  * @param storageChangeProto        the protobuf
        //  * @return                          the new storage charge object
        //  */
        // static StorageChange fromProtobuf(Proto.StorageChange storageChangeProto) {
        //     return new StorageChange(
        //         new BigInteger(storageChangeProto.getSlot().toByteArray()),
        //         new BigInteger(storageChangeProto.getValueRead().toByteArray()),
        //         storageChangeProto.hasValueWritten() ? (
        //             storageChangeProto.getValueWritten().getValue().size() == 0 ?
        //                 BigInteger.ZERO :
        //                 new BigInteger(storageChangeProto.getValueWritten().getValue().toByteArray())
        //         ) : null
        //     );
        // }
        //
        // /**
        //  * Create a storage charge from a byte array.
        //  *
        //  * @param bytes                     the byte array
        //  * @return                          the new storage charge object
        //  * @throws InvalidProtocolBufferException       when there is an issue with the protobuf
        //  */
        // public static StorageChange fromBytes(byte[] bytes) throws InvalidProtocolBufferException {
        //     return fromProtobuf(Proto.StorageChange.parseFrom(bytes));
        // }
        //
        // /**
        //  * Create the byte array.
        //  *
        //  * @return                          the byte array representation
        //  */
        // Proto.StorageChange toProtobuf() {
        //     var builder = Proto.StorageChange.newBuilder()
        //         .setSlot(ByteString.copyFrom(slot.toByteArray()))
        //         .setValueRead(ByteString.copyFrom(valueRead.toByteArray()));
        //     if (valueWritten != null) {
        //         if (valueWritten.equals(BigInteger.ZERO)) {
        //             builder.setValueWritten(BytesValue.newBuilder().setValue(ByteString.EMPTY).build());
        //         } else {
        //
        // builder.setValueWritten(BytesValue.newBuilder().setValue(ByteString.copyFrom(valueWritten.toByteArray())).build());
        //         }
        //     }
        //     return proto;
        // }
        //
        // /**
        //  * Create the byte array.
        //  *
        //  * @return                          the byte array representation
        //  */
        // public byte[] toBytes() {
        //     return toProtobuf().toByteArray();
        // }
    }
}