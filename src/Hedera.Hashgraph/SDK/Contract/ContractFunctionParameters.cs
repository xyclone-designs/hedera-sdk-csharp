using Google.Protobuf;
using Google.Protobuf.Collections;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Hedera.Hashgraph.SDK
{
	// an implementation of function selector and parameter encoding as specified here:
	// https://solidity.readthedocs.io/en/v0.5.7/abi-spec.html#

	/**
	 * Builder for encoding parameters for a Solidity contract constructor/function call.
	 * <p>
	 * If you require a type which is not supported here, please let us know on
	 * <a href="https://github.com/hashgraph/hedera-sdk-java/issues/298">this Github issue</a>.
	 */
	public sealed partial class ContractFunctionParameters
	{
		/**
		 * The length of a Solidity Address in bytes.
		 */
		public static readonly int ADDRESS_LEN = Utils.EntityIdHelper.SOLIDITY_ADDRESS_LEN;

		/**
		 * The length of a hexadecimal-encoded Solidity Address, in ASCII characters (bytes).
		 */
		public static readonly int ADDRESS_LEN_HEX = Utils.EntityIdHelper.SOLIDITY_ADDRESS_LEN_HEX;

		/**
		 * Function selector length in bytes
		 */
		public static readonly int SELECTOR_LEN = 4;

		/**
		 * Function selector length in hex characters
		 */
		public static readonly int SELECTOR_LEN_HEX = 8;

		// padding that we can substring without new allocations
		private static readonly ByteString padding = ByteString.CopyFrom(new byte[31]);
		private static readonly ByteString? negativePadding;

		//  static {
		//      byte[] fill = new byte[31];
		//Array.fill(fill, (byte) 0xFF);
		//      negativePadding = ByteString.CopyFrom(fill);
		//  }

		private readonly List<Argument> args = [];

		private static byte[] DecodeAddress(string address)
		{
			address = address.StartsWith("0x") ? address.Substring(2) : address;

			if (address.Length != ADDRESS_LEN_HEX)
			{
				throw new ArgumentException("Solidity Addresses must be 40 hex chars");
			}

			try
			{
				return Hex.Decode(address);
			}
			catch (Exception e)
			{
				throw new ArgumentException("failed to decode Solidity Address as hex", e);
			}
		}
		private static byte[] GetTruncatedBytes(BigInteger bigInt, int bitWidth)
		{
			byte[] bytes = bigInt.ToByteArray();
			int expectedBytes = bitWidth / 8;
			return bytes.Length <= expectedBytes
					? bytes
					: bytes[(bytes.Length - expectedBytes)..bytes.Length].CopyArray();
		}
		private static ByteString EncodeString(string str)
		{
			ByteString strBytes = ByteString.CopyFromUtf8(str);
			// prepend the size of the string in UTF-8 bytes
			return Int256(strBytes.Length, 32).Concat(RightPad32(strBytes));
		}
		private static ByteString EncodeBytes(byte[] bytes)
		{
			return Int256(bytes.Length, 32).Concat(RightPad32(ByteString.CopyFrom(bytes)));
		}
		private static ByteString EncodeBytes4(byte[] bytes)
		{
			if (bytes.Length > 4)
			{
				throw new ArgumentException("bytes4 encoding forbids byte array length greater than 4");
			}
			return RightPad32(ByteString.CopyFrom(bytes));
		}
		private static ByteString EncodeBytes32(byte[] bytes)
		{
			if (bytes.Length > 32)
			{
				throw new ArgumentException("byte32 encoding forbids byte array length greater than 32");
			}

			return RightPad32(ByteString.CopyFrom(bytes));
		}
		private static ByteString EncodeBool(bool val)
		{
			return Int256(val ? 1 : 0, 8);
		}
		private static ByteString EncodeArray(IEnumerable<ByteString> elements)
		{
			byte[] bytearray = [.. elements.SelectMany(_ => _.ToByteArray())];

			return Int256(bytearray.Length, 32).Concat(ByteString.CopyFrom(bytearray));
		}
		private static ByteString EncodeDynArr(IEnumerable<ByteString> elements)
		{
			int offsetsLen = elements.Count();

            // [len, offset[0], offset[1], ... offset[len - 1]]
            List<ByteString> head = new (offsetsLen + 1)
            {
                Uint256(elements.Count(), 32)
            };

			// points to start of dynamic segment, *not* including the length of the array
			long currOffset = offsetsLen * 32L;

			foreach (ByteString elem in elements)
			{
				head.Add(Uint256(currOffset, 64));
				currOffset += elem.Length;
			}

			return ByteString
				.CopyFrom([.. head.SelectMany(_ => _.ToByteArray())])
				.Concat(ByteString.CopyFrom([.. elements.SelectMany(_ => _.ToByteArray())]));
		}
		private static ByteString Int256(long val, int bitWidth)
		{
			return Int256(val, bitWidth, true);
		}
		private static ByteString Int256(long val, int bitWidth, bool signed)
		{
			// don't try to Get wider than a `long` as it should just be filled with padding
			bitWidth = Math.Min(bitWidth, 64);
			ByteString.Output output = ByteString.NewOutput(bitWidth / 8);

			try
			{
				// write bytes in big-endian order
				for (int i = bitWidth - 8; i >= 0; i -= 8)
				{
					// widening conversion sign-extends so we don't have to do anything special when
					// truncating a previously widened value
					byte u8 = (byte)(val >> i);
					output.write(u8);
				}

				// byte padding will sign-extend appropriately
				return LeftPad32(output.toByteString(), signed && val < 0);
			}
			finally
			{
				try
				{
					output.close();
				}
				catch (Exception)
				{
					// do nothing
				}
			}
		}
		private static ByteString Int256(BigInteger bigInt, int bitWidth)
		{
			return LeftPad32(GetTruncatedBytes(bigInt, bitWidth), bigInt.Sign < 0);
		}
		private static ByteString Uint256(long val, int bitWidth)
		{
			return Int256(val, bitWidth, false);
		}
		private static ByteString Uint256(BigInteger bigInt, int bitWidth)
		{
			if (bigInt.Sign < 0)
			{
				throw new ArgumentException("negative BigInteger passed to unsigned function");
			}
			return LeftPad32(GetTruncatedBytes(bigInt, bitWidth), false);
		}
		private static ByteString LeftPad32(ByteString input)
		{
			return LeftPad32(input, false);
		}
		private static ByteString LeftPad32(ByteString input, bool negative)
		{
			int rem = 32 - input.Length % 32;

			if (rem == 32)
				return input;

			string text = (negative ? negativePadding ?? padding : padding).ToStringUtf8()[..rem];
			ByteString bytestring = ByteString.CopyFromUtf8(text);

			return bytestring.Concat(input);
		}
		private static ByteString LeftPad32(byte[] input, bool negative)
		{
			return LeftPad32(ByteString.CopyFrom(input), negative);
		}
		private static ByteString RightPad32(ByteString input)
		{
			int rem = 32 - input.Length % 32;
			ByteString bytestring = ByteString.CopyFromUtf8(padding.ToStringUtf8()[.. rem]);
			return rem == 32 ? input : input.Concat(bytestring);
		}

		/**
		 * Add a parameter of type {@code string}.
		 * <p>
		 * For Solidity Addresses, use {@link #addAddress(string)}.
		 *
		 * @param param The string to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddString(string param)
		{
			args.Add(new Argument("string", EncodeString(param), true));

			return this;
		}
		/**
		 * Add a parameter of type {@code string[]}.
		 *
		 * @param strings The array of Strings to be Added
		 * @return {@code this}
		 * @ if any value in `strings` is null
		 */
		public ContractFunctionParameters AddStringArray(string[] strings)
		{
			ByteString argBytes = EncodeDynArr(strings.Select(_ => EncodeString(_)));

			args.Add(new Argument("string[]", argBytes, true));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes}, a byte-string.
		 *
		 * @param param The byte-string to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddBytes(byte[] param)
		{
			args.Add(new Argument("bytes", EncodeBytes(param), true));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes[]}, an array of byte-strings.
		 *
		 * @param param The array of byte-strings to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddBytesArray(byte[][] param)
		{
			ByteString argBytes = EncodeDynArr(param.Select(_ => EncodeBytes(_)));

			args.Add(new Argument("bytes[]", argBytes, true));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes4}, a 4-byte fixed-length byte-string.
		 *
		 * @param param The 4-byte array to be Added
		 * @return {@code this}
		 * @ if the length of the byte array is not 4.
		 */
		public ContractFunctionParameters AddBytes4(byte[] param)
		{
			args.Add(new Argument("bytes4", EncodeBytes4(param), false));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes4[]}, an array of 4-byte fixed-length byte-strings.
		 *
		 * @param param The array of 4-byte arrays to be Added
		 * @return {@code this}
		 * @ if the length of any byte array is not 4.
		 */
		public ContractFunctionParameters AddBytes4Array(byte[][] param)
		{
			args.Add(new Argument("bytes4[]", EncodeArray(param.Select(_ => EncodeBytes4(_))), true));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes32}, a 32-byte byte-string.
		 * <p>
		 * If applicable, the array will be right-padded with zero bytes to a length of 32 bytes.
		 *
		 * @param param The byte-string to be Added
		 * @return {@code this}
		 * @ if the length of the byte array is greater than 32.
		 */
		public ContractFunctionParameters AddBytes32(byte[] param)
		{
			args.Add(new Argument("bytes32", EncodeBytes32(param), false));

			return this;
		}
		/**
		 * Add a parameter of type {@code bytes32[]}, an array of 32-byte byte-strings.
		 * <p>
		 * Each byte array will be right-padded with zero bytes to a length of 32 bytes.
		 *
		 * @param param The array of byte-strings to be Added
		 * @return {@code this}
		 * @ if the length of any byte array is greater than 32.
		 */
		public ContractFunctionParameters AddBytes32Array(byte[][] param)
		{
			// array of fixed-size elements

			args.Add(new Argument("bytes32[]", EncodeArray(param.Select(_ => EncodeBytes32(_))), true));

			return this;
		}
		/**
		 * Add a bool parameter
		 *
		 * @param bool The bool to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddBool(bool val)
		{
			// bool Encodes to `uint8` of values [0, 1]
			args.Add(new Argument("bool", EncodeBool(val), false));
			return this;
		}
		/**
		 * Add a bool array parameter
		 *
		 * @param param The array of bools to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddBoolArray(bool[] param)
		{
			bool[] boolWrapperArray = new bool[param.Length];

			for (int i = 0; i < param.Length; i++)
			{
				boolWrapperArray[i] = param[i];
			}

			args.Add(new Argument("bool[]", EncodeArray(boolWrapperArray.Select(_ => EncodeBool(_))), true));

			return this;
		}
		/**
		 * Add an 8-bit integer.
		 * <p>
		 * The implementation is wasteful as we must pad to 32-bytes to store 1 byte.
		 *
		 * @param value The value to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt8(byte value)
		{
			args.Add(new Argument("int8", Int256(value, 8), false));

			return this;
		}
		/**
		 * Add a 16-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt16(int value)
		{
			args.Add(new Argument("int16", Int256(value, 16), false));

			return this;
		}
		/**
		 * Add a 24-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt24(int value)
		{
			args.Add(new Argument("int24", Int256(value, 24), false));

			return this;
		}
		/**
		 * Add a 32-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt32(int value)
		{
			args.Add(new Argument("int32", Int256(value, 32), false));

			return this;
		}
		/**
		 * Add a 40-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt40(long value)
		{
			args.Add(new Argument("int40", Int256(value, 40), false));

			return this;
		}
		/**
		 * Add a 48-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt48(long value)
		{
			args.Add(new Argument("int48", Int256(value, 48), false));

			return this;
		}
		/**
		 * Add a 56-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt56(long value)
		{
			args.Add(new Argument("int56", Int256(value, 56), false));

			return this;
		}
		/**
		 * Add a 64-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt64(long value)
		{
			args.Add(new Argument("int64", Int256(value, 64), false));

			return this;
		}
		/**
		 * Add a 72-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt72(BigInteger value)
		{
			args.Add(new Argument("int72", Int256(value, 72), false));

			return this;
		}
		/**
		 * Add a 80-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt80(BigInteger value)
		{
			args.Add(new Argument("int80", Int256(value, 80), false));

			return this;
		}
		/**
		 * Add a 88-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt88(BigInteger value)
		{
			args.Add(new Argument("int88", Int256(value, 88), false));

			return this;
		}
		/**
		 * Add a 96-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt96(BigInteger value)
		{
			args.Add(new Argument("int96", Int256(value, 96), false));

			return this;
		}
		/**
		 * Add a 104-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt104(BigInteger value)
		{
			args.Add(new Argument("int104", Int256(value, 104), false));

			return this;
		}
		/**
		 * Add a 112-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt112(BigInteger value)
		{
			args.Add(new Argument("int112", Int256(value, 112), false));

			return this;
		}
		/**
		 * Add a 120-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt120(BigInteger value)
		{
			args.Add(new Argument("int120", Int256(value, 120), false));

			return this;
		}
		/**
		 * Add a 128-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt128(BigInteger value)
		{
			args.Add(new Argument("int128", Int256(value, 128), false));

			return this;
		}
		/**
		 * Add a 136-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt136(BigInteger value)
		{
			args.Add(new Argument("int136", Int256(value, 136), false));

			return this;
		}
		/**
		 * Add a 144-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt144(BigInteger value)
		{
			args.Add(new Argument("int144", Int256(value, 144), false));

			return this;
		}
		/**
		 * Add a 152-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt152(BigInteger value)
		{
			args.Add(new Argument("int152", Int256(value, 152), false));

			return this;
		}
		/**
		 * Add a 160-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt160(BigInteger value)
		{
			args.Add(new Argument("int160", Int256(value, 160), false));

			return this;
		}
		/**
		 * Add a 168-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt168(BigInteger value)
		{
			args.Add(new Argument("int168", Int256(value, 168), false));

			return this;
		}
		/**
		 * Add a 176-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt176(BigInteger value)
		{
			args.Add(new Argument("int176", Int256(value, 176), false));

			return this;
		}
		/**
		 * Add a 184-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt184(BigInteger value)
		{
			args.Add(new Argument("int184", Int256(value, 184), false));

			return this;
		}
		/**
		 * Add a 192-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt192(BigInteger value)
		{
			args.Add(new Argument("int192", Int256(value, 192), false));

			return this;
		}
		/**
		 * Add a 200-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt200(BigInteger value)
		{
			args.Add(new Argument("int200", Int256(value, 200), false));

			return this;
		}
		/**
		 * Add a 208-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt208(BigInteger value)
		{
			args.Add(new Argument("int208", Int256(value, 208), false));

			return this;
		}
		/**
		 * Add a 216-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt216(BigInteger value)
		{
			args.Add(new Argument("int216", Int256(value, 216), false));

			return this;
		}
		/**
		 * Add a 224-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt224(BigInteger value)
		{
			args.Add(new Argument("int224", Int256(value, 224), false));

			return this;
		}
		/**
		 * Add a 232-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt232(BigInteger value)
		{
			args.Add(new Argument("int232", Int256(value, 232), false));

			return this;
		}
		/**
		 * Add a 240-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt240(BigInteger value)
		{
			args.Add(new Argument("int240", Int256(value, 240), false));

			return this;
		}
		/**
		 * Add a 248-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt248(BigInteger value)
		{
			args.Add(new Argument("int248", Int256(value, 248), false));

			return this;
		}
		/**
		 * Add a 256-bit integer.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt256(BigInteger value)
		{
			args.Add(new Argument("int256", Int256(value, 256), false));

			return this;
		}
		/**
		 * Add a dynamic array of 8-bit integers.
		 * <p>
		 * The implementation is wasteful as we must pad to 32-bytes to store 1 byte.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt8Array(byte[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 8).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int8[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 16-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt16Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 16).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int16[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 24-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt24Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 24).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int24[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 32-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt32Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 32).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int32[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 40-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt40Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 40).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int40[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 48-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt48Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 48).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int48[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 56-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt56Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 56).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int56[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 64-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt64Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 64).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int64[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 72-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt72Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 72).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int72[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 80-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt80Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 80).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int80[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 88-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt88Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 88).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int88[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 96-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt96Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 96).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int96[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 104-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt104Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 104).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int104[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 112-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt112Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 112).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int112[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 120-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt120Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 120).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int120[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 128-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt128Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 128).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int128[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 136-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt136Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 136).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int136[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 144-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt144Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 144).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int144[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 152-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt152Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 152).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int152[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 160-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt160Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 160).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int160[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 168-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt168Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 168).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int168[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 176-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt176Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 176).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int176[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 184-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt184Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 184).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int184[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 192-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt192Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 192).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int192[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 200-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt200Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 200).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int200[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 208-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt208Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 208).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int208[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 216-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt216Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 216).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int216[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 224-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt224Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 224).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int224[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 232-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt232Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 232).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int232[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 240-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt240Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 240).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int240[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 248-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt248Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 248).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int248[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 256-bit integers.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddInt256Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Int256(i, 256).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("int256[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add an unsigned 8-bit integer.
		 * <p>
		 * The implementation is wasteful as we must pad to 32-bytes to store 1 byte.
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint8(byte value)
		{
			args.Add(new Argument("uint8", Uint256(value, 8), false));

			return this;
		}
		/**
		 * Add a 16-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint16(int value)
		{
			args.Add(new Argument("uint16", Uint256(value, 16), false));

			return this;
		}
		/**
		 * Add a 24-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint24(int value)
		{
			args.Add(new Argument("uint24", Uint256(value, 24), false));

			return this;
		}
		/**
		 * Add a 32-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint32(int value)
		{
			args.Add(new Argument("uint32", Uint256(value, 32), false));

			return this;
		}
		/**
		 * Add a 40-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint40(long value)
		{
			args.Add(new Argument("uint40", Uint256(value, 40), false));

			return this;
		}
		/**
		 * Add a 48-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint48(long value)
		{
			args.Add(new Argument("uint48", Uint256(value, 48), false));

			return this;
		}
		/**
		 * Add a 56-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint56(long value)
		{
			args.Add(new Argument("uint56", Uint256(value, 56), false));

			return this;
		}
		/**
		 * Add a 64-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint64(long value)
		{
			args.Add(new Argument("uint64", Uint256(value, 64), false));

			return this;
		}
		/**
		 * Add a 72-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint72(BigInteger value)
		{
			args.Add(new Argument("uint72", Uint256(value, 72), false));

			return this;
		}
		/**
		 * Add a 80-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint80(BigInteger value)
		{
			args.Add(new Argument("uint80", Uint256(value, 80), false));

			return this;
		}
		/**
		 * Add a 88-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint88(BigInteger value)
		{
			args.Add(new Argument("uint88", Uint256(value, 88), false));

			return this;
		}
		/**
		 * Add a 96-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint96(BigInteger value)
		{
			args.Add(new Argument("uint96", Uint256(value, 96), false));

			return this;
		}
		/**
		 * Add a 104-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint104(BigInteger value)
		{
			args.Add(new Argument("uint104", Uint256(value, 104), false));

			return this;
		}
		/**
		 * Add a 112-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint112(BigInteger value)
		{
			args.Add(new Argument("uint112", Uint256(value, 112), false));

			return this;
		}
		/**
		 * Add a 120-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint120(BigInteger value)
		{
			args.Add(new Argument("uint120", Uint256(value, 120), false));

			return this;
		}
		/**
		 * Add a 128-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint128(BigInteger value)
		{
			args.Add(new Argument("uint128", Uint256(value, 128), false));

			return this;
		}
		/**
		 * Add a 136-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint136(BigInteger value)
		{
			args.Add(new Argument("uint136", Uint256(value, 136), false));

			return this;
		}
		/**
		 * Add a 144-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint144(BigInteger value)
		{
			args.Add(new Argument("uint144", Uint256(value, 144), false));

			return this;
		}
		/**
		 * Add a 152-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint152(BigInteger value)
		{
			args.Add(new Argument("uint152", Uint256(value, 152), false));

			return this;
		}
		/**
		 * Add a 160-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint160(BigInteger value)
		{
			args.Add(new Argument("uint160", Uint256(value, 160), false));

			return this;
		}
		/**
		 * Add a 168-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint168(BigInteger value)
		{
			args.Add(new Argument("uint168", Uint256(value, 168), false));

			return this;
		}
		/**
		 * Add a 176-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint176(BigInteger value)
		{
			args.Add(new Argument("uint176", Uint256(value, 176), false));

			return this;
		}
		/**
		 * Add a 184-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint184(BigInteger value)
		{
			args.Add(new Argument("uint184", Uint256(value, 184), false));

			return this;
		}
		/**
		 * Add a 192-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint192(BigInteger value)
		{
			args.Add(new Argument("uint192", Uint256(value, 192), false));

			return this;
		}
		/**
		 * Add a 200-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint200(BigInteger value)
		{
			args.Add(new Argument("uint200", Uint256(value, 200), false));

			return this;
		}
		/**
		 * Add a 208-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint208(BigInteger value)
		{
			args.Add(new Argument("uint208", Uint256(value, 208), false));

			return this;
		}
		/**
		 * Add a 216-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint216(BigInteger value)
		{
			args.Add(new Argument("uint216", Uint256(value, 216), false));

			return this;
		}
		/**
		 * Add a 224-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint224(BigInteger value)
		{
			args.Add(new Argument("uint224", Uint256(value, 224), false));

			return this;
		}
		/**
		 * Add a 232-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint232(BigInteger value)
		{
			args.Add(new Argument("uint232", Uint256(value, 232), false));

			return this;
		}
		/**
		 * Add a 240-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint240(BigInteger value)
		{
			args.Add(new Argument("uint240", Uint256(value, 240), false));

			return this;
		}
		/**
		 * Add a 248-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint248(BigInteger value)
		{
			args.Add(new Argument("uint248", Uint256(value, 248), false));

			return this;
		}
		/**
		 * Add a 256-bit unsigned integer.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param value The integer to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint256(BigInteger value)
		{
			args.Add(new Argument("uint256", Uint256(value, 256), false));

			return this;
		}
		/**
		 * Add a dynamic array of unsigned 8-bit integers.
		 * <p>
		 * The implementation is wasteful as we must pad to 32-bytes to store 1 byte.
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint8Array(byte[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 8).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint8[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 16-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint16Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 16).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint16[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 24-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint24Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 24).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint24[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 32-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint32Array(int[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 32).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint32[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 40-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint40Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 40).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint40[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 48-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint48Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 48).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint48[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 56-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint56Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 56).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint56[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 64-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 */
		public ContractFunctionParameters AddUint64Array(long[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 64).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint64[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 72-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint72Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 72).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint72[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 80-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint80Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 80).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint80[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 88-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint88Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 88).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint88[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 96-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint96Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 96).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint96[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 104-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint104Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 104).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint104[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 112-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint112Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 112).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint112[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 120-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint120Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 120).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint120[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 128-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint128Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 128).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint128[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 136-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint136Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 136).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint136[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 144-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint144Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 144).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint144[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 152-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint152Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 152).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint152[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 160-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint160Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 160).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint160[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 168-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint168Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 168).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint168[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 176-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint176Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 176).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint176[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 184-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint184Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 184).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint184[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 192-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint192Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 192).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint192[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 200-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint200Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 200).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint200[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 208-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint208Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 208).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint208[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 216-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint216Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 216).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint216[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 224-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint224Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 224).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint224[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 232-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint232Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 232).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint232[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 240-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint240Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 240).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint240[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 248-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint248Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 248).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint248[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a dynamic array of 256-bit unsigned integers.
		 * <p>
		 * The value will be treated as unsigned during encoding (it will be zero-padded instead of sign-extended to 32
		 * bytes).
		 *
		 * @param intArray The array of integers to be Added
		 * @return {@code this}
		 * @ if {@code bigInt.Sign < 0}.
		 */
		public ContractFunctionParameters AddUint256Array(BigInteger[] intArray)
		{
			ByteString arrayBytes = ByteString.CopyFrom([.. intArray.SelectMany(i => Uint256(i, 256).ToByteArray())]);

			arrayBytes = Uint256(intArray.Length, 32).Concat(arrayBytes);

			args.Add(new Argument("uint256[]", arrayBytes, true));

			return this;
		}
		/**
		 * Add a {@value ADDRESS_LEN_HEX}-character hex-encoded Solidity Address parameter with the type {@code Address}.
		 * <p>
		 * Note: Adding a {@code Address payable} or {@code contract} parameter must also use this function as the ABI does
		 * not support those types directly.
		 *
		 * @param Address The Address to be Added
		 * @return {@code this}
		 * @ if the Address is not exactly {@value ADDRESS_LEN_HEX} characters long or fails
		 *                                  to decode as hexadecimal.
		 */
		public ContractFunctionParameters AddAddress(string address)
		{
			byte[] addressBytes = DecodeAddress(address);

			args.Add(new Argument("address", LeftPad32(ByteString.CopyFrom(addressBytes)), false));

			return this;
		}
		/**
		 * Add an array of {@value ADDRESS_LEN_HEX}-character hex-encoded Solidity Addresses as a {@code Address[]} param.
		 *
		 * @param Addresses The array of Addresses to be Added
		 * @return {@code this}
		 * @ if any value is not exactly {@value ADDRESS_LEN_HEX} characters long or fails to
		 *                                  decode as hexadecimal.
		 * @     if any value in the array is null.
		 */
		public ContractFunctionParameters AddAddressArray(string[] addresses)
		{
			ByteString addressArray = EncodeArray(addresses.Select(_ =>
			{
				byte[] address = DecodeAddress(_);

				return LeftPad32(ByteString.CopyFrom(address));
			}));

			args.Add(new Argument("address[]", addressArray, true));

			return this;
		}
		/**
		 * Add a Solidity function reference as a {@value ADDRESS_LEN}-byte contract Address and a
		 * {@value SELECTOR_LEN}-byte function selector.
		 *
		 * @param Address  a hex-encoded {@value ADDRESS_LEN_HEX}-character Solidity Address.
		 * @param selector a
		 * @return {@code this}
		 * @ if {@code Address} is not {@value ADDRESS_LEN_HEX} characters or
		 *                                  {@code selector} is not {@value SELECTOR_LEN} bytes.
		 */
		public ContractFunctionParameters AddFunction(string address, byte[] selector)
		{
			return AddFunction(DecodeAddress(address), selector);
		}
		/**
		 * Add a Solidity function reference as a {@value ADDRESS_LEN}-byte contract Address and a constructed
		 * {@link ContractFunctionSelector}. The {@link ContractFunctionSelector} may not be modified after this call.
		 *
		 * @param Address  The Address used in the function to be Added
		 * @param selector The selector used in the function to be Added
		 * @return {@code this}
		 * @ if {@code Address} is not {@value ADDRESS_LEN_HEX} characters.
		 */
		public ContractFunctionParameters AddFunction(string address, ContractFunctionSelector selector)
		{
			// allow the `FunctionSelector` to be reused multiple times
			return AddFunction(DecodeAddress(address), selector.Finish());
		}
		private ContractFunctionParameters AddFunction(byte[] address, byte[] selector)
		{
			if (selector.Length != SELECTOR_LEN)
			{
				throw new ArgumentException("function selectors must be 4 bytes or 8 hex chars");
			}

			ByteString.Output output = ByteString.NewOutput(ADDRESS_LEN + SELECTOR_LEN);
			try
			{
				output.write(address, 0, address.Length);
				output.write(selector, 0, selector.Length);

				// function reference Encodes as `bytes24`
				args.Add(new Argument("function", RightPad32(output.toByteString()), false));

				return this;
			}
			finally
			{
				try
				{
					output.close();
				}
				catch (Exception ignored)
				{
					// do nothing
				}
			}
		}

		/**
		 * Get the encoding of the currently Added parameters as a {@link ByteString}.
		 * <p>
		 * You may continue to Add parameters and call this again.
		 *
		 * @return the Solidity encoding of the call parameters in the order they were Added.
		 */
		public ByteString ToBytes(string? funcName)
		{
			// offset for dynamic-length data, immediately after value arguments
			var dynamicOffset = args.Count * 32;

			var paramsBytes = new List<ByteString>(args.Count + 1);

			var dynamicArgs = new List<ByteString>();

			ContractFunctionSelector? functionSelector = funcName != null ? new ContractFunctionSelector(funcName) : null;

			// iterate the arguments and determine whether they are dynamic or not
			foreach (Argument arg in args)
			{
				if (functionSelector != null)
				{
					functionSelector.AddParamType(arg.Type);
				}

				if (arg.IsDynamic)
				{
					// dynamic arguments supply their offset in value position and append their data at
					// that offset
					paramsBytes.Add(Int256(dynamicOffset, 256));
					dynamicArgs.Add(arg.Value);
					dynamicOffset += arg.Value.Length;
				}
				else
				{
					// value arguments are dropped in the current arg position
					paramsBytes.Add(arg.Value);
				}
			}

			if (functionSelector != null)
			{
				paramsBytes.Insert(0, ByteString.CopyFrom(functionSelector.Finish()));
			}

			paramsBytes.AddRange(dynamicArgs);

			return ByteString.CopyFrom([.. paramsBytes.SelectMany(_ => _.ToByteArray())]);
		}
	}
}