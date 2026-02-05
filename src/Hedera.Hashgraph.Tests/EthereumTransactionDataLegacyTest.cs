// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class EthereumTransactionDataLegacyTest
    {
        // https://github.com/hashgraph/hedera-services/blob/1e01d9c6b8923639b41359c55413640b589c4ec7/hapi-utils/src/test/java/com/hedera/services/ethereum/EthTxDataTest.java#L49
        static readonly string RAW_TX_TYPE_0 = "f864012f83018000947e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc18180827653820277a0f9fbff985d374be4a55f296915002eec11ac96f1ce2df183adf992baa9390b2fa00c1e867cc960d9c74ec2e6a662b7908ec4c8cc9f3091e886bcefbeb2290fb792";
        static readonly string RAW_TX_TYPE_0_TRIMMED_LAST_BYTES = "f864012f83018000947e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc18180827653820277a0f9fbff985d374be4a55f296915002eec11ac96f1ce2df183adf992baa9390b2fa00c1e867cc960d9c74ec2e6a662b7908ec4c8cc9f3091e886bcefbeb2290000";
        static readonly string RAW_TX_TYPE_2 = "02f87082012a022f2f83018000947e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc181880de0b6b3a764000083123456c001a0df48f2efd10421811de2bfb125ab75b2d3c44139c4642837fb1fccce911fd479a01aaf7ae92bee896651dfc9d99ae422a296bf5d9f1ca49b2d96d82b79eb112d66";
        public virtual void LegacyToFromBytes()
        {
            var data = (EthereumTransactionDataLegacy)EthereumTransactionData.FromBytes(Hex.Decode(RAW_TX_TYPE_0));
            Assert.Equal(RAW_TX_TYPE_0, Hex.ToHexString(data.ToBytes()));

            // Chain ID is not part of the legacy ethereum transaction, so why are you calculating and checking it?
            // assertEquals("012a", Hex.toHexString(data.chainId()));
            Assert.Equal(Hex.ToHexString(data.nonce), "01");
            Assert.Equal(Hex.ToHexString(data.gasPrice), "2f");
            Assert.Equal(Hex.ToHexString(data.gasLimit), "018000");
            Assert.Equal(Hex.ToHexString(data.to), "7e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc181");
            Assert.Equal(Hex.ToHexString(data.value), "");
            Assert.Equal(Hex.ToHexString(data.callData), "7653");
            Assert.Equal(Hex.ToHexString(data.v), "0277");
            Assert.Equal(Hex.ToHexString(data.r), "f9fbff985d374be4a55f296915002eec11ac96f1ce2df183adf992baa9390b2f");
            Assert.Equal(Hex.ToHexString(data.s), "0c1e867cc960d9c74ec2e6a662b7908ec4c8cc9f3091e886bcefbeb2290fb792"); // We don't currently support a way to get the ethereum has, but we probably should
            // assertEquals("9ffbd69c44cf643ed8d1e756b505e545e3b5dd3a6b5ef9da1d8eca6679706594",
            //    Hex.toHexString(data.getEthereumHash()));
        }

        public virtual void Eip1559ToFromBytes()
        {
            var data = (EthereumTransactionDataEip1559)EthereumTransactionData.FromBytes(Hex.Decode(RAW_TX_TYPE_2));
            Assert.Equal(RAW_TX_TYPE_2, Hex.ToHexString(data.ToBytes()));
            Assert.Equal(Hex.ToHexString(data.chainId), "012a");
            Assert.Equal(Hex.ToHexString(data.nonce), "02");
            Assert.Equal(Hex.ToHexString(data.maxPriorityGas), "2f");
            Assert.Equal(Hex.ToHexString(data.maxGas), "2f");
            Assert.Equal(Hex.ToHexString(data.gasLimit), "018000");
            Assert.Equal(Hex.ToHexString(data.to), "7e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc181");
            Assert.Equal(Hex.ToHexString(data.value), "0de0b6b3a7640000");
            Assert.Equal(Hex.ToHexString(data.callData), "123456");
            Assert.Equal(Hex.ToHexString(data.accessList), "");
            Assert.Equal(Hex.ToHexString(data.recoveryId), "01");
            Assert.Equal(Hex.ToHexString(data.r), "df48f2efd10421811de2bfb125ab75b2d3c44139c4642837fb1fccce911fd479");
            Assert.Equal(Hex.ToHexString(data.s), "1aaf7ae92bee896651dfc9d99ae422a296bf5d9f1ca49b2d96d82b79eb112d66");
        }
    }
}