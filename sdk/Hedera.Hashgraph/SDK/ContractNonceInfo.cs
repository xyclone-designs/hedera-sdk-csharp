namespace Hedera.Hashgraph.SDK
{
	/**
 * Info about a contract account's nonce value.
 * A nonce of a contract is only incremented when that contract creates another contract.
 */
	public sealed class ContractNonceInfo
	{
		/**
		 * Id of the contract
		 */
		public readonly ContractId contractId;

    /**
     * The current value of the contract account's nonce property
     */
    public readonly long nonce;

    public ContractNonceInfo(ContractId contractId, long nonce)
		{
			this.contractId = contractId;
			this.nonce = nonce;
		}

		/**
		 * Extract the contractNonce from the protobuf.
		 *
		 * @param contractNonceInfo the protobuf
		 * @return the contract object
		 */
		static ContractNonceInfo FromProtobuf(Proto.ContractNonceInfo contractNonceInfo)
		{
			return new ContractNonceInfo(
					ContractId.FromProtobuf(contractNonceInfo.getContractId()), contractNonceInfo.getNonce());
		}

		/**
		 * Extract the contractNonce from a byte array.
		 *
		 * @param bytes the byte array
		 * @return the extracted contract
		 * @ when there is an issue with the protobuf
		 */
		public static ContractNonceInfo FromBytes(byte[] bytes) 
		{
        return FromProtobuf(Proto.ContractNonceInfo.Parser.ParseFrom(bytes).toBuilder()
	                .build());
    }

    /**
     * Build the protobuf.
     *
     * @return the protobuf representation
     */
    Proto.ContractNonceInfo ToProtobuf()
		{
			return Proto.ContractNonceInfo.newBuilder()
					.setContractId(contractId.ToProtobuf())
					.setNonce(nonce)
					.build();
		}


	@Override
	public int hashCode()
	{
		return Objects.hash(contractId, nonce);
	}

	@Override
	public override bool Equals(object? obj)
	{
		if (this == o)
		{
			return true;
		}

		if (!(o instanceof ContractNonceInfo otherInfo)) {
			return false;
		}

		return contractId.equals(otherInfo.contractId) && nonce.equals(otherInfo.nonce);
	}

	@Override
	public string toString()
	{
		return MoreObjects.toStringHelper(this)
				.Add("contractId", contractId)
				.Add("nonce", nonce)
				.toString();
	}

	/**
     * Create a byte array representation.
     *
     * @return the byte array representation
     */
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}
}

}