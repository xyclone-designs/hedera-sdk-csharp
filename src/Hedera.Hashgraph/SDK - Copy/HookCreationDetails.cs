// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Specifies the details of a hook's creation.
    /// <p>
    /// This class contains all the information needed to create a new hook,
    /// including the extension point, hook ID, implementation, and optional admin key.
    /// </summary>
    public class HookCreationDetails
    {
        /// <summary>
        /// Create new hook creation details with an admin key.
        /// </summary>
        /// <param name="extensionPoint">the extension point for the hook</param>
        /// <param name="hookId">the ID to create the hook at</param>
        /// <param name="hook">the hook implementation</param>
        /// <param name="adminKey">the admin key for managing the hook</param>
        public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook, Key adminKey)
        {
            ExtensionPoint = extensionPoint;
            HookId = hookId;
            Hook = hook;
            AdminKey = adminKey;
        }

		public HookExtensionPoint ExtensionPoint { get; }
		public long HookId { get; }
		public LambdaEvmHook Hook { get; }
		public Key AdminKey { get; }

		/// <summary>
		/// Create new hook creation details without an admin key.
		/// </summary>
		/// <param name="extensionPoint">the extension point for the hook</param>
		/// <param name="hookId">the ID to create the hook at</param>
		/// <param name="hook">the hook implementation</param>
		public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook) : this(extensionPoint, hookId, hook, null) { }

        /// <summary>
        /// Convert this HookCreationDetails to a protobuf message.
        /// </summary>
        /// <returns>the protobuf HookCreationDetails</returns>
        public virtual HookCreationDetails ToProtobuf()
        {
            Proto.HookCreationDetails proto = new()
            {
				ExtensionPoint = extensionPoint.GetProtoValue(),
				HookId = hookId,
				LambdaEvmHook = hook.ToProtobuf(),
			};

            if (adminKey != null)
            {
                builder.SetAdminKey(adminKey.ToProtobufKey());
            }

            return proto;
        }

        /// <summary>
        /// Create HookCreationDetails from a protobuf message.
        /// </summary>
        /// <param name="proto">the protobuf HookCreationDetails</param>
        /// <returns>a new HookCreationDetails instance</returns>
        public static HookCreationDetails FromProtobuf(Proto.HookCreationDetails proto)
        {
            var adminKey = proto.HasAdminKey() ? Key.FromProtobufKey(proto.GetAdminKey()) : null;

            return new HookCreationDetails((HookExtensionPoint)proto.ExtensionPoint, proto.GetHookId(), LambdaEvmHook.FromProtobuf(proto.GetLambdaEvmHook()), adminKey);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            
            HookCreationDetails that = (HookCreationDetails)o;

            return HookId == that.HookId && ExtensionPoint == that.ExtensionPoint && Hook.Equals(that.Hook) && Equals(AdminKey, that.AdminKey);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExtensionPoint, HookId, Hook, AdminKey);
        }
    }
}