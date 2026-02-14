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
    /// Pause transaction activity for a token.
    /// 
    /// This transaction MUST be signed by the Token `pause_key`.<br/>
    /// The `token` identified MUST exist, and MUST NOT be deleted.<br/>
    /// The `token` identified MAY be paused; if the token is already paused,
    /// this transaction SHALL have no effect.
    /// The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
    /// a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
    /// A `paused` token SHALL NOT be transferred or otherwise modified except to
    /// "up-pause" the token with `unpauseToken` or in a `rejectToken` transaction.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenPauseTransaction : Transaction<TokenPauseTransaction>
    {
        /// <summary>
		/// Constructor.
		/// </summary>
		public TokenPauseTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenPauseTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenPauseTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// The identified token SHALL be paused. Subsequent transactions
        /// involving that token SHALL fail until the token is "unpaused".
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenPause;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);
		}

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenPauseTransactionBody}</returns>
        public virtual Proto.TokenPauseTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenPauseTransactionBody();

            if (TokenId is not null)
			    builder.Token = TokenId.ToProtobuf();

			return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
		{
			bodyBuilder.TokenPause = ToProtobuf();
		}
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
		{
			scheduled.TokenPause = ToProtobuf();
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.pauseToken);

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