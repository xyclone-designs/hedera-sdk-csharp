// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.Reference.Cryptography;
using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ethereum;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;

using System;

namespace Hedera.Hashgraph.SDK.Cryptography
{
    /// <include file="Key.cs.xml" path='docs/member[@name="T:Key"]/*' />
    public abstract class Key : IKey
    {
        internal static readonly DerObjectIdentifier ID_ED25519 = new ("1.3.101.112");
        internal static readonly DerObjectIdentifier ID_ECDSA_SECP256K1 = new ("1.3.132.0.10");
        internal static readonly DerObjectIdentifier ID_EC_PUBLIC_KEY = new ("1.2.840.10045.2.1");
        internal static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
        internal static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (ECDSA_SECP256K1_CURVE.Curve, ECDSA_SECP256K1_CURVE.G, ECDSA_SECP256K1_CURVE.N, ECDSA_SECP256K1_CURVE.H);

		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.FromBytes(System.Byte[])"]/*' />
		public static Key? FromBytes(byte[] bytes)
		{
			return FromProtobufKey(Proto.Services.Key.Parser.ParseFrom(bytes));
		}
		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.FromProtobufKey(Proto.Services.Key)"]/*' />
		public static Key? FromProtobufKey(Proto.Services.Key key)
        {
            return key.KeyCase switch
            {
                Proto.Services.Key.KeyOneofCase.Ed25519 => PublicKeyED25519.FromBytesInternal(key.Ed25519.ToByteArray()),
                Proto.Services.Key.KeyOneofCase.ECDSASecp256K1 when (key.ECDSASecp256K1.Length == 20) => new EvmAddress(key.ECDSASecp256K1.ToByteArray()),
                Proto.Services.Key.KeyOneofCase.ECDSASecp256K1 => PublicKeyECDSA.FromBytesInternal(key.ECDSASecp256K1.ToByteArray()),
                Proto.Services.Key.KeyOneofCase.KeyList => KeyList.FromProtobuf(key.KeyList, null),
                Proto.Services.Key.KeyOneofCase.ThresholdKey => KeyList.FromProtobuf(key.ThresholdKey.Keys, key.ThresholdKey.Threshold),
                Proto.Services.Key.KeyOneofCase.ContractId => ContractId.FromProtobuf(key.ContractId),
                Proto.Services.Key.KeyOneofCase.DelegatableContractId => DelegateContractId.FromProtobuf(key.DelegatableContractId),
                Proto.Services.Key.KeyOneofCase.None or 
                Proto.Services.Key.KeyOneofCase.RSA3072 or 
                Proto.Services.Key.KeyOneofCase.ECDSA384 => null,

                _ => throw new InvalidOperationException("Key#fromProtobuf: unhandled key case: " + key.KeyCase),
            };
        }

        /// <include file="Key.cs.xml" path='docs/member[@name="M:Key.ToProtobufKey"]/*' />
        public abstract Proto.Services.Key ToProtobufKey();

		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.ToBytes"]/*' />
		public virtual byte[] ToBytes()
        {
            return ToProtobufKey().ToByteArray();
        }
    }
}
