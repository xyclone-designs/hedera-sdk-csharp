// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Java.Util;
using Java.Util.Concurrent;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Ethereum
{
    class EthereumFlowMockTest
    {
        static ByteString ETHEREUM_DATA = ByteString.FromHex("f864012f83018000947e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc18180827653820277a0f9fbff985d374be4a55f296915002eec11ac96f1ce2df183adf992baa9390b2fa00c1e867cc960d9c74ec2e6a662b7908ec4c8cc9f3091e886bcefbeb2290fb792");
        static ByteString LONG_CALL_DATA = ByteString.FromHex("00".Repeat(5121));
        public virtual void DontTruncateEthereumDataUnnecessarily()
        {
            IList<object> responses1 = List.Of((Function<object, object>)(o) =>
            {
                var signedTransaction = SignedTransaction.ParseFrom(((Transaction)o).GetSignedTransactionBytes());
                var transactionBody = TransactionBody.ParseFrom(signedTransaction.GetBodyBytes());
                AssertThat(transactionBody.GetDataCase()).IsEqualByComparingTo(TransactionBody.DataCase.ETHEREUMTRANSACTION);
                AssertThat(transactionBody.HasEthereumTransaction()).IsTrue();
                Assert.Equal(transactionBody.GetEthereumTransaction().GetEthereumData(), ETHEREUM_DATA);
                return TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCodeValue(0).Build();
            }, Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatusValue(ResponseCodeEnum.SUCCESS_VALUE))).Build());
            var responses = List.Of(responses1);
            using (var mocker = Mocker.WithResponses(responses))
            {
                new EthereumFlow().SetEthereumData(ETHEREUM_DATA.ToByteArray()).Execute(mocker.client).GetReceipt(mocker.client);
            }
        }

        public virtual void ExtractsCallData(string versionToTest)
        {
            IList<object> responses1 = List.Of((Function<object, object>)(o) =>
            {
                var signedTransaction = SignedTransaction.ParseFrom(((Transaction)o).GetSignedTransactionBytes());
                var transactionBody = TransactionBody.ParseFrom(signedTransaction.GetBodyBytes());
                AssertThat(transactionBody.GetDataCase()).IsEqualByComparingTo(TransactionBody.DataCase.ETHEREUMTRANSACTION);
                AssertThat(transactionBody.HasEthereumTransaction()).IsTrue();
                Assert.Equal(EthereumTransactionData.FromBytes(transactionBody.GetEthereumTransaction().GetEthereumData().ToByteArray()).callData, LONG_CALL_DATA.ToByteArray());
                return TransactionResponse.NewBuilder().SetNodeTransactionPrecheckCodeValue(0).Build();
            }, Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatusValue(ResponseCodeEnum.SUCCESS_VALUE))).Build());
            var responses = List.Of(responses1);
            EthereumFlow ethereumFlow;
            using (var mocker = Mocker.WithResponses(responses))
            {
                var ethereumData = EthereumTransactionData.FromBytes(ETHEREUM_DATA.ToByteArray());
                ethereumData.callData = LONG_CALL_DATA.ToByteArray();
                if (versionToTest.Equals("sync"))
                {
                    ethereumFlow = new EthereumFlow().SetMaxGasAllowance(Hbar.FromTinybars(25)).SetEthereumData(ethereumData.ToBytes());
                    ethereumFlow.Execute(mocker.client).GetReceipt(mocker.client);
                }
                else
                {
                    ethereumFlow = new EthereumFlow().SetMaxGasAllowance(Hbar.FromTinybars(25)).SetEthereumData(ethereumData.ToBytes());
                    ethereumFlow.ExecuteAsync(mocker.client).ThenCompose((response) => response.GetReceiptAsync(mocker.client)).Get();
                }

                AssertThat(ethereumFlow.GetEthereumData()).IsNotNull();
                Assert.Equal(ethereumFlow.GetMaxGasAllowance().ToTinybars(), 25);
            }
        }
    }
}