// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    /// <include file="test-evm-hook-call.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookCallTest"]" />
    public class EvmHookCallTest
    {
        [Fact]
        /// <include file="test-evm-hook-call.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Hook.EvmHookCallTest.RoundTripProtoAndGettersAndEquality"]" />
        public virtual void RoundTripProtoAndGettersAndEquality()
        {
            byte[] data = [1,2,3];
            ulong gas = 25000;
            var call = new EvmHookCall(data, gas);

            // getters
            Assert.Equal(call.GasLimit, gas);
            Assert.Equal(call.Data, [ 1, 2, 3 ]);

            // immutability of data
            var returned = call.Data;
            returned[0] = 9;

            Assert.Equal(call.Data, [ 1, 2, 3 ]);

            // proto round-trip
            var proto = call.ToProtobuf();
            var parsed = EvmHookCall.FromProtobuf(proto);

            Assert.Equal(parsed, call);
            Assert.Equal(parsed.GetHashCode(), call.GetHashCode());
        }
    }
}
