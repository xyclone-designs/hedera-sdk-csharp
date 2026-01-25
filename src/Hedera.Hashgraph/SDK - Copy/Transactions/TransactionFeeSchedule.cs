// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <summary>
    /// The fees for a specific transaction or query based on the fee data.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/transactionfeeschedule">Hedera Documentation</a>
    /// </summary>
    public class TransactionFeeSchedule : ICloneable
    {
        private RequestType requestType;
        private FeeData feeData;
        private IList<FeeData> fees;
        /// <summary>
        /// Constructor.
        /// </summary>
        public TransactionFeeSchedule()
        {
            requestType = RequestType.NONE;
            feeData = null;
            fees = new ();
        }

        /// <summary>
        /// Create a transaction fee schedule object from a protobuf.
        /// </summary>
        /// <param name="transactionFeeSchedule">the protobuf</param>
        /// <returns>                         the new transaction fee schedule</returns>
        public static TransactionFeeSchedule FromProtobuf(Proto.TransactionFeeSchedule transactionFeeSchedule)
        {
            var returnFeeSchedule = new TransactionFeeSchedule().SetRequestType(RequestType.ValueOf(transactionFeeSchedule.GetHederaFunctionality())).SetFeeData(transactionFeeSchedule.HasFeeData() ? FeeData.FromProtobuf(transactionFeeSchedule.GetFeeData()) : null);
            foreach (var feeData in transactionFeeSchedule.GetFeesList())
            {
                returnFeeSchedule.AddFee(FeeData.FromProtobuf(feeData));
            }

            return returnFeeSchedule;
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
        /// Extract the request type.
        /// </summary>
        /// <returns>                         the request type</returns>
        public virtual RequestType GetRequestType()
        {
            return requestType;
        }

        /// <summary>
        /// Assign the request type.
        /// </summary>
        /// <param name="requestType">the request type</param>
        /// <returns>{@code this}</returns>
        public virtual TransactionFeeSchedule SetRequestType(RequestType requestType)
        {
            requestType = requestType;
            return this;
        }

        /// <summary>
        /// Get the total fee charged for a transaction
        /// </summary>
        /// <returns>the feeData</returns>
        public virtual FeeData GetFeeData()
        {
            return feeData;
        }

        /// <summary>
        /// Set the total fee charged for a transaction
        /// </summary>
        /// <param name="feeData">the feeData to set</param>
        /// <returns>{@code this}</returns>
        public virtual TransactionFeeSchedule SetFeeData(FeeData feeData)
        {
            feeData = feeData;
            return this;
        }

        /// <summary>
        /// Extract the list of fee's.
        /// </summary>
        /// <returns>                         the list of fee's</returns>
        public virtual IList<FeeData> GetFees()
        {
            return Collections.UnmodifiableList(fees);
        }

        /// <summary>
        /// Add a fee to the schedule.
        /// </summary>
        /// <param name="fee">the fee to add</param>
        /// <returns>{@code this}</returns>
        public virtual TransactionFeeSchedule AddFee(FeeData fee)
        {
            fees.Add(Objects.RequireNonNull(fee));
            return this;
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TransactionFeeSchedule}</returns>
        public virtual Proto.TransactionFeeSchedule ToProtobuf()
        {
            var returnBuilder = Proto.TransactionFeeSchedule.NewBuilder().SetHederaFunctionality(GetRequestType().code);
            if (feeData != null)
            {
                returnBuilder.SetFeeData(feeData.ToProtobuf());
            }

            foreach (var fee in fees)
            {
                returnBuilder.AddFees(fee.ToProtobuf());
            }

            return returnBuilder.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("requestType", GetRequestType()).Add("feeData", GetFeeData()).Add("fees", GetFees()).ToString();
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }

        virtual IList<FeeData> CloneFees()
        {
            IList<FeeData> cloneFees = new List(fees.Count);
            foreach (var fee in fees)
            {
                cloneFees.Add(fee.Clone());
            }

            return cloneFees;
        }

        public virtual TransactionFeeSchedule Clone()
        {
            try
            {
                TransactionFeeSchedule clone = (TransactionFeeSchedule)base.Clone();
                clone.feeData = feeData != null ? feeData.Clone() : null;
                clone.fees = fees != null ? CloneFees() : null;
                return clone;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException();
            }
        }
    }
}