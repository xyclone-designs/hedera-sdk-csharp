// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.LiveHashes;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Nfts;
using Hedera.Hashgraph.SDK.Token;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <include file="AccountInfo.cs.xml" path='docs/member[@name="T:AccountInfo"]/*' />
    public sealed class AccountInfo
    {
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.AccountId"]/*' />
        public readonly AccountId AccountId;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.ContractAccountId"]/*' />
        public readonly string ContractAccountId;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.IsDeleted"]/*' />
        public readonly bool IsDeleted;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.ProxyAccountId"]/*' />
        public readonly AccountId? ProxyAccountId;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.ProxyReceived"]/*' />
        public readonly Hbar ProxyReceived;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.Key"]/*' />
        public readonly Key Key;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.Balance"]/*' />
        public readonly Hbar Balance;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.SendRecordThreshold"]/*' />
        public readonly Hbar SendRecordThreshold;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.ReceiveRecordThreshold"]/*' />
        public readonly Hbar ReceiveRecordThreshold;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.IsReceiverSigRequired"]/*' />
        public readonly bool IsReceiverSigRequired;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.ExpirationTime"]/*' />
        public readonly DateTimeOffset ExpirationTime;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.AutoRenewPeriod"]/*' />
        public readonly TimeSpan AutoRenewPeriod;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.LiveHashes"]/*' />
        public readonly List<LiveHash> LiveHashes;
        public readonly Dictionary<TokenId, TokenRelationship> TokenRelationships;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.AccountMemo"]/*' />
        public readonly string AccountMemo;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.OwnedNfts"]/*' />
        public readonly long OwnedNfts;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.MaxAutomaticTokenAssociations"]/*' />
        public readonly int MaxAutomaticTokenAssociations;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.AliasKey"]/*' />
        public readonly PublicKey? AliasKey;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.LedgerId"]/*' />
        public readonly LedgerId LedgerId;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.EthereumNonce"]/*' />
        public readonly long EthereumNonce;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.HbarAllowances"]/*' />
        public readonly List<HbarAllowance> HbarAllowances;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.TokenAllowances"]/*' />
        public readonly List<TokenAllowance> TokenAllowances;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.TokenNftAllowances"]/*' />
        public readonly List<TokenNftAllowance> TokenNftAllowances;
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="F:AccountInfo.StakingInfo"]/*' />
        public readonly StakingInfo? StakingInfo;
        
        /// <include file="AccountInfo.cs.xml" path='docs/member[@name="M:AccountInfo.#ctor(AccountId,System.String,System.Boolean,AccountId,System.Int64,Key,System.Int64,System.Int64,System.Int64,System.Boolean,DateTimeOffset,System.TimeSpan,System.Collections.Generic.IEnumerable{LiveHash},System.Collections.Generic.Dictionary{TokenId,TokenRelationship},System.String,System.Int64,System.Int32,PublicKey,LedgerId,System.Int64,StakingInfo)"]/*' />
        private AccountInfo(AccountId accountId, string contractAccountId, bool isDeleted, AccountId? proxyAccountId, long proxyReceived, Key key, long balance, long sendRecordThreshold, long receiveRecordThreshold, bool receiverSignatureRequired, DateTimeOffset expirationTime, TimeSpan autoRenewPeriod, IEnumerable<LiveHash> liveHashes, Dictionary<TokenId, TokenRelationship> tokenRelationships, string accountMemo, long ownedNfts, int maxAutomaticTokenAssociations, PublicKey? aliasKey, LedgerId ledgerId, long ethereumNonce, StakingInfo? stakingInfo)
        {
            AccountId = accountId;
            ContractAccountId = contractAccountId;
            IsDeleted = isDeleted;
            ProxyAccountId = proxyAccountId;
            ProxyReceived = Hbar.FromTinybars(proxyReceived);
            Key = key;
            Balance = Hbar.FromTinybars(balance);
            SendRecordThreshold = Hbar.FromTinybars(sendRecordThreshold);
            ReceiveRecordThreshold = Hbar.FromTinybars(receiveRecordThreshold);
            IsReceiverSigRequired = receiverSignatureRequired;
            ExpirationTime = expirationTime;
            AutoRenewPeriod = autoRenewPeriod;
            LiveHashes = [.. liveHashes];
            TokenRelationships = new Dictionary<TokenId, TokenRelationship>(tokenRelationships);
            AccountMemo = accountMemo;
            OwnedNfts = ownedNfts;
            MaxAutomaticTokenAssociations = maxAutomaticTokenAssociations;
            AliasKey = aliasKey;
            LedgerId = ledgerId;
            EthereumNonce = ethereumNonce;
            HbarAllowances = [];
            TokenAllowances = [];
            TokenNftAllowances = [];
            StakingInfo = stakingInfo;
        }
	
		/// <include file="AccountInfo.cs.xml" path='docs/member[@name="M:AccountInfo.FromBytes(System.Byte[])"]/*' />
		public static AccountInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.CryptoGetInfoResponse.Types.AccountInfo.Parser.ParseFrom(bytes));
		}
		/// <include file="AccountInfo.cs.xml" path='docs/member[@name="M:AccountInfo.FromProtobuf(Proto.Services.CryptoGetInfoResponse.Types.AccountInfo)"]/*' />
		public static AccountInfo FromProtobuf(Proto.Services.CryptoGetInfoResponse.Types.AccountInfo accountInfo)
        {
            return new AccountInfo(
				AccountId.FromProtobuf(accountInfo.AccountId), 
                accountInfo.ContractAccountId, 
                accountInfo.Deleted,
				accountInfo.ProxyAccountId.AccountNum > 0 ? AccountId.FromProtobuf(accountInfo.ProxyAccountId) : null, 
                accountInfo.ProxyReceived,
                Key.FromProtobufKey(accountInfo.Key), 
                (long)accountInfo.Balance,
                (long)accountInfo.GenerateSendRecordThreshold, 
                (long)accountInfo.GenerateReceiveRecordThreshold, 
                accountInfo.ReceiverSigRequired, 
                accountInfo.ExpirationTime.ToDateTimeOffset(),
                accountInfo.AutoRenewPeriod.ToTimeSpan(),
				[.. accountInfo.LiveHashes.Select(_ => LiveHash.FromProtobuf(_))],
				accountInfo.TokenRelationships.ToDictionary(_ => TokenId.FromProtobuf(_.TokenId), _ => TokenRelationship.FromProtobuf(_)), 
                accountInfo.Memo,
                accountInfo.OwnedNfts, 
                accountInfo.MaxAutomaticTokenAssociations,
				PublicKey.FromAliasBytes(accountInfo.Alias), 
                LedgerId.FromByteString(accountInfo.LedgerId), 
                accountInfo.EthereumNonce, 
                StakingInfo.FromProtobuf(accountInfo.StakingInfo));
        }

		/// <include file="AccountInfo.cs.xml" path='docs/member[@name="M:AccountInfo.ToBytes"]/*' />
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="AccountInfo.cs.xml" path='docs/member[@name="M:AccountInfo.ToProtobuf"]/*' />
		public Proto.Services.CryptoGetInfoResponse.Types.AccountInfo ToProtobuf()
        {
			Proto.Services.CryptoGetInfoResponse.Types.AccountInfo proto = new ()
            {
				AccountId = AccountId.ToProtobuf(),
				Deleted = IsDeleted,
				ProxyReceived = ProxyReceived.ToTinybars(),
				Key = Key.ToProtobufKey(),
				Balance = (ulong)Balance.ToTinybars(),
				GenerateSendRecordThreshold = (ulong)SendRecordThreshold.ToTinybars(),
				GenerateReceiveRecordThreshold = (ulong)ReceiveRecordThreshold.ToTinybars(),
				ReceiverSigRequired = IsReceiverSigRequired,
				ExpirationTime = ExpirationTime.ToProtoTimestamp(),
				AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
				Memo = AccountMemo,
				OwnedNfts = OwnedNfts,
				MaxAutomaticTokenAssociations = MaxAutomaticTokenAssociations,
				LedgerId = LedgerId.ToByteString(),
				EthereumNonce = EthereumNonce,
			};

            proto.LiveHashes.AddRange(LiveHashes.Select(_ => _.ToProtobuf()));
            
            if (ContractAccountId != null)
				proto.ContractAccountId = ContractAccountId;

			if (ProxyAccountId != null)
				proto.ProxyAccountId = ProxyAccountId.ToProtobuf();

            if (AliasKey != null)
				proto.Alias = AliasKey.ToProtobufKey().ToByteString();

            if (StakingInfo != null)
				proto.StakingInfo = StakingInfo.ToProtobuf();

            return proto;
        }
    }
}
