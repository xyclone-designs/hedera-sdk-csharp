// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Fee;

using Google.Protobuf;

namespace Hedera.Hashgraph.Tests.Integration
{
    /// <include file="FeeSchedulesTest.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.Integration.FeeSchedulesTest"]' />
    public class FeeSchedulesTest
    {
        [Fact]
        /// <include file="FeeSchedulesTest.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.Integration.FeeSchedulesTest.CanFetchFeeSchedules"]' />
        public virtual void CanFetchFeeSchedules()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                // note: is flaky in localnode env
                testEnv.AssumeNotLocalNode();
                
                ByteString feeSchedulesBytes = new FileContentsQuery { FileId = new FileId(0, 0, 111) }.Execute(testEnv.Client);
                FeeSchedules feeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes.ToByteArray());
                
                /*
                 * Test whether the file 0.0.111 actually contains stuff
                 */

                Assert.NotNull(feeSchedules.Current);
            }
        }
    }
}
