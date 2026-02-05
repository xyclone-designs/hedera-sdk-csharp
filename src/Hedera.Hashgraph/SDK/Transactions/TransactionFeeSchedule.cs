// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Fees;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// The fees for a specific transaction or query based on the fee data.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/transactionfeeschedule">Hedera Documentation</a>
    /// </summary>
    public class TransactionFeeSchedule : ICloneable
    {
        public TransactionFeeSchedule()
        {
            RequestType = RequestType.None;
            Feedata = null;
            Fees = [];
        }

		/// <summary>
		/// Create a transaction fee schedule object from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the new transaction fee schedule</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TransactionFeeSchedule FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TransactionFeeSchedule.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a transaction fee schedule object from a protobuf.
		/// </summary>
		/// <param name="transactionFeeSchedule">the protobuf</param>
		/// <returns>                         the new transaction fee schedule</returns>
		public static TransactionFeeSchedule FromProtobuf(Proto.TransactionFeeSchedule transactionFeeSchedule)
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

        /// <summary>
        /// Extract the request type.
        /// </summary>
        /// <returns>                         the request type</returns>
        public virtual RequestType RequestType { get; set; }
		/// <summary>
		/// Extract the list of fee's.
		/// </summary>
		/// <returns>                         the list of fee's</returns>
		public virtual IList<FeeData> Fees { get; protected set; }
		/// <summary>
		/// Set the total fee charged for a transaction
		/// </summary>
		/// <param name="feeData">the feeData to set</param>
		/// <returns>{@code this}</returns>
		public virtual FeeData? Feedata { get; set; }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link
		///         Proto.TransactionFeeSchedule}</returns>
		public virtual Proto.TransactionFeeSchedule ToProtobuf()
        {
			Proto.TransactionFeeSchedule proto = new ()
            {
				HederaFunctionality = (Proto.HederaFunctionality)RequestType
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
		public virtual IList<FeeData> CloneFees()
        {
            return [.. Fees.Select(_ => (FeeData)_.Clone())];
        }
    }
}