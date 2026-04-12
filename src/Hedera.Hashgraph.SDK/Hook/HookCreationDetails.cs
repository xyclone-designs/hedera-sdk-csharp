// SPDX-License-Identifier: Apache-2.0
// Using fully qualified names to avoid conflicts with generated classes
using Hedera.Hashgraph.SDK.Keys;

using System;

namespace Hedera.Hashgraph.SDK.Hook
{
    /// <include file="HookCreationDetails.cs.xml" path='docs/member[@name="T:HookCreationDetails"]/*' />
    public class HookCreationDetails
    {
        /// <include file="HookCreationDetails.cs.xml" path='docs/member[@name="M:HookCreationDetails.#ctor(HookExtensionPoint,System.Int64,EvmHook,Key)"]/*' />
        public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, EvmHook hook, Key? adminKey)
        {
            ExtensionPoint = extensionPoint;
            HookId = hookId;
            Hook = hook;
            AdminKey = adminKey;
        }
		/// <include file="HookCreationDetails.cs.xml" path='docs/member[@name="M:HookCreationDetails.#ctor(HookExtensionPoint,System.Int64,EvmHook)"]/*' />
		public HookCreationDetails(HookExtensionPoint extensionPoint, long hookId, EvmHook hook) : this(extensionPoint, hookId, hook, null) { }

		/// <include file="HookCreationDetails.cs.xml" path='docs/member[@name="M:HookCreationDetails.FromProtobuf(Proto.Services.HookCreationDetails)"]/*' />
		public static HookCreationDetails FromProtobuf(Proto.Services.HookCreationDetails proto)
		{
			var adminKey = Proto.Services.AdminKey is not null ? Key.FromProtobufKey(Proto.Services.AdminKey) : null;

			return new HookCreationDetails((HookExtensionPoint)Proto.Services.ExtensionPoint, Proto.Services.HookId, EvmHook.FromProtobuf(Proto.Services.EvmHook), adminKey);
		}

		public HookExtensionPoint ExtensionPoint { get; }
		public long HookId { get; }
		public EvmHook Hook { get; }
		public Key? AdminKey { get; }
		public bool HasAdminKey { get => AdminKey is not null; }

		/// <include file="HookCreationDetails.cs.xml" path='docs/member[@name="M:HookCreationDetails.ToProtobuf"]/*' />
		public virtual Proto.Services.HookCreationDetails ToProtobuf()
        {
            Proto.Services.HookCreationDetails proto = new()
            {
				ExtensionPoint = (Proto.Services.HookExtensionPoint)ExtensionPoint,
				HookId = HookId,
				EvmHook = Hook.ToProtobuf(),
			};

            if (AdminKey != null)
				Proto.Services.AdminKey = AdminKey.ToProtobufKey();

			return proto;
        }

		public override int GetHashCode()
		{
			return HashCode.Combine(ExtensionPoint, HookId, Hook, AdminKey);
		}
		public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o?.GetType())
                return false;
            
            HookCreationDetails that = (HookCreationDetails)o;

            return HookId == that.HookId && ExtensionPoint == that.ExtensionPoint && Hook.Equals(that.Hook) && Equals(AdminKey, that.AdminKey);
        }
    }
}
