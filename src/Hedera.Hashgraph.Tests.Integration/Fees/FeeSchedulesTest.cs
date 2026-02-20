// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.File;
using Hedera.Hashgraph.SDK.Fees;

using Google.Protobuf;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FeeSchedulesTest
    {
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