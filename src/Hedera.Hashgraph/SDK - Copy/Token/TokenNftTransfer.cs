// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Ids;
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
        public readonly TokenId TokenId;
        /// <summary>
        /// The accountID of the sender
        /// </summary>
        public readonly AccountId Sender;
        /// <summary>
        /// The accountID of the receiver
        /// </summary>
        public readonly AccountId Receiver;
        /// <summary>
        /// The serial number of the NFT
        /// </summary>
        public readonly long Serial;
        /// <summary>
        /// If true then the transfer is expected to be an approved allowance and the sender is expected to be the owner. The
        /// default is false.
        /// </summary>
        public bool IsApproved;
        // Optional typed hook calls for sender/receiver
        public NftHookCall SenderHookCall;
        public NftHookCall ReceiverHookCall;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="sender">the sender account id</param>
        /// <param name="receiver">the receiver account id</param>
        /// <param name="serial">the serial number</param>
        /// <param name="isApproved">is it approved</param>
        public TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved)
        {
            tokenId = tokenId;
            sender = sender;
            receiver = receiver;
            serial = serial;
            isApproved = isApproved;
            SenderHookCall = null;
            ReceiverHookCall = null;
        }
        public TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved, NftHookCall senderHookCall, NftHookCall receiverHookCall)
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
            var token = Ids.TokenId.FromProtobuf(tokenTransferList);

            var nftTransfers = new List<TokenNftTransfer>();
            foreach (var transfer in tokenTransferList.NftTransfers)
            {
                NftHookCall? senderHookCall = null;
                NftHookCall? receiverHookCall = null;

                if (transfer.PreTxSenderAllowanceHook is not null)
                    senderHookCall = ToNftHook(transfer.GetPreTxSenderAllowanceHook(), NftHookType.PreHookSender);
                else if (transfer.PrePostTxSenderAllowanceHook is not null)
					senderHookCall = ToNftHook(transfer.GetPrePostTxSenderAllowanceHook(), NftHookType.PrePostHookSender);

				if (transfer.HasPreTxReceiverAllowanceHook())
                    receiverHookCall = ToNftHook(transfer.GetPreTxReceiverAllowanceHook(), NftHookType.PreHookReceiver);
                else if (transfer.HasPrePostTxReceiverAllowanceHook())
                    receiverHookCall = ToNftHook(transfer.GetPrePostTxReceiverAllowanceHook(), NftHookType.PrePostHookReceiver);

                var sender = AccountId.FromProtobuf(transfer.SenderAccountID());
                var receiver = AccountId.FromProtobuf(transfer.ReceiverAccountID());

                nftTransfers.Add(new TokenNftTransfer(token, sender, receiver, transfer.SerialNumber, transfer.IsApproved.IsApproval, senderHookCall, receiverHookCall));
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
                SenderAccountID = Sender.ToProtobuf(),
                ReceiverAccountID = Receiver.ToProtobuf(),
                SerialNumber = Serial,
                IsApproval = IsApproved,
            };

			switch (SenderHookCall?.Type)
			{
				case NftHookType.PreHookSender:
					proto.PreTxReceiverAllowanceHook = SenderHookCall.ToProtobuf();
					break;
				case NftHookType.PrePostHookSender:
					proto.PrePostTxReceiverAllowanceHook = SenderHookCall.ToProtobuf();
					break;

				default: break;
			}

			switch (ReceiverHookCall?.Type)
			{
				case NftHookType.PreHookReceiver:
					proto.PreTxReceiverAllowanceHook = ReceiverHookCall.ToProtobuf();
					break;
				case NftHookType.PrePostHookReceiver:
					proto.PrePostTxReceiverAllowanceHook = ReceiverHookCall.ToProtobuf();
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
			int senderComparison = Sender.CompareTo(o?.Sender);
			if (senderComparison != 0)
			{
				return senderComparison;
			}

			int receiverComparison = Receiver.CompareTo(o?.Receiver);
			if (receiverComparison != 0)
			{
				return receiverComparison;
			}

			return Serial.CompareTo(o?.Serial);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(TokenId, Sender, Receiver, Serial, IsApproved);
		}
		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o?.GetType())
            {
                return false;
            }

            TokenNftTransfer that = (TokenNftTransfer)o;

            return Serial == that.Serial && IsApproved == that.IsApproved && TokenId.Equals(that.TokenId) && Sender.Equals(that.Sender) && Receiver.Equals(that.Receiver);
        }
    }
}