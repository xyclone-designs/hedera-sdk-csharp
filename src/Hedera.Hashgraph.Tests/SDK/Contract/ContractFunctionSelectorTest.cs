// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractFunctionSelectorTest
    {
        public virtual void Selector()
        {
            var signature = new ContractFunctionSelector("testFunction").AddAddress().AddAddressArray().AddBool().AddBytes().AddBytes32().AddBytes32Array().AddBytesArray().AddFunction().AddInt8().AddInt8Array().AddInt32().AddInt32Array().AddInt64().AddInt64Array().AddInt256().AddInt256Array().AddUint8().AddUint8Array().AddUint32().AddUint32Array().AddUint64().AddUint64Array().AddUint256().AddUint256Array().AddString().AddStringArray().Finish();
            Assert.Equal(Hex.ToHexString(signature), "4438e4ce");
        }

        public virtual void SelectorError()
        {
            var signature = new ContractFunctionSelector("testFunction").AddAddress();
            signature.Finish();
            Assert.Throws(typeof(InvalidOperationException), signature.AddStringArray());
            AssertThatNoException(, signature.Finish());
        }
    }
}