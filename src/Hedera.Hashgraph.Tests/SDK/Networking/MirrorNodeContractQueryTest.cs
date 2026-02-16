// SPDX-License-Identifier: Apache-2.0
using Com.Google.Gson;
using Com.Google.Protobuf;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    class MirrorNodeContractQueryTest
    {
        private MirrorNodeContractEstimateGasQuery mirrorNodeContractEstimateGasQuery;
        private MirrorNodeContractCallQuery mirrorNodeContractCallQuery;
        private ContractId mockContractId;
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void SetUp()
        {
            mirrorNodeContractEstimateGasQuery = new MirrorNodeContractEstimateGasQuery();
            mirrorNodeContractCallQuery = new MirrorNodeContractCallQuery();
            mockContractId = Mockito.Mock(typeof(ContractId));
        }

        public virtual void TestSetAndGetContractId()
        {
            mirrorNodeContractEstimateGasQuery.ContractId = mockContractId;
            Assert.Equal(mockContractId, mirrorNodeContractEstimateGasQuery.ContractId);
            mirrorNodeContractCallQuery.ContractId = mockContractId;
            Assert.Equal(mockContractId, mirrorNodeContractCallQuery.ContractId);
        }

        public virtual void TestSetContractIdWithNullThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.ContractId = null);
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractCallQuery.ContractId = null);
        }

        public virtual void TestSetAndGetContractEvmAddress()
        {
            string evmAddress = "0x1234567890abcdef1234567890abcdef12345678";
            mirrorNodeContractEstimateGasQuery.ContractEvmAddress = evmAddress;
            Assert.Equal(evmAddress, mirrorNodeContractEstimateGasQuery.ContractEvmAddress);
            Assert.Null(mirrorNodeContractEstimateGasQuery.ContractId);
            mirrorNodeContractCallQuery.ContractEvmAddress = evmAddress;
            Assert.Equal(evmAddress, mirrorNodeContractCallQuery.ContractEvmAddress);
            Assert.Null(mirrorNodeContractCallQuery.ContractId);
        }

        public virtual async void TestSetContractEvmAddressWithNullThrowsException()
        {
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.ContractEvmAddress = null);
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractCallQuery.ContractEvmAddress = null);
        }

        public virtual void TestSetAndGetcallData()
        {
            ByteString @params = ByteString.CopyFromUtf8("test");
            mirrorNodeContractEstimateGasQuery.SetFunctionParameters(@params);
            Assert.Equal(@params.ToByteArray(), mirrorNodeContractEstimateGasQuery.CallData);
            mirrorNodeContractCallQuery.SetFunctionParameters(@params);
            Assert.Equal(@params.ToByteArray(), mirrorNodeContractCallQuery.CallData);
        }

        public virtual void TestSetFunctionWithoutParameters()
        {
            mirrorNodeContractEstimateGasQuery.Function = ",ZyFunction");
            Assert.NotNull(mirrorNodeContractEstimateGasQuery.CallData);
        }

        public virtual void TestSetAndGetBlockNumber()
        {
            long blockNumber = 123456;
            mirrorNodeContractEstimateGasQuery.BlockNumber = blockNumber;
            Assert.Equal(blockNumber, mirrorNodeContractEstimateGasQuery.BlockNumber);
            mirrorNodeContractCallQuery.BlockNumber = blockNumber;
            Assert.Equal(blockNumber, mirrorNodeContractCallQuery.BlockNumber);
        }

        public virtual void TestSetAndGetValue()
        {
            long value = 1000;
            mirrorNodeContractEstimateGasQuery.Value = value;
            Assert.Equal(value, mirrorNodeContractEstimateGasQuery.Value);
            mirrorNodeContractCallQuery.Value = value;
            Assert.Equal(value, mirrorNodeContractCallQuery.Value);
        }

        public virtual void TestSetAndGetGas()
        {
            long gas = 50000;
            mirrorNodeContractEstimateGasQuery.GasLimit = gas;
            Assert.Equal(gas, mirrorNodeContractEstimateGasQuery.GasLimit);
            mirrorNodeContractCallQuery.GasLimit = gas;
            Assert.Equal(gas, mirrorNodeContractCallQuery.GasLimit);
        }

        public virtual void TestSetAndGetGasPrice()
        {
            long gasPrice = 200;
            mirrorNodeContractEstimateGasQuery.GasPrice = gasPrice;
            Assert.Equal(gasPrice, mirrorNodeContractEstimateGasQuery.GasPrice);
            mirrorNodeContractCallQuery.GasPrice = gasPrice;
            Assert.Equal(gasPrice, mirrorNodeContractCallQuery.GasPrice);
        }

        public virtual void TestEstimateGasWithMissingContractIdOrEvmAddressThrowsException()
        {
            ByteString @params = ByteString.CopyFromUtf8("gasParams");
            mirrorNodeContractEstimateGasQuery.SetFunctionParameters(@params);
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.Estimate(null));
            mirrorNodeContractCallQuery.SetFunctionParameters(@params);
            Assert.Throws<NullReferenceException>(() => mirrorNodeContractCallQuery.Estimate(null));
        }

        public virtual void TestCreateJsonPayloadAllFieldsSet()
        {
			byte[] data = Encoding.UTF8.GetBytes("testData");
			string senderAddress = "0x1234567890abcdef1234567890abcdef12345678";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 50000;
            long gasPrice = 2000;
            long value = 1000;
            string blockNumber = "latest";
            bool estimate = true;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.Add("data", "7465737444617461");
            expectedJson.Add("to", contractAddress);
            expectedJson.Add("estimate", estimate);
            expectedJson.Add("blockNumber", blockNumber);
            expectedJson.Add("from", senderAddress);
            expectedJson.Add("gas", gas);
            expectedJson.Add("gasPrice", gasPrice);
            expectedJson.Add("value", value);
            Assert.Equal(expectedJson.ToString(), jsonPayload);
        }

        public virtual void TestCreateJsonPayloadOnlyRequiredFieldsSet()
        {
			byte[] data = Encoding.UTF8.GetBytes("testData");
			string senderAddress = "";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 0;
            long gasPrice = 0;
            long value = 0;
            string blockNumber = "latest";
            bool estimate = true;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.Add("data", "7465737444617461");
            expectedJson.Add("to", contractAddress);
            expectedJson.Add("estimate", estimate);
            expectedJson.Add("blockNumber", blockNumber);
            Assert.Equal(expectedJson.ToString(), jsonPayload);
        }

        public virtual void TestCreateJsonPayloadSomeOptionalFieldsSet()
        {
			byte[] data = Encoding.UTF8.GetBytes("testData");
			string senderAddress = "0x1234567890abcdef1234567890abcdef12345678";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 50000;
            long gasPrice = 0;
            long value = 1000;
            string blockNumber = "latest";
            bool estimate = false;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.Add("data", "7465737444617461");
            expectedJson.Add("to", contractAddress);
            expectedJson.Add("estimate", estimate);
            expectedJson.Add("blockNumber", blockNumber);
            expectedJson.Add("from", senderAddress);
            expectedJson.Add("gas", gas);
            expectedJson.Add("value", value);
            Assert.Equal(expectedJson.ToString(), jsonPayload);
        }

        public virtual void TestCreateJsonPayloadAllOptionalFieldsDefault()
        {
            byte[] data = Encoding.UTF8.GetBytes("testData");
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            string senderAddress = "";
            long gas = 0;
            long gasPrice = 0;
            long value = 0;
            string blockNumber = "latest";
            bool estimate = false;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.Add("data", "7465737444617461");
            expectedJson.Add("to", contractAddress);
            expectedJson.Add("estimate", estimate);
            expectedJson.Add("blockNumber", blockNumber);
            Assert.Equal(expectedJson.ToString(), jsonPayload);
        }

        public virtual void TestParseHexEstimateToLong()
        {
            string responseBody = "{\"result\": \"0x1234\"}";
            long parsedResult = MirrorNodeContractQuery.ParseHexEstimateToLong(responseBody);
            Assert.Equal(0x1234, parsedResult);
        }

        public virtual void TestParseContractCallResult()
        {
            string responseBody = "{\"result\": \"0x1234abcdef\"}";
            string parsedResult = MirrorNodeContractQuery.ParseContractCallResult(responseBody);
            Assert.Equal("0x1234abcdef", parsedResult);
        }

        public virtual void ShouldSerialize()
        {
            ContractId testContractId = new ContractId(0, 0, 1234);
            string testEvmAddress = "0x1234567890abcdef1234567890abcdef12345678";
            AccountId testSenderId = new AccountId(0, 0, 5678);
            string testSenderEvmAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            ByteString testCallData = ByteString.CopyFromUtf8("testData");
            string testFunctionName = "myFunction";
            ContractFunctionParameters testParams = new ContractFunctionParameters().AddString("param1");
            long testValue = 1000;
            long testGasLimit = 500000;
            long testGasPrice = 20;
            long testBlockNumber = 123456;
            var mirrorNodeContractEstimateGasQuery = new MirrorNodeContractEstimateGasQuery
            {
				ContractId = testContractId,
				ContractEvmAddress = testEvmAddress,
				Sender = testSenderId,
				SenderEvmAddress = testSenderEvmAddress,
				Value = testValue,
				GasLimit = testGasLimit,
                GasPrice = testGasPrice,
				BlockNumber = testBlockNumber
			}
			.SetFunction(testFunctionName, testParams)
			.SetFunctionParameters(testCallData);
			var mirrorNodeContractCallQuery = new MirrorNodeContractCallQuery
            {
				ContractId = testContractId,
				ContractEvmAddress = testEvmAddress,
				Sender = testSenderId,
				SenderEvmAddress = testSenderEvmAddress,
				Value = testValue,
				GasLimit = testGasLimit,
				GasPrice = testGasPrice,
				BlockNumber = testBlockNumber
			}
			.SetFunction(testFunctionName, testParams)
			.SetFunctionParameters(testCallData);
            
            SnapshotMatcher.Expect(mirrorNodeContractEstimateGasQuery.ToString() + mirrorNodeContractCallQuery.ToString()).ToMatchSnapshot();
        }
    }
}