// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Nfts
{
	/// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="T:TokenNftTransfer"]/*' />
	public class TokenNftTransfer : IComparable<TokenNftTransfer>
    {
        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="F:TokenNftTransfer.TokenId"]/*' />
        public readonly TokenId TokenId;
        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="F:TokenNftTransfer.Sender"]/*' />
        public readonly AccountId Sender;
        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="F:TokenNftTransfer.Receiver"]/*' />
        public readonly AccountId Receiver;
        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="F:TokenNftTransfer.Serial"]/*' />
        public readonly long Serial;
        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="F:TokenNftTransfer.IsApproved"]/*' />
        public bool IsApproved;
        // Optional typed hook calls for sender/receiver
        public NftHookCall? SenderHookCall;
        public NftHookCall? ReceiverHookCall;

        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="M:TokenNftTransfer.#ctor(TokenId,AccountId,AccountId,System.Int64,System.Boolean,NftHookCall,NftHookCall)"]/*' />
        public TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved, NftHookCall? senderHookCall, NftHookCall? receiverHookCall)
        {
            TokenId = tokenId;
            Sender = sender;
            Receiver = receiver;
            Serial = serial;
            IsApproved = isApproved;
            SenderHookCall = senderHookCall;
            ReceiverHookCall = receiverHookCall;
        }

        public static IList<TokenNftTransfer> FromProtobuf(Proto.TokenTransferList tokenTransferList)
        {
            TokenId token = TokenId.FromProtobuf(tokenTransferList.Token);
			
			var nftTransfers = new List<TokenNftTransfer>();

            foreach (var transfer in tokenTransferList.NftTransfers)
            {
                NftHookCall? senderHookCall = null;
                NftHookCall? receiverHookCall = null;

                if (transfer.PreTxSenderAllowanceHook is not null)
                    senderHookCall = TransferTransaction.ToNftHook(transfer.PreTxSenderAllowanceHook, NftHookType.PreHookSender);
                else if (transfer.PrePostTxSenderAllowanceHook is not null)
					senderHookCall = TransferTransaction.ToNftHook(transfer.PrePostTxSenderAllowanceHook, NftHookType.PrePostHookSender);

				if (transfer.PreTxReceiverAllowanceHook is not null)
                    receiverHookCall = TransferTransaction.ToNftHook(transfer.PreTxReceiverAllowanceHook, NftHookType.PreHookReceiver);
                else if (transfer.PrePostTxReceiverAllowanceHook is not null)
                    receiverHookCall = TransferTransaction.ToNftHook(transfer.PrePostTxReceiverAllowanceHook, NftHookType.PrePostHookReceiver);

				AccountId sender = AccountId.FromProtobuf(transfer.SenderAccountID);
                AccountId receiver = AccountId.FromProtobuf(transfer.ReceiverAccountID);

                nftTransfers.Add(new TokenNftTransfer(token, sender, receiver, transfer.SerialNumber, transfer.IsApproval, senderHookCall, receiverHookCall));
            }

            return nftTransfers;
        }

        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="M:TokenNftTransfer.FromBytes(System.Byte[])"]/*' />
        public static TokenNftTransfer FromBytes(byte[] bytes)
        {
            Proto.TokenTransferList proto = new()
            {
				Token = new Proto.TokenID { }
			};

            proto.NftTransfers.Add(Proto.NftTransfer.Parser.ParseFrom(bytes));
            
            return FromProtobuf(proto)[0];
        }

        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="M:TokenNftTransfer.ToProtobuf"]/*' />
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

        /// <include file="TokenNftTransfer.cs.xml" path='docs/member[@name="M:TokenNftTransfer.ToBytes"]/*' />
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