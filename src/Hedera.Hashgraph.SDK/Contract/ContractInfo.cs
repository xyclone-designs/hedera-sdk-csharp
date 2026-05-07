// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
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
            ContractId = contractId;
            AccountId = accountId;
            ContractAccountId = contractAccountId;
            AdminKey = adminKey;
            ExpirationTime = expirationTime;
            AutoRenewPeriod = autoRenewPeriod;
            AutoRenewAccountId = autoRenewAccountId;
            Storage = storage;
            ContractMemo = contractMemo;
            Balance = balance;
            IsDeleted = isDeleted;
            TokenRelationships = tokenRelationships;
            LedgerId = ledgerId;
            StakingInfo = stakingInfo;
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
                ContractId.FromProtobuf(contractInfo.ContractId), 
                AccountId.FromProtobuf(contractInfo.AccountId), 
                contractInfo.ContractAccountId,
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
		public ContractId ContractId { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AccountId"]/*' />
		public AccountId AccountId { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ContractAccountId"]/*' />
		public string ContractAccountId { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AdminKey"]/*' />
		public Key? AdminKey { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ExpirationTime"]/*' />
		public DateTimeOffset ExpirationTime { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AutoRenewPeriod"]/*' />
		public TimeSpan AutoRenewPeriod { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.AutoRenewAccountId"]/*' />
		public AccountId AutoRenewAccountId { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.Storage"]/*' />
		public long Storage { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.ContractMemo"]/*' />
		public string ContractMemo { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.Balance"]/*' />
		public Hbar Balance { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.IsDeleted"]/*' />
		public bool IsDeleted { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="T:ContractInfo_2"]/*' />
		public Dictionary<TokenId, TokenRelationship> TokenRelationships { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.LedgerId"]/*' />
		public LedgerId LedgerId { get; }
		/// <include file="ContractInfo.cs.xml" path='docs/member[@name="F:ContractInfo.StakingInfo"]/*' />
		public StakingInfo StakingInfo { get; }

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
				ContractId = ContractId.ToProtobuf(),
				AccountId = AccountId.ToProtobuf(),
				ContractAccountId = ContractAccountId,
				ExpirationTime = ExpirationTime.ToProtoTimestamp(),
				AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
				Storage = Storage,
				Memo = ContractMemo,
				Balance = (ulong)Balance.ToTinybars(),
				LedgerId = LedgerId.ToByteString(),
			};

			if (AdminKey != null)
				proto.AdminKey = AdminKey.ToProtobufKey();

			if (StakingInfo != null)
				proto.StakingInfo = StakingInfo.ToProtobuf();

			if (AutoRenewAccountId != null)
				proto.AutoRenewAccountId = AutoRenewAccountId.ToProtobuf();

			return proto;
		}
	}
}
