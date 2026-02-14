// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <summary>
    /// Delete an account.<br/>
    /// This will mark an account deleted, and transfer all tokens to a "sweep"
    /// account.
    /// 
    /// A deleted account SHALL NOT hold a balance in any token type.<br/>
    /// A deleted account SHALL remain in state until it expires.<br/>
    /// Transfers that would increase the balance of a deleted account
    /// SHALL fail.<br/>
    /// A deleted account MAY be subject of a `cryptoUpdate` transaction to extend
    /// its expiration.<br/>
    /// When a deleted account expires it SHALL be removed entirely, and SHALL NOT
    /// be archived.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public sealed class AccountDeleteTransaction : Transaction<AccountDeleteTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal AccountDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal AccountDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// An account identifier.
		/// <p>
		/// This account SHALL be deleted if this transaction succeeds.<br/>
		/// This account SHOULD NOT hold any balance other than HBAR.<br/>
		/// If this account _does_ hold balances, the `transferAccountID` value
		/// MUST be set to a valid transfer account.<br/>
		/// This account MUST sign this transaction.<br/>
		/// This field MUST be set to a valid account identifier.
		/// </summary>
		/// <param name="deleteAccountId">The AccountId to be set</param>
		/// <returns>{@code this}</returns>
		public AccountId? AccountId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// An account identifier.
		/// <p>
		/// The identified account SHALL receive all tokens, token balances,
		/// and non-fungible/unique from the deleted account.<br/>
		/// The identified account MUST sign this transaction.<br/>
		/// If not set, the account to be deleted MUST NOT have a balance in any
		/// token, a balance in HBAR, or hold any NFT.
		/// </summary>
		/// <param name="transferAccountId">The AccountId to be set</param>
		/// <returns>{@code this}</returns>
		public AccountId? TransferAccountId { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.CryptoDelete;

			if (body.DeleteAccountID is not null)
				AccountId = AccountId.FromProtobuf(body.DeleteAccountID);

			if (body.TransferAccountID is not null)
				TransferAccountId = AccountId.FromProtobuf(body.TransferAccountID);
		}

		/// <summary>
		/// Build the transaction body.
		/// </summary>
		/// <returns>{@link CryptoDeleteTransactionBody}</returns>
		public Proto.CryptoDeleteTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoDeleteTransactionBody();

            if (AccountId != null)
                builder.DeleteAccountID = AccountId.ToProtobuf();

            if (TransferAccountId != null)
                builder.TransferAccountID = TransferAccountId.ToProtobuf();

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			AccountId?.ValidateChecksum(client);
			TransferAccountId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDelete = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDelete = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.cryptoDelete);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
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