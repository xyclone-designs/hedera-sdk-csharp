// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Cryptocurrency
{
    /// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="T:AccountAllowanceDeleteTransaction"]/*' />
    public class AccountAllowanceDeleteTransaction : Transaction<AccountAllowanceDeleteTransaction>
    {
		// <ownerId, <tokenId, index>>
		private readonly Dictionary<AccountId, Dictionary<TokenId, int>> nftMap = [];
		private readonly List<HbarAllowance> HbarAllowances = [];
        private readonly List<TokenAllowance> TokenAllowances = [];
        private readonly List<TokenNftAllowance> NftAllowances = [];

        /// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.#ctor"]/*' />
        public AccountAllowanceDeleteTransaction() { }
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.#ctor(Proto.Services.TransactionBody)"]/*' />
		internal AccountAllowanceDeleteTransaction(Proto.Services.TransactionBody txBody) : base(txBody)
		{
			InitFromTransactionBody();
		}
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.#ctor(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]/*' />
		internal AccountAllowanceDeleteTransaction(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs)
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

        /// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.GetHbarAllowanceDeletions"]/*' />
        public virtual List<HbarAllowance> GetHbarAllowanceDeletions()
        {
            return [.. HbarAllowances];
        }
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.GetTokenAllowanceDeletions"]/*' />
		public virtual List<TokenAllowance> GetTokenAllowanceDeletions()
		{
			return [.. TokenAllowances];
		}
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.GetTokenNftAllowanceDeletions"]/*' />
		public virtual List<TokenNftAllowance> GetTokenNftAllowanceDeletions()
		{
			List<TokenNftAllowance> retval = new(NftAllowances.Count);

			foreach (var allowance in NftAllowances)
			{
				retval.Add(TokenNftAllowance.CopyFrom(allowance));
			}

			return retval;
		}

		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.GetNftSerials(AccountId,TokenId)"]/*' />
		private List<long> GetNftSerials(AccountId ownerAccountId, TokenId tokenId)
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
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.NewNftSerials(AccountId,TokenId,System.Collections.Generic.Dictionary{TokenId,System.Int32})"]/*' />
		private List<long> NewNftSerials(AccountId ownerAccountId, TokenId tokenId, Dictionary<TokenId, int> innerMap)
		{
			innerMap.Add(tokenId, NftAllowances.Count);
			TokenNftAllowance newAllowance = new(tokenId, ownerAccountId, null, null, [], default);
			NftAllowances.Add(newAllowance);
			return newAllowance.SerialNumbers;
		}

		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.DeleteAllHbarAllowances(AccountId)"]/*' />
		public virtual AccountAllowanceDeleteTransaction DeleteAllHbarAllowances(AccountId ownerAccountId)
		{
			RequireNotFrozen();
			HbarAllowances.Add(new HbarAllowance(ownerAccountId, null, null));
			return this;
		}
		/// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.DeleteAllTokenAllowances(TokenId,AccountId)"]/*' />
		public virtual AccountAllowanceDeleteTransaction DeleteAllTokenAllowances(TokenId tokenId, AccountId ownerAccountId)
        {
            RequireNotFrozen();
            TokenAllowances.Add(new TokenAllowance(tokenId, ownerAccountId, null, 0));
            return this;
        }
        /// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.DeleteAllTokenNftAllowances(NftId,AccountId)"]/*' />
        public virtual AccountAllowanceDeleteTransaction DeleteAllTokenNftAllowances(NftId nftId, AccountId ownerAccountId)
        {
            RequireNotFrozen();
            ArgumentNullException.ThrowIfNull(nftId);
            GetNftSerials(ownerAccountId, nftId.TokenId).Add(nftId.Serial);
            return this;
        }

        /// <include file="AccountAllowanceDeleteTransaction.cs.xml" path='docs/member[@name="M:AccountAllowanceDeleteTransaction.ToProtobuf"]/*' />
        public virtual Proto.Services.CryptoDeleteAllowanceTransactionBody ToProtobuf()
        {
            var builder = new Proto.Services.CryptoDeleteAllowanceTransactionBody();
            foreach (var allowance in NftAllowances)
            {
                builder.NftAllowances.Add(allowance.ToRemoveProtobuf());
            }

            return builder;
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.CryptoService.CryptoServiceClient.deleteAllowances);

			return Proto.Services.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override void ValidateChecksums(Client client)
		{
			foreach (var allowance in NftAllowances)
			{
				allowance.ValidateChecksums(client);
			}
		}
		public override void OnFreeze(Proto.Services.TransactionBody bodyBuilder)
        {
            bodyBuilder.CryptoDeleteAllowance = ToProtobuf();
        }
        public override void OnScheduled(Proto.Services.SchedulableTransactionBody scheduled)
        {
            scheduled.CryptoDeleteAllowance = ToProtobuf();
        }

		public override ResponseStatus MapResponseStatus(Proto.Services.Response response)
        {
            throw new NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Services.TransactionResponse response, AccountId nodeId, Proto.Services.Transaction request)
        {
            throw new NotImplementedException();
        }
    }
}
