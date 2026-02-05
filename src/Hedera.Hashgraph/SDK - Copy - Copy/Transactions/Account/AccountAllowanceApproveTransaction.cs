// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// # Approve Allowance
    /// This transaction body provides a mechanism to add "allowance" entries
    /// for an account. These allowances enable one account to spend or transfer
    /// token balances (for fungible/common tokens), individual tokens (for
    /// non-fungible/unique tokens), or all non-fungible tokens owned by the
    /// account, now or in the future (if `approved_for_all` is set).
    /// </summary>
    public class AccountAllowanceApproveTransaction : Transaction<AccountAllowanceApproveTransaction>
    {
        private readonly IList<HbarAllowance> hbarAllowances = [];
        private readonly IList<TokenAllowance> tokenAllowances = [];
        private readonly IList<TokenNftAllowance> nftAllowances = [];
        // key is "{ownerId}:{spenderId}".  OwnerId may be "FEE_PAYER"
        // <ownerId:spenderId, <tokenId, index>>
        private readonly Dictionary<string, Dictionary<TokenId, int>> nftMap = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountAllowanceApproveTransaction()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        AccountAllowanceApproveTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        AccountAllowanceApproveTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoApproveAllowance;

            foreach (var allowanceProto in body.CryptoAllowances)
            {
                hbarAllowances.Add(HbarAllowance.FromProtobuf(allowanceProto));
            }

            foreach (var allowanceProto in body.TokenAllowances)
            {
                tokenAllowances.Add(TokenAllowance.FromProtobuf(allowanceProto));
            }

            foreach (var allowanceProto in body.NftAllowances)
            {
                if (allowanceProto.ApprovedForAll ?? false)
                {
                    nftAllowances.Add(TokenNftAllowance.FromProtobuf(allowanceProto));
                }
                else
                {
                    GetNftSerials(
                        allowanceProto.Owner is not null 
                            ? AccountId.FromProtobuf(allowanceProto.Owner)
                            : null, 
                        AccountId.FromProtobuf(allowanceProto.Spender),
                        allowanceProto.DelegatingSpender is not null 
                            ? AccountId.FromProtobuf(allowanceProto.DelegatingSpender) 
                            : null, 
                        TokenId.FromProtobuf(allowanceProto.TokenId)).Concat(allowanceProto.SerialNumbers);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="spenderAccountId">the spender account id</param>
        /// <param name="amount">the amount of hbar</param>
        /// <returns>                         an account allowance approve transaction</returns>
        /// <remarks>@deprecated- Use {@link #approveHbarAllowance(AccountId, AccountId, Hbar)} instead</remarks>
        public virtual AccountAllowanceApproveTransaction AddHbarAllowance(AccountId spenderAccountId, Hbar amount)
        {
            return ApproveHbarAllowance(null, spenderAccountId, amount);
        }

        /// <summary>
        /// Approves the Hbar allowance.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <param name="amount">amount of hbar add</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction ApproveHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(amount);
            hbarAllowances.Add(new HbarAllowance(ownerAccountId, spenderAccountId, amount));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>                         list of hbar allowance records</returns>
        /// <remarks>@deprecated- Use {@link #getHbarApprovals()} instead</remarks>
        public virtual IList<HbarAllowance> GetHbarAllowances()
        {
            return GetHbarApprovals();
        }

        /// <summary>
        /// Extract the list of hbar allowances.
        /// </summary>
        /// <returns>                         array list of hbar allowances</returns>
        public virtual IList<HbarAllowance> GetHbarApprovals()
        {
            return [.. hbarAllowances];
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="spenderAccountId">the spenders account id</param>
        /// <param name="amount">the hbar amount</param>
        /// <returns>                         an account allowance approve transaction</returns>
        /// <remarks>@deprecated- Use {@link #approveTokenAllowance(TokenId, AccountId, AccountId, long)} instead</remarks>
        public virtual AccountAllowanceApproveTransaction AddTokenAllowance(TokenId tokenId, AccountId spenderAccountId, long amount)
        {
            return ApproveTokenAllowance(tokenId, null, spenderAccountId, amount);
        }

        /// <summary>
        /// Approves the Token allowance.
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <param name="amount">amount of tokens</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction ApproveTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            RequireNotFrozen();
            tokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>                         a list of token allowances</returns>
        /// <remarks>@deprecated- Use {@link #getTokenApprovals()} instead</remarks>
        public virtual IList<TokenAllowance> GetTokenAllowances()
        {
            return GetTokenApprovals();
        }

        /// <summary>
        /// Extract a list of token allowance approvals.
        /// </summary>
        /// <returns>                         array list of token approvals.</returns>
        public virtual IList<TokenAllowance> GetTokenApprovals()
        {
            return [.. tokenAllowances];
        }

        /// <summary>
        /// Extract the owner as a string.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <returns>                         a string representation of the account id
        ///                                  or FEE_PAYER</returns>
        private static string OwnerToString(AccountId ownerAccountId)
        {
            return ownerAccountId != null ? ownerAccountId.ToString() : "FEE_PAYER";
        }

        /// <summary>
        /// Return a list of NFT serial numbers.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <param name="delegatingSpender">delegating spender's account id</param>
        /// <param name="tokenId">the token's id</param>
        /// <returns>list of NFT serial numbers</returns>
        private IList<long> GetNftSerials(AccountId ownerAccountId, AccountId spenderAccountId, AccountId delegatingSpender, TokenId tokenId)
        {
            var key = OwnerToString(ownerAccountId) + ":" + spenderAccountId;
            if (nftMap.ContainsKey(key))
            {
                var innerMap = nftMap[key];
                if (innerMap.ContainsKey(tokenId))
                {
                    return nftAllowances[innerMap[tokenId]].serialNumbers;
                }
                else
                {
                    return NewNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, tokenId, innerMap);
                }
            }
            else
            {
                Dictionary<TokenId, int> innerMap = [];
                nftMap.Add(key, innerMap);
                return NewNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, tokenId, innerMap);
            }
        }

        /// <summary>
        /// Add NFT serials.
        /// </summary>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <param name="delegatingSpender">delegating spender's account id</param>
        /// <param name="tokenId">the token's id</param>
        /// <param name="innerMap">list of token id's and serial number records</param>
        /// <returns>list of NFT serial numbers</returns>
        private IList<long> NewNftSerials(AccountId ownerAccountId, AccountId spenderAccountId, AccountId delegatingSpender, TokenId tokenId, Dictionary<TokenId, int> innerMap)
        {
            innerMap.Add(tokenId, nftAllowances.Count);
            TokenNftAllowance newAllowance = new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, delegatingSpender, new (), default);
            nftAllowances.Add(newAllowance);
            return newAllowance.serialNumbers;
        }

        /// <summary>
        /// </summary>
        /// <param name="nftId">the nft id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecated- Use {@link #approveTokenNftAllowance(NftId, AccountId, AccountId, AccountId)} instead</remarks>
        public virtual AccountAllowanceApproveTransaction AddTokenNftAllowance(NftId nftId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            GetNftSerials(null, spenderAccountId, null, nftId.TokenId).Add(nftId.Serial);
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecated- Use {@link #approveTokenNftAllowanceAllSerials(TokenId, AccountId, AccountId)} instead</remarks>
        public virtual AccountAllowanceApproveTransaction AddAllTokenNftAllowance(TokenId tokenId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            nftAllowances.Add(new TokenNftAllowance(tokenId, null, spenderAccountId, null, [], true));
            return this;
        }

        /// <summary>
        /// Approve the NFT allowance.
        /// </summary>
        /// <param name="nftId">nft's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <param name="delegatingSpender">delegating spender's account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId, AccountId delegatingSpender)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftId);
            GetNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, nftId.TokenId).Add(nftId.Serial);
            return this;
        }

        /// <summary>
        /// Approve the NFT allowance.
        /// </summary>
        /// <param name="nftId">nft's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftId);
            GetNftSerials(ownerAccountId, spenderAccountId, null, nftId.TokenId).Add(nftId.Serial);
            return this;
        }

        /// <summary>
        /// Approve the token nft allowance on all serials.
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            nftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], true));
            return this;
        }

        /// <summary>
        /// Delete the token nft allowance on all serials.
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">owner's account id</param>
        /// <param name="spenderAccountId">spender's account id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceApproveTransaction DeleteTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            nftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], false));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns>{@code this}</returns>
        /// <remarks>@deprecated- Use {@link #getTokenNftApprovals()} instead</remarks>
        public virtual IList<TokenNftAllowance> GetTokenNftAllowances()
        {
            return GetTokenNftApprovals();
        }

        /// <summary>
        /// Returns the list of token nft allowances.
        /// </summary>
        /// <returns> list of token nft allowances.</returns>
        public virtual IList<TokenNftAllowance> GetTokenNftApprovals()
        {
            IList<TokenNftAllowance> retval = new List(nftAllowances.Count);
            foreach (var allowance in nftAllowances)
            {
                retval.Add(TokenNftAllowance.CopyFrom(allowance));
            }

            return retval;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return CryptoServiceGrpc.ApproveAllowancesMethod;
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoApproveAllowanceTransactionBody builder }</returns>
        public virtual Proto.CryptoApproveAllowanceTransactionBody Build()
        {
            var builder = new Proto.CryptoApproveAllowanceTransactionBody();

            foreach (var allowance in hbarAllowances)
            {
                builder.CryptoAllowances.Add(allowance.ToProtobuf());
            }

            foreach (var allowance in tokenAllowances)
            {
                builder.TokenAllowances.Add(allowance.ToProtobuf());
            }

            foreach (var allowance in nftAllowances)
            {
                builder.NftAllowances.Add(allowance.ToProtobuf());
            }

            return builder;
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoApproveAllowance = Build();
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoApproveAllowance = Build();
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var allowance in hbarAllowances)
            {
                allowance.ValidateChecksums(client);
            }

            foreach (var allowance in tokenAllowances)
            {
                allowance.ValidateChecksums(client);
            }

            foreach (var allowance in nftAllowances)
            {
                allowance.ValidateChecksums(client);
            }
        }
    }
}