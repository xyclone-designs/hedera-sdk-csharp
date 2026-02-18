// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Networking;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NetworkVersionInfoIntegrationTest
    {
        public virtual void CannotQueryNetworkVersionInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new NetworkVersionInfoQuery().Execute(testEnv.Client);
            }
        }
    }
}