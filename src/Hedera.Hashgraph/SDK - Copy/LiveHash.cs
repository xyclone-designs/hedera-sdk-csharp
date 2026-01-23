// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Transactions.Account;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A hash (presumably of some kind of credential or certificate), along with a
    /// list of keys (each of which is either a primitive or a threshold key). Each
    /// of them must reach its threshold when signing the transaction, to attach
    /// this livehash to this account. At least one of them must reach its
    /// threshold to delete this livehash from this account.
    /// 
    /// See <a href="https://docs.hedera.com/guides/core-concepts/accounts#livehash">Hedera Documentation</a>
    /// </summary>
    public class LiveHash
    {
        /// <summary>
        /// The account to which the livehash is attached
        /// </summary>
        public readonly AccountId AccountId;
        /// <summary>
        /// The SHA-384 hash of a credential or certificate
        /// </summary>
        public readonly ByteString Hash;
        /// <summary>
        /// A list of keys (primitive or threshold), all of which must sign to attach the livehash to an account, and any one of which can later delete it.
        /// </summary>
        public readonly KeyList Keys;
        /// <summary>
        /// The duration for which the livehash will remain valid
        /// </summary>
        public readonly Duration Duration;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="hash">the hash</param>
        /// <param name="keys">the key list</param>
        /// <param name="duration">the duration</param>
        private LiveHash(AccountId accountId, ByteString hash, KeyList keys, Duration duration)
        {
            AccountId = accountId;
            Hash = hash;
            Keys = keys;
            Duration = duration;
        }

        /// <summary>
        /// Create a live hash from a protobuf.
        /// </summary>
        /// <param name="liveHash">the protobuf</param>
        /// <returns>                         the new live hash</returns>
        protected static LiveHash FromProtobuf(Proto.LiveHash liveHash)
        {
            return new LiveHash(
                AccountId.FromProtobuf(liveHash.AccountId), 
                liveHash.Hash,
                KeyList.FromProtobuf(liveHash.Keys, null), 
                Utils.DurationConverter.FromProtobuf(liveHash.Duration));
        }

        /// <summary>
        /// Create a live hash from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new live hash</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static LiveHash FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.LiveHash.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Convert the live hash into a protobuf.
        /// </summary>
        /// <returns>                         the protobuf</returns>
        protected virtual Proto.LiveHash ToProtobuf()
        {
			Proto.LiveHash proto = new ()
			{
				AccountId = AccountId.ToProtobuf(),
				Hash = Hash,
				Duration = Utils.DurationConverter.ToProtobuf(Duration),
			};

			if (Keys.Iterator() is IEnumerator<Proto.Key> keys)
                while (keys.MoveNext())
					proto.Keys.Keys.Add(keys.Current);

            return proto;
        }

        /// <summary>
        /// Extract the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual ByteString ToBytes()
        {
            return ToProtobuf().ToByteString();
        }
    }
}