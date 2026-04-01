// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Contract;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractInfoQueryTest
    {
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Query();
            new ContractInfoQuery
            {
				ContractId = ContractId.FromString("0.0.5005")

			}.OnMakeRequest(builder, new Proto.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}