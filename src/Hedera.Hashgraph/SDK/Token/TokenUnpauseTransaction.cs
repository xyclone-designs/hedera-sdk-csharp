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
    /// Resume transaction activity for a token.
    /// 
    /// This transaction MUST be signed by the Token `pause_key`.<br/>
    /// The `token` identified MUST exist, and MUST NOT be deleted.<br/>
    /// The `token` identified MAY not be paused; if the token is not paused,
    /// this transaction SHALL have no effect.
    /// The `token` identified MUST have a `pause_key` set, the `pause_key` MUST be
    /// a valid `Key`, and the `pause_key` MUST NOT be an empty `KeyList`.<br/>
    /// An `unpaused` token MAY be transferred or otherwise modified.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class TokenUnpauseTransaction : Transaction<TokenUnpauseTransaction>
    {
        /// <summary>
		/// Constructor
		/// </summary>
		public TokenUnpauseTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal TokenUnpauseTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
		///            records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenUnpauseTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// A token identifier.
        /// <p>
        /// The identified token SHALL be "unpaused". Subsequent transactions
        /// involving that token MAY succeed.
        /// </summary>
        /// <param name="TokenId">the token id</param>
        /// <returns>{@code this}</returns>
        public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUnpause;

            if (body.Token is not null)
            {
                TokenId = TokenId.FromProtobuf(body.Token);
            }
        }
        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenUnpauseTransactionBody}</returns>
        public virtual Proto.TokenUnpauseTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenUnpauseTransactionBody();

            if (TokenId != null)
            {
                builder.Token = TokenId.ToProtobuf();
            }

            return builder;
        }

		public override void ValidateChecksums(Client client)
		{
			if (TokenId != null)
			{
				TokenId.ValidateChecksum(client);
			}
		}

		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUnpause = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUnpause = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.unpauseToken);

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