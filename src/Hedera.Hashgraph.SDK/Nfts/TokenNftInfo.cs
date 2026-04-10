// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Networking;

using System;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="T:TokenNftInfo"]/*' />
	public class TokenNftInfo
    {
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.NftId"]/*' />
        public NftId NftId;
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.AccountId"]/*' />
        public AccountId AccountId;
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.CreationTime"]/*' />
        public DateTimeOffset CreationTime;
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.Metadata"]/*' />
        public byte[] Metadata;
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.LedgerId"]/*' />
        public LedgerId LedgerId;
        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="F:TokenNftInfo.SpenderId"]/*' />
        public AccountId SpenderId;

        /// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="M:TokenNftInfo.#ctor(NftId,AccountId,DateTimeOffset,System.Byte[],LedgerId,AccountId)"]/*' />
        internal TokenNftInfo(NftId nftId, AccountId accountId, DateTimeOffset creationTime, byte[] metadata, LedgerId ledgerId, AccountId spenderId)
        {
            NftId = nftId;
            AccountId = accountId;
            CreationTime = creationTime;
            Metadata = metadata;
            LedgerId = ledgerId;
            SpenderId = spenderId;
        }

		/// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="M:TokenNftInfo.FromBytes(System.Byte[])"]/*' />
		public static TokenNftInfo FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.TokenNftInfo.Parser.ParseFrom(bytes));
		}
		/// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="M:TokenNftInfo.FromProtobuf(Proto.TokenNftInfo)"]/*' />
		public static TokenNftInfo FromProtobuf(Proto.TokenNftInfo info)
        {
            return new TokenNftInfo(
                NftId.FromProtobuf(info.NftID), 
                AccountId.FromProtobuf(info.AccountID), 
                info.CreationTime.ToDateTimeOffset(), 
                info.Metadata.ToByteArray(), 
                LedgerId.FromByteString(info.LedgerId), 
                AccountId.FromProtobuf(info.SpenderId));
        }

		/// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="M:TokenNftInfo.ToBytes"]/*' />
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="TokenNftInfo.cs.xml" path='docs/member[@name="M:TokenNftInfo.ToProtobuf"]/*' />
		public virtual Proto.TokenNftInfo ToProtobuf()
        {
            Proto.TokenNftInfo proto = new()
            {
				NftID = NftId.ToProtobuf(),
				AccountID = AccountId.ToProtobuf(),
				CreationTime = CreationTime.ToProtoTimestamp(),
				Metadata = ByteString.CopyFrom(Metadata),
				LedgerId = LedgerId.ToByteString(),
			};
                
            if (SpenderId != null)
				proto.SpenderId = SpenderId.ToProtobuf();

			return proto;
        }
    }
}