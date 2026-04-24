// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Fee;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="T:TransactionFeeSchedule"]/*' />
    public class TransactionFeeSchedule : ICloneable
    {
        public TransactionFeeSchedule()
        {
            RequestType = RequestType.None;
            Feedata = null;
            Fees = [];
        }

		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="M:TransactionFeeSchedule.FromBytes(System.Byte[])"]/*' />
		public static TransactionFeeSchedule FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TransactionFeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="M:TransactionFeeSchedule.FromProtobuf(Proto.Services.TransactionFeeSchedule)"]/*' />
		public static TransactionFeeSchedule FromProtobuf(Proto.Services.TransactionFeeSchedule transactionFeeSchedule)
        {
            var returnFeeSchedule = new TransactionFeeSchedule
            {
				RequestType = (RequestType)transactionFeeSchedule.HederaFunctionality,
				Feedata = FeeData.FromProtobuf(transactionFeeSchedule.FeeData),
			};

			foreach (FeeData fee in transactionFeeSchedule.Fees.Select(_ => FeeData.FromProtobuf(_)))
				returnFeeSchedule.Fees.Add(fee);

			return returnFeeSchedule;
        }

        /// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="P:TransactionFeeSchedule.RequestType"]/*' />
        public virtual RequestType RequestType { get; set; }
		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="P:TransactionFeeSchedule.Fees"]/*' />
		public virtual List<FeeData> Fees { get; internal set; }
		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="P:TransactionFeeSchedule.Feedata"]/*' />
		public virtual FeeData? Feedata { get; set; }

		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="M:TransactionFeeSchedule.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TransactionFeeSchedule.cs.xml" path='docs/member[@name="M:TransactionFeeSchedule.ToProtobuf"]/*' />
		public virtual Proto.Services.TransactionFeeSchedule ToProtobuf()
        {
			Proto.Services.TransactionFeeSchedule proto = new ()
            {
				HederaFunctionality = (Proto.Services.HederaFunctionality)RequestType
			};

            if (Feedata != null) proto.FeeData = Feedata.ToProtobuf();

            foreach (var fee in Fees)
                proto.Fees.AddRange(Fees.Select(_ => _.ToProtobuf()));

            return proto;
        }

		public virtual object Clone()
		{
			return new TransactionFeeSchedule
			{
				Fees = CloneFees(),
				Feedata = (FeeData?)Feedata?.Clone(),
			};
		}
		public virtual List<FeeData> CloneFees()
        {
            return [.. Fees.Select(_ => (FeeData)_.Clone())];
        }
    }
}
