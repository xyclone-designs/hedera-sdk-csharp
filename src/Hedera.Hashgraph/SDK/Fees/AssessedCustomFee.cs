// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A custom transfer fee that was assessed during the handling of a CryptoTransfer.
    /// </summary>
    public class AssessedCustomFee
    {
        public AssessedCustomFee(long amount, TokenId tokenId, AccountId feeCollectorAccountId, IList<AccountId> payerAccountIdList)
        {
            Amount = amount;
            TokenId = tokenId;
            FeeCollectorAccountId = feeCollectorAccountId;
            PayerAccountIdList = payerAccountIdList;
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
		/// <summary>
		/// Convert the protobuf object to an assessed custom fee object.
		/// </summary>
		/// <param name="assessedCustomFee">protobuf response object</param>
		/// <returns>                         the converted assessed custom fee object</returns>
		public static AssessedCustomFee FromProtobuf(Proto.AssessedCustomFee assessedCustomFee)
        {
            return new AssessedCustomFee(
                assessedCustomFee.Amount, 
                TokenId.FromProtobuf(assessedCustomFee.TokenId),
                AccountId.FromProtobuf(assessedCustomFee.FeeCollectorAccountId),
				[.. assessedCustomFee.EffectivePayerAccountId.Select(_ => AccountId.FromProtobuf(_))]);
        }

		/// <summary>
		/// The number of units assessed for the fee
		/// </summary>
		public long Amount { get; }
		/// <summary>
		/// The denomination of the fee; taken as hbar if left unset
		/// </summary>
		public TokenId TokenId { get; }
		/// <summary>
		/// The account to receive the assessed fee
		/// </summary>
		public AccountId FeeCollectorAccountId { get; }
		/// <summary>
		/// The account(s) whose final balances would have been higher in the absence of this assessed fee
		/// </summary>
		public IList<AccountId> PayerAccountIdList { get; }

		/// <summary>
		/// Create a byte array representation.
		/// </summary>
		/// <returns>                         the converted assessed custom fees</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf representation.
		/// </summary>
		/// <returns>{@link Proto.AssessedCustomFee}</returns>
		public virtual Proto.AssessedCustomFee ToProtobuf()
        {
            Proto.AssessedCustomFee proto = new()
            {
				Amount = Amount
			};

            if (TokenId != null)
                proto.TokenId = TokenId.ToProtobuf();

            if (FeeCollectorAccountId != null)
                proto.FeeCollectorAccountId = FeeCollectorAccountId.ToProtobuf();

			proto.EffectivePayerAccountId.AddRange(PayerAccountIdList.Select(_ => _.ToProtobuf()));

			return proto;
        }
    }
}