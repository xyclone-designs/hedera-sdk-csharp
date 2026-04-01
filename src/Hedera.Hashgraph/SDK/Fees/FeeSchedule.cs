// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Fees
{
    /// <include file="FeeSchedule.cs.xml" path='docs/member[@name="T:FeeSchedule"]/*' />
    public class FeeSchedule : ICloneable
    {
        internal DateTimeOffset ExpirationTime = new ();
        /// <include file="FeeSchedule.cs.xml" path='docs/member[@name="M:FeeSchedule.#ctor"]/*' />
        public FeeSchedule() { }

		/// <include file="FeeSchedule.cs.xml" path='docs/member[@name="M:FeeSchedule.FromBytes(System.Byte[])"]/*' />
		public static FeeSchedule FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <include file="FeeSchedule.cs.xml" path='docs/member[@name="M:FeeSchedule.FromProtobuf(Proto.FeeSchedule)"]/*' />
		public static FeeSchedule FromProtobuf(Proto.FeeSchedule feeSchedule)
        {
            return new FeeSchedule
			{
                ExpirationTime = feeSchedule.ExpiryTime.ToDateTimeOffset(),
                TransactionFeeSchedules = [.. feeSchedule.TransactionFeeSchedule.Select(_ => TransactionFeeSchedule.FromProtobuf(_))]
            };
        }

		public virtual List<TransactionFeeSchedule> TransactionFeeSchedules
		{
			set => field = value.CloneToList();
			get => field.CloneToList();

		} = [];

        public void AddTransactionFeeSchedules(params TransactionFeeSchedule[] transactionFeeSchedules)
        {
            TransactionFeeSchedules = [.. TransactionFeeSchedules, .. transactionFeeSchedules];
        }

        /// <include file="FeeSchedule.cs.xml" path='docs/member[@name="M:FeeSchedule.ToBytes"]/*' />
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
        public virtual object Clone()
        {
            return new FeeSchedule
            {
				TransactionFeeSchedules = TransactionFeeSchedules.CloneToList()
			};
        }
		/// <include file="FeeSchedule.cs.xml" path='docs/member[@name="M:FeeSchedule.ToProtobuf"]/*' />
		public virtual Proto.FeeSchedule ToProtobuf()
		{
			Proto.FeeSchedule proto = new()
			{
				ExpiryTime = ExpirationTime.ToProtoTimestampSeconds()
			};

			proto.TransactionFeeSchedule.AddRange(TransactionFeeSchedules.Select(_ => _.ToProtobuf()));

			return proto;
		}
	}
}