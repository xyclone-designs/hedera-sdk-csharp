// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Update the custom fees for a given token. If the token does not have a
    /// fee schedule, the network response returned will be
    /// CUSTOM_SCHEDULE_ALREADY_HAS_NO_FEES. You will need to sign the transaction
    /// with the fee schedule key to update the fee schedule for the token. If you
    /// do not have a fee schedule key set for the token, you will not be able to
    /// update the fee schedule.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/tokens/update-a-fee-schedule">Hedera Documentation</a>
    /// </summary>
    public class TokenFeeScheduleUpdateTransaction : Transaction<TokenFeeScheduleUpdateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenFeeScheduleUpdateTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenFeeScheduleUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenFeeScheduleUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// This SHALL identify the token type to modify with an updated
        /// custom fee schedule.<br/>
        /// The identified token MUST exist, and MUST NOT be deleted.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// A list of custom fees representing a fee schedule.
		/// <p>
		/// This list MAY be empty to remove custom fees from a token.<br/>
		/// If the identified token is a non-fungible/unique type, the entries
		/// in this list MUST NOT declare a `fractional_fee`.<br/>
		/// If the identified token is a fungible/common type, the entries in this
		/// list MUST NOT declare a `royalty_fee`.<br/>
		/// Any token type MAY include entries that declare a `fixed_fee`.
		/// </summary>
		/// <param name="customFees">the list of custom fees</param>
		/// <returns>{@code this}</returns>
		public virtual IList<CustomFee> CustomFees 
        { 
            get => CustomFee.DeepCloneList(field);
			set 
            { 
                RequireNotFrozen(); 
                field = CustomFee.DeepCloneList(value); 
            } 
        } = [];

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenFeeScheduleUpdate;

            TokenId = TokenId.FromProtobuf(body.TokenId);

			foreach (var fee in body.CustomFees)
            {
                CustomFees.Add(CustomFee.FromProtobuf(fee));
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenFeeScheduleUpdateTransactionBody}</returns>
        public virtual Proto.TokenFeeScheduleUpdateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenFeeScheduleUpdateTransactionBody();

            if (TokenId != null)
            {
                builder.TokenId = TokenId.ToProtobuf();
            }

            builder.CustomFees.Clear();

            foreach (var fee in CustomFees)
            {
                builder.CustomFees.Add(fee.ToProtobuf());
            }

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);

			foreach (CustomFee fee in CustomFees)
				fee.ValidateChecksums(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenFeeScheduleUpdate = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule TokenFeeScheduleUpdateTransaction");
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.updateTokenFeeSchedule);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}