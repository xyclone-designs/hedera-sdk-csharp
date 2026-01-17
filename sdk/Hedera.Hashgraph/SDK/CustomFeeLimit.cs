using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
     * A maximum custom fee that the user is willing to pay.
     * <p>
     * This message is used to specify the maximum custom fee that given user is
     * willing to pay.
     */
    public class CustomFeeLimit {

        public AccountId? PayerId { get; set; }
        public List<CustomFixedFee>? CustomFees { get; set; } 

        public static CustomFeeLimit FromProtobuf(Proto.CustomFeeLimit customFeeLimit) 
        {
            return new CustomFeeLimit
            {
                PayerId = AccountId.FromProtobuf(customFeeLimit.AccountId),
                CustomFees = [.. customFeeLimit.Fees.Select(_ => CustomFixedFee.FromProtobuf(_))]
            };
        }

        public Proto.CustomFeeLimit ToProtobuf()
        {
            Proto.CustomFeeLimit proto = new();

            if (PayerId is not null) proto.AccountId = PayerId.ToProtobuf();
            if (CustomFees?.Select(_ => _.ToProtobuf().FixedFee) is IEnumerable<Proto.FixedFee> customfees) proto.Fees.AddRange(customfees);

            return proto;
        }
    }

}