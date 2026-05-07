// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Fee;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <include file="TokenInfo.cs.xml" path='docs/member[@name="T:TokenInfo"]/*' />
    public class TokenInfo
    {
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.TokenId"]/*' />
        public TokenId TokenId { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.Name"]/*' />
        public string Name { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.Symbol"]/*' />
        public string Symbol { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.Decimals"]/*' />
        public uint Decimals { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.TotalSupply"]/*' />
        public ulong TotalSupply { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.TreasuryAccountId"]/*' />
        public AccountId TreasuryAccountId { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.AdminKey"]/*' />
        public Key? AdminKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.KycKey"]/*' />
        public Key? KycKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.FreezeKey"]/*' />
        public Key? FreezeKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.WipeKey"]/*' />
        public Key? WipeKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.SupplyKey"]/*' />
        public Key? SupplyKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.FeeScheduleKey"]/*' />
        public Key? FeeScheduleKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.DefaultFreezeStatus"]/*' />
        public bool DefaultFreezeStatus { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.DefaultKycStatus"]/*' />
        public bool DefaultKycStatus { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.IsDeleted"]/*' />
        public bool IsDeleted { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.AutoRenewAccount"]/*' />
        public AccountId AutoRenewAccount { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.AutoRenewPeriod"]/*' />
        public TimeSpan AutoRenewPeriod { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.ExpirationTime"]/*' />
        public DateTimeOffset ExpirationTime { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.TokenMemo"]/*' />
        public string TokenMemo { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.CustomFees"]/*' />
        public IList<CustomFee> CustomFees { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.TokenType"]/*' />
        public TokenType TokenType { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.SupplyType"]/*' />
        public TokenSupplyType SupplyType { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.MaxSupply"]/*' />
        public long MaxSupply { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.PauseKey"]/*' />
        public Key? PauseKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.PauseStatus"]/*' />
        public bool PauseStatus;
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.Metadata"]/*' />
        public byte[] Metadata { get; } = []; 
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.MetadataKey"]/*' />
        public Key? MetadataKey { get; }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="F:TokenInfo.LedgerId"]/*' />
        public LedgerId LedgerId { get; }

        internal TokenInfo(TokenId tokenId, string name, string symbol, uint decimals, ulong totalSupply, AccountId treasuryAccountId, Key? adminKey, Key? kycKey, Key? freezeKey, Key? wipeKey, Key? supplyKey, Key? feeScheduleKey, bool defaultFreezeStatus, bool defaultKycStatus, bool isDeleted, AccountId autoRenewAccount, TimeSpan autoRenewPeriod, DateTimeOffset expirationTime, string tokenMemo, IEnumerable<CustomFee> customFees, TokenType tokenType, TokenSupplyType supplyType, long maxSupply, Key? pauseKey, bool pauseStatus, byte[] metadata, Key? metadataKey, LedgerId ledgerId)
        {
            TokenId = tokenId;
            Name = name;
            Symbol = symbol;
            Decimals = decimals;
            TotalSupply = totalSupply;
            TreasuryAccountId = treasuryAccountId;
            AdminKey = adminKey;
            KycKey = kycKey;
            FreezeKey = freezeKey;
            WipeKey = wipeKey;
            SupplyKey = supplyKey;
            FeeScheduleKey = feeScheduleKey;
            DefaultFreezeStatus = defaultFreezeStatus;
            DefaultKycStatus = defaultKycStatus;
            IsDeleted = isDeleted;
            AutoRenewAccount = autoRenewAccount;
            AutoRenewPeriod = autoRenewPeriod;
            ExpirationTime = expirationTime;
            TokenMemo = tokenMemo;
            CustomFees = [..customFees];
            TokenType = tokenType;
            SupplyType = supplyType;
            MaxSupply = maxSupply;
            PauseKey = pauseKey;
            PauseStatus = pauseStatus;
            Metadata = metadata;
            MetadataKey = metadataKey;
            LedgerId = ledgerId;
        }

		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.KycStatusFromProtobuf(Proto.Services.TokenKycStatus)"]/*' />
		public static bool KycStatusFromProtobuf(Proto.Services.TokenKycStatus kycStatus)
        {
            return kycStatus == Proto.Services.TokenKycStatus.Granted;
        }
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.PauseStatusFromProtobuf(Proto.Services.TokenPauseStatus)"]/*' />
		public static bool PauseStatusFromProtobuf(Proto.Services.TokenPauseStatus pauseStatus)
		{
			return pauseStatus == Proto.Services.TokenPauseStatus.Paused;
		}
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.FreezeStatusFromProtobuf(Proto.Services.TokenFreezeStatus)"]/*' />
		public static bool FreezeStatusFromProtobuf(Proto.Services.TokenFreezeStatus freezeStatus)
		{
			return freezeStatus == Proto.Services.TokenFreezeStatus.Frozen;
		}

		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.FromBytes(System.Byte[])"]/*' />
		public static TokenInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.TokenGetInfoResponse.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.FromProtobuf(Proto.Services.TokenGetInfoResponse)"]/*' />
		public static TokenInfo FromProtobuf(Proto.Services.TokenGetInfoResponse response)
        {
			return new TokenInfo(
                TokenId.FromProtobuf(response.TokenInfo.TokenId),
				response.TokenInfo.Name,
				response.TokenInfo.Symbol,
				response.TokenInfo.Decimals,
				response.TokenInfo.TotalSupply, 
                AccountId.FromProtobuf(response.TokenInfo.Treasury),
				Key.FromProtobufKey(response.TokenInfo.AdminKey),
				Key.FromProtobufKey(response.TokenInfo.KycKey),
				Key.FromProtobufKey(response.TokenInfo.FreezeKey),
				Key.FromProtobufKey(response.TokenInfo.WipeKey),
				Key.FromProtobufKey(response.TokenInfo.SupplyKey),
				Key.FromProtobufKey(response.TokenInfo.FeeScheduleKey), 
                FreezeStatusFromProtobuf(response.TokenInfo.DefaultFreezeStatus), 
                KycStatusFromProtobuf(response.TokenInfo.DefaultKycStatus),
				response.TokenInfo.Deleted,
				AccountId.FromProtobuf(response.TokenInfo.AutoRenewAccount),
				response.TokenInfo.AutoRenewPeriod.ToTimeSpan(),
				response.TokenInfo.Expiry.ToDateTimeOffset(),
				response.TokenInfo.Memo, 
                CustomFeesFromProto(response.TokenInfo), 
                (TokenType)response.TokenInfo.TokenType, 
                (TokenSupplyType)response.TokenInfo.SupplyType,
				response.TokenInfo.MaxSupply,
				Key.FromProtobufKey(response.TokenInfo.PauseKey), 
                PauseStatusFromProtobuf(response.TokenInfo.PauseStatus),
				response.TokenInfo.Metadata.ToByteArray(),
				Key.FromProtobufKey(response.TokenInfo.MetadataKey), 
                LedgerId.FromByteString(response.TokenInfo.LedgerId));
		}

        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.CustomFeesFromProto(Proto.Services.TokenInfo)"]/*' />
        private static IList<CustomFee> CustomFeesFromProto(Proto.Services.TokenInfo info)
        {
            return [.. info.CustomFees.Select(_ => CustomFee.FromProtobuf(_))];
        }

		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.ToProtobuf"]/*' />
		public virtual Proto.Services.TokenGetInfoResponse ToProtobuf()
		{
            Proto.Services.TokenInfo proto = new()
            {
				AutoRenewAccount = AutoRenewAccount.ToProtobuf(),
				AutoRenewPeriod = AutoRenewPeriod.ToProtoDuration(),
				Decimals = Decimals,
				DefaultFreezeStatus = FreezeStatusToProtobuf(DefaultFreezeStatus),
				DefaultKycStatus = KycStatusToProtobuf(DefaultKycStatus),
				Deleted = IsDeleted,
				Expiry = ExpirationTime.ToProtoTimestamp(),
				LedgerId = LedgerId.ToByteString(),
				PauseStatus = PauseStatusToProtobuf(PauseStatus),
				MaxSupply = MaxSupply,
				Memo = TokenMemo,
				Metadata = ByteString.CopyFrom(Metadata),
				Name = Name,
                TokenId = TokenId.ToProtobuf(),
				TotalSupply = TotalSupply,
				TokenType = (Proto.Services.TokenType)TokenType,
				Treasury = TreasuryAccountId.ToProtobuf(),
				SupplyType = (Proto.Services.TokenSupplyType)SupplyType,
				Symbol = Symbol,
			};

            if (AdminKey is not null) 
                proto.AdminKey = AdminKey.ToProtobufKey();
            if (FeeScheduleKey is not null) 
                proto.FeeScheduleKey = FeeScheduleKey.ToProtobufKey();
            if (FreezeKey is not null) 
                proto.FreezeKey = FreezeKey.ToProtobufKey();
            if (KycKey is not null) 
                proto.KycKey = KycKey.ToProtobufKey();
            if (PauseKey is not null) 
                proto.PauseKey = PauseKey.ToProtobufKey();
            if (MetadataKey is not null) 
                proto.MetadataKey = MetadataKey.ToProtobufKey();
            if (SupplyKey is not null) 
                proto.SupplyKey = SupplyKey.ToProtobufKey();
            if (WipeKey is not null)
                proto.WipeKey = WipeKey.ToProtobufKey();

			proto.CustomFees.AddRange(CustomFees.Select(_ => _.ToProtobuf()));

			return new Proto.Services.TokenGetInfoResponse
            {
                TokenInfo = proto
            };
		}
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.KycStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.Services.TokenKycStatus KycStatusToProtobuf(bool kycStatus)
        {
            return kycStatus ? Proto.Services.TokenKycStatus.Granted : Proto.Services.TokenKycStatus.Revoked;
        }
        /// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.PauseStatusToProtobuf(System.Boolean)"]/*' />
        public static Proto.Services.TokenPauseStatus PauseStatusToProtobuf(bool pauseStatus)
        {
            return pauseStatus ? Proto.Services.TokenPauseStatus.Paused : Proto.Services.TokenPauseStatus.Unpaused;
        }
		/// <include file="TokenInfo.cs.xml" path='docs/member[@name="M:TokenInfo.FreezeStatusToProtobuf(System.Boolean)"]/*' />
		public static Proto.Services.TokenFreezeStatus FreezeStatusToProtobuf(bool freezeStatus)
		{
			return freezeStatus ? Proto.Services.TokenFreezeStatus.Frozen : Proto.Services.TokenFreezeStatus.Unfrozen;
		}
    }
}
