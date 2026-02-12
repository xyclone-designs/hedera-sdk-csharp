// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <summary>
	/// Class to encapsulate the nft methods for token allowance's.
	/// </summary>
	public class TokenNftAllowance
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="ownerAccountId">the grantor's account id</param>
        /// <param name="spenderAccountId">the spender's account id</param>
        /// <param name="delegatingSpender">the delegating spender's account id</param>
        /// <param name="serialNumbers">the list of serial numbers</param>
        /// <param name="allSerials">grant for all serial's</param>
        internal TokenNftAllowance(TokenId tokenId, AccountId? ownerAccountId, AccountId? spenderAccountId, AccountId? delegatingSpender, IEnumerable<long> serialNumbers, bool? allSerials)
        {
            TokenId = tokenId;
            OwnerAccountId = ownerAccountId;
            SpenderAccountId = spenderAccountId;
            DelegatingSpender = delegatingSpender;
            SerialNumbers = [.. serialNumbers];
            AllSerials = allSerials;
        }

		/// <summary>
		/// The NFT token type that the allowance pertains to
		/// </summary>
		public readonly TokenId TokenId;
		/// <summary>
		/// The account ID of the token owner (ie. the grantor of the allowance)
		/// </summary>
		public readonly AccountId? OwnerAccountId;
		/// <summary>
		/// The account ID of the token allowance spender
		/// </summary>
		public readonly AccountId? SpenderAccountId;
		/// <summary>
		/// The account ID of the spender who is granted approvedForAll allowance and granting
		/// approval on an NFT serial to another spender.
		/// </summary>
		public readonly AccountId? DelegatingSpender;
		/// <summary>
		/// The list of serial numbers that the spender is permitted to transfer.
		/// </summary>
		public readonly IList<long> SerialNumbers;
		/// <summary>
		/// If true, the spender has access to all of the owner's NFT units of type tokenId (currently
		/// owned and any in the future).
		/// </summary>
		public readonly bool? AllSerials;

		/// <summary>
		/// Create a copy of a nft token allowance object.
		/// </summary>
		/// <param name="allowance">the nft token allowance to copj</param>
		/// <returns>                         a new copy</returns>
		public static TokenNftAllowance CopyFrom(TokenNftAllowance allowance)
        {
            return new TokenNftAllowance(allowance.TokenId, allowance.OwnerAccountId, allowance.SpenderAccountId, allowance.DelegatingSpender, allowance.SerialNumbers, allowance.AllSerials);
        }
		/// <summary>
		/// Create a nft token allowance from a byte array.
		/// </summary>
		/// <param name="bytes">the byte array</param>
		/// <returns>                         the nft token allowance</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static TokenNftAllowance FromBytes(byte[] bytes)
		{
			return FromProtobuf(Proto.NftAllowance.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a nft token allowance from a protobuf.
		/// </summary>
		/// <param name="allowanceProto">the protobuf</param>
		/// <returns>                         the nft token allowance</returns>
		public static TokenNftAllowance FromProtobuf(Proto.NftAllowance allowanceProto)
        {
            return new TokenNftAllowance(
                TokenId.FromProtobuf(allowanceProto.TokenId), 
                AccountId.FromProtobuf(allowanceProto.Owner),
                AccountId.FromProtobuf(allowanceProto.Spender),
                AccountId.FromProtobuf(allowanceProto.DelegatingSpender),
                allowanceProto.SerialNumbers,
                allowanceProto.ApprovedForAll);
        }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public virtual byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		/// <returns>                         the protobuf representation</returns>
		public virtual Proto.NftAllowance ToProtobuf()
        {
            Proto.NftAllowance proto = new()
            {
				ApprovedForAll = AllSerials,
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.AccountID owneraccountid)
				proto.Owner = owneraccountid;
			if (SpenderAccountId?.ToProtobuf() is Proto.AccountID spenderaccountid)
				proto.Spender = spenderaccountid;
			if (DelegatingSpender?.ToProtobuf() is Proto.AccountID delegatingspender)
				proto.DelegatingSpender = delegatingspender;

			proto.SerialNumbers.AddRange(SerialNumbers);

            return proto;
        }
        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the remove protobuf</returns>
        public virtual Proto.NftRemoveAllowance ToRemoveProtobuf()
        {
			Proto.NftRemoveAllowance proto = new()
            {
				TokenId = TokenId.ToProtobuf(),
			};

			if (OwnerAccountId?.ToProtobuf() is Proto.AccountID owneraccountid)
				proto.Owner = owneraccountid;

			proto.SerialNumbers.AddRange(SerialNumbers);
			
            return proto;
        }

		/// <summary>
		/// Validate the configured client.
		/// </summary>
		/// <param name="client">the configured client</param>
		/// <exception cref="BadEntityIdException">if entity ID is formatted poorly</exception>
		public virtual void ValidateChecksums(Client client)
		{
			TokenId.ValidateChecksum(client);
			OwnerAccountId?.ValidateChecksum(client);
			SpenderAccountId?.ValidateChecksum(client);
			DelegatingSpender?.ValidateChecksum(client);
		}
	}
}