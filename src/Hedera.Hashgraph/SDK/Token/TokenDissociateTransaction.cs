// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Dissociate an account from one or more HTS tokens.
    /// 
    /// If the identified account is not found,
    /// the transaction SHALL return `INVALID_ACCOUNT_ID`.<br/>
    /// If the identified account has been deleted,
    /// the transaction SHALL return `ACCOUNT_DELETED`.<br/>
    /// If any of the identified tokens is not found,
    /// the transaction SHALL return `INVALID_TOKEN_REF`.<br/>
    /// If any of the identified tokens has been deleted,
    /// the transaction SHALL return `TOKEN_WAS_DELETED`.<br/>
    /// If an association does not exist for any of the identified tokens,
    /// the transaction SHALL return `TOKEN_NOT_ASSOCIATED_TO_ACCOUNT`.<br/>
    /// If the identified account has a nonzero balance for any of the identified
    /// tokens, and that token is neither deleted nor expired, the
    /// transaction SHALL return `TRANSACTION_REQUIRES_ZERO_TOKEN_BALANCES`.<br/>
    /// If one of the identified tokens is a fungible/common token that is expired,
    /// the account MAY disassociate from that token, even if that token balance is
    /// not zero for that account.<br/>
    /// If one of the identified tokens is a non-fungible/unique token that is
    /// expired, the account MUST NOT disassociate if that account holds any
    /// individual NFT of that token. In this situation the transaction SHALL
    /// return `TRANSACTION_REQUIRED_ZERO_TOKEN_BALANCES`.<br/>
    /// The identified account MUST sign this transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenDissociateTransaction : Transaction<TokenDissociateTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenDissociateTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(5);
        }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		public TokenDissociateTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public TokenDissociateTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// An account identifier.
		/// <p>
		/// The identified account SHALL be dissociated from each of the
		/// tokens identified in the `tokens` field.
		/// This field is REQUIRED and MUST be a valid account identifier.<br/>
		/// The identified account MUST exist in state.<br/>
		/// The identified account MUST NOT be deleted.<br/>
		/// The identified account MUST NOT be expired.
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <returns>{@code this}</returns>
		public virtual AccountId? Account
		{
			get;
			set
			{
				RequireNotFrozen();
				field = value;
			}
		}
		/// <summary>
		/// A list of token identifiers.
		/// <p>
		/// Each token identified in this list SHALL be dissociated from
		/// the account identified in the `account` field.<br/>
		/// This list MUST NOT be empty.
		/// Each entry in this list MUST be a valid token identifier.<br/>
		/// Each entry in this list MUST be currently associated to the
		/// account identified in `account`.<br/>
		/// Entries in this list MAY be expired, if the token type is
		/// fungible/common.<br/>
		/// Each entry in this list MUST NOT be deleted.
		/// </summary>
		/// <param name="tokens">the list of token id's.</param>
		/// <returns>{@code this}</returns>
		public IList<TokenId> TokenIds 
        { 
            get => [..field]; 
            set 
            {
				RequireNotFrozen();
				field = [.. value];
            } 
        } = [];
		
        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenDissociate;

            if (body.Account is not null)
            {
				Account = AccountId.FromProtobuf(body.Account);
            }

            foreach (var token in body.Tokens)
                TokenIds.Add(TokenId.FromProtobuf(token));
        }
        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenDissociateTransactionBody}</returns>
        public virtual Proto.TokenDissociateTransactionBody ToProtobuf()
        {
			Proto.TokenDissociateTransactionBody builder = new ();

            if (Account != null)
				builder.Account = Account.ToProtobuf();

			foreach (var token in TokenIds)
				builder.Tokens.Add(token.ToProtobuf());

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			Account?.ValidateChecksum(client);

			foreach (var token in TokenIds)
				token?.ValidateChecksum(client);
		}
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenDissociate = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenDissociate = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.dissociateTokens);

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