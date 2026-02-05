// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @deprecated- User mirror nodes for contract traceability instead
    /// 
    /// The storage changes to a smart contract's storage as a side effect of the function call.
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/smart-contracts/contractcalllocal#contractstatechange">Hedera Documentation</a>
    /// </remarks>
    [Obsolete("Obsolete")]
    public class ContractStateChange
    {
        /// <summary>
        /// The contract to which the storage changes apply to
        /// </summary>
        public readonly ContractId ContractId;
        /// <summary>
        /// The list of storage changes
        /// </summary>
        public readonly IList<StorageChange> StorageChanges;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contractId">the contract id</param>
        /// <param name="storageChanges">the list of storage change objects</param>
        ContractStateChange(ContractId contractId, IList<StorageChange> storageChanges)
        {
            ContractId = contractId;
            StorageChanges = storageChanges;
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