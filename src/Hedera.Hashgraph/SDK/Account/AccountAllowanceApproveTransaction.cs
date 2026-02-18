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
		// key is "{ownerId}:{spenderId}".  OwnerId may be "FEE_PAYER"
		// <ownerId:spenderId, <tokenId, index>>
		private readonly Dictionary<string, Dictionary<TokenId, int>> NftMap = [];
		private readonly List<HbarAllowance> HbarAllowances = [];
        private readonly List<TokenAllowance> TokenAllowances = [];
        private readonly List<TokenNftAllowance> NftAllowances = [];

		/// <summary>
		/// Constructor.
		/// </summary>
		public AccountAllowanceApproveTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal AccountAllowanceApproveTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		internal AccountAllowanceApproveTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

		/// <summary>
		/// Extract the owner as a string.
		/// </summary>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <returns>                         a string representation of the account id
		///                                  or FEE_PAYER</returns>
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

		/// <summary>
		/// Return a list of NFT serial numbers.
		/// </summary>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <param name="spenderAccountId">spender's account id</param>
		/// <param name="delegatingSpender">delegating spender's account id</param>
		/// <param name="tokenId">the token's id</param>
		/// <returns>list of NFT serial numbers</returns>
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
		/// <summary>
		/// Add NFT serials.
		/// </summary>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <param name="spenderAccountId">spender's account id</param>
		/// <param name="delegatingSpender">delegating spender's account id</param>
		/// <param name="tokenId">the token's id</param>
		/// <param name="innerMap">list of token id's and serial number records</param>
		/// <returns>list of NFT serial numbers</returns>
		private List<long> NewNftSerials(AccountId? ownerAccountId, AccountId spenderAccountId, AccountId? delegatingSpender, TokenId tokenId, Dictionary<TokenId, int> innerMap)
		{
			innerMap.Add(tokenId, NftAllowances.Count);
			TokenNftAllowance newAllowance = new (tokenId, ownerAccountId, spenderAccountId, delegatingSpender, [], default);
			NftAllowances.Add(newAllowance);
			return newAllowance.SerialNumbers;
		}

		/// <summary>
		/// </summary>
		/// <returns>                         list of hbar allowance records</returns>
		/// <remarks>@deprecated- Use {@link #getHbarApprovals()} instead</remarks>
		public virtual List<HbarAllowance> GetHbarAllowances()
        {
            return GetHbarApprovals();
        }
        /// <summary>
        /// Extract the list of hbar allowances.
        /// </summary>
        /// <returns>                         array list of hbar allowances</returns>
        public virtual List<HbarAllowance> GetHbarApprovals()
        {
            return [.. HbarAllowances];
        }
		/// <summary>
		/// Extract a list of token allowance approvals.
		/// </summary>
		/// <returns>                         array list of token approvals.</returns>
		public virtual List<TokenAllowance> GetTokenApprovals()
		{
			return [.. TokenAllowances];
		}
		/// <summary>
		/// </summary>
		/// <returns> a list of token allowances</returns>
		/// <remarks>@deprecated- Use {@link #getTokenApprovals()} instead</remarks>
		public virtual List<TokenAllowance> GetTokenAllowances()
        {
            return GetTokenApprovals();
        }
		/// <summary>
		/// Returns the list of token nft allowances.
		/// </summary>
		/// <returns> list of token nft allowances.</returns>
		public virtual List<TokenNftAllowance> GetTokenNftApprovals()
		{
			List<TokenNftAllowance> retval = new (NftAllowances.Count);
			foreach (var allowance in NftAllowances)
			{
				retval.Add(TokenNftAllowance.CopyFrom(allowance));
			}

			return retval;
		}
		/// <summary>
		/// </summary>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecated- Use {@link #getTokenNftApprovals()} instead</remarks>
		public virtual List<TokenNftAllowance> GetTokenNftAllowances()
		{
			return GetTokenNftApprovals();
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
		/// <param name="spenderAccountId">the spender account id</param>
		/// <param name="amount">the amount of hbar</param>
		/// <returns>                         an account allowance approve transaction</returns>
		/// <remarks>@deprecated- Use {@link #approveHbarAllowance(AccountId, AccountId, Hbar)} instead</remarks>
		public virtual AccountAllowanceApproveTransaction AddHbarAllowance(AccountId spenderAccountId, Hbar amount)
		{
			return ApproveHbarAllowance(null, spenderAccountId, amount);
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
            NftAllowances.Add(new TokenNftAllowance(tokenId, null, spenderAccountId, null, [], true));
            return this;
        }
		/// <summary>
		/// Approves the Hbar allowance.
		/// </summary>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <param name="spenderAccountId">spender's account id</param>
		/// <param name="amount">amount of hbar add</param>
		/// <returns>{@code this}</returns>
		public virtual AccountAllowanceApproveTransaction ApproveHbarAllowance(AccountId? ownerAccountId, AccountId spenderAccountId, Hbar amount)
		{
			RequireNotFrozen();
			ArgumentNullException.ThrowIfNull(amount);
			HbarAllowances.Add(new HbarAllowance(ownerAccountId, spenderAccountId, amount));
			return this;
		}
		/// <summary>
		/// Approves the Token allowance.
		/// </summary>
		/// <param name="tokenId">the token's id</param>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <param name="spenderAccountId">spender's account id</param>
		/// <param name="amount">amount of tokens</param>
		/// <returns>{@code this}</returns>
		public virtual AccountAllowanceApproveTransaction ApproveTokenAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId spenderAccountId, long amount)
		{
			RequireNotFrozen();
			TokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, spenderAccountId, amount));
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
		/// Approve the token nft allowance on all serials.
		/// </summary>
		/// <param name="tokenId">the token's id</param>
		/// <param name="ownerAccountId">owner's account id</param>
		/// <param name="spenderAccountId">spender's account id</param>
		/// <returns>{@code this}</returns>
		public virtual AccountAllowanceApproveTransaction ApproveTokenNftAllowanceAllSerials(TokenId tokenId, AccountId ownerAccountId, AccountId spenderAccountId)
        {
            RequireNotFrozen();
            NftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], true));
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
            NftAllowances.Add(new TokenNftAllowance(tokenId, ownerAccountId, spenderAccountId, null, [], false));
            return this;
        }

        /// <summary>
        /// Build the correct transaction body.
        /// </summary>
        /// <returns>{@link Proto.CryptoApproveAllowanceTransactionBody builder }</returns>
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
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}