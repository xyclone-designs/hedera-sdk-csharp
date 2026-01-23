// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// Custom fractional fee utility class.
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
    /// </summary>
    public class CustomFractionalFee : CustomFeeBase<CustomFractionalFee>
    {
        private long numerator = 0;
        private long denominator = 1;
        private long min = 0;
        private long max = 0;
        private FeeAssessmentMethod assessmentMethod = FeeAssessmentMethod.INCLUSIVE;
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomFractionalFee()
        {
        }

        /// <summary>
        /// Create a custom fractional fee from a fee protobuf.
        /// </summary>
        /// <param name="fractionalFee">the fractional fee protobuf</param>
        /// <returns>the new custom fractional fee object</returns>
        static CustomFractionalFee FromProtobuf(FractionalFee fractionalFee)
        {
            var fraction = fractionalFee.GetFractionalAmount();
            return new CustomFractionalFee().SetNumerator(fraction.GetNumerator()).SetDenominator(fraction.GetDenominator()).SetMin(fractionalFee.GetMinimumAmount()).SetMax(fractionalFee.GetMaximumAmount()).SetAssessmentMethod(FeeAssessmentMethod.ValueOf(fractionalFee.GetNetOfTransfers()));
        }

        override CustomFractionalFee DeepCloneSubclass()
        {
            return new CustomFractionalFee().SetNumerator(numerator).SetDenominator(denominator).SetMin(min).SetMax(max).SetAssessmentMethod(assessmentMethod).FinishDeepClone(this);
        }

        /// <summary>
        /// Extract the numerator.
        /// </summary>
        /// <returns>the numerator</returns>
        public virtual long GetNumerator()
        {
            return numerator;
        }

        /// <summary>
        /// Assign the numerator.
        /// </summary>
        /// <param name="numerator">the numerator</param>
        /// <returns>{@code this}</returns>
        public virtual CustomFractionalFee SetNumerator(long numerator)
        {
            numerator = numerator;
            return this;
        }

        /// <summary>
        /// Extract the denominator.
        /// </summary>
        /// <returns>the denominator</returns>
        public virtual long GetDenominator()
        {
            return denominator;
        }

        /// <summary>
        /// Assign the denominator can not be zero (0).
        /// </summary>
        /// <param name="denominator">the denominator</param>
        /// <returns>{@code this}</returns>
        public virtual CustomFractionalFee SetDenominator(long denominator)
        {
            denominator = denominator;
            return this;
        }

        /// <summary>
        /// Extract the minimum fee amount.
        /// </summary>
        /// <returns>the minimum fee amount</returns>
        public virtual long GetMin()
        {
            return min;
        }

        /// <summary>
        /// Assign the minimum fee amount.
        /// </summary>
        /// <param name="min">the fee amount</param>
        /// <returns>{@code this}</returns>
        public virtual CustomFractionalFee SetMin(long min)
        {
            min = min;
            return this;
        }

        /// <summary>
        /// Extract the fee amount.
        /// </summary>
        /// <returns>the fee amount</returns>
        public virtual long GetMax()
        {
            return max;
        }

        /// <summary>
        /// Assign the maximum fee amount.
        /// </summary>
        /// <param name="max">the fee amount</param>
        /// <returns>{@code this}</returns>
        public virtual CustomFractionalFee SetMax(long max)
        {
            max = max;
            return this;
        }

        /// <summary>
        /// Extract the assessment method inclusive / exclusive.
        /// </summary>
        /// <returns>the assessment method inclusive / exclusive</returns>
        public virtual FeeAssessmentMethod GetAssessmentMethod()
        {
            return assessmentMethod;
        }

        /// <summary>
        /// Assign the assessment method inclusive / exclusive.
        /// <p>
        /// If the assessment method field is set, the token's custom fee is charged
        /// to the sending account and the receiving account receives the full token
        /// transfer amount. If this field is set to false, the receiver pays for
        /// the token custom fees and gets the remaining token balance.
        /// INCLUSIVE(false)
        /// EXCLUSIVE(true)
        /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/custom-token-fees#fractional-fee">Hedera Documentation</a>
        /// </summary>
        /// <param name="assessmentMethod">inclusive / exclusive</param>
        /// <returns>{@code this}</returns>
        public virtual CustomFractionalFee SetAssessmentMethod(FeeAssessmentMethod assessmentMethod)
        {
            Objects.RequireNonNull(assessmentMethod);
            assessmentMethod = assessmentMethod;
            return this;
        }

        public override string ToString()
        {
            return ToStringHelper().Add("numerator", GetNumerator()).Add("denominator", GetDenominator()).Add("min", GetMin()).Add("max", GetMax()).Add("assessmentMethod", GetAssessmentMethod()).ToString();
        }

        /// <summary>
        /// Convert the fractional fee object to a protobuf.
        /// </summary>
        /// <returns>the protobuf object</returns>
        virtual FractionalFee ToFractionalFeeProtobuf()
        {
            return FractionalFee.NewBuilder().SetMinimumAmount(GetMin()).SetMaximumAmount(GetMax()).SetFractionalAmount(Fraction.NewBuilder().SetNumerator(GetNumerator()).SetDenominator(GetDenominator())).SetNetOfTransfers(assessmentMethod.code).Build();
        }

        override Proto.CustomFee ToProtobuf()
        {
            var customFeeBuilder = Proto.CustomFee.NewBuilder().SetFractionalFee(ToFractionalFeeProtobuf());
            return FinishToProtobuf(customFeeBuilder);
        }
    }
}