namespace Hedera.Hashgraph.SDK
{
	/**
	 * A typed hook call for NFT transfers.
	 */
	public class NftHookCall : HookCall
	{
		public NftHookCall(long hookId, EvmHookCall evmHookCall, NftHookType type) : base(hookId, evmHookCall)
		{
			Type = type;
		}

		public NftHookType Type { get; }
	}
}