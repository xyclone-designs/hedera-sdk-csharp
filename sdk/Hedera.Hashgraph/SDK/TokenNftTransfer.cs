namespace Hedera.Hashgraph.SDK
{
	/**
 * Internal utility class.
 */
	public class TokenNftTransfer : Comparable<TokenNftTransfer> {
	/**
     * The ID of the token
     */
	public readonly TokenId tokenId;
    /**
     * The accountID of the sender
     */
    public readonly AccountId sender;
    /**
     * The accountID of the receiver
     */
    public readonly AccountId receiver;
    /**
     * The serial number of the NFT
     */
    public readonly long serial;
	/**
     * If true then the transfer is expected to be an approved allowance and the sender is expected to be the owner. The
     * default is false.
     */
	public bool isApproved;

	// Optional typed hook calls for sender/receiver
	NftHookCall senderHookCall;
	NftHookCall receiverHookCall;

	/**
     * Constructor.
     *
     * @param tokenId    the token id
     * @param sender     the sender account id
     * @param receiver   the receiver account id
     * @param serial     the serial number
     * @param isApproved is it approved
     */
	TokenNftTransfer(TokenId tokenId, AccountId sender, AccountId receiver, long serial, bool isApproved)
	{
		this.tokenId = tokenId;
		this.sender = sender;
		this.receiver = receiver;
		this.serial = serial;
		this.isApproved = isApproved;
		this.senderHookCall = null;
		this.receiverHookCall = null;
	}

	TokenNftTransfer(
			TokenId tokenId,
			AccountId sender,
			AccountId receiver,
			long serial,
			bool isApproved,
			@Nullable NftHookCall senderHookCall,
			@Nullable NftHookCall receiverHookCall)
	{
		this.tokenId = tokenId;
		this.sender = sender;
		this.receiver = receiver;
		this.serial = serial;
		this.isApproved = isApproved;
		this.senderHookCall = senderHookCall;
		this.receiverHookCall = receiverHookCall;
	}

	static List<TokenNftTransfer> FromProtobuf(TokenTransferList tokenTransferList)
	{
		var token = TokenId.FromProtobuf(tokenTransferList.getToken());
		var nftTransfers = new ArrayList<TokenNftTransfer>();

		for (var transfer : tokenTransferList.getNftTransfersList())
		{
			NftHookCall senderHookCall = null;
			NftHookCall receiverHookCall = null;

			if (transfer.hasPreTxSenderAllowanceHook())
			{
				senderHookCall = toNftHook(transfer.getPreTxSenderAllowanceHook(), NftHookType.PRE_HOOK_SENDER);
			}
			else if (transfer.hasPrePostTxSenderAllowanceHook())
			{
				senderHookCall =
						toNftHook(transfer.getPrePostTxSenderAllowanceHook(), NftHookType.PRE_POST_HOOK_SENDER);
			}

			if (transfer.hasPreTxReceiverAllowanceHook())
			{
				receiverHookCall = toNftHook(transfer.getPreTxReceiverAllowanceHook(), NftHookType.PRE_HOOK_RECEIVER);
			}
			else if (transfer.hasPrePostTxReceiverAllowanceHook())
			{
				receiverHookCall =
						toNftHook(transfer.getPrePostTxReceiverAllowanceHook(), NftHookType.PRE_POST_HOOK_RECEIVER);
			}

			var sender = AccountId.FromProtobuf(transfer.getSenderAccountID());
			var receiver = AccountId.FromProtobuf(transfer.getReceiverAccountID());

			nftTransfers.Add(new TokenNftTransfer(
					token,
					sender,
					receiver,
					transfer.getSerialNumber(),
					transfer.getIsApproval(),
					senderHookCall,
					receiverHookCall));
		}
		return nftTransfers;
	}

	/**
     * Convert a byte array to a token NFT transfer object.
     *
     * @param bytes the byte array
     * @return the converted token nft transfer object
     * @ when there is an issue with the protobuf
     */
	[Obsolete]
	public static TokenNftTransfer FromBytes(byte[] bytes) 
	{
        return FromProtobuf(TokenTransferList.newBuilder()
                    .setToken(TokenID.newBuilder().build())
                    .AddNftTransfers(NftTransfer.Parser.ParseFrom(bytes))
                    .build())
            .get(0);
	}

	/**
		* Create the protobuf.
		*
		* @return the protobuf representation
		*/
	public Proto.NftTransfer ToProtobuf()
		{
		var builder = NftTransfer.newBuilder()
				.setSenderAccountID(sender.ToProtobuf())
				.setReceiverAccountID(receiver.ToProtobuf())
				.setSerialNumber(serial)
				.setIsApproval(isApproved);

		if (senderHookCall != null)
		{
			switch (senderHookCall.getType())
			{
				case PRE_HOOK_SENDER->builder.setPreTxSenderAllowanceHook(senderHookCall.ToProtobuf());
				case PRE_POST_HOOK_SENDER->builder.setPrePostTxSenderAllowanceHook(senderHookCall.ToProtobuf());
				default -> { }
			}
		}
		if (receiverHookCall != null)
		{
			switch (receiverHookCall.getType())
			{
				case PRE_HOOK_RECEIVER->builder.setPreTxReceiverAllowanceHook(receiverHookCall.ToProtobuf());
				case PRE_POST_HOOK_RECEIVER->builder.setPrePostTxReceiverAllowanceHook(receiverHookCall.ToProtobuf());
				default -> { }
			}
		}

		return builder.build();
	}



	/**
     * Convert the token NFT transfer object to a byte array.
     *
     * @return the converted token NFT transfer object
     */
	[Obsolete]
	public byte[] ToBytes()
	{
		return ToProtobuf().ToByteArray();
	}

	@Override
	public int compareTo(TokenNftTransfer o)
	{
		int senderComparison = sender.compareTo(o.sender);
		if (senderComparison != 0)
		{
			return senderComparison;
		}
		int receiverComparison = receiver.compareTo(o.receiver);
		if (receiverComparison != 0)
		{
			return receiverComparison;
		}
		return long.compare(serial, o.serial);
	}

	@Override
	public override bool Equals(object? obj)
	{
		if (this == o)
		{
			return true;
		}
		if (o == null || getClass() != o.getClass())
		{
			return false;
		}
		TokenNftTransfer that = (TokenNftTransfer)obj;
		return serial == that.serial
				&& isApproved == that.isApproved
				&& tokenId.equals(that.tokenId)
				&& sender.equals(that.sender)
				&& receiver.equals(that.receiver);
	}

	@Override
	public int hashCode()
	{
		return Objects.hash(tokenId, sender, receiver, serial, isApproved);
	}
}

}