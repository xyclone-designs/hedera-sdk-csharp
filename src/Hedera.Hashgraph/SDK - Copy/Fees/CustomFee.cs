// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Transactions.Account;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Base class for custom fees.
    /// </summary>
    public abstract class CustomFee
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomFee()
        {
        }

		/// <summary>
		/// The account to receive the custom fee
		/// </summary>
		public virtual AccountId? FeeCollectorAccountId { get; protected set; }
		/// <summary>
		/// If true, exempts all the token's fee collection accounts from this fee.
		/// (The token's treasury and the above fee_collector_account_id will always
		/// be exempt. Please see <a href="https://hips.hedera.com/hip/hip-573">HIP-573</a>
		/// for details.)
		/// </summary>
		public virtual bool AllCollectorsAreExempt { get; protected set; }

		/// <summary>
		/// Convert the protobuf object to a custom fee object.
		/// </summary>
		/// <param name="customFee">protobuf response object</param>
		/// <returns>                         the converted custom fee object</returns>
		public static CustomFee FromProtobufInner(Proto.CustomFee customFee)
        {
            return customFee.FeeCase switch
            {
                Proto.CustomFee.FeeOneofCase.FixedFee => CustomFixedFee.FromProtobuf(customFee.FixedFee),
                Proto.CustomFee.FeeOneofCase.FractionalFee => CustomFractionalFee.FromProtobuf(customFee.FractionalFee),
                Proto.CustomFee.FeeOneofCase.RoyaltyFee => CustomRoyaltyFee.FromProtobuf(customFee.RoyaltyFee),

                _ => throw new InvalidOperationException("CustomFee#fromProtobuf: unhandled fee case: " + customFee.FeeCase),
            };
        }
        public static CustomFee FromProtobuf(Proto.CustomFee customFee)
        {
            var outFee = FromProtobufInner(customFee);

            if (customFee.FeeCollectorAccountId != null)
				outFee.FeeCollectorAccountId = AccountId.FromProtobuf(customFee.FeeCollectorAccountId);

			outFee.AllCollectorsAreExempt = customFee.AllCollectorsAreExempt;

            return outFee;
        }
        /// <summary>
        /// Convert byte array to a custom fee object.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the converted custom fee object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static CustomFee FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.CustomFee.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create a new copy of a custom fee list.
        /// </summary>
        /// <param name="customFees">existing custom fee list</param>
        /// <returns>                         new custom fee list</returns>
        public static IList<CustomFee> DeepCloneList(IList<CustomFee> customFees)
        {
            var returnCustomFees = new List<CustomFee>(customFees.Count);
            foreach (var fee in customFees)
            {
                returnCustomFees.Add(fee.DeepClone());
            }

            return returnCustomFees;
        }

		/// <summary>
		/// Create a deep clone.
		/// </summary>
		/// <returns>                         the correct cloned fee type</returns>
		public abstract CustomFee DeepClone();
        /// <summary>
        /// Verify the validity of the client object.
        /// </summary>
        /// <param name="client">the configured client</param>
        /// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
        public virtual void ValidateChecksums(Client client)
        {
            FeeCollectorAccountId?.ValidateChecksum(client);
        }

        /// <summary>
        /// Finalize the builder into the protobuf.
        /// </summary>
        /// <param name="customFeeBuilder">the builder object</param>
        /// <returns>                             the protobuf</returns>
        protected virtual Proto.CustomFee FinishToProtobuf(Proto.CustomFee customFeeBuilder)
        {
            if (FeeCollectorAccountId != null)
				customFeeBuilder.FeeCollectorAccountId = FeeCollectorAccountId.ToProtobuf();

			customFeeBuilder.AllCollectorsAreExempt = AllCollectorsAreExempt;
            return customFeeBuilder;
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                             the protobuf for the custom fee object</returns>
        public abstract Proto.CustomFee ToProtobuf();
        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                             the byte array representing the protobuf</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}