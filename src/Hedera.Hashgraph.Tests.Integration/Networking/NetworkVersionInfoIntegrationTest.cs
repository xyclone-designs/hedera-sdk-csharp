// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Networking;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    /// <include file="NetworkVersionInfoIntegrationTest.cs.xml" path="docs/member[@name="T:Hedera.Hashgraph.SDK.Tests.Integration.NetworkVersionInfoIntegrationTest"]" />
    public class NetworkVersionInfoIntegrationTest
    {
        [Fact]
        /// <include file="NetworkVersionInfoIntegrationTest.cs.xml" path="docs/member[@name="M:Hedera.Hashgraph.SDK.Tests.Integration.NetworkVersionInfoIntegrationTest.CannotQueryNetworkVersionInfo"]" />
        public virtual void CannotQueryNetworkVersionInfo()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                new NetworkVersionInfoQuery().Execute(testEnv.Client);
            }
        }
    }
}
