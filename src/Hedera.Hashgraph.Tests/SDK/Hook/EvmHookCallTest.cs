// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Hook;

namespace Hedera.Hashgraph.Tests.SDK.Hook
{
    class EvmHookCallTest
    {
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