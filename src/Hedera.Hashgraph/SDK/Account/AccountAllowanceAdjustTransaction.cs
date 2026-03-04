// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public class AccountAllowanceAdjustTransaction : Transaction<AccountAllowanceAdjustTransaction>
    {
		// key is "{ownerId}:{spenderId}".  OwnerId may be "FEE_PAYER"
		private readonly Dictionary<string, Dictionary<TokenId, int>> nftMap = [];
		private readonly List<HbarAllowance> HbarAllowances = [];
        private readonly List<TokenAllowance> TokenAllowances = [];
        private readonly List<TokenNftAllowance> NftAllowances = [];
        
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceAdjustTransaction"]/*' />
        public AccountAllowanceAdjustTransaction() { }
		internal AccountAllowanceAdjustTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		internal AccountAllowanceAdjustTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		private static string OwnerToString(AccountId? ownerAccountId)
		{
			return ownerAccountId?.ToString() ?? "FEE_PAYER";
		}

		private void InitFromTransactionBody()
        {
            throw new NotSupportedException("Cannot construct AccountAllowanceAdjustTransaction from bytes");
        }

        private AccountAllowanceAdjustTransaction AdjustHbarAllowance(AccountId? ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            RequireNotFrozen();
            HbarAllowances.Add(new HbarAllowance(ownerAccountId, spenderAccountId, amount));
            return this;
        }
		private AccountAllowanceAdjustTransaction AdjustTokenAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId spenderAccountId, long amount)
		{
			RequireNotFrozen();
			TokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount));
			return this;
		}



		/// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:AddHbarAllowance(AccountId,Hbar)"]/*' />
		[Obsolete]
		public virtual AccountAllowanceAdjustTransaction AddHbarAllowance(AccountId spenderAccountId, Hbar amount)
        {
            return AdjustHbarAllowance(null, spenderAccountId, amount);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GrantHbarAllowance(AccountId,AccountId,Hbar)"]/*' />
        public virtual AccountAllowanceAdjustTransaction GrantHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            if (amount.CompareTo(Hbar.ZERO) < 0)
            {
                throw new ArgumentException("amount passed to grantHbarAllowance must be positive");
            }

            return AdjustHbarAllowance(ownerAccountId, spenderAccountId, amount);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:RevokeHbarAllowance(AccountId,AccountId,Hbar)"]/*' />
        public virtual AccountAllowanceAdjustTransaction RevokeHbarAllowance(AccountId ownerAccountId, AccountId spenderAccountId, Hbar amount)
        {
            if (amount.CompareTo(Hbar.ZERO) < 0)
            {
                throw new ArgumentException("amount passed to revokeHbarAllowance must be positive");
            }

            return AdjustHbarAllowance(ownerAccountId, spenderAccountId, amount.Negated());
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:AddTokenAllowance(TokenId,AccountId,System.Int64)"]/*' />
        public virtual AccountAllowanceAdjustTransaction AddTokenAllowance(TokenId tokenId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, null, spenderAccountId, amount);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GrantTokenAllowance(TokenId,AccountId,AccountId,System.Int64)"]/*' />
        public virtual AccountAllowanceAdjustTransaction GrantTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:RevokeTokenAllowance(TokenId,AccountId,AccountId,System.Int64)"]/*' />
        public virtual AccountAllowanceAdjustTransaction RevokeTokenAllowance(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId, long amount)
        {
            return AdjustTokenAllowance(tokenId, ownerAccountId, spenderAccountId, -amount);
        }

		/// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GetHbarAllowances"]/*' />
		public virtual List<HbarAllowance> GetHbarAllowances()
		{
			return [.. HbarAllowances];
		}
		/// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GetTokenAllowances"]/*' />
		public virtual List<TokenAllowance> GetTokenAllowances()
        {
            return [.. TokenAllowances];
        }
		/// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GetTokenNftAllowances"]/*' />
		public virtual List<TokenNftAllowance> GetTokenNftAllowances()
		{
            return [.. NftAllowances.Select(_ => TokenNftAllowance.CopyFrom(_))];
		}

        private List<long> GetNftSerials(AccountId? ownerAccountId, AccountId spenderAccountId, TokenId tokenId)
        {
            var key = OwnerToString(ownerAccountId) + ":" + spenderAccountId;

            if (nftMap.TryGetValue(key, out Dictionary<TokenId, int>? nftInnerMap))
            {
                if (nftInnerMap.TryGetValue(tokenId, out int value))
                {
                    return NftAllowances[value].SerialNumbers;
                }
                else
                {
                    return NewNftSerials(ownerAccountId, spenderAccountId, tokenId, nftInnerMap);
                }
            }
            else
            {
                Dictionary<TokenId, int> innerMap = [];
                nftMap.Add(key, innerMap);
                return NewNftSerials(ownerAccountId, spenderAccountId, tokenId, innerMap);
            }
        }
        private List<long> NewNftSerials(AccountId? ownerAccountId, AccountId spenderAccountId, TokenId tokenId, Dictionary<TokenId, int> innerMap)
        {
            innerMap.Add(tokenId, NftAllowances.Count);
            TokenNftAllowance newAllowance = new (tokenId, ownerAccountId, spenderAccountId, null, [], default);
            NftAllowances.Add(newAllowance);
            return newAllowance.SerialNumbers;
        }

        private AccountAllowanceAdjustTransaction AdjustNftAllowance(TokenId tokenId, long serial, AccountId? ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            GetNftSerials(ownerAccountId, spenderAccountId, tokenId).Add(serial);
            return this;
        }
        private AccountAllowanceAdjustTransaction AdjustNftAllowanceAllSerials(TokenId tokenId, bool allSerials, AccountId? ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
			NftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], allSerials));
            return this;
        }

        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:AddTokenNftAllowance(NftId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction AddTokenNftAllowance(NftId nftId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, nftId.Serial, null, spenderAccountId);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:AddAllTokenNftAllowance(TokenId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction AddAllTokenNftAllowance(TokenId tokenId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, true, null, spenderAccountId);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GrantTokenNftAllowance(NftId,AccountId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction GrantTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, nftId.Serial, ownerAccountId, spenderAccountId);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:GrantTokenNftAllowanceAllSerials(TokenId,AccountId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction GrantTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, true, ownerAccountId, spenderAccountId);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:RevokeTokenNftAllowance(NftId,AccountId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction RevokeTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowance(nftId.TokenId, -nftId.Serial, ownerAccountId, spenderAccountId);
        }
        /// <include file="AccountAllowanceAdjustTransaction.cs.xml" path='docs/member[@name="M:RevokeTokenNftAllowanceAllSerials(TokenId,AccountId,AccountId)"]/*' />
        public virtual AccountAllowanceAdjustTransaction RevokeTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            return AdjustNftAllowanceAllSerials(tokenId, false, ownerAccountId, spenderAccountId);
        }

		public override void ValidateChecksums(Client client)
		{
			foreach (var allowance in HbarAllowances)
			{
				allowance.ValidateChecksums(client);
			}

			foreach (var allowance in TokenAllowances)
			{
				allowance.ValidateChecksums(client);
			}

			foreach (var allowance in NftAllowances)
			{
				allowance.ValidateChecksums(client);
			}
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            throw new NotSupportedException("Cannot schedule AccountAllowanceAdjustTransaction");
        }

        public override MethodDescriptor GetMethodDescriptor()
		{
			throw new NotSupportedException("Cannot get method descriptor for AccountAllowanceAdjustTransaction");
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