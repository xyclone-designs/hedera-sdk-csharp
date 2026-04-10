// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="T:TokenAirdropTransaction"]/*' />
    public class TokenAirdropTransaction : AbstractTokenTransferTransaction<TokenAirdropTransaction>
    {
        /// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenAirdropTransaction.#ctor"]/*' />
        public TokenAirdropTransaction() : base()
        {
            DefaultMaxTransactionFee = new Hbar(1);
        }
		/// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenAirdropTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal TokenAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenAirdropTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal TokenAirdropTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenAirdropTransaction.ToProtobuf"]/*' />
		public Proto.TokenAirdropTransactionBody ToProtobuf()
		{
			var transfers = SortTransfersAndBuild();
			var builder = new Proto.TokenAirdropTransactionBody();
			foreach (var transfer in transfers)
			{
				builder.TokenTransfers.Add(transfer.ToProtobuf());
			}

			return builder;
		}

		/// <include file="TokenAirdropTransaction.cs.xml" path='docs/member[@name="M:TokenAirdropTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.TokenAirdrop;

			foreach (var tokenTransferList in body.TokenTransfers)
			{
				var token = TokenId.FromProtobuf(tokenTransferList.Token);
				foreach (var transfer in tokenTransferList.Transfers)
				{
					tokenTransfers.Add(new TokenTransfer(
						token,
						AccountId.FromProtobuf(transfer.AccountID),
						transfer.Amount,
						tokenTransferList.ExpectedDecimals,
						transfer.IsApproval));
				}

				foreach (var transfer in tokenTransferList.NftTransfers)
				{
					nftTransfers.Add(new TokenNftTransfer(
						token,
						AccountId.FromProtobuf(transfer.SenderAccountID),
						AccountId.FromProtobuf(transfer.ReceiverAccountID),
						transfer.SerialNumber,
						transfer.IsApproval,
						null,
						null));
				}
			}
		}

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.TokenService.TokenServiceClient.airdropTokens);

			return Proto.TokenService.Descriptor.FindMethodByName(methodname);
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenAirdrop = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenAirdrop = ToProtobuf();
        }

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.TransactionResponse response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}