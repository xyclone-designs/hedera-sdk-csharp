// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Io.Github.JsonSnapshot;
using Java.Lang.Reflect;
using Java.Math;
using Java.Nio.Charset;
using Java.Util;
using Java.Util.Function;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    public class ContractFunctionParametersTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        private static IList<Arguments> Int256Arguments()
        {
            return List.Of(Arguments.Of(0, "0000000000000000000000000000000000000000000000000000000000000000"), Arguments.Of(2, "0000000000000000000000000000000000000000000000000000000000000002"), Arguments.Of(255, "00000000000000000000000000000000000000000000000000000000000000ff"), Arguments.Of(4095, "0000000000000000000000000000000000000000000000000000000000000fff"), Arguments.Of(127 << 24, "000000000000000000000000000000000000000000000000000000007f000000"), Arguments.Of(2047 << 20, "000000000000000000000000000000000000000000000000000000007ff00000"), Arguments.Of(0xdeadbeef, "00000000000000000000000000000000000000000000000000000000deadbeef"), Arguments.Of(-1, "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"), Arguments.Of(-2, "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffe"), Arguments.Of(-256, "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff00"), Arguments.Of(-4096, "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff000"), Arguments.Of(255 << 24, "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffff000000"), Arguments.Of(4095 << 20, "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff00000"), Arguments.Of(0xdeadbeef, "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffdeadbeef"));
        }

        private static IList<Arguments> UInt256Arguments()
        {
            return List.Of(Arguments.Of(0, "0000000000000000000000000000000000000000000000000000000000000000", 8), Arguments.Of(2, "0000000000000000000000000000000000000000000000000000000000000002", 8), Arguments.Of(255, "00000000000000000000000000000000000000000000000000000000000000ff", 8), Arguments.Of(4095, "0000000000000000000000000000000000000000000000000000000000000fff", 32), Arguments.Of(127 << 24, "000000000000000000000000000000000000000000000000000000007f000000", 32), Arguments.Of(2047 << 20, "000000000000000000000000000000000000000000000000000000007ff00000", 32), Arguments.Of(0xdeadbeef, "00000000000000000000000000000000000000000000000000000000deadbeef", 32), Arguments.Of(-1, "000000000000000000000000000000000000000000000000ffffffffffffffff", 64), Arguments.Of(-2, "000000000000000000000000000000000000000000000000fffffffffffffffe", 64), Arguments.Of(-256, "000000000000000000000000000000000000000000000000ffffffffffffff00", 64), Arguments.Of(-4096, "000000000000000000000000000000000000000000000000fffffffffffff000", 64), Arguments.Of(255 << 24, "000000000000000000000000000000000000000000000000ffffffffff000000", 64), Arguments.Of(4095 << 20, "000000000000000000000000000000000000000000000000fffffffffff00000", 64), Arguments.Of(0xdeadbeef, "00000000000000000000000000000000000000000000000000000000deadbeef", 64));
        }

        public virtual void IntTypes()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddUint8((byte)0x1).AddInt8((byte)-0x2).AddUint32(0x3).AddInt32(-0x4).AddUint64(0x4).AddInt64(-0x5).AddUint256(BigInteger.ValueOf(0x6)).AddInt256(BigInteger.ValueOf(-0x7)).AddUint8Array(new byte[] { (byte)0x1, (byte)0x2, (byte)0x3, (byte)0x4 }).AddInt8Array(new byte[] { (byte)-0x5, (byte)0x6, (byte)0x7, (byte)-0x8 }).AddUint32Array(new int[] { 0x9, 0xA, 0xB, 0xC }).AddInt32Array(new int[] { -0xD, 0xE, 0xF, -0x10 }).AddUint64Array(new long[] { 0x11, 0x12, 0x13, 0x14 }).AddInt64Array(new long[] { -0x15, 0x16, 0x17, -0x18 }).AddUint256Array(new BigInteger[] { BigInteger.ValueOf(0x19) }).AddInt256Array(new BigInteger[] { BigInteger.ValueOf(-0x1A) });
            Assert.Equal("11bcd903" + "0000000000000000000000000000000000000000000000000000000000000001" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffe" + "0000000000000000000000000000000000000000000000000000000000000003" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffc" + "0000000000000000000000000000000000000000000000000000000000000004" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffb" + "0000000000000000000000000000000000000000000000000000000000000006" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff9" + "0000000000000000000000000000000000000000000000000000000000000200" + "00000000000000000000000000000000000000000000000000000000000002a0" + "0000000000000000000000000000000000000000000000000000000000000340" + "00000000000000000000000000000000000000000000000000000000000003e0" + "0000000000000000000000000000000000000000000000000000000000000480" + "0000000000000000000000000000000000000000000000000000000000000520" + "00000000000000000000000000000000000000000000000000000000000005c0" + "0000000000000000000000000000000000000000000000000000000000000600" + "0000000000000000000000000000000000000000000000000000000000000004" + "0000000000000000000000000000000000000000000000000000000000000001" + "0000000000000000000000000000000000000000000000000000000000000002" + "0000000000000000000000000000000000000000000000000000000000000003" + "0000000000000000000000000000000000000000000000000000000000000004" + "0000000000000000000000000000000000000000000000000000000000000004" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffb" + "0000000000000000000000000000000000000000000000000000000000000006" + "0000000000000000000000000000000000000000000000000000000000000007" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff8" + "0000000000000000000000000000000000000000000000000000000000000004" + "0000000000000000000000000000000000000000000000000000000000000009" + "000000000000000000000000000000000000000000000000000000000000000a" + "000000000000000000000000000000000000000000000000000000000000000b" + "000000000000000000000000000000000000000000000000000000000000000c" + "0000000000000000000000000000000000000000000000000000000000000004" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff3" + "000000000000000000000000000000000000000000000000000000000000000e" + "000000000000000000000000000000000000000000000000000000000000000f" + "fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff0" + "0000000000000000000000000000000000000000000000000000000000000004" + "0000000000000000000000000000000000000000000000000000000000000011" + "0000000000000000000000000000000000000000000000000000000000000012" + "0000000000000000000000000000000000000000000000000000000000000013" + "0000000000000000000000000000000000000000000000000000000000000014" + "0000000000000000000000000000000000000000000000000000000000000004" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffeb" + "0000000000000000000000000000000000000000000000000000000000000016" + "0000000000000000000000000000000000000000000000000000000000000017" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffe8" + "0000000000000000000000000000000000000000000000000000000000000001" + "0000000000000000000000000000000000000000000000000000000000000019" + "0000000000000000000000000000000000000000000000000000000000000001" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffe6", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void Uint256BitLength()
        {
            var params = new ContractFunctionParameters().AddUint256(BigInteger.ValueOf(2).Pow(255));
            Assert.Equal("2fbebd38" + "8000000000000000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void Uint256Errors()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddUint256(BigInteger.ValueOf(-0x1));
            }); /*
        assertThrows(IllegalArgumentException.class, () -> {
            new ContractFunctionParameters()
                .addUint256(BigInteger.valueOf(2).pow(256));
        });
         */
        }

        public virtual void Addresses()
        {
            var params = new ContractFunctionParameters().AddAddress("1122334455667788990011223344556677889900").AddAddress("0x1122334455667788990011223344556677889900").AddAddressArray(new string[] { "1122334455667788990011223344556677889900", "1122334455667788990011223344556677889900" });
            Assert.Equal("7d48c86d" + "0000000000000000000000001122334455667788990011223344556677889900" + "0000000000000000000000001122334455667788990011223344556677889900" + "0000000000000000000000000000000000000000000000000000000000000060" + "0000000000000000000000000000000000000000000000000000000000000002" + "0000000000000000000000001122334455667788990011223344556677889900" + "0000000000000000000000001122334455667788990011223344556677889900", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void AddressesError()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddAddress("112233445566778899001122334455667788990011");
            });
        }

        public virtual void Functions()
        {
            var params = new ContractFunctionParameters().AddFunction("1122334455667788990011223344556677889900", new byte[] { 1, 2, 3, 4 }).AddFunction("0x1122334455667788990011223344556677889900", new ContractFunctionSelector("randomFunction").AddBool());
            Assert.Equal("c99c40cd" + "1122334455667788990011223344556677889900010203040000000000000000" + "112233445566778899001122334455667788990063441d820000000000000000", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void FunctionsError()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddFunction("112233445566778899001122334455667788990011", new byte[] { 1, 2, 3, 4 });
            });
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddFunction("1122334455667788990011223344556677889900", new byte[] { 1, 2, 3, 4, 5 });
            });
        }

        public virtual void Bytes4Encoding()
        {
            var params = new ContractFunctionParameters().AddBytes4(new byte[] { 1, 2, 3, 4 });
            Assert.Equal("580526ee" + "0102030400000000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void Bytes4EncodingError()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddBytes4(new byte[] { 1, 2, 3, 4, 5 });
            });
        }

        public virtual void Bytes4UTF8Encoding()
        {
            var params = new ContractFunctionParameters().AddBytes4("ABCD".GetBytes(StandardCharsets.UTF_8));
            Assert.Equal("580526ee" + "4142434400000000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void Bytes4UTF8EncodingError()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddBytes4("ABCDE".GetBytes(StandardCharsets.UTF_8));
            });
        }

        public virtual void Bytes()
        {
            var params = new ContractFunctionParameters().AddBytes32(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 });
            Assert.Equal("11e814c1" + "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void BytesError()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ContractFunctionParameters().AddBytes32(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33 });
            });
        }

        public virtual void Bool()
        {
            var params = new ContractFunctionParameters().AddBool(true).AddBool(false);
            Assert.Equal("b3cedfcf" + "0000000000000000000000000000000000000000000000000000000000000001" + "0000000000000000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void DynamicParamsEncoding()
        {
            ByteString paramsStringArg = new ContractFunctionParameters().AddString("Hello, world!").ToBytes("set_message");
            ByteString paramsBytesArg = new ContractFunctionParameters().AddBytes("Hello, world!".GetBytes(StandardCharsets.UTF_8)).ToBytes("set_message");
            string paramsStringArgHex = Hex.ToHexString(paramsStringArg.ToByteArray());
            string paramsBytesArgHex = Hex.ToHexString(paramsBytesArg.ToByteArray());
            Assert.Equal("2e982602" + "0000000000000000000000000000000000000000000000000000000000000020" + "000000000000000000000000000000000000000000000000000000000000000d" + "48656c6c6f2c20776f726c642100000000000000000000000000000000000000", paramsStringArgHex);

            // signature should encode differently but the contents are identical
            Assert.Equal("010473a7" + "0000000000000000000000000000000000000000000000000000000000000020" + "000000000000000000000000000000000000000000000000000000000000000d" + "48656c6c6f2c20776f726c642100000000000000000000000000000000000000", paramsBytesArgHex);
        }

        public virtual void StaticParamsEncoding()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddInt32(0x11223344).AddInt32(-65536).AddUint64(-65536).AddAddress("00112233445566778899aabbccddeeff00112233");
            string paramsHex = Hex.ToHexString(@params.ToBytes(null).ToByteArray());
            Assert.Equal("0000000000000000000000000000000000000000000000000000000011223344" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff0000" + "000000000000000000000000000000000000000000000000ffffffffffff0000" + "00000000000000000000000000112233445566778899aabbccddeeff00112233", paramsHex);
        }

        public virtual void MixedParamsEncoding()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddInt256(BigInteger.ValueOf(0xdeadbeef).ShiftLeft(8)).AddString("Hello, world!").AddBytes(new byte[] { -1, -18, 63, 127 }).AddBool(true).AddUint8Array(new byte[] { -1, 127 });
            string paramsHex = Hex.ToHexString(@params.ToBytes("foo").ToByteArray());
            Assert.Equal("6a5bb8f2" + "ffffffffffffffffffffffffffffffffffffffffffffffffffffffdeadbeef00" + "00000000000000000000000000000000000000000000000000000000000000a0" + "00000000000000000000000000000000000000000000000000000000000000e0" + "0000000000000000000000000000000000000000000000000000000000000001" + "0000000000000000000000000000000000000000000000000000000000000120" + "000000000000000000000000000000000000000000000000000000000000000d" + "48656c6c6f2c20776f726c642100000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000004" + "ffee3f7f00000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000002" + "00000000000000000000000000000000000000000000000000000000000000ff" + "000000000000000000000000000000000000000000000000000000000000007f", paramsHex);
        }

        public virtual void ArrayTypesEncoding()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddStringArray(new string[] { "hello", ",", "world!" }).AddInt32Array(new int[] { 0x88, 0x99, 0xAA, 0xBB }).AddInt256Array(new BigInteger[] { BigInteger.ValueOf(0x1111) });
            Assert.Equal("025838fc" + "0000000000000000000000000000000000000000000000000000000000000060" + "00000000000000000000000000000000000000000000000000000000000001a0" + "0000000000000000000000000000000000000000000000000000000000000240" + "0000000000000000000000000000000000000000000000000000000000000003" + "0000000000000000000000000000000000000000000000000000000000000060" + "00000000000000000000000000000000000000000000000000000000000000a0" + "00000000000000000000000000000000000000000000000000000000000000e0" + "0000000000000000000000000000000000000000000000000000000000000005" + "68656c6c6f000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000001" + "2c00000000000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000006" + "776f726c64210000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000004" + "0000000000000000000000000000000000000000000000000000000000000088" + "0000000000000000000000000000000000000000000000000000000000000099" + "00000000000000000000000000000000000000000000000000000000000000aa" + "00000000000000000000000000000000000000000000000000000000000000bb" + "0000000000000000000000000000000000000000000000000000000000000001" + "0000000000000000000000000000000000000000000000000000000000001111", Hex.ToHexString(@params.ToBytes("foo").ToByteArray()));
        }

        public virtual void FixedBytes4ArrayEncoding()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddBytes4Array(new byte[] { new[] { 1, 2, 3, 4 }, new[] { 5, 6, 7, 8 }, new[] { 9, 10, 11, 12 } });
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000020" + "0000000000000000000000000000000000000000000000000000000000000003" + "0102030400000000000000000000000000000000000000000000000000000000" + "0506070800000000000000000000000000000000000000000000000000000000" + "090a0b0c00000000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes(null).ToByteArray()));
        }

        public virtual void FixedBytesArrayEncoding()
        {

            // each string should be padded to 32 bytes and have no length prefix
            ContractFunctionParameters params = new ContractFunctionParameters().AddBytes32Array(new byte[] { "Hello".GetBytes(StandardCharsets.UTF_8), ",".GetBytes(StandardCharsets.UTF_8), "world!".GetBytes(StandardCharsets.UTF_8) });
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000020" + "0000000000000000000000000000000000000000000000000000000000000003" + "48656c6c6f000000000000000000000000000000000000000000000000000000" + "2c00000000000000000000000000000000000000000000000000000000000000" + "776f726c64210000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes(null).ToByteArray()));
        }

        public virtual void DynBytesArrayEncoding()
        {

            // result should be the exact same as the strings test below
            ContractFunctionParameters params = new ContractFunctionParameters().AddBytesArray(new byte[] { "Hello".GetBytes(StandardCharsets.UTF_8), ",".GetBytes(StandardCharsets.UTF_8), "world!".GetBytes(StandardCharsets.UTF_8) });
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000020" + "0000000000000000000000000000000000000000000000000000000000000003" + "0000000000000000000000000000000000000000000000000000000000000060" + "00000000000000000000000000000000000000000000000000000000000000a0" + "00000000000000000000000000000000000000000000000000000000000000e0" + "0000000000000000000000000000000000000000000000000000000000000005" + "48656c6c6f000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000001" + "2c00000000000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000006" + "776f726c64210000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes(null).ToByteArray()));
        }

        public virtual void StringArrayEncoding()
        {
            ContractFunctionParameters params = new ContractFunctionParameters().AddStringArray(new string[] { "Hello", ",", "world!" });
            Assert.Equal("0000000000000000000000000000000000000000000000000000000000000020" + "0000000000000000000000000000000000000000000000000000000000000003" + "0000000000000000000000000000000000000000000000000000000000000060" + "00000000000000000000000000000000000000000000000000000000000000a0" + "00000000000000000000000000000000000000000000000000000000000000e0" + "0000000000000000000000000000000000000000000000000000000000000005" + "48656c6c6f000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000001" + "2c00000000000000000000000000000000000000000000000000000000000000" + "0000000000000000000000000000000000000000000000000000000000000006" + "776f726c64210000000000000000000000000000000000000000000000000000", Hex.ToHexString(@params.ToBytes(null).ToByteArray()));
        }

        public virtual void BigIntChecks()
        {
            ContractFunctionParameters params = new ContractFunctionParameters();

            // allowed values for BigInteger
            @params.AddInt256(BigInteger.ONE.ShiftLeft(254));
            @params.AddInt256(BigInteger.ONE.Negate().ShiftLeft(255));
            string rangeErr = "BigInteger out of range for Solidity integers";
            Assert.Throws<ArgumentException>(() => @params.AddInt256(BigInteger.ONE.ShiftLeft(255))).Satisfies((error) => Assert.Equal(error.GetMessage(), rangeErr));
            Assert.Throws<ArgumentException>(() => @params.AddInt256(BigInteger.ONE.Negate().ShiftLeft(256))).Satisfies((error) => Assert.Equal(error.GetMessage(), rangeErr));
        }

        public virtual void AddressParamChecks()
        {
            ContractFunctionParameters params = new ContractFunctionParameters();
            string lenErr = "Solidity addresses must be 40 hex chars";
            Assert.Throws<ArgumentException>(() => @params.AddAddress("")).Satisfies((error) => Assert.Equal(error.GetMessage(), lenErr));
            Assert.Throws<ArgumentException>(() => @params.AddAddress("aabbccdd")).Satisfies((error) => Assert.Equal(error.GetMessage(), lenErr));
            Assert.Throws<ArgumentException>(() => @params.AddAddress("00112233445566778899aabbccddeeff0011223344")).Satisfies((error) => Assert.Equal(error.GetMessage(), lenErr));
            Assert.Throws<ArgumentException>(() => @params.AddAddress("gghhii--__zz66778899aabbccddeeff00112233")).Satisfies((error) => Assert.Equal(error.GetMessage(), "failed to decode Solidity address as hex"));
        }

        public virtual void Int256EncodesCorrectly(long val, string hexString)
        {
            Assert.Equal(hexString, Hex.ToHexString(ContractFunctionParameters.Int256(val, 64).ToByteArray()));
        }

        public virtual void UInt256EncodesCorrectly(long val, string hexString, int bitWidth)
        {
            Assert.Equal(hexString, Hex.ToHexString(ContractFunctionParameters.Uint256(val, bitWidth).ToByteArray()));
        }

        public virtual void IntSizesEncodeCorrectly()
        {
            IList<string> snapshotStrings = new List();
            for (int n = 8; n <= 256; n += 8)
            {
                var bitWidth = n;
                var argType = ((Supplier<Class<TWildcardTodo>>)() =>
                {
                    if (bitWidth == 8)
                    {
                        return typeof(byte);
                    }
                    else if (bitWidth <= 32)
                    {
                        return typeof(int);
                    }
                    else if (bitWidth <= 64)
                    {
                        return typeof(long);
                    }
                    else
                    {
                        return typeof(BigInteger);
                    }
                }).Get();
                var argVal = ((Supplier<object>)() =>
                {
                    if (bitWidth == 8)
                    {
                        return (byte)(1 << (bitWidth - 1));
                    }
                    else if (bitWidth <= 32)
                    {
                        return (1 << (bitWidth - 1));
                    }
                    else if (bitWidth <= 64)
                    {
                        return (1 << (bitWidth - 1));
                    }
                    else
                    {
                        return BigInteger.ONE.ShiftLeft(bitWidth - 1);
                    }
                }).Get();
                var argArrayVal = Array.NewInstance(argType, 2);
                Array.Set(argArrayVal, 0, argVal);
                Array.Set(argArrayVal, 1, argVal);
                var argArrayType = argArrayVal.GetType();
                var cl = typeof(ContractFunctionParameters);
                var addIntMethod = cl.GetMethod("addInt" + n, argType);
                var addUintMethod = cl.GetMethod("addUint" + n, argType);
                var addIntArrayMethod = cl.GetMethod("addInt" + n + "Array", argArrayType);
                var addUintArrayMethod = cl.GetMethod("addUint" + n + "Array", argArrayType);
                var params = new ContractFunctionParameters();
                addIntMethod.Invoke(@params, argVal);
                addUintMethod.Invoke(@params, argVal);
                addIntArrayMethod.Invoke(@params, argArrayVal);
                addUintArrayMethod.Invoke(@params, argArrayVal);
                snapshotStrings.Add("bitWidth = " + bitWidth + ": " + Hex.ToHexString(@params.ToBytes(null).ToByteArray()));
            }

            SnapshotMatcher.Expect(snapshotStrings.ToArray()).ToMatchSnapshot();
        }
    }
}