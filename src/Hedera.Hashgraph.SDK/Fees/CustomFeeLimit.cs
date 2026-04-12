// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="CustomFeeLimit.cs.xml" path='docs/member[@name="T:CustomFeeLimit"]/*' />
    public class CustomFeeLimit
    {
        public virtual AccountId? PayerId { get; set; } 
        public virtual List<CustomFixedFee> CustomFees { get; set; } = [];

        public static CustomFeeLimit FromProtobuf(Proto.Services.CustomFeeLimit customFeeLimit)
        {
            return new CustomFeeLimit
            {
                PayerId = AccountId.FromProtobuf(customFeeLimit.AccountId),
                CustomFees = [.. customFeeLimit.Fees.Select(_ => CustomFixedFee.FromProtobuf(_))]

            };
        }

		public virtual Proto.Services.CustomFeeLimit ToProtobuf()
        {
			Proto.Services.CustomFeeLimit protobuf = new ()
            {
                AccountId = PayerId?.ToProtobuf(),
			};

            protobuf.Fees.AddRange(CustomFees.Select(_ => _.ToFixedFeeProtobuf()));

            return protobuf;
        }
    }
}
