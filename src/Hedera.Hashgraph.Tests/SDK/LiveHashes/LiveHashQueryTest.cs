// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.LiveHashes;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.LiveHashes
{
    public class LiveHashQueryTest
    {
        private static readonly byte[] hash = [0, 1, 2];

        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Services.Query();
            new LiveHashQuery
            {
                Hash = hash,
				AccountId = AccountId.FromString("0.0.100"),

			}.OnMakeRequest(builder, new Proto.Services.QueryHeader());

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}