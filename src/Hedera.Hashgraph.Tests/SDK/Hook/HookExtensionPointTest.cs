// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class HookExtensionPointTest
    {
        public virtual void RoundTripAllEnumValues()
        {
            foreach (HookExtensionPoint value in HookExtensionPoint.Values())
            {
                var proto = value.GetProtoValue();
                Assert.NotNull(proto);
                var restored = HookExtensionPoint.FromProtobuf(proto);
                Assert.Equal(value, restored, "Round-trip mismatch for " + value);
            }
        }

        public virtual void ProtoValuesAreStable()
        {
            foreach (HookExtensionPoint value in HookExtensionPoint.Values())
            {
                var proto = value.GetProtoValue();

                // basic sanity: ordinal-like mapping should not be negative
                Assert.True(proto.GetNumber() >= 0);
                Assert.True(value.ToString().Length > 0);
            }
        }
    }
}