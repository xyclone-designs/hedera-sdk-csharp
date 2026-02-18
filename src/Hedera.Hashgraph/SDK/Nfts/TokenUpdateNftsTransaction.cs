// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <summary>
	/// Modify the metadata field for an individual non-fungible/unique token (NFT).
	/// 
	/// Updating the metadata of an NFT SHALL NOT affect ownership or
	/// the ability to transfer that NFT.<br/>
	/// This transaction SHALL affect only the specific serial numbered tokens
	/// identified.
	/// This transaction SHALL modify individual token metadata.<br/>
	/// This transaction MUST be signed by the token `metadata_key`.<br/>
	/// The token `metadata_key` MUST be a valid `Key`.<br/>
	/// The token `metadata_key` MUST NOT be an empty `KeyList`.
	/// 
	/// ### Block Stream Effects
	/// None
	/// </summary>
	public class TokenUpdateNftsTransaction : Transaction<TokenUpdateNftsTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenUpdateNftsTransaction() { }
		internal TokenUpdateNftsTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal TokenUpdateNftsTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private List<long> _Serials = [];

		/// <summary>
		/// A token identifier.<br/>
		/// This is the token type (i.e. collection) for which to update NFTs.
		/// <p>
		/// This field is REQUIRED.<br/>
		/// The identified token MUST exist, MUST NOT be paused, MUST have the type
		/// non-fungible/unique, and MUST have a valid `metadata_key`.
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <returns>{@code this}</returns>
		public virtual TokenId? TokenId { get; set { RequireNotFrozen(); field = value; } }
		/// <summary>
		/// A list of serial numbers to be updated.
		/// <p>
		/// This field is REQUIRED.<br/>
		/// This list MUST have at least one(1) entry.<br/>
		/// This list MUST NOT have more than ten(10) entries.
		/// </summary>
		/// <param name="serials">the list of serial numbers</param>
		/// <returns>{@code this}</returns>
		public virtual List<long> Serials 
		{
			get 
			{ 
				RequireNotFrozen(); 
				return _Serials;
			} 
			set 
			{ 
				RequireNotFrozen();
				_Serials = [.. value];
			} 
		}
		public virtual IReadOnlyList<long> Serials_Read { get => _Serials.AsReadOnly(); }
        /// <summary>
        /// A new value for the metadata.
        /// <p>
        /// If this field is not set, the metadata SHALL NOT change.<br/>
        /// This value, if set, MUST NOT exceed 100 bytes.
        /// </summary>
        /// <param name="metadata">the metadata</param>
        /// <returns>{@code this}</returns>
        public virtual byte[]? Metadata { get; set { RequireNotFrozen(); field = value; } } = [];

		/// <summary>
		/// Initialize from the transaction body.
		/// </summary>
		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenUpdateNfts;

            if (body.Token is not null)
				TokenId = TokenId.FromProtobuf(body.Token);

			Serials = body.SerialNumbers;

            if (body.Metadata is not null)
                Metadata = body.Metadata.ToByteArray();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link Proto.TokenUpdateNftsTransactionBody}</returns>
        public virtual Proto.TokenUpdateNftsTransactionBody ToProtobuf()
        {
            var builder = new Proto.TokenUpdateNftsTransactionBody();

            if (TokenId != null)
				builder.Token = TokenId.ToProtobuf();

			foreach (var serial in Serials)
				builder.SerialNumbers.Add(serial);

			if (Metadata != null)
				builder.Metadata = ByteString.CopyFrom(Metadata);

			return builder;
        }

        public override void ValidateChecksums(Client client)
        {
			TokenId?.ValidateChecksum(client);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenUpdateNfts = ToProtobuf();
        }
		public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenUpdateNfts = ToProtobuf();
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.updateNfts);

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