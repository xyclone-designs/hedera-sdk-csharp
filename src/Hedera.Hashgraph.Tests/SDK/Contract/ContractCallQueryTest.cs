// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Keys;

using System;
using System.Text.RegularExpressions;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractCallQueryTest
    {
        private static readonly PrivateKey unusedPrivateKey = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private readonly DateTimeOffset validStart = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public virtual void ShouldSerialize()
        {
            var builder = new Proto.Query();
            new ContractCallQuery()
			{
				ContractId = ContractId.FromString("0.0.5005"),
				Gas = 1541,
				SenderAccountId = AccountId.FromString("1.2.3"),
			}
            .SetFunction("foo", new ContractFunctionParameters().AddString("Hello").AddString("world!"))
            .OnMakeRequest(builder, new Proto.QueryHeader());
            
            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
        public virtual void SetFunctionParameters()
        {
            var builder = new Proto.Query();
            new ContractCallQuery()
            {
				ContractId = ContractId.FromString("0.0.5005"),
				Gas = 1541,
				SenderAccountId = AccountId.FromString("1.2.3"),
				FunctionParameters = new ContractFunctionParameters().AddString("Hello").AddString("world!").ToBytes(null).ToByteArray()

			}.OnMakeRequest(builder, new Proto.QueryHeader());
            
            Verifier.Verify(Regex.Replace(builder.ToString(), "@[A-Za-z0-9]+", ""));
        }
    }
}