// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK.Hook
{
    // Using fully qualified names to avoid conflicts with generated classes
    /// <summary>
    /// Enum representing the Hiero extension points that accept a hook.
    /// <p>
    /// Extension points define where hooks can be attached to customize behavior
    /// in the Hiero network.
    /// </summary>
    public enum HookExtensionPoint
    {
		/// <summary>
		/// Used to customize an account's allowances during a CryptoTransfer transaction.
		/// <p>
		/// This hook allows accounts to define custom logic for approving or rejecting
		/// token transfers, providing fine-grained control over allowance behavior.
		/// </summary>
		AccountAllowanceHook = Proto.HookExtensionPoint.AccountAllowanceHook,
	}
}