// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk.BaseNodeAddress;
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class BaseNodeAddressTest
    {
        virtual void FromString()
        {
            var ipAddress = BaseNodeAddress.FromString("35.237.200.180:50211");
            AssertThat(ipAddress.GetName()).IsNull();
            Assert.Equal(ipAddress.GetAddress(), "35.237.200.180");
            Assert.Equal(ipAddress.GetPort(), PORT_NODE_PLAIN);
            AssertThat(ipAddress).HasToString("35.237.200.180:50211");
            var ipAddressSecure = ipAddress.ToSecure();
            AssertThat(ipAddressSecure.GetName()).IsNull();
            Assert.Equal(ipAddressSecure.GetAddress(), "35.237.200.180");
            Assert.Equal(ipAddressSecure.GetPort(), PORT_NODE_TLS);
            AssertThat(ipAddressSecure).HasToString("35.237.200.180:50212");
            var ipAddressInsecure = ipAddressSecure.ToInsecure();
            AssertThat(ipAddressInsecure.GetName()).IsNull();
            Assert.Equal(ipAddressInsecure.GetAddress(), "35.237.200.180");
            Assert.Equal(ipAddressInsecure.GetPort(), PORT_NODE_PLAIN);
            AssertThat(ipAddressInsecure).HasToString("35.237.200.180:50211");
            var urlAddress = BaseNodeAddress.FromString("0.testnet.hedera.com:50211");
            AssertThat(urlAddress.GetName()).IsNull();
            Assert.Equal(urlAddress.GetAddress(), "0.testnet.hedera.com");
            Assert.Equal(urlAddress.GetPort(), PORT_NODE_PLAIN);
            AssertThat(urlAddress).HasToString("0.testnet.hedera.com:50211");
            var urlAddressSecure = urlAddress.ToSecure();
            AssertThat(urlAddressSecure.GetName()).IsNull();
            Assert.Equal(urlAddressSecure.GetAddress(), "0.testnet.hedera.com");
            Assert.Equal(urlAddressSecure.GetPort(), PORT_NODE_TLS);
            AssertThat(urlAddressSecure).HasToString("0.testnet.hedera.com:50212");
            var urlAddressInsecure = urlAddressSecure.ToInsecure();
            AssertThat(urlAddressInsecure.GetName()).IsNull();
            Assert.Equal(urlAddressInsecure.GetAddress(), "0.testnet.hedera.com");
            Assert.Equal(urlAddressInsecure.GetPort(), PORT_NODE_PLAIN);
            AssertThat(urlAddressInsecure).HasToString("0.testnet.hedera.com:50211");
            var processAddress = BaseNodeAddress.FromString("in-process:testingProcess");
            Assert.Equal(processAddress.GetName(), "testingProcess");
            AssertThat(processAddress.GetAddress()).IsNull();
            Assert.Equal(processAddress.GetPort(), 0);
            AssertThat(processAddress).HasToString("testingProcess");
            var processAddressSecure = processAddress.ToSecure();
            Assert.Equal(processAddressSecure.GetName(), "testingProcess");
            AssertThat(processAddressSecure.GetAddress()).IsNull();
            Assert.Equal(processAddressSecure.GetPort(), 0);
            AssertThat(processAddressSecure).HasToString("testingProcess");
            var processAddressInsecure = processAddressSecure.ToInsecure();
            Assert.Equal(processAddressInsecure.GetName(), "testingProcess");
            AssertThat(processAddressInsecure.GetAddress()).IsNull();
            Assert.Equal(processAddressInsecure.GetPort(), 0);
            AssertThat(processAddressInsecure).HasToString("testingProcess");
            var mirrorNodeAddress = BaseNodeAddress.FromString("mainnet-public.mirrornode.hedera.com:443");
            AssertThat(mirrorNodeAddress.GetName()).IsNull();
            Assert.Equal(mirrorNodeAddress.GetAddress(), "mainnet-public.mirrornode.hedera.com");
            Assert.Equal(mirrorNodeAddress.GetPort(), PORT_MIRROR_TLS);
            AssertThat(mirrorNodeAddress).HasToString("mainnet-public.mirrornode.hedera.com:443");
            var mirrorNodeAddressSecure = mirrorNodeAddress.ToSecure();
            AssertThat(mirrorNodeAddressSecure.GetName()).IsNull();
            Assert.Equal(mirrorNodeAddressSecure.GetAddress(), "mainnet-public.mirrornode.hedera.com");
            Assert.Equal(mirrorNodeAddressSecure.GetPort(), PORT_MIRROR_TLS);
            AssertThat(mirrorNodeAddressSecure).HasToString("mainnet-public.mirrornode.hedera.com:443");
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => BaseNodeAddress.FromString("this is a random string with spaces:443"));
            AssertThatExceptionOfType(typeof(InvalidOperationException)).IsThrownBy(() => BaseNodeAddress.FromString("mainnet-public.mirrornode.hedera.com:notarealport"));
        }
    }
}