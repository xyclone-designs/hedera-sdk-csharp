// SPDX-License-Identifier: Apache-2.0
using Java.Nio.Charset.StandardCharsets;
using Java.Util;
using Javax.Annotation;
using Org.Bouncycastle.Jcajce.Provider.Digest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Builder class for Solidity function selectors.
    /// </summary>
    public sealed class ContractFunctionSelector
    {
        private Keccak.Digest256 digest;
        private bool needsComma = false;
        private byte[] finished = null;
        /// <summary>
        /// Start building a selector for a function with a given name.
        /// </summary>
        /// <param name="funcName">The name of the function</param>
        public ContractFunctionSelector(string funcName)
        {
            digest = new Digest256();
            digest.Update(funcName.GetBytes(US_ASCII));
            digest.Update((byte)'(');
        }

        /// <summary>
        /// Add a string parameter.
        /// </summary>
        /// <returns>                         string</returns>
        public ContractFunctionSelector AddString()
        {
            return AddParamType("string");
        }

        /// <summary>
        /// Add a string array parameter.
        /// </summary>
        /// <returns>                         array string</returns>
        public ContractFunctionSelector AddStringArray()
        {
            return AddParamType("string[]");
        }

        /// <summary>
        /// Add a bytes parameter.
        /// </summary>
        /// <returns>                         bytes</returns>
        public ContractFunctionSelector AddBytes()
        {
            return AddParamType("bytes");
        }

        /// <summary>
        /// Add a bytes array parameter.
        /// </summary>
        /// <returns>                         bytes array</returns>
        public ContractFunctionSelector AddBytesArray()
        {
            return AddParamType("bytes[]");
        }

        /// <summary>
        /// Add a bytes 32 parameter.
        /// </summary>
        /// <returns>                         bytes 32</returns>
        public ContractFunctionSelector AddBytes32()
        {
            return AddParamType("bytes32");
        }

        /// <summary>
        /// Add a bytes 32 array parameter.
        /// </summary>
        /// <returns>                         bytes 32 array</returns>
        public ContractFunctionSelector AddBytes32Array()
        {
            return AddParamType("bytes32[]");
        }

        /// <summary>
        /// Add a bool parameter.
        /// </summary>
        /// <returns>                         bool</returns>
        public ContractFunctionSelector AddBool()
        {
            return AddParamType("bool");
        }

        /// <summary>
        /// Add an int 8 parameter.
        /// </summary>
        /// <returns>                         int 8</returns>
        public ContractFunctionSelector AddInt8()
        {
            return AddParamType("int8");
        }

        /// <summary>
        /// Add an int 32 parameter.
        /// </summary>
        /// <returns>                         int 32</returns>
        public ContractFunctionSelector AddInt32()
        {
            return AddParamType("int32");
        }

        /// <summary>
        /// Add an int 64 parameter.
        /// </summary>
        /// <returns>                         int 64</returns>
        public ContractFunctionSelector AddInt64()
        {
            return AddParamType("int64");
        }

        /// <summary>
        /// Add an int 256 parameter.
        /// </summary>
        /// <returns>                         int 256</returns>
        public ContractFunctionSelector AddInt256()
        {
            return AddParamType("int256");
        }

        /// <summary>
        /// Add an int 8 array parameter.
        /// </summary>
        /// <returns>                         int 8 array</returns>
        public ContractFunctionSelector AddInt8Array()
        {
            return AddParamType("int8[]");
        }

        /// <summary>
        /// Add an int 32 array parameter.
        /// </summary>
        /// <returns>                         int 32 array</returns>
        public ContractFunctionSelector AddInt32Array()
        {
            return AddParamType("int32[]");
        }

        /// <summary>
        /// Add an int 64 array parameter.
        /// </summary>
        /// <returns>                         int 64 array</returns>
        public ContractFunctionSelector AddInt64Array()
        {
            return AddParamType("int64[]");
        }

        /// <summary>
        /// Add an int 256 array parameter.
        /// </summary>
        /// <returns>                         int 256 array</returns>
        public ContractFunctionSelector AddInt256Array()
        {
            return AddParamType("int256[]");
        }

        /// <summary>
        /// Add an unsigned int 8 parameter.
        /// </summary>
        /// <returns>                         unsigned int 8</returns>
        public ContractFunctionSelector AddUint8()
        {
            return AddParamType("uint8");
        }

        /// <summary>
        /// Add an unsigned int 32 parameter.
        /// </summary>
        /// <returns>                         unsigned int 32</returns>
        public ContractFunctionSelector AddUint32()
        {
            return AddParamType("uint32");
        }

        /// <summary>
        /// Add an unsigned int 64 parameter.
        /// </summary>
        /// <returns>                         unsigned int 64</returns>
        public ContractFunctionSelector AddUint64()
        {
            return AddParamType("uint64");
        }

        /// <summary>
        /// Add an unsigned int 256 parameter.
        /// </summary>
        /// <returns>                         unsigned int 256</returns>
        public ContractFunctionSelector AddUint256()
        {
            return AddParamType("uint256");
        }

        /// <summary>
        /// Add an unsigned int 8 array parameter.
        /// </summary>
        /// <returns>                         unsigned int 8 array</returns>
        public ContractFunctionSelector AddUint8Array()
        {
            return AddParamType("uint8[]");
        }

        /// <summary>
        /// Add an unsigned int 32 array parameter.
        /// </summary>
        /// <returns>                         unsigned int 32 array</returns>
        public ContractFunctionSelector AddUint32Array()
        {
            return AddParamType("uint32[]");
        }

        /// <summary>
        /// Add an unsigned int 64 array parameter.
        /// </summary>
        /// <returns>                         unsigned int 64 array</returns>
        public ContractFunctionSelector AddUint64Array()
        {
            return AddParamType("uint64[]");
        }

        /// <summary>
        /// Add an unsigned int 256 array parameter.
        /// </summary>
        /// <returns>                         unsigned int 256 array</returns>
        public ContractFunctionSelector AddUint256Array()
        {
            return AddParamType("uint256[]");
        }

        /// <summary>
        /// Add an address parameter.
        /// </summary>
        /// <returns>                         address</returns>
        public ContractFunctionSelector AddAddress()
        {
            return AddParamType("address");
        }

        /// <summary>
        /// Add an address array parameter.
        /// </summary>
        /// <returns>                         address array</returns>
        public ContractFunctionSelector AddAddressArray()
        {
            return AddParamType("address[]");
        }

        /// <summary>
        /// Add a function parameter.
        /// </summary>
        /// <returns>                         function.</returns>
        public ContractFunctionSelector AddFunction()
        {
            return AddParamType("function");
        }

        /// <summary>
        /// Add a Solidity type name to this selector;
        /// {@see https://solidity.readthedocs.io/en/v0.5.9/types.html}
        /// </summary>
        /// <param name="typeName">the name of the Solidity type for a parameter.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="IllegalStateException">if {@link #finish()} has already been called.</exception>
        ContractFunctionSelector AddParamType(string typeName)
        {
            if (finished != null)
            {
                throw new InvalidOperationException("FunctionSelector already finished");
            }

            Objects.RequireNonNull(digest);
            if (needsComma)
            {
                digest.Update((byte)',');
            }

            digest.Update(typeName.GetBytes(US_ASCII));
            needsComma = true;
            return this;
        }

        /// <summary>
        /// Complete the function selector after all parameters have been added and get the selector
        /// bytes.
        /// <p>
        /// No more parameters may be added after this method call.
        /// <p>
        /// However, this can be called multiple times; it will always return the same result.
        /// </summary>
        /// <returns>the computed selector bytes.</returns>
        byte[] Finish()
        {
            if (finished == null)
            {
                Objects.RequireNonNull(digest);
                digest.Update((byte)')');
                finished = Array.CopyOf(digest.Digest(), 4);

                // release digest state
                digest = null;
            }

            return finished;
        }
    }
}