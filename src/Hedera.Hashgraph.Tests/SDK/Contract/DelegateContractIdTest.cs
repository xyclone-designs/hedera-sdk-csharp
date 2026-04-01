// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Contract;

using Org.BouncyCastle.Utilities.Encoders;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Contract
{
    class DelegateContractIdTest
    {
        public virtual void FromString()
        {
            Verifier.Verify(DelegateContractId.FromString("0.0.5005").ToString());
        }

        public virtual void FromSolidityAddress()
        {
            Verifier.Verify(DelegateContractId.FromSolidityAddress("000000000000000000000000000000000000138D").ToString());
        }

        public virtual void FromSolidityAddressWith0x()
        {
            Verifier.Verify(DelegateContractId.FromSolidityAddress("0x000000000000000000000000000000000000138D").ToString());
        }

        public virtual void ToBytes()
        {
            Verifier.Verify(Hex.ToHexString(new DelegateContractId(0, 0, 5005).ToBytes()));
        }

        public virtual void FromBytes()
        {
            Verifier.Verify(DelegateContractId.FromBytes(new DelegateContractId(0, 0, 5005).ToBytes()).ToString());
        }

        public virtual void ToSolidityAddress()
        {
            Verifier.Verify(new DelegateContractId(0, 0, 5005).ToEvmAddress());
        }

        public virtual void FromEvmAddressIncorrectSizeTooShort()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "abc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeTooLong()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0123456789abcdef0123456789abcdef0123456789abcdef");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressIncorrectSizeWith0xPrefix()
        {
			ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                DelegateContractId.FromEvmAddress(0, 0, "0xabc123");
            });
            
            Assert.Contains(exception.Message, "Solidity addresses must be 20 bytes or 40 hex chars");
        }

        public virtual void FromEvmAddressCorrectSize()
        {
            string correctAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, correctAddress);

            Assert.NotNull(id.EvmAddress);
            Assert.Equal(Hex.ToHexString(id.EvmAddress), "742d35cc6634c0532925a3b844bc454e4438f44e");
        }

        public virtual void ToEvmAddressNormalContractId()
        {
            DelegateContractId id = new DelegateContractId(0, 0, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressWithDifferentShardAndRealm()
        {
            DelegateContractId id = new DelegateContractId(1, 1, 123);
            Assert.Equal(id.ToEvmAddress(), "000000000000000000000000000000000000007b");
        }

        public virtual void ToEvmAddressLongZeroAddress()
        {
            string longZeroAddress = "00000000000000000000000000000000000004d2";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, longZeroAddress);
            Assert.Equal(id.ToEvmAddress(), longZeroAddress.ToLower());
        }

        public virtual void ToEvmAddressNormalEvmAddress()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(0, 0, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }

        public virtual void ToEvmAddressNormalEvmAddressWithShardAndRealm()
        {
            string evmAddress = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
            DelegateContractId id = DelegateContractId.FromEvmAddress(1, 1, evmAddress);
            string expected = evmAddress.ToLower();
            Assert.Equal(id.ToEvmAddress(), expected);
        }
    }
}