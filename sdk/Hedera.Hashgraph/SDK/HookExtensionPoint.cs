namespace Hedera.Hashgraph.SDK
{
	// Using fully qualified names to avoid conflicts with generated classes

/**
 * Enum representing the Hiero extension points that accept a hook.
 * <p>
 * Extension points define where hooks can be attached to customize behavior
 * in the Hiero network.
 */
public enum HookExtensionPoint {
		/**
		 * Used to customize an account's allowances during a CryptoTransfer transaction.
		 * <p>
		 * This hook allows accounts to define custom logic for approving or rejecting
		 * token transfers, providing fine-grained control over allowance behavior.
		 */
		AccountAllowanceHook = Proto.HookExtensionPoint.AccountAllowanceHook;

    private readonly Proto.HookExtensionPoint protoValue;

    HookExtensionPoint(Proto.HookExtensionPoint protoValue) {
        this.protoValue = protoValue;
    }

    /**
     * Get the protobuf value for this extension point.
     *
     * @return the protobuf enum value
     */
    public Proto.HookExtensionPoint getProtoValue() {
        return protoValue;
    }

    /**
     * Create a HookExtensionPoint from a protobuf value.
     *
     * @param protoValue the protobuf enum value
     * @return the corresponding HookExtensionPoint
     * @ if the protobuf value is not recognized
     */
    public static HookExtensionPoint FromProtobuf(Proto.HookExtensionPoint protoValue) {
        return protoValue switch
		{
            ACCOUNT_ALLOWANCE_HOOK => ACCOUNT_ALLOWANCE_HOOK;
            UNRECOGNIZED => throw new ArgumentException("Unrecognized hook extension point: " + protoValue);
        };
    }
}

}