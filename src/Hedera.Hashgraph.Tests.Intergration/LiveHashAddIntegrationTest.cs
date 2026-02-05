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
        virtual void CannotCreateLiveHashBecauseItsNotSupported()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction().SetKeyWithoutAlias(key).SetInitialBalance(new Hbar(1)).Execute(testEnv.client);
                var accountId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).accountId);
                AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                {
                    new LiveHashAddTransaction().SetAccountId(accountId).SetDuration(Duration.OfDays(30)).SetHash(HASH).SetKeys(key).Execute(testEnv.client).GetReceipt(testEnv.client);
                }).WithMessageContaining(Status.NOT_SUPPORTED.ToString());
            }
        }
    }
}