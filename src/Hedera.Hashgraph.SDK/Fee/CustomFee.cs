// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Fee
{
    /// <include file="CustomFee.cs.xml" path='docs/member[@name="T:CustomFee"]/*' />
    public abstract class CustomFee
    {
        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.#ctor"]/*' />
        public CustomFee() { }

		/// <include file="CustomFee.cs.xml" path='docs/member[@name="P:CustomFee.FeeCollectorAccountId"]/*' />
		public virtual AccountId? FeeCollectorAccountId { get; internal set; }
		/// <include file="CustomFee.cs.xml" path='docs/member[@name="P:CustomFee.AllCollectorsAreExempt"]/*' />
		public virtual bool AllCollectorsAreExempt { get; internal set; }

		/// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.FromProtobufInner(Proto.Services.CustomFee)"]/*' />
		public static CustomFee FromProtobufInner(Proto.Services.CustomFee customFee)
        {
            return customFee.FeeCase switch
            {
                Proto.Services.CustomFee.FeeOneofCase.FixedFee => CustomFixedFee.FromProtobuf(customFee.FixedFee),
                Proto.Services.CustomFee.FeeOneofCase.FractionalFee => CustomFractionalFee.FromProtobuf(customFee.FractionalFee),
                Proto.Services.CustomFee.FeeOneofCase.RoyaltyFee => CustomRoyaltyFee.FromProtobuf(customFee.RoyaltyFee),

                _ => throw new InvalidOperationException("CustomFee#fromProtobuf: unhandled fee case: " + customFee.FeeCase),
            };
        }
        public static CustomFee FromProtobuf(Proto.Services.CustomFee customFee)
        {
            var outFee = FromProtobufInner(customFee);

            if (customFee.FeeCollectorAccountId != null)
				outFee.FeeCollectorAccountId = AccountId.FromProtobuf(customFee.FeeCollectorAccountId);

			outFee.AllCollectorsAreExempt = customFee.AllCollectorsAreExempt;

            return outFee;
        }
        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.FromBytes(System.Byte[])"]/*' />
        public static CustomFee FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.CustomFee.Parser.ParseFrom(bytes));
        }

        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.DeepCloneList(System.Collections.Generic.IList{CustomFee})"]/*' />
        public static IList<CustomFee> DeepCloneList(IList<CustomFee> customFees)
        {
            var returnCustomFees = new List<CustomFee>(customFees.Count);
            foreach (var fee in customFees)
            {
                returnCustomFees.Add(fee.DeepClone());
            }

            return returnCustomFees;
        }

		/// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.DeepClone"]/*' />
		public abstract CustomFee DeepClone();
        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.ValidateChecksums(Client)"]/*' />
        public virtual void ValidateChecksums(Client client)
        {
            FeeCollectorAccountId?.ValidateChecksum(client);
        }

        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.FinishToProtobuf(Proto.Services.CustomFee)"]/*' />
        protected virtual Proto.Services.CustomFee FinishToProtobuf(Proto.Services.CustomFee customFeeBuilder)
        {
            if (FeeCollectorAccountId != null)
				customFeeBuilder.FeeCollectorAccountId = FeeCollectorAccountId.ToProtobuf();

			customFeeBuilder.AllCollectorsAreExempt = AllCollectorsAreExempt;
            return customFeeBuilder;
        }

        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.ToProtobuf"]/*' />
        public abstract Proto.Services.CustomFee ToProtobuf();
        /// <include file="CustomFee.cs.xml" path='docs/member[@name="M:CustomFee.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}
