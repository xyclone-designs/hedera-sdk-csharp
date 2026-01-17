namespace Hedera.Hashgraph.SDK
{
	/**
     * A typed hook call for fungible (HBAR and FT) transfers.
     */
    public class FungibleHookCall : HookCall 
    {
        public FungibleHookCall(long hookId, EvmHookCall evmHookCall, FungibleHookType type) : base(hookId, evmHookCall)
		{
            Type = type;
		}

		public FungibleHookType Type { get; }
	}
}