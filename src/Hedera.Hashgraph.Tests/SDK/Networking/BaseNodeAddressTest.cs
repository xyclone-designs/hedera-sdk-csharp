// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Networking;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class BaseNodeAddressTest
    {
        public virtual void FromString()
        {
            var ipAddress = BaseNodeAddress.FromString("35.237.200.180:50211");
            Assert.Null(ipAddress.Name);
            Assert.Equal(ipAddress.Address, "35.237.200.180");
            Assert.Equal(ipAddress.Port, PORT_NODE_PLAIN);
            Assert.Equal(ipAddress.ToString(), "35.237.200.180:50211");
            var ipAddressSecure = ipAddress.ToSecure();
            Assert.Null(ipAddressSecure.Name);
            Assert.Equal(ipAddressSecure.Address, "35.237.200.180");
            Assert.Equal(ipAddressSecure.Port, PORT_NODE_TLS);
            Assert.Equal(ipAddressSecure.ToString(), "35.237.200.180:50212");
            var ipAddressInsecure = ipAddressSecure.ToInsecure();
            Assert.Null(ipAddressInsecure.Name);
            Assert.Equal(ipAddressInsecure.Address, "35.237.200.180");
            Assert.Equal(ipAddressInsecure.Port, PORT_NODE_PLAIN);
            Assert.Equal(ipAddressInsecure.ToString(), "35.237.200.180:50211");
            var urlAddress = BaseNodeAddress.FromString("0.testnet.hedera.com:50211");
            Assert.Null(urlAddress.Name);
            Assert.Equal(urlAddress.Address, "0.testnet.hedera.com");
            Assert.Equal(urlAddress.Port, PORT_NODE_PLAIN);
            Assert.Equal(urlAddress.ToString(), "0.testnet.hedera.com:50211");
            var urlAddressSecure = urlAddress.ToSecure();
            Assert.Null(urlAddressSecure.Name);
            Assert.Equal(urlAddressSecure.Address, "0.testnet.hedera.com");
            Assert.Equal(urlAddressSecure.Port, PORT_NODE_TLS);
            Assert.Equal(urlAddressSecure.ToString(), "0.testnet.hedera.com:50212");
            var urlAddressInsecure = urlAddressSecure.ToInsecure();
            Assert.Null(urlAddressInsecure.Name);
            Assert.Equal(urlAddressInsecure.Address, "0.testnet.hedera.com");
            Assert.Equal(urlAddressInsecure.Port, PORT_NODE_PLAIN);
            Assert.Equal(urlAddressInsecure.ToString(), "0.testnet.hedera.com:50211");
            var processAddress = BaseNodeAddress.FromString("in-process:testingProcess");
            Assert.Equal(processAddress.Name, "testingProcess");
            Assert.Null(processAddress.Address);
            Assert.Equal(processAddress.Port, 0);
            Assert.Equal(processAddress.ToString(), "testingProcess");
            var processAddressSecure = processAddress.ToSecure();
            Assert.Equal(processAddressSecure.Name, "testingProcess");
            Assert.Null(processAddressSecure.Address);
            Assert.Equal(processAddressSecure.Port, 0);
            Assert.Equal(processAddressSecure.ToString(), "testingProcess");
            var processAddressInsecure = processAddressSecure.ToInsecure();
            Assert.Equal(processAddressInsecure.Name, "testingProcess");
            Assert.Null(processAddressInsecure.Address);
            Assert.Equal(processAddressInsecure.Port, 0);
            Assert.Equal(processAddressInsecure.ToString(), "testingProcess");
            var mirrorNodeAddress = BaseNodeAddress.FromString("mainnet-public.mirrornode.hedera.com:443");
            Assert.Null(mirrorNodeAddress.Name);
            Assert.Equal(mirrorNodeAddress.Address, "mainnet-public.mirrornode.hedera.com");
            Assert.Equal(mirrorNodeAddress.Port, PORT_MIRROR_TLS);
            Assert.Equal(mirrorNodeAddress.ToString(), "mainnet-public.mirrornode.hedera.com:443");
            var mirrorNodeAddressSecure = mirrorNodeAddress.ToSecure();
            Assert.Null(mirrorNodeAddressSecure.Name);
            Assert.Equal(mirrorNodeAddressSecure.Address, "mainnet-public.mirrornode.hedera.com");
            Assert.Equal(mirrorNodeAddressSecure.Port, PORT_MIRROR_TLS);
            Assert.Equal(mirrorNodeAddressSecure.ToString(), "mainnet-public.mirrornode.hedera.com:443");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => BaseNodeAddress.FromString("this is a random string with spaces:443"));
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => BaseNodeAddress.FromString("mainnet-public.mirrornode.hedera.com:notarealport"));
        }
    }
}