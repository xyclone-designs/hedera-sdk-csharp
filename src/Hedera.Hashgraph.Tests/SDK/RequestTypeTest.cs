// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Java.Util;
using Java.Util.Stream;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK
{
    class RequestTypeTest
    {
        public virtual void ValueOf()
        {
            var codeValues = Proto.HederaFunctionality.Values();
            var requestTypeValues = RequestType.Values();
            var pair = IntStream.Range(0, codeValues.Length - 1).MapToObj((i) => Map.Entry(codeValues[i], requestTypeValues[i])).Collect(Collectors.ToList());
            pair.ForEach((a) =>
            {
                var code = a.GetKey();
                var requestType = a.GetValue();
                Assert.Equal(RequestType.ValueOf(code).ToString(), requestType.ToString());
            });
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
            var pairs = new object[]
            {
                new[]
                {
                    Proto.HederaFunctionality.AtomicBatch,
                    RequestType.AtomicBatch
                },
                new[]
                {
                    Proto.HederaFunctionality.LambdaSstore,
                    RequestType.LambdaSstore
                },
                new[]
                {
                    Proto.HederaFunctionality.HookDispatch,
                    RequestType.HookDispatch
				}
            };

            foreach (var pair in pairs)
            {
                var code = (Proto.HederaFunctionality)pair[0];
                var req = (RequestType)pair[1];

                Assert.Equal((RequestType)code, req);
                Assert.Equal((Proto.Hede)req, code);
            }
        }
    }
}