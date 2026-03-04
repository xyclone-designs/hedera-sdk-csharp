// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class HookExtensionPointTest
    {
        public virtual void RoundTripAllEnumValues()
        {
            foreach (HookExtensionPoint value in Enum.GetValues<HookExtensionPoint>())
            {
                var proto = (Proto.HookExtensionPoint)value;
                
                Assert.NotNull(proto);

                var restored =  (HookExtensionPoint)proto;

                Assert.Equal(value, restored);
                //Assert.Equal(value, restored, "Round-trip mismatch for " + value.ToString());
            }
        }

        public virtual void ProtoValuesAreStable()
        {
			foreach (HookExtensionPoint value in Enum.GetValues<HookExtensionPoint>())
			{
				var proto = (Proto.HookExtensionPoint)value;

				// basic sanity: ordinal-like mapping should not be negative
				Assert.True((int)proto >= 0);
                Assert.True(value.ToString().Length > 0);
            }
        }
    }
}