// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class EvmHookCallTest
    {
        virtual void RoundTripProtoAndGettersAndEquality()
        {
            byte[] data = new byte[]
            {
                1,
                2,
                3
            };
            long gas = 25000;
            var call = new EvmHookCall(data, gas);

            // getters
            Assert.Equal(call.GetGasLimit(), gas);
            AssertThat(call.GetData()).ContainsExactly(1, 2, 3);

            // immutability of data
            var returned = call.GetData();
            returned[0] = 9;
            AssertThat(call.GetData()).ContainsExactly(1, 2, 3);

            // proto round-trip
            var proto = call.ToProtobuf();
            var parsed = EvmHookCall.FromProtobuf(proto);
            Assert.Equal(parsed, call);
            Assert.Equal(parsed.GetHashCode(), call.GetHashCode());
        }

        virtual void NullDataThrows()
        {
            AssertThatThrownBy(() => new EvmHookCall(null, 1)).IsInstanceOf(typeof(NullReferenceException)).HasMessage("data cannot be null");
        }
    }
}