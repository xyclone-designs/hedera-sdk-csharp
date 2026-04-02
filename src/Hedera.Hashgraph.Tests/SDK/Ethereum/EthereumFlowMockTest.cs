// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Google.Protobuf;

using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.Tests.SDK.Ethereum
{
    public class EthereumFlowMockTest
    {
        static ByteString ETHEREUM_DATA = ByteString.CopyFrom(
            Convert.FromHexString("f864012f83018000947e3a9eaf9bcc39e2ffa38eb30bf7a93feacbc18180827653820277a0f9fbff985d374be4a55f296915002eec11ac96f1ce2df183adf992baa9390b2fa00c1e867cc960d9c74ec2e6a662b7908ec4c8cc9f3091e886bcefbeb2290fb792")
        );
        static ByteString LONG_CALL_DATA = ByteString.CopyFrom(
            Convert.FromHexString(new string('0', 5121 * 2))
        );
        [Fact]
        public virtual void DontTruncateEthereumDataUnnecessarily()
        {
            IList<object> responses1 =
            [
                (Func<object, object>)
                (_ =>
                {
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(((Proto.Transaction)_).SignedTransactionBytes);
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
                    
                    Assert.Equal(transactionBody.DataCase, Proto.TransactionBody.DataOneofCase.EthereumTransaction);
                    Assert.True(transactionBody.EthereumTransaction is not null);
                    Assert.Equal(transactionBody.EthereumTransaction.EthereumData, ETHEREUM_DATA);
                
                    return new Proto.TransactionResponse
                    {
                        NodeTransactionPrecheckCode = 0
                    };
                }),
                new Proto.Response
                {
                    TransactionGetReceipt = new Proto.TransactionGetReceiptResponse
                    {
                        Receipt = new Proto.TransactionReceipt
                        {
                            Status = Proto.ResponseCodeEnum.Success
                        }
                    }
                }
            ];

            using (var mocker = Mocker.WithResponses([responses1]))
            {
                new EthereumFlow
                {
                    EthereumData = EthereumTransactionData.FromBytes(ETHEREUM_DATA.ToByteArray())

                }.Execute(mocker.client).GetReceipt(mocker.client);
            }
        }
        [Theory]
        [InlineData("")]
        public virtual void ExtractsCallData(string versionToTest)
        {
            IList<object> responses =
            [
                (Func<object, object>)
                (_ =>
                {
                    var signedTransaction = Proto.SignedTransaction.Parser.ParseFrom(((Proto.Transaction)_).SignedTransactionBytes);
                    var transactionBody = Proto.TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);

                    Assert.Equal(transactionBody.DataCase, Proto.TransactionBody.DataOneofCase.EthereumTransaction);
                    Assert.True(transactionBody.EthereumTransaction is not null);
                    Assert.Equal(EthereumTransactionData.FromBytes(transactionBody.EthereumTransaction.EthereumData.ToByteArray()).CallData, LONG_CALL_DATA.ToByteArray());

                    return new Proto.TransactionResponse
                    {
                        NodeTransactionPrecheckCode = 0
                    };
                }),
                new Proto.Response
                {
                    TransactionGetReceipt = new Proto.TransactionGetReceiptResponse
                    {
                        Receipt = new Proto.TransactionReceipt
                        {
                            Status = Proto.ResponseCodeEnum.Success
                        }
                    }
                }
            ];
            
            EthereumFlow ethereumFlow;

            using (var mocker = Mocker.WithResponses([responses]))
            {
                var ethereumData = EthereumTransactionData.FromBytes(LONG_CALL_DATA.ToByteArray());

                if (versionToTest.Equals("sync"))
                {
                    ethereumFlow = new EthereumFlow
                    {
                        MaxGasAllowance = Hbar.FromTinybars(25),
                        EthereumData = EthereumTransactionData.FromBytes(ethereumData.ToBytes())
                    };

                    ethereumFlow.Execute(mocker.client).GetReceipt(mocker.client);
                }
                else
                {
                    ethereumFlow = new EthereumFlow
                    {
                        MaxGasAllowance = Hbar.FromTinybars(25), 
                        EthereumData = EthereumTransactionData.FromBytes(ethereumData.ToBytes())
                    };

                    ethereumFlow
                        .ExecuteAsync(mocker.client)
                        .ContinueWith(async (response) => (await response).GetReceiptAsync(mocker.client))
                        .GetAwaiter()
                        .GetResult();
                }

                Assert.NotNull(ethereumFlow.EthereumData);
                Assert.Equal(ethereumFlow.MaxGasAllowance.ToTinybars(), 25);
            }
        }
    }
}