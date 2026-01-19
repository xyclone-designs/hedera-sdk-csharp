using Org.BouncyCastle.Utilities;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Specifies the details of a hook's creation.
     * <p>
     * This class contains all the information needed to create a new hook,
     * including the extension point, hook ID, implementation, and optional admin key.
     */
    public class HookCreationDetails 
    {
        public HookExtensionPoint ExtensionPoint { get; }
        public long HookId { get; }
        public LambdaEvmHook Hook { get; }
        public Key? AdminKey { get; }
        public bool HasAdminKey { get => AdminKey is not null; }

        /**
         * Create new hook creation details with an admin key.
         *
         * @param extensionPoint the extension point for the hook
         * @param hookId the ID to create the hook at
         * @param hook the hook implementation
         * @param adminKey the admin key for managing the hook
         */
        public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook, Key? adminKey) {
            ExtensionPoint = extensionPoint;
            HookId = hookId;
            Hook = hook;
            AdminKey = adminKey;
        }

        /**
         * Create new hook creation details without an admin key.
         *
         * @param extensionPoint the extension point for the hook
         * @param hookId the ID to create the hook at
         * @param hook the hook implementation
         */
        public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, LambdaEvmHook hook) : this(extensionPoint, hookId, hook, null) { }

        /**
         * Convert this HookCreationDetails to a protobuf message.
         *
         * @return the protobuf HookCreationDetails
         */
        public Proto.HookCreationDetails ToProtobuf() 
        {
            return new Proto.HookCreationDetails
            {
                AdminKey = AdminKey.ToProtobufKey(),
                ExtensionPoint = ExtensionPoint.ToProto(),
                HookId = HookId,
                LambdaEvmHook = Hook.ToProtobuf(),
            };
        }

        /**
         * Create HookCreationDetails from a protobuf message.
         *
         * @param proto the protobuf HookCreationDetails
         * @return a new HookCreationDetails instance
         */
        public static HookCreationDetails FromProtobuf(Proto.HookCreationDetails proto) {
            var adminKey = proto.AdminKey is not null ? Key.FromProtobufKey(proto.AdminKey) : null;

            return new HookCreationDetails(
                    proto.ExtensionPoint.ToSDK(),
                    proto.HookId,
                    LambdaEvmHook.FromProtobuf(proto.LambdaEvmHook),
                    adminKey);
        }

        public override int GetHashCode() 
        {
            return HashCode.Combine(ExtensionPoint, HookId, Hook, AdminKey);
        }
		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			HookCreationDetails that = (HookCreationDetails)obj;

			return HookId == that.HookId
					&& ExtensionPoint == that.ExtensionPoint
					&& Hook.Equals(that.Hook)
					&& Equals(AdminKey, that.AdminKey);
		}
	}
}