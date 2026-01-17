using System.Diagnostics.Contracts;

namespace Hedera.Hashgraph.SDK
{
	/**
 * @deprecated - User mirror nodes for contract traceability instead
 *
 * The storage changes to a smart contract's storage as a side effect of the function call.
 * See <a href="https://docs.hedera.com/guides/docs/hedera-api/smart-contracts/contractcalllocal#contractstatechange">Hedera Documentation</a>
 */
	[Obsolete]
public class ContractStateChange
	{
		/**
		 * The contract to which the storage changes apply to
		 */
		public readonly ContractId contractId;

    /**
     * The list of storage changes
     */
    public readonly List<StorageChange> storageChanges;

		/**
		 * Constructor.
		 *
		 * @param contractId                the contract id
		 * @param storageChanges            the list of storage change objects
		 */
		ContractStateChange(ContractId contractId, List<StorageChange> storageChanges)
		{
			this.contractId = contractId;
			this.storageChanges = storageChanges;
		}

		// /**
		//  * Create contract stage change object from protobuf.
		//  *
		//  * @param stateChangeProto          the protobuf
		//  * @return                          the contract stage change object
		//  */
		// static ContractStateChange FromProtobuf(Proto.ContractStateChange stateChangeProto) {
		//     List<StorageChange> storageChanges = new ArrayList<>(stateChangeProto.getStorageChangesCount());
		//     for (var storageChangeProto : stateChangeProto.getStorageChangesList()) {
		//         storageChanges.Add(StorageChange.FromProtobuf(storageChangeProto));
		//     }
		//     return new ContractStateChange(ContractId.FromProtobuf(stateChangeProto.getContractID()), storageChanges);
		// }
		//
		// /**
		//  * Create contract stage change object from byte array.
		//  *
		//  * @param bytes                     the byte array
		//  * @return                          the contract stage change object
		//  * @       when there is an issue with the protobuf
		//  */
		// public static ContractStateChange FromBytes(byte[] bytes)  {
		//     return FromProtobuf(Proto.ContractStateChange.Parser.ParseFrom(bytes));
		// }
		//
		// /**
		//  * Create the protobuf.
		//  *
		//  * @return                          the protobuf representation
		//  */
		// Proto.ContractStateChange ToProtobuf() {
		//     var builder = Proto.ContractStateChange.newBuilder()
		//         .setContractID(contractId.ToProtobuf());
		//     for (var storageChange : storageChanges) {
		//         builder.AddStorageChanges(storageChange.ToProtobuf());
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
	}

}