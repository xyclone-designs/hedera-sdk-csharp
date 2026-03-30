// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK
{
    class RequestTypeTest
    {
        public virtual void ValueOf()
        {
            var codeValues = Enum.GetValues<Proto.HederaFunctionality>();
            var requestTypeValues = Enum.GetValues<RequestType>();

            foreach (var pair in Enumerable
                .Range(0, codeValues.Length - 1)
                .Select(i => KeyValuePair.Create(codeValues[i], requestTypeValues[i])))
            {
                var code = pair.Key;
                var requestType = pair.Value;
                Assert.Equal(((RequestType)code).ToString(), requestType.ToString());
            }
        }

        public virtual void ValueOfMapsNewFunctions()
        {
            Assert.Equal((RequestType)Proto.HederaFunctionality.AtomicBatch, RequestType.AtomicBatch);
            Assert.Equal((RequestType)Proto.HederaFunctionality.LambdaSstore, RequestType.LambdaSstore);
            Assert.Equal((RequestType)Proto.HederaFunctionality.HookDispatch, RequestType.HookDispatch);
        }

        public virtual void ToStringStableForNewEntries()
        {
            Assert.Equal(RequestType.AtomicBatch.ToString(), "ATOMIC_BATCH");
            Assert.Equal(RequestType.LambdaSstore.ToString(), "LAMBDA_S_STORE");
            Assert.Equal(RequestType.HookDispatch.ToString(), "HOOK_DISPATCH");
        }

        public virtual void RoundTripNewEntries()
        {
            var pairs = new object[][]
            {
                [
                    Proto.HederaFunctionality.AtomicBatch,
                    RequestType.AtomicBatch
                ],
                [
                    Proto.HederaFunctionality.LambdaSstore,
                    RequestType.LambdaSstore
                ],
                [
                    Proto.HederaFunctionality.HookDispatch,
                    RequestType.HookDispatch
				]
            };

            foreach (var pair in pairs)
            {
                var code = (Proto.HederaFunctionality)pair[0];
                var req = (RequestType)pair[1];

                Assert.Equal((RequestType)code, req);
                Assert.Equal((Proto.HederaFunctionality)req, code);
            }
        }
    }
}