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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A custom transfer fee that was assessed during the handling of a CryptoTransfer.
    /// </summary>
    public class AssessedCustomFee
    {
        /// <summary>
        /// The number of units assessed for the fee
        /// </summary>
        public readonly long amount;
        /// <summary>
        /// The denomination of the fee; taken as hbar if left unset
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The account to receive the assessed fee
        /// </summary>
        public readonly AccountId feeCollectorAccountId;
        /// <summary>
        /// The account(s) whose final balances would have been higher in the absence of this assessed fee
        /// </summary>
        public readonly IList<AccountId> payerAccountIdList;
        AssessedCustomFee(long amount, TokenId tokenId, AccountId feeCollectorAccountId, IList<AccountId> payerAccountIdList)
        {
            amount = amount;
            tokenId = tokenId;
            feeCollectorAccountId = feeCollectorAccountId;
            payerAccountIdList = payerAccountIdList;
        }

        /// <summary>
        /// Convert the protobuf object to an assessed custom fee object.
        /// </summary>
        /// <param name="assessedCustomFee">protobuf response object</param>
        /// <returns>                         the converted assessed custom fee object</returns>
        static AssessedCustomFee FromProtobuf(Proto.AssessedCustomFee assessedCustomFee)
        {
            var payerList = new List<AccountId>(assessedCustomFee.GetEffectivePayerAccountIdCount());
            foreach (var payerId in assessedCustomFee.GetEffectivePayerAccountIdList())
            {
                payerList.Add(AccountId.FromProtobuf(payerId));
            }

            return new AssessedCustomFee(assessedCustomFee.GetAmount(), assessedCustomFee.HasTokenId() ? TokenId.FromProtobuf(assessedCustomFee.GetTokenId()) : null, assessedCustomFee.HasFeeCollectorAccountId() ? AccountId.FromProtobuf(assessedCustomFee.GetFeeCollectorAccountId()) : null, payerList);
        }

        /// <summary>
        /// Convert a byte array into an assessed custom fee object.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the converted assessed custom fee object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static AssessedCustomFee FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.AssessedCustomFee.Parser.ParseFrom(bytes));
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("amount", amount).Add("tokenId", tokenId).Add("feeCollectorAccountId", feeCollectorAccountId).Add("payerAccountIdList", payerAccountIdList).ToString();
        }

        /// <summary>
        /// Create the protobuf representation.
        /// </summary>
        /// <returns>{@link Proto.AssessedCustomFee}</returns>
        virtual Proto.AssessedCustomFee ToProtobuf()
        {
            var builder = Proto.AssessedCustomFee.NewBuilder().SetAmount(amount);
            if (tokenId != null)
            {
                builder.SetTokenId(tokenId.ToProtobuf());
            }

            if (feeCollectorAccountId != null)
            {
                builder.SetFeeCollectorAccountId(feeCollectorAccountId.ToProtobuf());
            }

            foreach (var payerId in payerAccountIdList)
            {
                builder.AddEffectivePayerAccountId(payerId.ToProtobuf());
            }

            return proto;
        }

        /// <summary>
        /// Create a byte array representation.
        /// </summary>
        /// <returns>                         the converted assessed custom fees</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}