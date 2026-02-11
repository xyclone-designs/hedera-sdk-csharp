// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <summary>
    /// A maximum custom fee that the user is willing to pay.
    /// <p>
    /// This message is used to specify the maximum custom fee that given user is
    /// willing to pay.
    /// </summary>
    public class CustomFeeLimit
    {
        public virtual AccountId? PayerId { get; set; } 
        public virtual IList<CustomFixedFee> CustomFees { get; set; } = [];

        public static CustomFeeLimit FromProtobuf(Proto.CustomFeeLimit customFeeLimit)
        {
            return new CustomFeeLimit
            {
                PayerId = AccountId.FromProtobuf(customFeeLimit.AccountId),
                CustomFees = [.. customFeeLimit.Fees.Select(_ => CustomFixedFee.FromProtobuf(_))]

            };
        }

		public virtual Proto.CustomFeeLimit ToProtobuf()
        {
			Proto.CustomFeeLimit protobuf = new ()
            {
                AccountId = PayerId?.ToProtobuf(),
			};

            protobuf.Fees.AddRange(CustomFees.Select(_ => _.ToFixedFeeProtobuf()));

            return protobuf;
        }
    }
}