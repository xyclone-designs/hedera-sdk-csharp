// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Revoke "Know Your Customer"(KYC) from one account for a single token.
    /// 
    /// This transaction MUST be signed by the `kyc_key` for the token.<br/>
    /// The identified token MUST have a `kyc_key` set to a valid `Key` value.<br/>
    /// The token `kyc_key` MUST NOT be an empty `KeyList`.<br/>
    /// The identified token MUST exist and MUST NOT be deleted.<br/>
    /// The identified account MUST exist and MUST NOT be deleted.<br/>
    /// The identified account MUST have an association to the identified token.<br/>
    /// On success the association between the identified account and the identified
    /// token SHALL NOT be marked as "KYC granted".
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenRevokeKycTransaction : Transaction<TokenRevokeKycTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenRevokeKycTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenRevokeKycTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenRevokeKycTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// The identified token SHALL revoke "KYC" for the account
        /// identified by the `account` field.<br/>
        /// The identified token MUST be associated to the account identified
        /// by the `account` field.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// An account identifier.
		/// <p>
		/// The token identified by the `token` field SHALL revoke "KYC" for the
		/// identified account.<br/>
		/// This account MUST be associated to the token identified
		/// by the `token` field.
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <returns>{@code this}</returns>
		public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }

		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenRevokeKyc;

            if (body.Token is not null)
                TokenId = TokenId.FromProtobuf(body.Token);

            if (body.Account is not null)
                AccountId = AccountId.FromProtobuf(body.Account);
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenRevokeKycTransactionBody}</returns>
        public virtual Proto.TokenRevokeKycTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenRevokeKycTransactionBody();

            if (TokenId != null)
                builder.Token = TokenId.ToProtobuf();

            if (AccountId != null)
                builder.Account = AccountId.ToProtobuf();

            return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
			AccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenRevokeKyc = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenRevokeKyc = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.freezeTokenAccount);

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