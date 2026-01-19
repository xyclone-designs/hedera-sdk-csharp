using System;

namespace Hedera.Hashgraph.SDK
{
	// Using fully qualified names to avoid conflicts with generated classes

	/**
	 * Enum representing the Hiero extension points that accept a hook.
	 * <p>
	 * Extension points define where hooks can be attached to customize behavior
	 * in the Hiero network.
	 */
	public enum HookExtensionPoint
	{
		/**
		 * Used to customize an account's allowances during a CryptoTransfer transaction.
		 * <p>
		 * This hook allows accounts to define custom logic for approving or rejecting
		 * token transfers, providing fine-grained control over allowance behavior.
		 */

		AccountAllowanceHook = Proto.HookExtensionPoint.AccountAllowanceHook,
	}

	public static class HookExtensionPointExtensions
	{
		public static HookExtensionPoint ToSDK(this Proto.HookExtensionPoint hookextensionpoint)
		{
			return hookextensionpoint switch
			{
				Proto.HookExtensionPoint.AccountAllowanceHook => HookExtensionPoint.AccountAllowanceHook,

				_ => throw new ArgumentException(string.Format("Invalid HookExtensionPoint '{0}'", hookextensionpoint))
			};
		}
		public static Proto.HookExtensionPoint ToProto(this HookExtensionPoint hookextensionpoint)
		{
			return hookextensionpoint switch
			{
				HookExtensionPoint.AccountAllowanceHook => Proto.HookExtensionPoint.AccountAllowanceHook,

				_ => throw new ArgumentException(string.Format("Invalid HookExtensionPoint '{0}'", hookextensionpoint))
			};
		}
	}
}