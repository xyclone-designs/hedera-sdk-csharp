// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Account
{
    public class AccountBalanceQueryTest
    {
        public virtual void ShouldSerializeWithAccountId()
        {
            var builder = new Proto.Services.Query();
            new AccountBalanceQuery
            {
                AccountId = AccountId.FromString("0.0.5005")
            
            }.OnMakeRequest(builder, new Proto.Services.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
        public virtual void ShouldSerializeWithContractId()
        {
            var builder = new Proto.Services.Query();
            new AccountBalanceQuery
            {
                ContractId = ContractId.FromString("0.0.5005")
            
            }.OnMakeRequest(builder, new Proto.Services.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}