// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Contract;
using Hedera.Hashgraph.SDK.Ethereum;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;

using System;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <include file="Key.cs.xml" path='docs/member[@name="T:Key"]/*' />
    public abstract class Key
    {
        internal static readonly DerObjectIdentifier ID_ED25519 = new ("1.3.101.112");
        internal static readonly DerObjectIdentifier ID_ECDSA_SECP256K1 = new ("1.3.132.0.10");
        internal static readonly DerObjectIdentifier ID_EC_PUBLIC_KEY = new ("1.2.840.10045.2.1");
        internal static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
        internal static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (ECDSA_SECP256K1_CURVE.Curve, ECDSA_SECP256K1_CURVE.G, ECDSA_SECP256K1_CURVE.N, ECDSA_SECP256K1_CURVE.H);

		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.FromBytes(System.Byte[])"]/*' />
		public static Key? FromBytes(byte[] bytes)
		{
			return FromProtobufKey(Proto.Key.Parser.ParseFrom(bytes));
		}
		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.FromProtobufKey(Proto.Key)"]/*' />
		public static Key? FromProtobufKey(Proto.Key key)
        {
            return key.KeyCase switch
            {
                Proto.Key.KeyOneofCase.Ed25519 => PublicKeyED25519.FromBytesInternal(key.Ed25519.ToByteArray()),
                Proto.Key.KeyOneofCase.ECDSASecp256K1 when (key.ECDSASecp256K1.Length == 20) => new EvmAddress(key.ECDSASecp256K1.ToByteArray()),
                Proto.Key.KeyOneofCase.ECDSASecp256K1 => PublicKeyECDSA.FromBytesInternal(key.ECDSASecp256K1.ToByteArray()),
                Proto.Key.KeyOneofCase.KeyList => KeyList.FromProtobuf(key.KeyList, null),
                Proto.Key.KeyOneofCase.ThresholdKey => KeyList.FromProtobuf(key.ThresholdKey.Keys, key.ThresholdKey.Threshold),
                Proto.Key.KeyOneofCase.ContractID => ContractId.FromProtobuf(key.ContractID),
                Proto.Key.KeyOneofCase.DelegatableContractId => DelegateContractId.FromProtobuf(key.DelegatableContractId),
                Proto.Key.KeyOneofCase.None or 
                Proto.Key.KeyOneofCase.RSA3072 or 
                Proto.Key.KeyOneofCase.ECDSA384 => null,

                _ => throw new InvalidOperationException("Key#fromProtobuf: unhandled key case: " + key.KeyCase),
            };
        }

        /// <include file="Key.cs.xml" path='docs/member[@name="M:Key.ToProtobufKey"]/*' />
        public abstract Proto.Key ToProtobufKey();

		/// <include file="Key.cs.xml" path='docs/member[@name="M:Key.ToBytes"]/*' />
		public virtual byte[] ToBytes()
        {
            return ToProtobufKey().ToByteArray();
        }
    }
}