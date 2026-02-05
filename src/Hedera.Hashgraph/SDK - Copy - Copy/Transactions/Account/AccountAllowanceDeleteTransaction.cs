// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Token;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// Delete one or more allowances.
    /// Given one or more, previously approved, allowances for non-fungible/unique
    /// tokens to be transferred by a spending account from an owning account;
    /// this transaction removes a specified set of those allowances.
    /// 
    /// The owner account for each listed allowance MUST sign this transaction.
    /// Allowances for HBAR cannot be removed with this transaction. The owner
    /// account MUST submit a new `cryptoApproveAllowance` transaction with the
    /// amount set to `0` to "remove" that allowance.
    /// Allowances for fungible/common tokens cannot be removed with this
    /// transaction. The owner account MUST submit a new `cryptoApproveAllowance`
    /// transaction with the amount set to `0` to "remove" that allowance.
    /// 
    /// ### Block Stream Effects
    /// None
    /// </summary>
    public class AccountAllowanceDeleteTransaction : Transaction<AccountAllowanceDeleteTransaction>
    {
        private readonly IList<HbarAllowance> hbarAllowances = [];
        private readonly IList<TokenAllowance> tokenAllowances = [];
        private readonly IList<TokenNftAllowance> nftAllowances = [];
        // <ownerId, <tokenId, index>>
        private readonly Dictionary<AccountId, Dictionary<TokenId, int>> nftMap = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountAllowanceDeleteTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        AccountAllowanceDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AccountAllowanceDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoDeleteAllowance;
            foreach (var allowanceProto in body.NftAllowances)
            {
                GetNftSerials(AccountId.FromProtobuf(allowanceProto.Owner), TokenId.FromProtobuf(allowanceProto.TokenId))
                    .Add(allowanceProto.SerialNumbers);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual AccountAllowanceDeleteTransaction DeleteAllHbarAllowances(AccountId ownerAccountId)
        {
            RequireNotFrozen();
            hbarAllowances.Add(new HbarAllowance(ownerAccountId, null, null));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>                         a list of hbar allowance records</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual IList<HbarAllowance> GetHbarAllowanceDeletions()
        {
            return [.. hbarAllowances];
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual AccountAllowanceDeleteTransaction DeleteAllTokenAllowances(TokenId tokenId, AccountId ownerAccountId)
        {
            RequireNotFrozen();
            tokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, null, 0));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>                         a list of token allowance records</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual IList<TokenAllowance> GetTokenAllowanceDeletions()
        {
            return [.. tokenAllowances];
        }

        /// <summary>
        /// Remove all nft token allowances.
        /// </summary>
        /// <param name="nftId">nft's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <returns>                         {@code this}</returns>
        public virtual AccountAllowanceDeleteTransaction DeleteAllTokenNftAllowances(NftId nftId, AccountId ownerAccountId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftId);
            GetNftSerials(ownerAccountId, nftId.TokenId).Add(nftId.Serial);
            return this;
        }

        /// <summary>
        /// Return list of nft tokens to be deleted.
        /// </summary>
        /// <returns>                         list of token nft allowances</returns>
        public virtual IList<TokenNftAllowance> GetTokenNftAllowanceDeletions()
        {
            IList<TokenNftAllowance> retval = new List<TokenNftAllowance>(nftAllowances.Count);
            foreach (var allowance in nftAllowances)
            {
                retval.Add(TokenNftAllowance.CopyFrom(allowance));
            }

            return retval;
        }

        /// <summary>
        /// Return list of nft serial numbers.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="tokenId">the token's id</param>
        /// <returns>                         list of nft serial numbers</returns>
        private IList<long> GetNftSerials(AccountId ownerAccountId, TokenId tokenId)
        {
            var key = ownerAccountId;
            if (nftMap.ContainsKey(key))
            {
                var innerMap = nftMap[key];
                if (innerMap.ContainsKey(tokenId))
                {
                    return nftAllowances[innerMap[tokenId]].serialNumbers;
                }
                else
                {
                    return NewNftSerials(ownerAccountId, tokenId, innerMap);
                }
            }
            else
            {
                Dictionary<TokenId, int> innerMap = [];
                nftMap.Add(key, innerMap);
                return NewNftSerials(ownerAccountId, tokenId, innerMap);
            }
        }

        /// <summary>
        /// Return serial numbers of new nft's.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="tokenId">the token's id</param>
        /// <param name="innerMap">list of token id's and serial number records</param>
        /// <returns>                         list of nft serial numbers</returns>
        private IList<long> NewNftSerials(AccountId ownerAccountId, TokenId tokenId, Dictionary<TokenId, int> innerMap)
        {
            innerMap.Add(tokenId, nftAllowances.Count);
            TokenNftAllowance newAllowance = new TokenNftAllowance(tokenId, ownerAccountId, null, null, new (), default);
            nftAllowances.Add(newAllowance);
            return newAllowance.serialNumbers;
        }

        override MethodDescriptor<Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.GetDeleteAllowancesMethod();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link CryptoDeleteAllowanceTransactionBody}</returns>
        public virtual Proto.CryptoDeleteAllowanceTransactionBody Build()
        {
            var builder = new Proto.CryptoDeleteAllowanceTransactionBody();
            foreach (var allowance in nftAllowances)
            {
                builder.NftAllowances.Add(allowance.ToRemoveProtobuf());
            }

            return builder;
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDeleteAllowance = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDeleteAllowance = Build();
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var allowance in nftAllowances)
            {
                allowance.ValidateChecksums(client);
            }
        }
    }
}