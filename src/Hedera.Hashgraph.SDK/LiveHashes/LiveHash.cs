// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK.LiveHashes
{
    /// <include file="LiveHash.cs.xml" path='docs/member[@name="T:LiveHash"]/*' />
    public class LiveHash
    {
        /// <include file="LiveHash.cs.xml" path='docs/member[@name="M:LiveHash.#ctor(AccountId,ByteString,KeyList,System.TimeSpan)"]/*' />
        internal LiveHash(AccountId accountId, ByteString hash, KeyList keys, TimeSpan duration)
        {
            AccountId = accountId;
            Hash = hash;
            Keys = keys;
            Duration = duration;
        }

		/// <include file="LiveHash.cs.xml" path='docs/member[@name="M:LiveHash.FromBytes(System.Byte[])"]/*' />
		public static LiveHash FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.Services.LiveHash.Parser.ParseFrom(bytes));
		}
		/// <include file="LiveHash.cs.xml" path='docs/member[@name="M:LiveHash.FromProtobuf(Proto.Services.LiveHash)"]/*' />
		public static LiveHash FromProtobuf(Proto.Services.LiveHash liveHash)
        {
            return new LiveHash(
                AccountId.FromProtobuf(liveHash.AccountId), 
                liveHash.Hash,
                KeyList.FromProtobuf(liveHash.Keys, null), 
                liveHash.Duration.ToTimeSpan());
        }

		/// <include file="LiveHash.cs.xml" path='docs/member[@name="F:LiveHash.AccountId"]/*' />
		public readonly AccountId AccountId;
		/// <include file="LiveHash.cs.xml" path='docs/member[@name="F:LiveHash.Hash"]/*' />
		public readonly ByteString Hash;
		/// <include file="LiveHash.cs.xml" path='docs/member[@name="F:LiveHash.Keys"]/*' />
		public readonly KeyList Keys;
		/// <include file="LiveHash.cs.xml" path='docs/member[@name="F:LiveHash.Duration"]/*' />
		public readonly TimeSpan Duration;

		/// <include file="LiveHash.cs.xml" path='docs/member[@name="M:LiveHash.ToBytes"]/*' />
		public virtual ByteString ToBytes()
		{
			return ToProtobuf().ToByteString();
		}
		/// <include file="LiveHash.cs.xml" path='docs/member[@name="M:LiveHash.ToProtobuf"]/*' />
		public virtual Proto.Services.LiveHash ToProtobuf()
        {
			return new Proto.Services.LiveHash
			{
				AccountId = AccountId.ToProtobuf(),
				Hash = Hash,
				Duration = Duration.ToProtoDuration(),
                Keys = Keys.ToProtobuf(),
			};
        }
    }
}
