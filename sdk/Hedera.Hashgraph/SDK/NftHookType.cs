namespace Hedera.Hashgraph.SDK
{
	/**
	 * Hook type for NFT transfers, indicating side (sender/receiver) and timing (pre / pre-post).
	 */
	public enum NftHookType
	{
		PreHookSendER,
		PrePostHookSender,
		PreHookReceiver,
		PrePostHookReceiver
	}

}