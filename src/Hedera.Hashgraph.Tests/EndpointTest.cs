// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class EndpointTest
    {
        virtual void ValidateAllowsOnlyIp()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        virtual void ValidateAllowsOnlyDomain()
        {
            var ep = new Endpoint().SetDomainName("node1.test.local").SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }

        virtual void ValidateThrowsOnIpAndDomain()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetDomainName("node1.test.local").SetPort(50211);
            await Assert.ThrowsAsync<ArgumentException>(() => Endpoint.ValidateNoIpAndDomain(ep));
        }

        virtual void ValidateNoOpOnNull()
        {
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(null)).DoesNotThrowAnyException();
        }

        virtual void ValidateAllowsEmptyDomainWithIp()
        {
            var ep = new Endpoint().SetAddress(new byte[] { 10, 0, 0, 1 }).SetDomainName("").SetPort(50211);
            AssertThatCode(() => Endpoint.ValidateNoIpAndDomain(ep)).DoesNotThrowAnyException();
        }
    }
}