// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Cryptocurrency;

using System;
using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountRecordsQueryTest
    {
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Services.Query();
            new AccountRecordsQuery()
            {
				AccountId = AccountId.FromString("0.0.5005"),
				MaxQueryPayment = Hbar.FromTinybars(100000),
			
            }.OnMakeRequest(builder, new Proto.Services.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}