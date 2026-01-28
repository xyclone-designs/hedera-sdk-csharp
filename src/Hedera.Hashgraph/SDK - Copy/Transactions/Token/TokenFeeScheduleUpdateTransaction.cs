// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Fees;
using Hedera.Hashgraph.SDK.Ids;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Token
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
        private TokenId tokenId = null;
        private IList<CustomFee> customFees = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenFeeScheduleUpdateTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenFeeScheduleUpdateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenFeeScheduleUpdateTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Extract the token id.
        /// </summary>
        /// <returns>                         the token id</returns>
        public virtual TokenId GetTokenId()
        {
            return tokenId;
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
        public virtual TokenFeeScheduleUpdateTransaction SetTokenId(TokenId tokenId)
        {
            ArgumentNullException.ThrowIfNull(tokenId);
            RequireNotFrozen();
            tokenId = tokenId;
            return this;
        }

        /// <summary>
        /// Extract the list of custom fees.
        /// </summary>
        /// <returns>                         the list of custom fees</returns>
        public virtual IList<CustomFee> GetCustomFees()
        {
            return CustomFee.DeepCloneList(customFees);
        }

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
        public virtual TokenFeeScheduleUpdateTransaction SetCustomFees(IList<CustomFee> customFees)
        {
            ArgumentNullException.ThrowIfNull(customFees);
            RequireNotFrozen();
            customFees = CustomFee.DeepCloneList(customFees);
            return this;
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = sourceTransactionBody.TokenFeeScheduleUpdate();

            if (body.HasTokenId())
            {
                tokenId = TokenId.FromProtobuf(body.GetTokenId());
            }

            foreach (var fee in body.CustomFees)
            {
                customFees.Add(CustomFee.FromProtobuf(fee));
            }
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenFeeScheduleUpdateTransactionBody}</returns>
        public virtual Proto.TokenFeeScheduleUpdateTransactionBody Build()
        {
            var builder = new Proto.TokenFeeScheduleUpdateTransactionBody();

            if (tokenId != null)
            {
                builder.TokenId = tokenId.ToProtobuf();
            }

            builder.CustomFees.Clear();

            foreach (var fee in customFees)
            {
                builder.CustomFees.Add(fee.ToProtobuf());
            }

            return builder;
        }

        override void ValidateChecksums(Client client)
        {
            if (tokenId != null)
            {
                tokenId.ValidateChecksum(client);
            }

            foreach (CustomFee fee in customFees)
            {
                fee.ValidateChecksums(client);
            }
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetUpdateTokenFeeScheduleMethod();
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenFeeScheduleUpdate = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule TokenFeeScheduleUpdateTransaction");
        }
    }
}