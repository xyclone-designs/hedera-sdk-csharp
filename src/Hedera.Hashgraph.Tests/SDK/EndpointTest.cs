// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class EndpointTest
    {
        public virtual void ValidateAllowsOnlyIp()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        public virtual void ValidateAllowsOnlyDomain()
        {
            var ep = new Endpoint().SetDomainName("node1.test.local").SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        public virtual void ValidateThrowsOnIpAndDomain()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetDomainName("node1.test.local").SetPort(50211);
            Assert.Throws<ArgumentException>(() => Endpoint.ValidateNoIpAndDomain(ep));
        }

        public virtual void ValidateNoOpOnNull()
        {
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(null)).DoesNotThrowAnyException();
        }

        public virtual void ValidateAllowsEmptyDomainWithIp()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 10, 0, 0, 1 }).SetDomainName("").SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }
    }
}