// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Gson;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using Org.Mockito;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
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

        virtual void SetUp()
        {
            mirrorNodeContractEstimateGasQuery = new MirrorNodeContractEstimateGasQuery();
            mirrorNodeContractCallQuery = new MirrorNodeContractCallQuery();
            mockContractId = Mockito.Mock(typeof(ContractId));
        }

        virtual void TestSetAndGetContractId()
        {
            mirrorNodeContractEstimateGasQuery.SetContractId(mockContractId);
            AssertEquals(mockContractId, mirrorNodeContractEstimateGasQuery.GetContractId());
            mirrorNodeContractCallQuery.SetContractId(mockContractId);
            AssertEquals(mockContractId, mirrorNodeContractCallQuery.GetContractId());
        }

        virtual void TestSetContractIdWithNullThrowsException()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.SetContractId(null));
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractCallQuery.SetContractId(null));
        }

        virtual void TestSetAndGetContractEvmAddress()
        {
            string evmAddress = "0x1234567890abcdef1234567890abcdef12345678";
            mirrorNodeContractEstimateGasQuery.SetContractEvmAddress(evmAddress);
            AssertEquals(evmAddress, mirrorNodeContractEstimateGasQuery.GetContractEvmAddress());
            Assert.Null(mirrorNodeContractEstimateGasQuery.GetContractId());
            mirrorNodeContractCallQuery.SetContractEvmAddress(evmAddress);
            AssertEquals(evmAddress, mirrorNodeContractCallQuery.GetContractEvmAddress());
            Assert.Null(mirrorNodeContractCallQuery.GetContractId());
        }

        virtual void TestSetContractEvmAddressWithNullThrowsException()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.SetContractEvmAddress(null));
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractCallQuery.SetContractEvmAddress(null));
        }

        virtual void TestSetAndGetcallData()
        {
            ByteString params = ByteString.CopyFromUtf8("test");
            mirrorNodeContractEstimateGasQuery.SetFunctionParameters(@params);
            AssertArrayEquals(@params.ToByteArray(), mirrorNodeContractEstimateGasQuery.GetCallData());
            mirrorNodeContractCallQuery.SetFunctionParameters(@params);
            AssertArrayEquals(@params.ToByteArray(), mirrorNodeContractCallQuery.GetCallData());
        }

        virtual void TestSetFunctionWithoutParameters()
        {
            mirrorNodeContractEstimateGasQuery.SetFunction("myFunction");
            AssertNotNull(mirrorNodeContractEstimateGasQuery.GetCallData());
        }

        virtual void TestSetAndGetBlockNumber()
        {
            long blockNumber = 123456;
            mirrorNodeContractEstimateGasQuery.SetBlockNumber(blockNumber);
            AssertEquals(blockNumber, mirrorNodeContractEstimateGasQuery.GetBlockNumber());
            mirrorNodeContractCallQuery.SetBlockNumber(blockNumber);
            AssertEquals(blockNumber, mirrorNodeContractCallQuery.GetBlockNumber());
        }

        virtual void TestSetAndGetValue()
        {
            long value = 1000;
            mirrorNodeContractEstimateGasQuery.SetValue(value);
            AssertEquals(value, mirrorNodeContractEstimateGasQuery.GetValue());
            mirrorNodeContractCallQuery.SetValue(value);
            AssertEquals(value, mirrorNodeContractCallQuery.GetValue());
        }

        virtual void TestSetAndGetGas()
        {
            long gas = 50000;
            mirrorNodeContractEstimateGasQuery.SetGasLimit(gas);
            AssertEquals(gas, mirrorNodeContractEstimateGasQuery.GetGasLimit());
            mirrorNodeContractCallQuery.SetGasLimit(gas);
            AssertEquals(gas, mirrorNodeContractCallQuery.GetGasLimit());
        }

        virtual void TestSetAndGetGasPrice()
        {
            long gasPrice = 200;
            mirrorNodeContractEstimateGasQuery.SetGasPrice(gasPrice);
            AssertEquals(gasPrice, mirrorNodeContractEstimateGasQuery.GetGasPrice());
            mirrorNodeContractCallQuery.SetGasPrice(gasPrice);
            AssertEquals(gasPrice, mirrorNodeContractCallQuery.GetGasPrice());
        }

        virtual void TestEstimateGasWithMissingContractIdOrEvmAddressThrowsException()
        {
            ByteString params = ByteString.CopyFromUtf8("gasParams");
            mirrorNodeContractEstimateGasQuery.SetFunctionParameters(@params);
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractEstimateGasQuery.Estimate(null));
            mirrorNodeContractCallQuery.SetFunctionParameters(@params);
            await Assert.ThrowsAsync<NullReferenceException>(() => mirrorNodeContractCallQuery.Estimate(null));
        }

        virtual void TestCreateJsonPayloadAllFieldsSet()
        {
            byte[] data = "testData".GetBytes();
            string senderAddress = "0x1234567890abcdef1234567890abcdef12345678";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 50000;
            long gasPrice = 2000;
            long value = 1000;
            string blockNumber = "latest";
            bool estimate = true;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.AddProperty("data", "7465737444617461");
            expectedJson.AddProperty("to", contractAddress);
            expectedJson.AddProperty("estimate", estimate);
            expectedJson.AddProperty("blockNumber", blockNumber);
            expectedJson.AddProperty("from", senderAddress);
            expectedJson.AddProperty("gas", gas);
            expectedJson.AddProperty("gasPrice", gasPrice);
            expectedJson.AddProperty("value", value);
            AssertEquals(expectedJson.ToString(), jsonPayload);
        }

        virtual void TestCreateJsonPayloadOnlyRequiredFieldsSet()
        {
            byte[] data = "testData".GetBytes();
            string senderAddress = "";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 0;
            long gasPrice = 0;
            long value = 0;
            string blockNumber = "latest";
            bool estimate = true;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.AddProperty("data", "7465737444617461");
            expectedJson.AddProperty("to", contractAddress);
            expectedJson.AddProperty("estimate", estimate);
            expectedJson.AddProperty("blockNumber", blockNumber);
            AssertEquals(expectedJson.ToString(), jsonPayload);
        }

        virtual void TestCreateJsonPayloadSomeOptionalFieldsSet()
        {
            byte[] data = "testData".GetBytes();
            string senderAddress = "0x1234567890abcdef1234567890abcdef12345678";
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            long gas = 50000;
            long gasPrice = 0;
            long value = 1000;
            string blockNumber = "latest";
            bool estimate = false;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.AddProperty("data", "7465737444617461");
            expectedJson.AddProperty("to", contractAddress);
            expectedJson.AddProperty("estimate", estimate);
            expectedJson.AddProperty("blockNumber", blockNumber);
            expectedJson.AddProperty("from", senderAddress);
            expectedJson.AddProperty("gas", gas);
            expectedJson.AddProperty("value", value);
            AssertEquals(expectedJson.ToString(), jsonPayload);
        }

        virtual void TestCreateJsonPayloadAllOptionalFieldsDefault()
        {
            byte[] data = "testData".GetBytes();
            string contractAddress = "0xabcdefabcdefabcdefabcdefabcdefabcdef";
            string senderAddress = "";
            long gas = 0;
            long gasPrice = 0;
            long value = 0;
            string blockNumber = "latest";
            bool estimate = false;
            string jsonPayload = MirrorNodeContractQuery.CreateJsonPayload(data, senderAddress, contractAddress, gas, gasPrice, value, blockNumber, estimate);
            JsonObject expectedJson = new JsonObject();
            expectedJson.AddProperty("data", "7465737444617461");
            expectedJson.AddProperty("to", contractAddress);
            expectedJson.AddProperty("estimate", estimate);
            expectedJson.AddProperty("blockNumber", blockNumber);
            AssertEquals(expectedJson.ToString(), jsonPayload);
        }

        virtual void TestParseHexEstimateToLong()
        {
            string responseBody = "{\"result\": \"0x1234\"}";
            long parsedResult = MirrorNodeContractQuery.ParseHexEstimateToLong(responseBody);
            AssertEquals(0x1234, parsedResult);
        }

        virtual void TestParseContractCallResult()
        {
            string responseBody = "{\"result\": \"0x1234abcdef\"}";
            string parsedResult = MirrorNodeContractQuery.ParseContractCallResult(responseBody);
            AssertEquals("0x1234abcdef", parsedResult);
        }

        virtual void ShouldSerialize()
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
            var mirrorNodeContractEstimateGasQuery = new MirrorNodeContractEstimateGasQuery().SetContractId(testContractId).SetContractEvmAddress(testEvmAddress).SetSender(testSenderId).SetSenderEvmAddress(testSenderEvmAddress).SetFunction(testFunctionName, testParams).SetFunctionParameters(testCallData).SetValue(testValue).SetGasLimit(testGasLimit).SetGasPrice(testGasPrice).SetBlockNumber(testBlockNumber);
            var mirrorNodeContractCallQuery = new MirrorNodeContractCallQuery().SetContractId(testContractId).SetContractEvmAddress(testEvmAddress).SetSender(testSenderId).SetSenderEvmAddress(testSenderEvmAddress).SetFunction(testFunctionName, testParams).SetFunctionParameters(testCallData).SetValue(testValue).SetGasLimit(testGasLimit).SetGasPrice(testGasPrice).SetBlockNumber(testBlockNumber);
            SnapshotMatcher.Expect(mirrorNodeContractEstimateGasQuery.ToString() + mirrorNodeContractCallQuery.ToString()).ToMatchSnapshot();
        }
    }
}