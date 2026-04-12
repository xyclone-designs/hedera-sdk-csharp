// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Token;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Contract
{
    /// <include file="ContractInfo.cs.xml" path='docs/member[@name="T:ContractInfo"]/*' />
    public sealed class ContractInfo
    {
        /// <include file="ContractInfo.cs.xml" path='docs/member[@name="M:ContractInfo.#ctor(ContractId,AccountId,System.String,Key,DateTimeOffset,System.TimeSpan,AccountId,System.Int64,System.String,Hbar,System.Boolean,System.Collections.Generic.Dictionary{TokenId,TokenRelationship},LedgerId,StakingInfo)"]/*' />
        private ContractInfo(ContractId contractId, AccountId accountId, string contractAccountId, Key? adminKey, DateTimeOffset expirationTime, TimeSpan autoRenewPeriod, AccountId autoRenewAccountId, long storage, string contractMemo, Hbar balance, bool isDeleted, Dictionary<TokenId, TokenRelationship> tokenRelationships, LedgerId ledgerId, StakingInfo stakingInfo)
        {
            this.ContractId = contractId;
            this.AccountId = accountId;
            this.ContractAccountId = contractAccountId;
            this.AdminKey = adminKey;
            this.ExpirationTime = expirationTime;
            this.AutoRenewPeriod = autoRenewPeriod;
            this.AutoRenewAccountId = autoRenewAccountId;
            this.Storage = storage;
            this.ContractMemo = contractMemo;
            this.Balance = balance;
            this.IsDeleted = isDeleted;
            this.TokenRelationships = tokenRelationships;
            this.LedgerId = ledgerId;
            this.StakingInfo = stakingInfo;
        }

		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="M:ContractInfo.FromBytes(System.Byte[])"]/*' />
		public static ContractInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.ContractGetInfoResponse.Types.ContractInfo.Parser.ParseFrom(bytes));
		}
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="M:ContractInfo.FromProtobuf(Proto.Services.ContractGetInfoResponse.Types.ContractInfo)"]/*' />
		public static ContractInfo FromProtobuf(Proto.Services.ContractGetInfoResponse.Types.ContractInfo contractInfo)
        {
            return new ContractInfo(
                ContractId.FromProtobuf(contractInfo.ContractID), 
                AccountId.FromProtobuf(contractInfo.AccountID), 
                contractInfo.ContractAccountID,
				Key.FromProtobufKey(contractInfo.AdminKey), 
                contractInfo.ExpirationTime.ToDateTimeOffset(), 
                contractInfo.AutoRenewPeriod.ToTimeSpan(), 
                AccountId.FromProtobuf(contractInfo.AutoRenewAccountId), 
                contractInfo.Storage, 
                contractInfo.Memo, 
                Hbar.FromTinybars((long)contractInfo.Balance), 
                contractInfo.Deleted,
				contractInfo.TokenRelationships.ToDictionary(
				    _ => TokenId.FromProtobuf(_.TokenId),
				    _ => TokenRelationship.FromProtobuf(_)), 
                LedgerId.FromByteString(contractInfo.LedgerId), 
                StakingInfo.FromProtobuf(contractInfo.StakingInfo));
        }

		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ContractId"]/*' />
		public readonly ContractId ContractId;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AccountId"]/*' />
		public readonly AccountId AccountId;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ContractAccountId"]/*' />
		public readonly string ContractAccountId;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AdminKey"]/*' />
		public readonly Key? AdminKey;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ExpirationTime"]/*' />
		public readonly DateTimeOffset ExpirationTime;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AutoRenewPeriod"]/*' />
		public readonly TimeSpan AutoRenewPeriod;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AutoRenewAccountId"]/*' />
		public readonly AccountId AutoRenewAccountId;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.Storage"]/*' />
		public readonly long Storage;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ContractMemo"]/*' />
		public readonly string ContractMemo;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.Balance"]/*' />
		public readonly Hbar Balance;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.IsDeleted"]/*' />
		public readonly bool IsDeleted;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="T:ContractInfo_2"]/*' />
		public readonly Dictionary<TokenId, TokenRelationship> TokenRelationships;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.LedgerId"]/*' />
		public readonly LedgerId LedgerId;
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.StakingInfo"]/*' />
		public readonly StakingInfo StakingInfo;


		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="M:ContractInfo.ToBytes"]/*' />
		public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="M:ContractInfo.ToProtobuf"]/*' />
		public Proto.Services.ContractGetInfoResponse.Types.ContractInfo ToProtobuf()
		{
			Proto.Services.ContractGetInfoResponse.Types.ContractInfo proto = new()
			{
				ContractID = ContractId.ToProtobuf(),
				AccountID = AccountId.ToProtobuf(),
				ContractAccountID = ContractAccountId,
				ExpirationTime = ExpirationTime.ToProtoTimestamp(),
				AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
				Storage = Storage,
				Memo = ContractMemo,
				Balance = (ulong)Balance.ToTinybars(),
				LedgerId = LedgerId.ToByteString(),
			};

			if (AdminKey != null)
				Proto.Services.AdminKey = AdminKey.ToProtobufKey();

			if (StakingInfo != null)
				Proto.Services.StakingInfo = StakingInfo.ToProtobuf();

			if (AutoRenewAccountId != null)
				Proto.Services.AutoRenewAccountId = AutoRenewAccountId.ToProtobuf();

			return proto;
		}
	}
}
