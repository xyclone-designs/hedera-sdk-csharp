using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Base class for custom fees.
	 */
	internal abstract class CustomFee
	{
		/**
		 * The account to receive the custom fee
		 */
		protected AccountId? FeeCollectorAccountId = null;
		/**
		 * If true, exempts all the token's fee collection accounts from this fee
		 */
		protected bool AllCollectorsAreExempt = false;

		/**
		 * Constructor.
		 */
		internal CustomFee() { }

		/**
		 * Convert byte array to a custom fee object.
		 *
		 * @param bytes                     the byte array
		 * @return                          the converted custom fee object
		 * @       when there is an issue with the protobuf
		 */
		public static CustomFee FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.CustomFee.Parser.ParseFrom(bytes));
		}
		/**
		 * Convert the protobuf object to a custom fee object.
		 *
		 * @param customFee                 protobuf response object
		 * @return                          the converted custom fee object
		 */
		public static CustomFee FromProtobuf(Proto.CustomFee customFee)
		{
			CustomFee outFee = FromProtobufInner(customFee);

			if (customFee.FeeCollectorAccountId is not null)
				outFee.FeeCollectorAccountId = AccountId.FromProtobuf(customFee.FeeCollectorAccountId);

			outFee.AllCollectorsAreExempt = customFee.AllCollectorsAreExempt;

			return outFee;
		}
		public static CustomFee FromProtobufInner(Proto.CustomFee customFee)
		{
			switch (customFee.FeeCase)
			{
				case Proto.CustomFee.FeeOneofCase.FixedFee:
					return CustomFixedFee.FromProtobuf(customFee.FixedFee);

				case Proto.CustomFee.FeeOneofCase.FractionalFee:
					return CustomFractionalFee.FromProtobuf(customFee.FractionalFee);

				case Proto.CustomFee.FeeOneofCase.RoyaltyFee:
					return CustomRoyaltyFee.FromProtobuf(customFee.RoyaltyFee);

				default:
					throw new ArgumentException("CustomFee#FromProtobuf: unhandled fee case: " + customFee.FeeCase);
			}
		}
		/**
		 * Create a new copy of a custom fee list.
		 *
		 * @param customFees                existing custom fee list
		 * @return                          new custom fee list
		 */
		public static List<CustomFee> DeepCloneList(List<CustomFee> customFees)
		{
			return [.. customFees.Select(_ => _.DeepClone())];
		}

		/**
		 * Create a deep clone.
		 *
		 * @return                          the correct cloned fee type
		 */
		public abstract CustomFee DeepClone();
		/**
		 * Create the protobuf.
		 *
		 * @return                              the protobuf for the custom fee object
		 */
		public abstract Proto.CustomFee ToProtobuf();

		/**
		 * Verify the validity of the client object.
		 *
		 * @param client                    the configured client
		 * @     if entity ID is formatted poorly
		 */
		public virtual void ValidateChecksums(Client client)
		{
			FeeCollectorAccountId?.ValidateChecksum(client);
		}

		/**
		 * Finalize the builder into the protobuf.
		 *
		 * @param customFeeBuilder              the builder object
		 * @return                              the protobuf
		 */
		protected Proto.CustomFee FinishToProtobuf(Proto.CustomFee customFeeBuilder)
		{
			if (FeeCollectorAccountId != null)
				customFeeBuilder.FeeCollectorAccountId = FeeCollectorAccountId.ToProtobuf();

			customFeeBuilder.AllCollectorsAreExempt = AllCollectorsAreExempt;

			return customFeeBuilder;
		}

		/**
		 * Create the byte array.
		 *
		 * @return                              the byte array representing the protobuf
		 */
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
	}
}