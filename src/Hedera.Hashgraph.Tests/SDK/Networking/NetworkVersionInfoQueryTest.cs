// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Networking;

using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NetworkVersionInfoQueryTest
    {
        [Fact]
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Query();
            new NetworkVersionInfoQuery
            {
				MaxQueryPayment = Hbar.FromTinybars(100000)

			}.OnMakeRequest(builder, new Proto.QueryHeader
            {
                Payment = new Proto.Transaction
                {
                    SignedTransactionBytes = ByteString.CopyFromUtf8("deadbeef")
                }
            });

            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}