// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Associate a Hedera Token Service (HTS) token and an account.
    /// 
    /// An association MUST exist between an account and a token before that
    /// account may transfer or receive that token.<br/>
    /// If the identified account is not found,
    /// the transaction SHALL return `INVALID_ACCOUNT_ID`.<br/>
    /// If the identified account has been deleted,
    /// the transaction SHALL return `ACCOUNT_DELETED`.<br/>
    /// If any of the identified tokens is not found,
    /// the transaction SHALL return `INVALID_TOKEN_REF`.<br/>
    /// If any of the identified tokens has been deleted,
    /// the transaction SHALL return `TOKEN_WAS_DELETED`.<br/>
    /// If an association already exists for any of the identified tokens,
    /// the transaction SHALL return `TOKEN_ALREADY_ASSOCIATED_TO_ACCOUNT`.<br/>
    /// The identified account MUST sign this transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenAssociateTransaction : Transaction<TokenAssociateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenAssociateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenAssociateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenAssociateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// An account identifier.
        /// <p>
        /// The identified account SHALL be associated to each of the
        /// tokens identified in the `tokens` field.<br/>
        /// This field is REQUIRED and MUST be a valid account identifier.<br/>
        /// The identified account MUST exist in state.<br/>
        /// The identified account MUST NOT be deleted.<br/>
        /// The identified account MUST NOT be expired.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
        /// <summary>
        /// A list of token identifiers.
        /// <p>
        /// Each token identified in this list SHALL be separately associated with
        /// the account identified in the `account` field.<br/>
        /// This list MUST NOT be empty.
        /// Each entry in this list MUST be a valid token identifier.<br/>
        /// Each entry in this list MUST NOT be currently associated to the
        /// account identified in `account`.<br/>
        /// Each entry in this list MUST NOT be expired.<br/>
        /// Each entry in this list MUST NOT be deleted.
        /// </summary>
        /// <param name="tokens">the list of token id's</param>
        /// <returns>{@code this}</returns>
        public virtual List<TokenId> TokenIds { get; set { RequireNotFrozen(); field = [.. value]; } } = [];

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenAssociate;

			if (body.Account is not null)
			{
				AccountId = AccountId.FromProtobuf(body.Account);
			}

			foreach (var token in body.Tokens)
			{
				TokenIds.Add(TokenId.FromProtobuf(token));
			}
		}
		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link Proto.TokenAssociateTransactionBody}</returns>
		public virtual Proto.TokenAssociateTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenAssociateTransactionBody();

            if (AccountId != null)
				builder.Account = AccountId.ToProtobuf();

			foreach (var token in TokenIds)
				if (token != null)
					builder.Tokens.Add(token.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);

			foreach (var token in TokenIds)
				token?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenAssociate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenAssociate = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.associateTokens);

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