// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// </summary>
    /// <remarks>@deprecatedwith no replacement</remarks>
    [Obsolete("Obsolete")]
    public class AccountAllowanceAdjustTransaction : Transaction<AccountAllowanceAdjustTransaction>
    {
        private readonly IList<HbarAllowance> hbarAllowances = [];
        private readonly IList<TokenAllowance> tokenAllowances = [];
        private readonly IList<TokenNftAllowance> nftAllowances = [];
        // key is "{ownerId}:{spenderId}".  OwnerId may be "FEE_PAYER"
        private readonly Dictionary<string, Dictionary<TokenId, int>> nftMap = [];
        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountAllowanceAdjustTransaction()
        {
        }

        AccountAllowanceAdjustTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        AccountAllowanceAdjustTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        private void InitFromTransactionBody()
        {
            throw new NotSupportedException("Cannot construct AccountAllowanceAdjustTransaction from bytes");
        }

        private AccountAllowanceAdjustTransaction AdjustHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            RequireNotFrozen();
            hbarAllowances.Add(new HbarAllowance(ownerAccountId, spenderAccountId, amount));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="spenderAccountId">the spender account id</param>
        /// <param name="amount">the amount of hbar</param>
        /// <returns>                         an account allowance adjust transaction</returns>
        /// <remarks>
        /// @deprecated- Use {@link #grantHbarAllowance(AccountId, AccountId, Hbar)} or
        /// {@link #revokeHbarAllowance(AccountId, AccountId, Hbar)} instead
        /// </remarks>
        public virtual AccountAllowanceAdjustTransaction AddHbarAllowance(AccountId spenderAccountId, Hbar amount)
        {
            return AdjustHbarAllowance(null, spenderAccountId, amount);
        }

        /// <summary>
        ///  Grants Hbar allowance.
        /// </summary>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <param name="amount">the amount of Hbar</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceAdjustTransaction GrantHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            if (amount.CompareTo(Hbar.ZERO) < 0)
            {
                throw new ArgumentException("amount passed to grantHbarAllowance must be positive");
            }

            return AdjustHbarAllowance(ownerAccountId, spenderAccountId, amount);
        }

        /// <summary>
        /// Revokes Hbar allowance
        /// </summary>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <param name="amount">the amount of Hbar</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceAdjustTransaction RevokeHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            if (amount.CompareTo(Hbar.ZERO) < 0)
            {
                throw new ArgumentException("amount passed to revokeHbarAllowance must be positive");
            }

            return AdjustHbarAllowance(ownerAccountId, spenderAccountId, amount.Negated());
        }

        /// <summary>
        /// Get the Hbar allowances
        /// </summary>
        /// <returns>the Hbar allowances</returns>
        public virtual IList<HbarAllowance> GetHbarAllowances()
        {
            return [.. hbarAllowances];
        }

        private AccountAllowanceAdjustTransaction AdjustTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            RequireNotFrozen();
            tokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <param name="amount">the amount of hbar</param>
        /// <returns>                         an account allowance adjust transaction</returns>
        /// <remarks>
        /// @deprecated- Use {@link #grantTokenAllowance(TokenId, AccountId, AccountId, long)} or
        /// {@link #revokeTokenAllowance(TokenId, AccountId, AccountId, long)} instead
        /// </remarks>
        public virtual AccountAllowanceAdjustTransaction AddTokenAllowance(TokenId tokenId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, null, spenderAccountId, amount);
        }

        /// <summary>
        /// Grants token allowance.
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">the owner's id</param>
        /// <param name="spenderAccountId">the spender's id</param>
        /// <param name="amount">the amount of tokens</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceAdjustTransaction GrantTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount);
        }

        /// <summary>
        /// Revokes token allowance.
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">the owner's id</param>
        /// <param name="spenderAccountId">the spender's id</param>
        /// <param name="amount">the amount of tokens</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceAdjustTransaction RevokeTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, ownerAccountId, spenderAccountId, -amount);
        }

        /// <summary>
        /// Get the token allowances
        /// </summary>
        /// <returns>the token allowances</returns>
        public virtual IList<TokenAllowance> GetTokenAllowances()
        {
            return [.. tokenAllowances];
        }

        private static string OwnerToString(AccountId ownerAccountId)
        {
            return ownerAccountId != null ? ownerAccountId.ToString() : "FEE_PAYER";
        }

        private IList<long> GetNftSerials(AccountId ownerAccountId, AccountId spenderAccountId, TokenId tokenId)
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
                    return NewNftSerials(ownerAccountId, spenderAccountId, tokenId, innerMap);
                }
            }
            else
            {
                Dictionary<TokenId, int> innerMap = [];
                nftMap.Add(key, innerMap);
                return NewNftSerials(ownerAccountId, spenderAccountId, tokenId, innerMap);
            }
        }

        private IList<long> NewNftSerials(AccountId ownerAccountId, AccountId spenderAccountId, TokenId tokenId, Dictionary<TokenId, int> innerMap)
        {
            innerMap.Add(tokenId, nftAllowances.Count);
            TokenNftAllowance newAllowance = new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, new (), default);
            nftAllowances.Add(newAllowance);
            return newAllowance.serialNumbers;
        }

        private AccountAllowanceAdjustTransaction AdjustNftAllowance(TokenId tokenId, long serial, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            GetNftSerials(ownerAccountId, spenderAccountId, tokenId).Add(serial);
            return this;
        }

        private AccountAllowanceAdjustTransaction AdjustNftAllowanceAllSerials(TokenId tokenId, bool allSerials, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            nftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], allSerials));
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="nftId">the NFT's id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>                     an account allowance adjust transaction</returns>
        /// <remarks>
        /// @deprecated- Use {@link #grantTokenNftAllowance(NftId, AccountId, AccountId)} or
        /// {@link #revokeTokenNftAllowance(NftId, AccountId, AccountId)} instead
        /// </remarks>
        public virtual AccountAllowanceAdjustTransaction AddTokenNftAllowance(NftId nftId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, nftId.Serial, null, spenderAccountId);
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>                     an account allowance adjust transaction</returns>
        /// <remarks>
        /// @deprecated- Use {@link #grantTokenNftAllowanceAllSerials(TokenId, AccountId, AccountId)} or
        /// {@link #revokeTokenNftAllowanceAllSerials(TokenId, AccountId, AccountId)} instead
        /// </remarks>
        public virtual AccountAllowanceAdjustTransaction AddAllTokenNftAllowance(TokenId tokenId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, true, null, spenderAccountId);
        }

        /// <summary>
        /// Grants NFT allowance.
        /// </summary>
        /// <param name="nftId">the NFT's id</param>
        /// <param name="ownerAccountId">the owner's id</param>
        /// <param name="spenderAccountId">the spender's id</param>
        /// <returns>{@code this}</returns>
        public virtual AccountAllowanceAdjustTransaction GrantTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, nftId.Serial, ownerAccountId, spenderAccountId);
        }

        /// <summary>
        /// Grants allowance for all NFT serials of a token
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>                     an account allowance adjust transaction</returns>
        public virtual AccountAllowanceAdjustTransaction GrantTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, true, ownerAccountId, spenderAccountId);
        }

        /// <summary>
        /// </summary>
        /// <param name="nftId">the NFT's id</param>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>                     an account allowance adjust transaction</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual AccountAllowanceAdjustTransaction RevokeTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, -nftId.Serial, ownerAccountId, spenderAccountId);
        }

        /// <summary>
        /// Revokes allowance for all NFT serials of a token
        /// </summary>
        /// <param name="tokenId">the token's id</param>
        /// <param name="ownerAccountId">the owner's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <returns>                     an account allowance adjust transaction</returns>
        public virtual AccountAllowanceAdjustTransaction RevokeTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, false, ownerAccountId, spenderAccountId);
        }

        /// <summary>
        /// Get the NFT allowances
        /// </summary>
        /// <returns>a copy of {@link #nftAllowances}</returns>
        public virtual IList<TokenNftAllowance> GetTokenNftAllowances()
        {
            IList<TokenNftAllowance> retval = new List<TokenNftAllowance>(nftAllowances.Count);
            foreach (var allowance in nftAllowances)
            {
                retval.Add(TokenNftAllowance.CopyFrom(allowance));
            }

            return retval;
        }

        override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            throw new NotSupportedException("Cannot get method descriptor for AccountAllowanceAdjustTransaction");
        }

        override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
        }

        override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule AccountAllowanceAdjustTransaction");
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