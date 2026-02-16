// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Token;

using System;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountInfoQueryTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Query();
            new AccountInfoQuery()
            {
				AccountId = AccountId.FromString("0.0.5005"),
				MaxQueryPayment = Hbar.FromTinybars(100000),
			
            }.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}