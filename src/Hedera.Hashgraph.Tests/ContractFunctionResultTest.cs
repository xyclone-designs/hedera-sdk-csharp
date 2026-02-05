// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Java.Math;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class ContractFunctionResultTest
    {
        static readonly string CALL_RESULT_HEX = "00000000000000000000000000000000000000000000000000000000ffffffff" + "7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff" + "00000000000000000000000011223344556677889900aabbccddeeff00112233" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff" + "00000000000000000000000000000000000000000000000000000000000000c0" + "0000000000000000000000000000000000000000000000000000000000000100" + "000000000000000000000000000000000000000000000000000000000000000d" + "48656c6c6f2c20776f726c642100000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000014" + "48656c6c6f2c20776f726c642c20616761696e21000000000000000000000000";
        private static readonly string STRING_ARRAY_RESULT_HEX = "0000000000000000000000000000000000000000000000000000000000000020" + "0000000000000000000000000000000000000000000000000000000000000002" + "0000000000000000000000000000000000000000000000000000000000000040" + "0000000000000000000000000000000000000000000000000000000000000080" + "000000000000000000000000000000000000000000000000000000000000000C" + "72616E646F6D2062797465730000000000000000000000000000000000000000" + "000000000000000000000000000000000000000000000000000000000000000C" + "72616E646F6D2062797465730000000000000000000000000000000000000000";
        private static readonly byte[] callResult = Hex.Decode(CALL_RESULT_HEX);
        private static readonly byte[] stringArrayCallResult = Hex.Decode(STRING_ARRAY_RESULT_HEX);
        virtual void ProvidesResultsCorrectly()
        {
            var result = new ContractFunctionResult(com.hedera.hashgraph.sdk.proto.ContractFunctionResult.NewBuilder().SetContractID(ContractId.FromString("1.2.3").ToProtobuf()).SetContractCallResult(ByteString.CopyFrom(callResult)).SetEvmAddress(BytesValue.NewBuilder().SetValue(ByteString.CopyFrom(Hex.Decode("98329e006610472e6B372C080833f6D79ED833cf"))).Build()).SetSenderId(AccountId.FromString("1.2.3").ToProtobuf()).AddContractNonces(new ContractNonceInfo(ContractId.FromString("1.2.3"), 10).ToProtobuf()));

            // interpretation varies based on width
            AssertThat(result.GetBool(0)).IsTrue();
            Assert.Equal(result.GetInt32(0), -1);
            Assert.Equal(result.GetInt64(0), (1 << 32) - 1);
            Assert.Equal(result.GetInt256(0), BigInteger.ONE.ShiftLeft(32).Subtract(BigInteger.ONE));
            Assert.Equal(result.GetInt256(1), BigInteger.ONE.ShiftLeft(255).Subtract(BigInteger.ONE));
            Assert.Equal(result.GetAddress(2), "11223344556677889900aabbccddeeff00112233");

            // unsigned integers (where applicable)
            Assert.Equal(result.GetUint32(3), -1);
            Assert.Equal(result.GetUint64(3), -1);

            // BigInteger can represent the full range and so should be 2^256 - 1
            Assert.Equal(result.GetUint256(3), BigInteger.ONE.ShiftLeft(256).Subtract(BigInteger.ONE));
            Assert.Equal(result.GetString(4), "Hello, world!");
            Assert.Equal(result.GetString(5), "Hello, world, again!");
            Assert.Equal(result.senderAccountId, AccountId.FromString("1.2.3"));
            Assert.Equal(result.contractId, ContractId.FromString("1.2.3"));
            Assert.Equal(result.evmAddress, ContractId.FromEvmAddress(1, 2, "98329e006610472e6B372C080833f6D79ED833cf"));

            // assert.Equal(result.stateChanges.size(), 1);
            // ContractStateChange resultStateChange = result.stateChanges.get(0);
            // assert.Equal(resultStateChange.contractId, ContractId.fromString("1.2.3"));
            // assert.Equal(resultStateChange.storageChanges.size(), 1);
            // StorageChange resultStorageChange = resultStateChange.storageChanges.get(0);
            // assert.Equal(resultStorageChange.slot, BigInteger.valueOf(555));
            // assert.Equal(resultStorageChange.valueRead, BigInteger.valueOf(666));
            // assert.Equal(resultStorageChange.valueWritten, BigInteger.valueOf(777));
            AssertThat(result.contractNonces).ContainsOnly(new ContractNonceInfo(ContractId.FromString("1.2.3"), 10));
        }

        virtual void CanGetStringArrayResult()
        {
            var result = new ContractFunctionResult(com.hedera.hashgraph.sdk.proto.ContractFunctionResult.NewBuilder().SetContractCallResult(ByteString.CopyFrom(stringArrayCallResult)));
            var strings = result.GetStringArray(0);
            Assert.Equal(strings[0], "random bytes");
            Assert.Equal(strings[1], "random bytes");
        }

        virtual void CanToFromBytesStateChanges()
        {
        }
    }
}