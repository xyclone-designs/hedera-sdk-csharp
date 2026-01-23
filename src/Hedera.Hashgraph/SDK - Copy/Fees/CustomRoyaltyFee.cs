// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Custom royalty fee utility class.
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#royalty-fee">Hedera Documentation</a>
    /// </summary>
    public class CustomRoyaltyFee : CustomFeeBase<CustomRoyaltyFee>
    {
        private long numerator = 0;
        private long denominator = 1;
        private CustomFixedFee fallbackFee = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomRoyaltyFee()
        {
        }

		/// <summary>
		/// Create a custom royalty fee from a royalty fee protobuf.
		/// </summary>
		/// <param name="royaltyFee">the royalty fee protobuf</param>
		/// <returns>                         the new royalty fee object</returns>
		public static CustomRoyaltyFee FromProtobuf(Proto.RoyaltyFee royaltyFee)
        {
            var fraction = royaltyFee.GetExchangeValueFraction();
            var returnFee = new CustomRoyaltyFee().SetNumerator(fraction.GetNumerator()).SetDenominator(fraction.GetDenominator());
            if (royaltyFee.HasFallbackFee())
            {
                returnFee.fallbackFee = CustomFixedFee.FromProtobuf(royaltyFee.GetFallbackFee());
            }

            return returnFee;
        }

		public override CustomRoyaltyFee DeepCloneSubclass()
        {
            return new CustomRoyaltyFee
            {
				numerator = numerator,
				denominator = denominator,
				fallbackFee = fallbackFee?.DeepCloneSubclass(),
				FeeCollectorAccountId = FeeCollectorAccountId,
				AllCollectorsAreExempt = AllCollectorsAreExempt,
			};
        }

        /// <summary>
        /// Extract the numerator.
        /// </summary>
        /// <returns>                         the numerator</returns>
        public virtual long GetNumerator()
        {
            return numerator;
        }

        /// <summary>
        /// Assign the numerator.
        /// </summary>
        /// <param name="numerator">the numerator</param>
        /// <returns>{@code this}</returns>
        public virtual CustomRoyaltyFee SetNumerator(long numerator)
        {
            numerator = numerator;
            return this;
        }

        /// <summary>
        /// Extract the denominator.
        /// </summary>
        /// <returns>                         the denominator</returns>
        public virtual long GetDenominator()
        {
            return denominator;
        }

        /// <summary>
        /// Assign the denominator can not be zero (0).
        /// </summary>
        /// <param name="denominator">the denominator</param>
        /// <returns>{@code this}</returns>
        public virtual CustomRoyaltyFee SetDenominator(long denominator)
        {
            denominator = denominator;
            return this;
        }

        /// <summary>
        /// The fallback fee is a fixed fee that is charged to the NFT receiver
        /// when there is no fungible value exchanged with the sender of the NFT.
        /// </summary>
        /// <param name="fallbackFee">the fallback fee amount</param>
        /// <returns>{@code this}</returns>
        public virtual CustomRoyaltyFee SetFallbackFee(CustomFixedFee fallbackFee)
        {
            Objects.RequireNonNull(fallbackFee);
            fallbackFee = fallbackFee.DeepCloneSubclass();
            return this;
        }

        /// <summary>
        /// Get the fallback fixed fee.
        /// </summary>
        /// <returns>the fallback fixed fee</returns>
        public virtual CustomFixedFee GetFallbackFee()
        {
            return fallbackFee != null ? fallbackFee.DeepCloneSubclass() : null;
        }

        /// <summary>
        /// Convert the royalty fee object to a protobuf.
        /// </summary>
        /// <returns>                         the protobuf object</returns>
        public virtual RoyaltyFee ToRoyaltyFeeProtobuf()
        {
            var royaltyFeeBuilder = RoyaltyFee.NewBuilder().SetExchangeValueFraction(Fraction.NewBuilder().SetNumerator(numerator).SetDenominator(denominator));
            if (fallbackFee != null)
            {
                royaltyFeeBuilder.FallbackFee = fallbackFee.ToFixedFeeProtobuf();
            }

            return royaltyFeeBuilder.Build();
        }

        public override Proto.CustomFee ToProtobuf()
        {
            return FinishToProtobuf(new Proto.CustomFee
            {
				RoyaltyFee = ToRoyaltyFeeProtobuf()
			});
        }
    }
}