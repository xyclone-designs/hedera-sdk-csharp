// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Time;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class LiveHashAddIntegrationTest
    {
        private static readonly byte[] HASH = Hex.Decode("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");
        public virtual void CannotCreateLiveHashBecauseItsNotSupported()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                Assert.Throws(typeof(PrecheckStatusException), () =>
                {
                    new LiveHashAddTransaction().SetAccountId(accountId).SetDuration(Duration.OfDays(30)).SetHash(HASH).SetKeys(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }
    }
}