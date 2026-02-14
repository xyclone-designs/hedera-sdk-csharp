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
		/// <summary>
		 /// Constructor.
		 /// </summary>
		 /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		 /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal AbstractTokenTransferTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs) { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
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

		/// <summary>
		/// Extract the list of token id decimals.
		/// </summary>
		/// <returns>the list of token id decimals</returns>
		public virtual Dictionary<TokenId, uint?> GetTokenIdDecimals()
        {
            Dictionary<TokenId, uint?> decimalsMap = [];
            foreach (var transfer in tokenTransfers)
            {
                decimalsMap.Add(transfer.TokenId, transfer.ExpectedDecimals);
            }

            return decimalsMap;
        }
        /// <summary>
        /// Extract the list of token transfer records.
        /// </summary>
        /// <returns>the list of token transfer records</returns>
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
		/// <summary>
		/// Extract the of token nft transfers.
		/// </summary>
		/// <returns>list of token nft transfers</returns>
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

		/// <summary>
		/// Add an approved nft transfer.
		/// </summary>
		/// <param name="nftId">the nft's id</param>
		/// <param name="sender">the sender account id</param>
		/// <param name="receiver">the receiver account id</param>
		/// <returns>the updated transaction</returns>
		public virtual T AddApprovedNftTransfer(NftId nftId, AccountId sender, AccountId receiver)
		{
			return DoAddNftTransfer(nftId, sender, receiver, true, null, null);
		}
        /// <summary>
        /// Add an approved token transfer to the transaction.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="value">the value</param>
        /// <returns>the updated transaction</returns>
        public virtual T AddApprovedTokenTransfer(TokenId tokenId, AccountId accountId, long value)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, true, null, null);
        }
        /// <summary>
        /// Add a non-approved token transfer with decimals.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="value">the value</param>
        /// <param name="decimals">the decimals</param>
        /// <returns>the updated transaction</returns>
        public virtual T AddTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, uint decimals)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, false, decimals, null);
        }
        /// <summary>
        /// Add an approved token transfer with decimals.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="accountId">the account id</param>
        /// <param name="value">the value</param>
        /// <param name="decimals">the decimals</param>
        /// <returns>the updated transaction</returns>
        public virtual T AddApprovedTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, uint decimals)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, true, decimals, null);
        }
		/// <summary>
		/// Add a non-approved nft transfer.
		/// </summary>
		/// <param name="nftId">the nft's id</param>
		/// <param name="sender">the sender account id</param>
		/// <param name="receiver">the receiver account id</param>
		/// <returns>the updated transaction</returns>
		public virtual T AddNftTransfer(NftId nftId, AccountId sender, AccountId receiver)
		{
			return DoAddNftTransfer(nftId, sender, receiver, false, null, null);
		}
		/// <summary>
		/// Add a non-approved token transfer to the transaction.
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <param name="accountId">the account id</param>
		/// <param name="value">the value</param>
		/// <returns>the updated transaction</returns>
		public virtual T AddTokenTransfer(TokenId tokenId, AccountId accountId, long value)
		{
			return DoAddTokenTransfer(tokenId, accountId, value, false, null, null);
		}
		/// <summary>
		/// </summary>
		/// <param name="nftId">the NFT id</param>
		/// <param name="isApproved">whether the transfer is approved</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecated- Use {@link #addApprovedNftTransfer(NftId, AccountId, AccountId)} instead</remarks>
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
		/// <summary>
		/// </summary>
		/// <param name="tokenId">the token id</param>
		/// <param name="accountId">the account id</param>
		/// <param name="isApproved">whether the transfer is approved</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecated- Use {@link #addApprovedTokenTransfer(TokenId, AccountId, long)} instead</remarks>
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