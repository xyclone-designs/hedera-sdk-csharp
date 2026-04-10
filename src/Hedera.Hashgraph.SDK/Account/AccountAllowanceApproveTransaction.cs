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
    /// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="T:AccountAllowanceApproveTransaction"]/*' />
    public class AccountAllowanceApproveTransaction : Transaction<AccountAllowanceApproveTransaction>
    {
		// key is "{ownerId}:{spenderId}".  OwnerId may be "FEE_PAYER"
		// <ownerId:spenderId, <tokenId, index>>
		private readonly Dictionary<string, Dictionary<TokenId, int>> NftMap = [];
		private readonly List<HbarAllowance> HbarAllowances = [];
        private readonly List<TokenAllowance> TokenAllowances = [];
        private readonly List<TokenNftAllowance> NftAllowances = [];

		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.#ctor"]/*' />
		public AccountAllowanceApproveTransaction() { }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.#ctor(Proto.TransactionBody)"]/*' />
		internal AccountAllowanceApproveTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal AccountAllowanceApproveTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.OwnerToString(AccountId)"]/*' />
		private static string OwnerToString(AccountId? ownerAccountId)
		{
			return ownerAccountId?.ToString() ?? "FEE_PAYER";
		}

		private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoApproveAllowance;

            foreach (var allowanceProto in body.CryptoAllowances)
            {
                HbarAllowances.Add(HbarAllowance.FromProtobuf(allowanceProto));
            }

            foreach (var allowanceProto in body.TokenAllowances)
            {
                TokenAllowances.Add(TokenAllowance.FromProtobuf(allowanceProto));
            }

            foreach (var allowanceProto in body.NftAllowances)
            {
                if (allowanceProto.ApprovedForAll ?? false)
                {
                    NftAllowances.Add(TokenNftAllowance.FromProtobuf(allowanceProto));
                }
                else
                {
                    GetNftSerials(
                        AccountId.FromProtobuf(allowanceProto.Owner), 
                        AccountId.FromProtobuf(allowanceProto.Spender),
                        AccountId.FromProtobuf(allowanceProto.DelegatingSpender) , 
                        TokenId.FromProtobuf(allowanceProto.TokenId)).Concat(allowanceProto.SerialNumbers);
                }
            }
        }

		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetNftSerials(AccountId,AccountId,AccountId,TokenId)"]/*' />
		private List<long> GetNftSerials(AccountId? ownerAccountId, AccountId spenderAccountId, AccountId? delegatingSpender, TokenId tokenId)
		{
			var key = OwnerToString(ownerAccountId) + ":" + spenderAccountId;

			if (NftMap.TryGetValue(key, out Dictionary<TokenId, int>? nftmap))
			{
                if (nftmap.TryGetValue(tokenId, out int value))
				{
					return NftAllowances[value].SerialNumbers;
				}
				else
				{
					return NewNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, tokenId, nftmap);
				}
			}
			else
			{
				Dictionary<TokenId, int> innerMap = [];
				NftMap.Add(key, innerMap);
				return NewNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, tokenId, innerMap);
			}
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.NewNftSerials(AccountId,AccountId,AccountId,TokenId,System.Collections.Generic.Dictionary{TokenId,System.Int32})"]/*' />
		private List<long> NewNftSerials(AccountId? ownerAccountId, AccountId spenderAccountId, AccountId? delegatingSpender, TokenId tokenId, Dictionary<TokenId, int> innerMap)
		{
			innerMap.Add(tokenId, NftAllowances.Count);
			TokenNftAllowance newAllowance = new (tokenId, ownerAccountId, spenderAccountId, delegatingSpender, [], default);
			NftAllowances.Add(newAllowance);
			return newAllowance.SerialNumbers;
		}

		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetHbarAllowances"]/*' />
		public virtual List<HbarAllowance> GetHbarAllowances()
        {
            return GetHbarApprovals();
        }
        /// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetHbarApprovals"]/*' />
        public virtual List<HbarAllowance> GetHbarApprovals()
        {
            return [.. HbarAllowances];
        }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetTokenApprovals"]/*' />
		public virtual List<TokenAllowance> GetTokenApprovals()
		{
			return [.. TokenAllowances];
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetTokenAllowances"]/*' />
		public virtual List<TokenAllowance> GetTokenAllowances()
        {
            return GetTokenApprovals();
        }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetTokenNftApprovals"]/*' />
		public virtual List<TokenNftAllowance> GetTokenNftApprovals()
		{
			List<TokenNftAllowance> retval = new (NftAllowances.Count);
			foreach (var allowance in NftAllowances)
			{
				retval.Add(TokenNftAllowance.CopyFrom(allowance));
			}

			return retval;
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.GetTokenNftAllowances"]/*' />
		public virtual List<TokenNftAllowance> GetTokenNftAllowances()
		{
			return GetTokenNftApprovals();
		}

		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.AddTokenAllowance(TokenId,AccountId,System.Int64)"]/*' />
		public virtual AccountAllowanceApproveTransaction AddTokenAllowance(TokenId tokenId, AccountId spenderAccountId, long amount)
		{
			return ApproveTokenAllowance(tokenId, null, spenderAccountId, amount);
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.AddTokenNftAllowance(NftId,AccountId)"]/*' />
		public virtual AccountAllowanceApproveTransaction AddTokenNftAllowance(NftId nftId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            GetNftSerials(null, spenderAccountId, null, nftId.TokenId).Add(nftId.Serial);
            return this;
        }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.AddHbarAllowance(AccountId,Hbar)"]/*' />
		public virtual AccountAllowanceApproveTransaction AddHbarAllowance(AccountId spenderAccountId, Hbar amount)
		{
			return ApproveHbarAllowance(null, spenderAccountId, amount);
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.AddAllTokenNftAllowance(TokenId,AccountId)"]/*' />
		public virtual AccountAllowanceApproveTransaction AddAllTokenNftAllowance(TokenId tokenId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            NftAllowances.Add(new TokenNftAllowance(tokenId, null, spenderAccountId, null, [], true));
            return this;
        }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ApproveHbarAllowance(AccountId,AccountId,Hbar)"]/*' />
		public virtual AccountAllowanceApproveTransaction ApproveHbarAllowance(AccountId? ownerAccountId, AccountId spenderAccountId, Hbar amount)
		{
			RequireNotFrozen();
			ArgumentNullException.ThrowIfNull(amount);
			HbarAllowances.Add(new HbarAllowance(ownerAccountId, spenderAccountId, amount));
			return this;
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ApproveTokenAllowance(TokenId,AccountId,AccountId,System.Int64)"]/*' />
		public virtual AccountAllowanceApproveTransaction ApproveTokenAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId spenderAccountId, long amount)
		{
			RequireNotFrozen();
			TokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount));
			return this;
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ApproveTokenNftAllowance(NftId,AccountId,AccountId)"]/*' />
		public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftId);
            GetNftSerials(ownerAccountId, spenderAccountId, null, nftId.TokenId).Add(nftId.Serial);
            return this;
        }
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ApproveTokenNftAllowance(NftId,AccountId,AccountId,AccountId)"]/*' />
		public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowance(NftId nftId, AccountId ownerAccountId, AccountId spenderAccountId, AccountId delegatingSpender)
		{
			RequireNotFrozen();
			ArgumentNullException.ThrowIfNull(nftId);
			GetNftSerials(ownerAccountId, spenderAccountId, delegatingSpender, nftId.TokenId).Add(nftId.Serial);
			return this;
		}
		/// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ApproveTokenNftAllowanceAllSerials(TokenId,AccountId,AccountId)"]/*' />
		public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            NftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], true));
            return this;
        }
        /// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.DeleteTokenNftAllowanceAllSerials(TokenId,AccountId,AccountId)"]/*' />
        public virtual AccountAllowanceApproveTransaction DeleteTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            NftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], false));
            return this;
        }

        /// <include file="AccountAllowanceApproveTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceApproveTransaction.ToProtobuf"]/*' />
        public virtual Proto.CryptoApproveAllowanceTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoApproveAllowanceTransactionBody();

            foreach (var allowance in HbarAllowances)
            {
                builder.CryptoAllowances.Add(allowance.ToProtobuf());
            }

            foreach (var allowance in TokenAllowances)
            {
                builder.TokenAllowances.Add(allowance.ToProtobuf());
            }

            foreach (var allowance in NftAllowances)
            {
                builder.NftAllowances.Add(allowance.ToProtobuf());
            }

            return builder;
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
            bodyBuilder.CryptoApproveAllowance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoApproveAllowance = ToProtobuf();
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.approveAllowances);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
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