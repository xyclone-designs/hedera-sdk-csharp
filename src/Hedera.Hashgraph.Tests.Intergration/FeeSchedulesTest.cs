// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class FeeSchedulesTest
    {
        virtual void CanFetchFeeSchedules()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {

                // note: is flaky in localnode env
                testEnv.AssumeNotLocalNode();
                ByteString feeSchedulesBytes = new FileContentsQuery().SetFileId(new FileId(0, 0, 111)).Execute(testEnv.client);
                FeeSchedules feeSchedules = FeeSchedules.FromBytes(feeSchedulesBytes.ToByteArray());
                /*
             * Test whether the file 0.0.111 actually contains stuff
             */
                AssertThat(feeSchedules.GetCurrent()).IsNotNull();
            }
        }
    }
}