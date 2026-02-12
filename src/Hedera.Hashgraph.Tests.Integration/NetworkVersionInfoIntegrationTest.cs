// SPDX-License-Identifier: Apache-2.0
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NetworkVersionInfoIntegrationTest
    {
        virtual void CannotQueryNetworkVersionInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new NetworkVersionInfoQuery().Execute(testEnv.client);
            }
        }
    }
}