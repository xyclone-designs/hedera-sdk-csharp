// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class EndpointTest
    {
        public virtual void ValidateAllowsOnlyIp()
        {
            var ep = new Endpoint
            {
				Address = new byte[] { 127, 0, 0, 1 },
				Port = 50211
			};
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        public virtual void ValidateAllowsOnlyDomain()
        {
            var ep = new Endpoint
            {
				DomainName = "node1.test.local",
				Port = 50211,
			};
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        public virtual void ValidateThrowsOnIpAndDomain()
        {
            var ep = new Endpoint
            {
				Address = new byte[] { 127, 0, 0, 1 },
				DomainName = "node1.test.local",
				Port = 50211
			};
            Assert.Throws<ArgumentException>(() => Endpoint.ValidateNoIpAndDomain(ep));
        }

        public virtual void ValidateNoOpOnNull()
        {
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(null)).DoesNotThrowAnyException();
        }

        public virtual void ValidateAllowsEmptyDomainWithIp()
        {
            var ep = new Endpoint
            {
				Address = new byte[] { 10, 0, 0, 1 },
				DomainName = "",
				Port = 50211
			};
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }
    }
}