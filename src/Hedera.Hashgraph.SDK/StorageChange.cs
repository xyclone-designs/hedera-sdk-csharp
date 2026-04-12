// SPDX-License-Identifier: Apache-2.0
using System;
using System.Numerics;

namespace Hedera.Hashgraph.SDK
{
	/// <include file="StorageChange.cs.xml" path='docs/member[@name="T:StorageChange"]/*' />
	[Obsolete("Obsolete")]
    public class StorageChange
    {
        /// <include file="StorageChange.cs.xml" path='docs/member[@name="F:.Slot"]/*' />
        public readonly BigInteger Slot;
        /// <include file="StorageChange.cs.xml" path='docs/member[@name="F:.ValueRead"]/*' />
        public readonly BigInteger ValueRead;
        /// <include file="StorageChange.cs.xml" path='docs/member[@name="F:.ValueWritten"]/*' />
        public readonly BigInteger ValueWritten;
        /// <include file="StorageChange.cs.xml" path='docs/member[@name="M:StorageChange(BigInteger,BigInteger,BigInteger)"]/*' />
        StorageChange(BigInteger slot, BigInteger valueRead, BigInteger valueWritten)
        {
            Slot = slot;
            ValueRead = valueRead;
            ValueWritten = valueWritten;
        } // /**

        //  * Create a storage charge from a protobuf.
        //  *
        //  * @param storageChangeProto        the protobuf
        //  * @return                          the new storage charge object
        //  */
        // static StorageChange fromProtobuf(Proto.Services.StorageChange storageChangeProto) {
        //     return new StorageChange(
        //         new BigInteger(storageChangeProto.Services.getSlot().toByteArray()),
        //         new BigInteger(storageChangeProto.Services.getValueRead().toByteArray()),
        //         storageChangeProto.Services.hasValueWritten() ? (
        //             storageChangeProto.Services.getValueWritten().getValue().Length == 0 ?
        //                 BigInteger.Zero :
        //                 new BigInteger(storageChangeProto.Services.getValueWritten().getValue().toByteArray())
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
        //     return fromProtobuf(Proto.Services.StorageChange.parseFrom(bytes));
        // }
        //
        // /**
        //  * Create the byte array.
        //  *
        //  * @return                          the byte array representation
        //  */
        // Proto.Services.StorageChange toProtobuf() {
        //     var builder = Proto.Services.StorageChange.newBuilder()
        //         .setSlot(ByteString.copyFrom(slot.toByteArray()))
        //         .setValueRead(ByteString.copyFrom(valueRead.toByteArray()));
        //     if (valueWritten != null) {
        //         if (valueWritten.equals(BigInteger.Zero)) {
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
