// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class HookExtensionPointTest
    {
        virtual void RoundTripAllEnumValues()
        {
            foreach (HookExtensionPoint value in HookExtensionPoint.Values())
            {
                var proto = value.GetProtoValue();
                AssertNotNull(proto);
                var restored = HookExtensionPoint.FromProtobuf(proto);
                AssertEquals(value, restored, "Round-trip mismatch for " + value);
            }
        }

        virtual void ProtoValuesAreStable()
        {
            foreach (HookExtensionPoint value in HookExtensionPoint.Values())
            {
                var proto = value.GetProtoValue();

                // basic sanity: ordinal-like mapping should not be negative
                AssertTrue(proto.GetNumber() >= 0);
                AssertTrue(value.ToString().Length > 0);
            }
        }
    }
}