// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
	/// <include file="ContractStateChange.cs.xml" path='docs/member[@name="T:ContractStateChange"]/*' />
	[Obsolete("Obsolete")]
    public class ContractStateChange
    {
        /// <include file="ContractStateChange.cs.xml" path='docs/member[@name="F:.ContractId"]/*' />
        public readonly ContractId ContractId;
        /// <include file="ContractStateChange.cs.xml" path='docs/member[@name="F:.StorageChanges"]/*' />
        public readonly List<StorageChange> StorageChanges;
        /// <include file="ContractStateChange.cs.xml" path='docs/member[@name="M:ContractStateChange(ContractId,System.Collections.Generic.IEnumerable{StorageChange})"]/*' />
        ContractStateChange(ContractId contractId, IEnumerable<StorageChange> storageChanges)
        {
            ContractId = contractId;
            StorageChanges = [.. storageChanges];
        } // /**
        //  * Create contract stage change object from protobuf.
        //  *
        //  * @param stateChangeProto          the protobuf
        //  * @return                          the contract stage change object
        //  */
        // static ContractStateChange fromProtobuf(Proto.ContractStateChange stateChangeProto) {
        //     List<StorageChange> storageChanges = new List<>(stateChangeProto.getStorageChangesCount());
        //     for (var storageChangeProto : stateChangeProto.getStorageChangesList()) {
        //         storageChanges.add(StorageChange.fromProtobuf(storageChangeProto));
        //     }
        //     return new ContractStateChange(ContractId.fromProtobuf(stateChangeProto.getContractID()), storageChanges);
        // }
        //
        // /**
        //  * Create contract stage change object from byte array.
        //  *
        //  * @param bytes                     the byte array
        //  * @return                          the contract stage change object
        //  * @throws InvalidProtocolBufferException       when there is an issue with the protobuf
        //  */
        // public static ContractStateChange fromBytes(byte[] bytes) throws InvalidProtocolBufferException {
        //     return fromProtobuf(Proto.ContractStateChange.parseFrom(bytes));
        // }
        //
        // /**
        //  * Create the protobuf.
        //  *
        //  * @return                          the protobuf representation
        //  */
        // Proto.ContractStateChange toProtobuf() {
        //     var builder = Proto.ContractStateChange.newBuilder()
        //         .setContractID(contractId.toProtobuf());
        //     for (var storageChange : storageChanges) {
        //         builder.addStorageChanges(storageChange.toProtobuf());
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