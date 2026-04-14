// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Transactions
{
    /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="T:TransferTransaction"]/*' />
    public partial class TransferTransaction : AbstractTokenTransferTransaction<TransferTransaction>
    {
        private readonly List<HbarTransfer> hbarTransfers = [];
        
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.#ctor"]/*' />
        public TransferTransaction()
        {
            DefaultMaxTransactionFee = new Hbar(1);
        }
		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal TransferTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal TransferTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		public static NftHookCall ToNftHook(Proto.Services.HookCall proto, NftHookType type)
		{
			return new NftHookCall(proto.HookId, EvmHookCall.FromProtobuf(proto.EvmHookCall), type);
		}
		public static FungibleHookCall ToFungibleHook(Proto.Services.HookCall proto, FungibleHookType type)
		{
			return new FungibleHookCall(proto.HookId, EvmHookCall.FromProtobuf(proto.EvmHookCall), type);
		}

		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.InitFromTransactionBody"]/*' />
		private void InitFromTransactionBody()
		{
			var body = SourceTransactionBody.CryptoTransfer;

			foreach (var transfer in body.Transfers.AccountAmounts)
				hbarTransfers.Add(HbarTransfer.FromProtobuf(transfer));

			foreach (var tokenTransferList in body.TokenTransfers)
			{
				var fungibleTokenTransfers = TokenTransfer.FromProtobuf(tokenTransferList);
				tokenTransfers.AddRange(fungibleTokenTransfers);
				var nftTokenTransfers = TokenNftTransfer.FromProtobuf(tokenTransferList);
				nftTransfers.AddRange(nftTokenTransfers);
			}
		}
		private TransferTransaction DoAddHbarTransfer(AccountId accountId, Hbar value, bool isApproved, FungibleHookCall? hookCall)
		{
			RequireNotFrozen();
			foreach (var transfer in hbarTransfers)
			{
				if (transfer.AccountId.Equals(accountId))
				{
					long combinedTinybars = transfer.Amount.ToTinybars() + value.ToTinybars();
					transfer.Amount = Hbar.FromTinybars(combinedTinybars);
					transfer.IsApproved = transfer.IsApproved || isApproved;
					return this;
				}
			}

			hbarTransfers.Add(new HbarTransfer(accountId, value, isApproved, hookCall));

			return this;
		}

		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.GetHbarTransfers"]/*' />
		public virtual Dictionary<AccountId, Hbar> GetHbarTransfers()
        {
            Dictionary<AccountId, Hbar> transfers = [];
            foreach (var transfer in hbarTransfers)
            {
                transfers.Add(transfer.AccountId, transfer.Amount);
            }

            return transfers;
        }
		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.ToProtobuf"]/*' />
		public virtual Proto.Services.CryptoTransferTransactionBody ToProtobuf()
		{
			var transfers = SortTransfersAndBuild();
			var builder = new Proto.Services.CryptoTransferTransactionBody();

			var hbarTransfersList = new Proto.Services.TransferList();

			foreach (var transfer in hbarTransfers.OrderBy(_ => _.AccountId).ThenBy(_ => _.IsApproved))
			{
				hbarTransfersList.AccountAmounts.Add(transfer.ToProtobuf());
			}

			builder.Transfers = hbarTransfersList;

			foreach (var transfer in transfers)
			{
				builder.TokenTransfers.Add(transfer.ToProtobuf());
			}

			return builder;
		}
		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddHbarTransfer(AccountId,Hbar)"]/*' />
		public virtual TransferTransaction AddHbarTransfer(AccountId accountId, Hbar value)
		{
			return DoAddHbarTransfer(accountId, value, false, null);
		}
		/// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddHbarTransfer(EvmAddress,Hbar)"]/*' />
		public virtual TransferTransaction AddHbarTransfer(EvmAddress evmAddress, Hbar value)
        {
            AccountId accountId = AccountId.FromEvmAddress(evmAddress, 0, 0);
            return DoAddHbarTransfer(accountId, value, false, null);
        }
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddApprovedHbarTransfer(AccountId,Hbar)"]/*' />
        public virtual TransferTransaction AddApprovedHbarTransfer(AccountId accountId, Hbar value)
        {
            return DoAddHbarTransfer(accountId, value, true, null);
        }
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddTokenTransferWithHook(TokenId,AccountId,System.Int64,FungibleHookCall)"]/*' />
        public virtual TransferTransaction AddTokenTransferWithHook(TokenId tokenId, AccountId accountId, long value, FungibleHookCall? hookCall)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, false, null, hookCall);
        }
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddNftTransferWithHook(NftId,AccountId,AccountId,NftHookCall,NftHookCall)"]/*' />
        public virtual TransferTransaction AddNftTransferWithHook(NftId nftId, AccountId senderAccountId, AccountId receiverAccountId, NftHookCall senderHookCall, NftHookCall receiverHookCall)
        {
            return DoAddNftTransfer(nftId, senderAccountId, receiverAccountId, false, senderHookCall, receiverHookCall);
        }
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.AddHbarTransferWithHook(AccountId,Hbar,FungibleHookCall)"]/*' />
        public virtual TransferTransaction AddHbarTransferWithHook(AccountId accountId, Hbar amount, FungibleHookCall hookCall)
        {
            RequireNotFrozen();
            return DoAddHbarTransfer(accountId, amount, false, hookCall);
        }
        /// <include file="TransferTransaction.cs.xml" path='docs/member[@name="M:TransferTransaction.SetHbarTransferApproval(AccountId,System.Boolean)"]/*' />
        public virtual TransferTransaction SetHbarTransferApproval(AccountId accountId, bool isApproved)
        {
            RequireNotFrozen();
            foreach (var transfer in hbarTransfers)
            {
                if (transfer.AccountId.Equals(accountId))
                {
                    transfer.IsApproved = isApproved;
                    return this;
                }
            }

            return this;
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var transfer in hbarTransfers)
            {
                transfer.AccountId.ValidateChecksum(client);
            }
        }
        public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoTransfer = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoTransfer = ToProtobuf();
        }

        public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.cryptoDelete);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
		{
			throw new NotImplementedException();
		}
		public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
		{
			throw new NotImplementedException();
		}
	}
}
