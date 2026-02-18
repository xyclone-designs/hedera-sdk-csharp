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
    /// <summary>
    /// The fee schedule for a specific hedera functionality and the time period this fee schedule will expire.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/feeschedule">Hedera Documentation</a>
    /// </summary>
    public class FeeSchedule : ICloneable
    {
        private Timestamp ExpirationTime = new ();
        /// <summary>
        /// Constructor.
        /// </summary>
        public FeeSchedule() { }

		/// <summary>
		/// Create a fee schedule from byte array.
		/// </summary>
		/// <param name="bytes">the bye array</param>
		/// <returns>                         the fee schedule</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static FeeSchedule FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.FeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a fee schedule from a protobuf.
		/// </summary>
		/// <param name="feeSchedule">the protobuf</param>
		/// <returns>                         the fee schedule</returns>
		public static FeeSchedule FromProtobuf(Proto.FeeSchedule feeSchedule)
        {
            return new FeeSchedule
			{
                ExpirationTime = feeSchedule.ExpiryTime is not null ? Utils.TimestampConverter.FromProtobuf(feeSchedule.ExpiryTime) : new Timestamp(),
                TransactionFeeSchedules = [.. feeSchedule.TransactionFeeSchedule.Select(_ => TransactionFeeSchedule.FromProtobuf(_))]
            };
        }

		public virtual List<TransactionFeeSchedule> TransactionFeeSchedules
		{
			set => field = value.CloneToList();
			get => new ReadOnlyCollection<TransactionFeeSchedule>(field.CloneToList());

		} = [];

        public void AddTransactionFeeSchedules(params TransactionFeeSchedule[] transactionFeeSchedules)
        {
            TransactionFeeSchedules = [.. TransactionFeeSchedules, .. transactionFeeSchedules];
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         a byte array representation</returns>
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
		/// <summary>
		/// Convert to a protobuf.
		/// </summary>
		/// <returns>                         the protobuf</returns>
		public virtual Proto.FeeSchedule ToProtobuf()
		{
			Proto.FeeSchedule proto = new()
			{
				ExpiryTime = Utils.TimestampConverter.ToSecondsProtobuf(ExpirationTime)
			};

			proto.TransactionFeeSchedule.AddRange(TransactionFeeSchedules.Select(_ => _.ToProtobuf()));

			return proto;
		}
	}
}