// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;

using System;
using System.Text.RegularExpressions;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountBalanceQueryTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerializeWithAccountId()
        {
            var builder = new Proto.Query();
            new AccountBalanceQuery
            {
                AccountId = AccountId.FromString("0.0.5005")
            
            }.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
        public virtual void ShouldSerializeWithContractId()
        {
            var builder = new Proto.Query();
            new AccountBalanceQuery
            {
                ContractId = ContractId.FromString("0.0.5005")
            
            }.OnMakeRequest(builder, new Proto.QueryHeader());

            SnapshotMatcher.Expect(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}