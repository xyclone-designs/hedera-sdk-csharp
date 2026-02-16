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

namespace Hedera.Hashgraph.Tests.SDK
{
    class RequestTypeTest
    {
        public virtual void ValueOf()
        {
            var codeValues = HederaFunctionality.Values();
            var requestTypeValues = RequestType.Values();
            var pair = IntStream.Range(0, codeValues.Length - 1).MapToObj((i) => Map.Entry(codeValues[i], requestTypeValues[i])).Collect(Collectors.ToList());
            pair.ForEach((a) =>
            {
                var code = a.GetKey();
                var requestType = a.GetValue();
                AssertThat(RequestType.ValueOf(code)).HasToString(requestType.ToString());
            });
        }

        public virtual void ValueOfMapsNewFunctions()
        {
            Assert.Equal(RequestType.ValueOf(HederaFunctionality.AtomicBatch), RequestType.ATOMIC_BATCH);
            Assert.Equal(RequestType.ValueOf(HederaFunctionality.LambdaSStore), RequestType.LAMBDA_S_STORE);
            Assert.Equal(RequestType.ValueOf(HederaFunctionality.HookDispatch), RequestType.HOOK_DISPATCH);
        }

        public virtual void ToStringStableForNewEntries()
        {
            Assert.Equal(RequestType.ATOMIC_BATCH.ToString(), "ATOMIC_BATCH");
            Assert.Equal(RequestType.LAMBDA_S_STORE.ToString(), "LAMBDA_S_STORE");
            Assert.Equal(RequestType.HOOK_DISPATCH.ToString(), "HOOK_DISPATCH");
        }

        public virtual void RoundTripNewEntries()
        {
            var pairs = new object[]
            {
                new[]
                {
                    HederaFunctionality.AtomicBatch,
                    RequestType.ATOMIC_BATCH
                },
                new[]
                {
                    HederaFunctionality.LambdaSStore,
                    RequestType.LAMBDA_S_STORE
                },
                new[]
                {
                    HederaFunctionality.HookDispatch,
                    RequestType.HOOK_DISPATCH
                }
            };
            foreach (var pair in pairs)
            {
                var code = (HederaFunctionality)pair[0];
                var req = (RequestType)pair[1];
                Assert.Equal(RequestType.ValueOf(code), req);
                Assert.Equal(req.code, code);
            }
        }
    }
}