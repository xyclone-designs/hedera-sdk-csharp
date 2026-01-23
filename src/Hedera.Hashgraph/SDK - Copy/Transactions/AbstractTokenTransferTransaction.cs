// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions
{
    abstract class AbstractTokenTransferTransaction<T> : Transaction<T> where T : AbstractTokenTransferTransaction<T>
    {
        protected readonly List<TokenTransfer> tokenTransfers = new ();
        protected readonly List<TokenNftTransfer> nftTransfers = new ();
        protected AbstractTokenTransferTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        AbstractTokenTransferTransaction(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AbstractTokenTransferTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
        }

        /// <summary>
        /// Extract the list of token id decimals.
        /// </summary>
        /// <returns>the list of token id decimals</returns>
        public virtual Dictionary<TokenId, int> GetTokenIdDecimals()
        {
            Dictionary<TokenId, int> decimalsMap = new HashMap();
            foreach (var transfer in tokenTransfers)
            {
                decimalsMap.Put(transfer.tokenId, transfer.expectedDecimals);
            }

            return decimalsMap;
        }

        /// <summary>
        /// Extract the list of token transfer records.
        /// </summary>
        /// <returns>the list of token transfer records</returns>
        public virtual Dictionary<TokenId, Dictionary<AccountId, long>> GetTokenTransfers()
        {
            Dictionary<TokenId, Dictionary<AccountId, long>> transfers = new HashMap();
            foreach (var transfer in tokenTransfers)
            {
                var current = transfers[transfer.tokenId] != null ? transfers[transfer.tokenId] : new HashMap<AccountId, long>();
                current.Put(transfer.accountId, transfer.amount);
                transfers.Put(transfer.tokenId, current);
            }

            return transfers;
        }

        protected virtual T DoAddTokenTransfer(TokenId tokenId, AccountId accountId, long amount, bool isApproved, int expectedDecimals, FungibleHookCall hookCall)
        {
            RequireNotFrozen();
            foreach (var transfer in tokenTransfers)
            {
                if (transfer.tokenId.Equals(tokenId))
                {
                    if (transfer.expectedDecimals != null && !transfer.expectedDecimals.Equals(expectedDecimals))
                    {
                        throw new ArgumentException("expected decimals for a token cannot be changed once set");
                    }

                    if (transfer.accountId.Equals(accountId) && transfer.isApproved == isApproved)
                    {
                        if (expectedDecimals != null)
                        {
                            transfer.expectedDecimals = expectedDecimals;
                        }

                        transfer.amount += amount;
                        transfer.hookCall = hookCall;

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
        public virtual T AddTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, int decimals)
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
        public virtual T AddApprovedTokenTransferWithDecimals(TokenId tokenId, AccountId accountId, long value, int decimals)
        {
            return DoAddTokenTransfer(tokenId, accountId, value, true, decimals, null);
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
                if (transfer.tokenId.Equals(tokenId) && transfer.accountId.Equals(accountId))
                {
                    transfer.isApproved = isApproved;

                    // noinspection unchecked
                    return (T)this;
                }
            }


            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Extract the of token nft transfers.
        /// </summary>
        /// <returns>list of token nft transfers</returns>
        public virtual Dictionary<TokenId, IList<TokenNftTransfer>> GetTokenNftTransfers()
        {
            Dictionary<TokenId, IList<TokenNftTransfer>> transfers = new HashMap();
            foreach (var transfer in nftTransfers)
            {
                var current = transfers[transfer.tokenId] != null ? transfers[transfer.tokenId] : new List<TokenNftTransfer>();
                current.Add(transfer);
                transfers.Put(transfer.tokenId, current);
            }

            return transfers;
        }

        protected virtual T DoAddNftTransfer(NftId nftId, AccountId sender, AccountId receiver, bool isApproved, NftHookCall senderHookCall, NftHookCall receiverHookCall)
        {
            RequireNotFrozen();
            nftTransfers.Add(new TokenNftTransfer(nftId.TokenId, sender, receiver, nftId.Serial, isApproved, senderHookCall, receiverHookCall));

            // noinspection unchecked
            return (T)this;
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
                if (transfer.tokenId.Equals(nftId.TokenId) && transfer.serial == nftId.Serial)
                {
                    transfer.isApproved = isApproved;

                    // noinspection unchecked
                    return (T)this;
                }
            }


            // noinspection unchecked
            return (T)this;
        }

        protected virtual List<TokenTransferList> SortTransfersAndBuild()
        {
            var transferLists = new List<TokenTransferList>();
            tokenTransfers.Sort(Comparator.Comparing((TokenTransfer a) => a.tokenId).ThenComparing((a) => a.accountId).ThenComparing((a) => a.isApproved));
            nftTransfers.Sort(Comparator.Comparing((TokenNftTransfer a) => a.tokenId).ThenComparing((a) => a.sender).ThenComparing((a) => a.receiver).ThenComparing((a) => a.serial));
            var i = 0;
            var j = 0;

            // Effectively merge sort
            while (i < tokenTransfers.Count || j < nftTransfers.Count)
            {
                if (i < tokenTransfers.Count && j < nftTransfers.Count)
                {
                    var iTokenId = tokenTransfers[i].tokenId;
                    var jTokenId = nftTransfers[j].tokenId;
                    var last = !transferLists.IsEmpty() ? transferLists[transferLists.Count - 1] : null;
                    var lastTokenId = last != null ? last.tokenId : null;
                    if (last != null && iTokenId.CompareTo(lastTokenId) == 0)
                    {
                        last.transfers.Add(tokenTransfers[i++]);
                        continue;
                    }

                    if (last != null && jTokenId.CompareTo(lastTokenId) == 0)
                    {
                        last.nftTransfers.Add(nftTransfers[j++]);
                        continue;
                    }

                    var result = iTokenId.CompareTo(jTokenId);
                    if (result == 0)
                    {
                        transferLists.Add(new TokenTransferList(iTokenId, tokenTransfers[i].expectedDecimals, tokenTransfers[i++], nftTransfers[j++]));
                    }
                    else if (result < 0)
                    {
                        transferLists.Add(new TokenTransferList(iTokenId, tokenTransfers[i].expectedDecimals, tokenTransfers[i++], null));
                    }
                    else
                    {
                        transferLists.Add(new TokenTransferList(jTokenId, null, null, nftTransfers[j++]));
                    }
                }
                else if (i < tokenTransfers.Count)
                {
                    var iTokenId = tokenTransfers[i].tokenId;
                    var last = !transferLists.IsEmpty() ? transferLists[transferLists.Count - 1] : null;
                    var lastTokenId = last != null ? last.tokenId : null;
                    if (last != null && iTokenId.CompareTo(lastTokenId) == 0)
                    {
                        last.transfers.Add(tokenTransfers[i++]);
                        continue;
                    }

                    transferLists.Add(new TokenTransferList(iTokenId, tokenTransfers[i].expectedDecimals, tokenTransfers[i++], null));
                }
                else
                {
                    var jTokenId = nftTransfers[j].tokenId;
                    var last = !transferLists.IsEmpty() ? transferLists[transferLists.Count - 1] : null;
                    var lastTokenId = last != null ? last.tokenId : null;
                    if (last != null && jTokenId.CompareTo(lastTokenId) == 0)
                    {
                        last.nftTransfers.Add(nftTransfers[j++]);
                        continue;
                    }

                    transferLists.Add(new TokenTransferList(jTokenId, null, null, nftTransfers[j++]));
                }
            }

            return transferLists;
        }

        override void ValidateChecksums(Client client)
        {
            foreach (var transfer in nftTransfers)
            {
                transfer.tokenId.ValidateChecksum(client);
                transfer.sender.ValidateChecksum(client);
                transfer.receiver.ValidateChecksum(client);
            }

            foreach (var transfer in tokenTransfers)
            {
                transfer.tokenId.ValidateChecksum(client);
                transfer.accountId.ValidateChecksum(client);
            }
        }
    }
}