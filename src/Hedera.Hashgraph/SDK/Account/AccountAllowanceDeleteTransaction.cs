// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
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
		// <ownerId, <tokenId, index>>
		private readonly Dictionary<AccountId, Dictionary<TokenId, int>> nftMap = [];
		private readonly IList<HbarAllowance> HbarAllowances = [];
        private readonly IList<TokenAllowance> TokenAllowances = [];
        private readonly IList<TokenNftAllowance> NftAllowances = [];

        /// <summary>
        /// Constructor.
        /// </summary>
        public AccountAllowanceDeleteTransaction() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal AccountAllowanceDeleteTransaction(Proto.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal AccountAllowanceDeleteTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        private void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.CryptoDeleteAllowance;
            foreach (var allowanceProto in body.NftAllowances)
                if (GetNftSerials(AccountId.FromProtobuf(allowanceProto.Owner), TokenId.FromProtobuf(allowanceProto.TokenId)) is IList<long> nftserials)
                    foreach (long serialnumber in allowanceProto.SerialNumbers)
						nftserials.Add(serialnumber);
		}

        /// <summary>
        /// </summary>
        /// <returns>                         a list of hbar allowance records</returns>
        /// <remarks>@deprecatedwith no replacement</remarks>
        public virtual IList<HbarAllowance> GetHbarAllowanceDeletions()
        {
            return [.. HbarAllowances];
        }
		/// <summary>
		/// </summary>
		/// <returns>                         a list of token allowance records</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual IList<TokenAllowance> GetTokenAllowanceDeletions()
		{
			return [.. TokenAllowances];
		}
		/// <summary>
		/// Return list of nft tokens to be deleted.
		/// </summary>
		/// <returns>                         list of token nft allowances</returns>
		public virtual IList<TokenNftAllowance> GetTokenNftAllowanceDeletions()
		{
			List<TokenNftAllowance> retval = new(NftAllowances.Count);

			foreach (var allowance in NftAllowances)
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

			if (nftMap.TryGetValue(key, out Dictionary<TokenId, int>? nftmap))
			{
				if (nftmap.TryGetValue(tokenId, out int value))
				{
					return NftAllowances[value].SerialNumbers;
				}
				else
				{
					return NewNftSerials(ownerAccountId, tokenId, nftmap);
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
			innerMap.Add(tokenId, NftAllowances.Count);
			TokenNftAllowance newAllowance = new(tokenId, ownerAccountId, null, null, [], default);
			NftAllowances.Add(newAllowance);
			return newAllowance.SerialNumbers;
		}

		/// <summary>
		/// </summary>
		/// <param name="ownerAccountId">the owner's account id</param>
		/// <returns>{@code this}</returns>
		/// <remarks>@deprecatedwith no replacement</remarks>
		public virtual AccountAllowanceDeleteTransaction DeleteAllHbarAllowances(AccountId ownerAccountId)
		{
			RequireNotFrozen();
			HbarAllowances.Add(new HbarAllowance(ownerAccountId, null, null));
			return this;
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
            TokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, null, 0));
            return this;
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
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link CryptoDeleteAllowanceTransactionBody}</returns>
        public virtual Proto.CryptoDeleteAllowanceTransactionBody ToProtobuf()
        {
            var builder = new Proto.CryptoDeleteAllowanceTransactionBody();
            foreach (var allowance in NftAllowances)
            {
                builder.NftAllowances.Add(allowance.ToRemoveProtobuf());
            }

            return builder;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.deleteAllowances);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{
			foreach (var allowance in NftAllowances)
			{
				allowance.ValidateChecksums(client);
			}
		}
		public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDeleteAllowance = ToProtobuf();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDeleteAllowance = ToProtobuf();
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