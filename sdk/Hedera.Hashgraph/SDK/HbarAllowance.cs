using Google.Protobuf;

namespace Hedera.Hashgraph.SDK
{
	/**
     * An approved allowance of hbar transfers for a spender.
     *
     * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/cryptoallowance">Hedera Documentation</a>
     */
    public class HbarAllowance 
    {
        /**
         * Constructor.
         * @param ownerAccountId            the owner granting the allowance
         * @param spenderAccountId          the spender
         * @param amount                    the amount of hbar
         */
        HbarAllowance(AccountId? ownerAccountId, AccountId? spenderAccountId, Hbar? amount)
        {
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            Amount = amount;
        }

		/**
         * The amount of the spender's allowance in tinybars
         */
		public Hbar? Amount { get; }
		/**
         * The account ID of the hbar owner (ie. the grantor of the allowance)
         */
		public AccountId? OwnerAccountId { get; }
		/**
         * The account ID of the spender of the hbar allowance
         */
		public AccountId? SpenderAccountId { get; }

		/**
         * Create a hbar allowance from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the new hbar allowance
         * @       when there is an issue with the protobuf
         */
		public static HbarAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CryptoAllowance.Parser.ParseFrom(bytes));
		}
		/**
         * Create a hbar allowance from a crypto allowance protobuf.
         *
         * @param allowanceProto            the crypto allowance protobuf
         * @return                          the new hbar allowance
         */
		public static HbarAllowance FromProtobuf(Proto.CryptoAllowance allowanceProto) 
        {
            return new HbarAllowance(
                allowanceProto.Owner is not null ? AccountId.FromProtobuf(allowanceProto.Owner) : null,
                allowanceProto.Spender is not null ? AccountId.FromProtobuf(allowanceProto.Spender) : null,
                Hbar.FromTinybars(allowanceProto.Amount));
        }
        /**
         * Create a hbar allowance from a granted crypto allowance protobuf.
         *
         * @param allowanceProto            the granted crypto allowance protobuf
         * @return                          the new hbar allowance
         */
        public static HbarAllowance FromProtobuf(Proto.GrantedCryptoAllowance allowanceProto) 
        {
            return new HbarAllowance(
                null,
                AccountId.FromProtobuf(allowanceProto.Spender),
                Hbar.FromTinybars(allowanceProto.Amount));
        }

		/**
         * Create the byte array.
         *
         * @return                          a byte array representation
         */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/**
         * Validate that the client is configured correctly.
         *
         * @param client                    the client to verify
         * @     if entity ID is formatted poorly
         */
		public void ValidateChecksums(Client client)  
        {
			OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId?.ValidateChecksum(client);
		}

		/**
         * Convert a crypto allowance into a protobuf.
         *
         * @return                          the protobuf
         */
		public Proto.CryptoAllowance ToProtobuf() 
        {
            Proto.CryptoAllowance protobuf = new();

            if (Amount is not null) protobuf.Amount = Amount.ToTinybars();
            if (OwnerAccountId is not null) protobuf.Owner = OwnerAccountId.ToProtobuf();
            if (SpenderAccountId is not null) protobuf.Spender = SpenderAccountId.ToProtobuf();

            return protobuf;
        }
        /**
         * Convert a crypto allowance into a granted crypto allowance protobuf.
         *
         * @return                          the granted crypto allowance
         */
        public Proto.GrantedCryptoAllowance ToGrantedProtobuf()
        {
			Proto.GrantedCryptoAllowance protobuf = new();

			if (Amount is not null) protobuf.Amount = Amount.ToTinybars();
			if (SpenderAccountId is not null) protobuf.Spender = SpenderAccountId.ToProtobuf();

			return protobuf;
		}
    }
}