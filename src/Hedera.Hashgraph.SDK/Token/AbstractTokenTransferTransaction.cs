// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Hook;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Token
{
    public abstract class AbstractTokenTransferTransaction<T> : Transaction<T> where T : AbstractTokenTransferTransaction<T>
    {
        protected List<TokenTransfer> tokenTransfers = [];
        protected List<TokenNftTransfer> nftTransfers = [];

        protected AbstractTokenTransferTransaction() { }
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AbstractTokenTransferTransaction(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal AbstractTokenTransferTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs) { }
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AbstractTokenTransferTransaction(Proto.TransactionBody)"]/*' />
		internal AbstractTokenTransferTransaction(Proto.TransactionBody txBody) : base(txBody) { }

		protected virtual List<TokenTransferList> SortTransfersAndBuild()
		{
			var transferLists = new List<TokenTransferList>();

			tokenTransfers = [ ..tokenTransfers.OrderBy((a) => a.TokenId).ThenBy((a) => a.AccountId).ThenBy((a) => a.IsApproved) ];
			nftTransfers = [ ..nftTransfers.OrderBy((a) => a.TokenId).ThenBy((a) => a.Sender).ThenBy((a) => a.Receiver).ThenBy((a) => a.Serial) ];

			var i = 0;
			var j = 0;

			// Effectively merge sort
			while (i < tokenTransfers.Count || j < nftTransfers.Count)
			{
				if (i < tokenTransfers.Count && j < nftTransfers.Count)
				{
					var iTokenId = tokenTransfers[i].TokenId;
					var jTokenId = nftTransfers[j].TokenId;
					var last = transferLists.Any() ? transferLists[transferLists.Count - 1] : null;
					var lastTokenId = last != null ? last.TokenId : null;
					if (last != null && iTokenId.CompareTo(lastTokenId) == 0)
					{
						last.Transfers.Add(tokenTransfers[i++]);
						continue;
					}

					if (last != null && jTokenId.CompareTo(lastTokenId) == 0)
					{
						last.NftTransfers.Add(nftTransfers[j++]);
						continue;
					}

					var result = iTokenId.CompareTo(jTokenId);
					if (result == 0)
					{
						transferLists.Add(new TokenTransferList(iTokenId, (uint?)tokenTransfers[i].ExpectedDecimals, tokenTransfers[i++], nftTransfers[j++]));
					}
					else if (result < 0)
					{
						transferLists.Add(new TokenTransferList(iTokenId, (uint?)tokenTransfers[i].ExpectedDecimals, tokenTransfers[i++], null));
					}
					else
					{
						transferLists.Add(new TokenTransferList(jTokenId, null, null, nftTransfers[j++]));
					}
				}
				else if (i < tokenTransfers.Count)
				{
					var iTokenId = tokenTransfers[i].TokenId;
					var last = !transferLists.Any() ? transferLists[transferLists.Count - 1] : null;
					var lastTokenId = last != null ? last.TokenId : null;
					if (last != null && iTokenId.CompareTo(lastTokenId) == 0)
					{
						last.Transfers.Add(tokenTransfers[i++]);
						continue;
					}

					transferLists.Add(new TokenTransferList(iTokenId, tokenTransfers[i].ExpectedDecimals, tokenTransfers[i++], null));
				}
				else
				{
					var jTokenId = nftTransfers[j].TokenId;
					var last = !transferLists.Any() ? transferLists[^1] : null;
					var lastTokenId = last != null ? last.TokenId : null;
					if (last != null && jTokenId.CompareTo(lastTokenId) == 0)
					{
						last.NftTransfers.Add(nftTransfers[j++]);
						continue;
					}

					transferLists.Add(new TokenTransferList(jTokenId, null, null, nftTransfers[j++]));
				}
			}

			return transferLists;
		}
		protected virtual T DoAddTokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved, uint? expectedDecimals, FungibleHookCall? hookCall)
		{
			RequireNotFrozen();
			foreach (var transfer in tokenTransfers)
			{
				if (transfer.TokenId.Equals(tokenId))
				{
					if (transfer.ExpectedDecimals != null && !transfer.ExpectedDecimals.Equals(expectedDecimals))
					{
						throw new ArgumentException("expected decimals for a token cannot be changed once set");
					}

					if (transfer.AccountId.Equals(accountId) && transfer.IsApproved == isApproved)
					{
						if (expectedDecimals != null)
						{
							transfer.ExpectedDecimals = expectedDecimals;
						}

						transfer.Amount += amount;
						transfer.HookCall = hookCall;

						// noinspection unchecked
						return (T)this;
					}
				}
			}


			// Create new record
			var tt = new TokenTransfer(tokenId, accountId, amount, expectedDecimals, isApproved, hookCall);
			tokenTransfers.Add(tt);

			// noinspection unchecked
			return (T)this;
		}
		protected virtual T DoAddNftTransfer(NftId nftId, AccountId sender, AccountId receiver, bool isApproved, NftHookCall? senderHookCall, NftHookCall? receiverHookCall)
		{
			RequireNotFrozen();
			nftTransfers.Add(new TokenNftTransfer(nftId.TokenId, sender, receiver, nftId.Serial, isApproved, senderHookCall, receiverHookCall));

			// noinspection unchecked
			return (T)this;
		}

		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:GetTokenIdDecimals"]/*' />
		public virtual Dictionary<TokenId, uint?> GetTokenIdDecimals()
        {
            Dictionary<TokenId, uint?> decimalsMap = [];
            foreach (var transfer in tokenTransfers)
            {
                decimalsMap.Add(transfer.TokenId, transfer.ExpectedDecimals);
            }

            return decimalsMap;
        }
        /// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:GetTokenTransfers"]/*' />
        public virtual Dictionary<TokenId, Dictionary<AccountId, long>> GetTokenTransfers()
        {
            Dictionary<TokenId, Dictionary<AccountId, long>> transfers = [];
            foreach (var transfer in tokenTransfers)
            {
                var current = transfers[transfer.TokenId] != null ? transfers[transfer.TokenId] : new Dictionary<AccountId, long>();
                current.Add(transfer.AccountId, transfer.Amount);
                transfers.Add(transfer.TokenId, current);
            }

            return transfers;
        }
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:GetTokenNftTransfers"]/*' />
		public virtual Dictionary<TokenId, IList<TokenNftTransfer>> GetTokenNftTransfers()
		{
			Dictionary<TokenId, IList<TokenNftTransfer>> transfers = [];
			foreach (var transfer in nftTransfers)
			{
				var current = transfers[transfer.TokenId] != null ? transfers[transfer.TokenId] : new List<TokenNftTransfer>();
				current.Add(transfer);
				transfers.Add(transfer.TokenId, current);
			}

			return transfers;
		}

		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddApprovedNftTransfer(NftId,AccountId,AccountId)"]/*' />
		public virtual T AddApprovedNftTransfer(NftId nftId, AccountId sender, AccountId receiver)
		{
			return DoAddNftTransfer(nftId, sender, receiver, true, null, null);
		}
        /// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddApprovedTokenTransfer(TokenId,AccountId,System.Int64)"]/*' />
        public virtual T AddApprovedTokenTransfer(TokenId tokenId, AccountId accountId, long value)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, true, null, null);
        }
        /// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddTokenTransferWithDecimals(TokenId,AccountId,System.Int64,System.UInt32)"]/*' />
        public virtual T AddTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, uint decimals)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, false, decimals, null);
        }
        /// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddApprovedTokenTransferWithDecimals(TokenId,AccountId,System.Int64,System.UInt32)"]/*' />
        public virtual T AddApprovedTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, uint decimals)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, true, decimals, null);
        }
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddNftTransfer(NftId,AccountId,AccountId)"]/*' />
		public virtual T AddNftTransfer(NftId nftId, AccountId sender, AccountId receiver)
		{
			return DoAddNftTransfer(nftId, sender, receiver, false, null, null);
		}
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:AddTokenTransfer(TokenId,AccountId,System.Int64)"]/*' />
		public virtual T AddTokenTransfer(TokenId tokenId, AccountId accountId, long value)
		{
			return DoAddTokenTransfer(tokenId, accountId, value, false, null, null);
		}
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:SetNftTransferApproval(NftId,System.Boolean)"]/*' />
		public virtual T SetNftTransferApproval(NftId nftId, bool isApproved)
		{
			RequireNotFrozen();
			foreach (var transfer in nftTransfers)
			{
				if (transfer.TokenId.Equals(nftId.TokenId) && transfer.Serial == nftId.Serial)
				{
					transfer.IsApproved = isApproved;

					// noinspection unchecked
					return (T)this;
				}
			}


			// noinspection unchecked
			return (T)this;
		}
		/// <include file="AbstractTokenTransferTransaction.cs.xml" path='docs/member[@name="M:SetTokenTransferApproval(TokenId,AccountId,System.Boolean)"]/*' />
		public virtual T SetTokenTransferApproval(TokenId tokenId, AccountId accountId, bool isApproved)
        {
            RequireNotFrozen();
            foreach (var transfer in tokenTransfers)
            {
                if (transfer.TokenId.Equals(tokenId) && transfer.AccountId.Equals(accountId))
                {
                    transfer.IsApproved = isApproved;

                    // noinspection unchecked
                    return (T)this;
                }
            }


            // noinspection unchecked
            return (T)this;
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var transfer in nftTransfers)
            {
                transfer.TokenId.ValidateChecksum(client);
                transfer.Sender.ValidateChecksum(client);
                transfer.Receiver.ValidateChecksum(client);
            }

            foreach (var transfer in tokenTransfers)
            {
                transfer.TokenId.ValidateChecksum(client);
                transfer.AccountId.ValidateChecksum(client);
            }
        }
    }
}