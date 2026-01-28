// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Ids;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// 
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/token-service/tokengetnftinfo#tokennftinfo">Hedera Documentation</a>
    /// </summary>
    public class TokenNftInfo
    {
        /// <summary>
        /// The ID of the NFT
        /// </summary>
        public NftId NftId;
        /// <summary>
        /// The current owner of the NFT
        /// </summary>
        public AccountId AccountId;
        /// <summary>
        /// The effective consensus timestamp at which the NFT was minted
        /// </summary>
        public Timestamp CreationTime;
        /// <summary>
        /// Represents the unique metadata of the NFT
        /// </summary>
        public byte[] Metadata;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public LedgerId LedgerId;
        /// <summary>
        /// If an allowance is granted for the NFT, its corresponding spender account
        /// </summary>
        public AccountId SpenderId;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nftId">the id of the nft</param>
        /// <param name="accountId">the current owner of the nft</param>
        /// <param name="creationTime">the effective consensus time</param>
        /// <param name="metadata">the unique metadata</param>
        /// <param name="ledgerId">the ledger id of the response</param>
        /// <param name="spenderId">the spender of the allowance (null if not an allowance)</param>
        TokenNftInfo(NftId nftId, AccountId accountId, Timestamp creationTime, byte[] metadata, LedgerId ledgerId, AccountId spenderId)
        {
            NftId = nftId;
            AccountId = accountId;
            CreationTime = creationTime;
            Metadata = metadata;
            LedgerId = ledgerId;
            SpenderId = spenderId;
        }

        /// <summary>
        /// Create token nft info from a protobuf.
        /// </summary>
        /// <param name="info">the protobuf</param>
        /// <returns>                         the new token nft info</returns>
        static TokenNftInfo FromProtobuf(Proto.TokenNftInfo info)
        {
            return new TokenNftInfo(
                NftId.FromProtobuf(info.NftID), 
                AccountId.FromProtobuf(info.AccountID), 
                Utils.TimestampConverter.FromProtobuf(info.CreationTime), 
                info.Metadata.ToByteArray(), 
                LedgerId.FromByteString(info.LedgerId), 
                info.SpenderId is not null ? AccountId.FromProtobuf(info.SpenderId) : null);
        }

        /// <summary>
        /// Create token nft info from byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new token nft info</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenNftInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.TokenNftInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        public virtual Proto.TokenNftInfo ToProtobuf()
        {
            Proto.TokenNftInfo proto = new()
            {
				NftID = NftId.ToProtobuf(),
				AccountID = AccountId.ToProtobuf(),
				CreationTime = Utils.TimestampConverter.ToProtobuf(CreationTime),
				Metadata = ByteString.CopyFrom(Metadata),
				LedgerId = LedgerId.ToByteString(),
			};
                
            if (SpenderId != null)
				proto.SpenderId = SpenderId.ToProtobuf();

			return proto;
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}