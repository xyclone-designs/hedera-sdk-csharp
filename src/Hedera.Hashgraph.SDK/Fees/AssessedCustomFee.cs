// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="T:AssessedCustomFee"]/*' />
    public class AssessedCustomFee
    {
        public AssessedCustomFee(long amount, TokenId tokenId, AccountId feeCollectorAccountId, IEnumerable<AccountId> payerAccountIdList)
        {
            Amount = amount;
            TokenId = tokenId;
            FeeCollectorAccountId = feeCollectorAccountId;
            PayerAccountIdList = [.. payerAccountIdList];
        }

		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="M:AssessedCustomFee.FromBytes(System.Byte[])"]/*' />
		public static AssessedCustomFee FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.AssessedCustomFee.Parser.ParseFrom(bytes));
		}
		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="M:AssessedCustomFee.FromProtobuf(Proto.Services.AssessedCustomFee)"]/*' />
		public static AssessedCustomFee FromProtobuf(Proto.Services.AssessedCustomFee assessedCustomFee)
        {
            return new AssessedCustomFee(
                assessedCustomFee.Amount, 
                TokenId.FromProtobuf(assessedCustomFee.TokenId),
                AccountId.FromProtobuf(assessedCustomFee.FeeCollectorAccountId),
				[.. assessedCustomFee.EffectivePayerAccountId.Select(_ => AccountId.FromProtobuf(_))]);
        }

		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="P:AssessedCustomFee.Amount"]/*' />
		public long Amount { get; }
		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="P:AssessedCustomFee.TokenId"]/*' />
		public TokenId TokenId { get; }
		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="P:AssessedCustomFee.FeeCollectorAccountId"]/*' />
		public AccountId FeeCollectorAccountId { get; }
		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="P:AssessedCustomFee.PayerAccountIdList"]/*' />
		public IList<AccountId> PayerAccountIdList { get; }

		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="M:AssessedCustomFee.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="AssessedCustomFee.cs.xml" path='docs/member[@name="M:AssessedCustomFee.ToProtobuf"]/*' />
		public virtual Proto.Services.AssessedCustomFee ToProtobuf()
        {
            Proto.Services.AssessedCustomFee proto = new()
            {
				Amount = Amount
			};

            if (TokenId != null)
                Proto.Services.TokenId = TokenId.ToProtobuf();

            if (FeeCollectorAccountId != null)
                Proto.Services.FeeCollectorAccountId = FeeCollectorAccountId.ToProtobuf();

			Proto.Services.EffectivePayerAccountId.AddRange(PayerAccountIdList.Select(_ => _.ToProtobuf()));

			return proto;
        }
    }
}
