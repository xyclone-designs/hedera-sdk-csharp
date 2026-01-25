// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Transactions.Account;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    public class TokenNftTransfer : IComparable<TokenNftTransfer>
    {
        /// <summary>
        /// The ID of the token
        /// </summary>
        public readonly TokenId tokenId;
        /// <summary>
        /// The accountID of the sender
        /// </summary>
        public readonly AccountId sender;
        /// <summary>
        /// The accountID of the receiver
        /// </summary>
        public readonly AccountId receiver;
        /// <summary>
        /// The serial number of the NFT
        /// </summary>
        public readonly long serial;
        /// <summary>
        /// If true then the transfer is expected to be an approved allowance and the sender is expected to be the owner. The
        /// default is false.
        /// </summary>
        public bool isApproved;
        // Optional typed hook calls for sender/receiver
        NftHookCall senderHookCall;
        NftHookCall receiverHookCall;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="sender">the sender account id</param>
        /// <param name="receiver">the receiver account id</param>
        /// <param name="serial">the serial number</param>
        /// <param name="isApproved">is it approved</param>
        TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved)
        {
            tokenId = tokenId;
            sender = sender;
            receiver = receiver;
            serial = serial;
            isApproved = isApproved;
            senderHookCall = null;
            receiverHookCall = null;
        }

        TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved, NftHookCall senderHookCall, NftHookCall receiverHookCall)
        {
            tokenId = tokenId;
            sender = sender;
            receiver = receiver;
            serial = serial;
            isApproved = isApproved;
            senderHookCall = senderHookCall;
            receiverHookCall = receiverHookCall;
        }

        static IList<TokenNftTransfer> FromProtobuf(TokenTransferList tokenTransferList)
        {
            var token = TokenId.FromProtobuf(tokenTransferList.GetToken());
            var nftTransfers = new List<TokenNftTransfer>();
            foreach (var transfer in tokenTransferList.GetNftTransfersList())
            {
                NftHookCall senderHookCall = null;
                NftHookCall receiverHookCall = null;
                if (transfer.HasPreTxSenderAllowanceHook())
                {
                    senderHookCall = ToNftHook(transfer.GetPreTxSenderAllowanceHook(), NftHookType.PRE_HOOK_SENDER);
                }
                else if (transfer.HasPrePostTxSenderAllowanceHook())
                {
                    senderHookCall = ToNftHook(transfer.GetPrePostTxSenderAllowanceHook(), NftHookType.PRE_POST_HOOK_SENDER);
                }

                if (transfer.HasPreTxReceiverAllowanceHook())
                {
                    receiverHookCall = ToNftHook(transfer.GetPreTxReceiverAllowanceHook(), NftHookType.PRE_HOOK_RECEIVER);
                }
                else if (transfer.HasPrePostTxReceiverAllowanceHook())
                {
                    receiverHookCall = ToNftHook(transfer.GetPrePostTxReceiverAllowanceHook(), NftHookType.PRE_POST_HOOK_RECEIVER);
                }

                var sender = AccountId.FromProtobuf(transfer.GetSenderAccountID());
                var receiver = AccountId.FromProtobuf(transfer.GetReceiverAccountID());
                nftTransfers.Add(new TokenNftTransfer(token, sender, receiver, transfer.GetSerialNumber(), transfer.GetIsApproval(), senderHookCall, receiverHookCall));
            }

            return nftTransfers;
        }

        /// <summary>
        /// Convert a byte array to a token NFT transfer object.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>the converted token nft transfer object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static TokenNftTransfer FromBytes(byte[] bytes)
        {
            TokenTransferList proto = new()
            {
				Token = new Proto.TokenID { },
			};

            proto.NftTransfers.Add(Proto.NftTransfer.Parser.ParseFrom(bytes));
            
            return FromProtobuf(proto)[0];
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>the protobuf representation</returns>
        public virtual Proto.NftTransfer ToProtobuf()
        {
            Proto.NftTransfer proto = new()
            {
                SenderAccountID = sender.ToProtobuf(),
                ReceiverAccountID = receiver.ToProtobuf(),
                SerialNumber = serial,
                IsApproval = isApproved,
            };

			switch (senderHookCall?.Type)
			{
				case NftHookType.PreHookSender:
					proto.PreTxReceiverAllowanceHook = senderHookCall.ToProtobuf();
					break;
				case NftHookType.PrePostHookSender:
					proto.PrePostTxReceiverAllowanceHook = senderHookCall.ToProtobuf();
					break;

				default: break;
			}

			switch (receiverHookCall?.Type)
			{
				case NftHookType.PreHookReceiver:
					proto.PreTxReceiverAllowanceHook = receiverHookCall.ToProtobuf();
					break;
				case NftHookType.PrePostHookReceiver:
					proto.PrePostTxReceiverAllowanceHook = receiverHookCall.ToProtobuf();
					break;

				default: break;
			}

            return proto;
        }

        /// <summary>
        /// Convert the token NFT transfer object to a byte array.
        /// </summary>
        /// <returns>the converted token NFT transfer object</returns>
        public virtual byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
		public virtual int CompareTo(TokenNftTransfer? o)
		{
			int senderComparison = sender.CompareTo(o.sender);
			if (senderComparison != 0)
			{
				return senderComparison;
			}

			int receiverComparison = receiver.CompareTo(o.receiver);
			if (receiverComparison != 0)
			{
				return receiverComparison;
			}

			return serial.CompareTo(o.serial);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(tokenId, sender, receiver, serial, isApproved);
		}
		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            TokenNftTransfer that = (TokenNftTransfer)o;

            return serial == that.serial && isApproved == that.isApproved && tokenId.Equals(that.tokenId) && sender.Equals(that.sender) && receiver.Equals(that.receiver);
        }
    }
}