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
    /// <summary>
    /// A common base for the signing authority or key that entities in Hedera may have.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/key">Hedera Documentation</a>
    /// </summary>
    /// <remarks>
    /// @seeKeyList
    /// @seePublicKey
    /// </remarks>
    public abstract class Key
    {
        internal static readonly DerObjectIdentifier ID_ED25519 = new ("1.3.101.112");
        internal static readonly DerObjectIdentifier ID_ECDSA_SECP256K1 = new ("1.3.132.0.10");
        internal static readonly DerObjectIdentifier ID_EC_PUBLIC_KEY = new ("1.2.840.10045.2.1");
        internal static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
        internal static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (ECDSA_SECP256K1_CURVE.Curve, ECDSA_SECP256K1_CURVE.G, ECDSA_SECP256K1_CURVE.N, ECDSA_SECP256K1_CURVE.H);

		/// <summary>
		/// Create Key from proto.Key byte array
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns>Key representation</returns>
		/// <exception cref="InvalidProtocolBufferException"></exception>
		public static Key? FromBytes(byte[] bytes)
		{
			return FromProtobufKey(Proto.Key.Parser.ParseFrom(bytes));
		}
		/// <summary>
		/// Create a specific key type from the protobuf.
		/// </summary>
		/// <param name="key">the protobuf key of unknown type</param>
		/// <returns>                         the differentiated key</returns>
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

        /// <summary>
        /// Serialize this key as a protobuf object
        /// </summary>
        public abstract Proto.Key ToProtobufKey();

		/// <summary>
		/// Create the byte array.
		/// </summary>
		/// <returns>                         the byte array representation</returns>
		public virtual byte[] ToBytes()
        {
            return ToProtobufKey().ToByteArray();
        }
    }
}