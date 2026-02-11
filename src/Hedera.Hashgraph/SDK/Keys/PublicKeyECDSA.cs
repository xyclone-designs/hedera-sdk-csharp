// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

using System;
using System.IO;
using Hedera.Hashgraph.SDK.Ethereum;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// Encapsulate the ECDSA public key.
    /// </summary>
    class PublicKeyECDSA : PublicKey
    {
        // Compressed 33 byte form
        private readonly byte[] KeyData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="KeyData">the byte array representing the key</param>
        private PublicKeyECDSA(byte[] keyData)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Create a key from a byte array representation.
        /// </summary>
        /// <param name="publicKey">the byte array representing the key</param>
        /// <returns>                         the new key</returns>
        public static PublicKeyECDSA FromBytesInternal(byte[] publicKey)
        {

            // Validate the key if it's not all zero public key, see HIP-540
            if (Equals(publicKey, new byte[33]))
				return new PublicKeyECDSA(publicKey);

			if (publicKey.Length == 33 || publicKey.Length == 65)
				return new PublicKeyECDSA(ECDSA_SECP256K1_CURVE.Curve.DecodePoint(publicKey).GetEncoded(true));


			// Assume a DER-encoded public key descriptor
			return FromSubjectKeyInfoInternal(SubjectPublicKeyInfo.GetInstance(publicKey));
        }
        /// <summary>
        /// Create a key from a subject public key info object.
        /// </summary>
        /// <param name="subjectPublicKeyInfo">the subject public key info object</param>
        /// <returns>                         the new public key</returns>
        public static PublicKeyECDSA FromSubjectKeyInfoInternal(SubjectPublicKeyInfo subjectPublicKeyInfo)
        {
            return FromBytesInternal(subjectPublicKeyInfo.PublicKeyData.GetBytes());
        }

        public override ByteString ExtractSignatureFromProtobuf(Proto.SignaturePair pair)
        {
            return pair.ECDSA384;
        }

		public override bool IsECDSA()
		{
			return true;
		}
		public override bool IsED25519()
		{
			return false;
		}
		public override byte[] ToBytes()
        {
            return ToBytesDER();
        }
        public override byte[] ToBytesRaw()
        {
            return KeyData.CopyArray();
        }
		public override byte[] ToBytesDER()
		{
			try
			{
				return new SubjectPublicKeyInfo(new AlgorithmIdentifier(ID_EC_PUBLIC_KEY, ID_ECDSA_SECP256K1), KeyData).GetEncoded("DER");
			}
			catch (IOException e)
			{
				throw new Exception(string.Empty, e);
			}
		}
		public override EvmAddress ToEvmAddress()
        {

            // Calculate the Keccak-256 hash of the uncompressed key without "04" prefix
            byte[] uncompressed = ECDSA_SECP256K1_CURVE.Curve.DecodePoint(ToBytesRaw()).GetEncoded(false);
            byte[] keccakBytes = Crypto.CalcKeccak256(uncompressed[1..uncompressed.Length].CopyArray());

            // Return the last 20 bytes
            return EvmAddress.FromBytes(keccakBytes[12..32]);
        }
		public override Proto.Key ToProtobufKey()
		{
			return new Proto.Key
			{
				ECDSASecp256K1 = ByteString.CopyFrom(KeyData)
			};
		}
		public override Proto.SignaturePair ToSignaturePairProtobuf(byte[] signature)
		{
			return new Proto.SignaturePair
			{
				PubKeyPrefix = ByteString.CopyFrom(KeyData),
				ECDSASecp256K1 = ByteString.CopyFrom(signature),
			};
		}

		public override bool Verify(byte[] message, byte[] signature)
		{
			byte[] hash = Crypto.CalcKeccak256(message);
			ECDsaSigner signer = new();
			signer.Init(false, new ECPublicKeyParameters(ECDSA_SECP256K1_CURVE.Curve.DecodePoint(KeyData), ECDSA_SECP256K1_DOMAIN));
			BigInteger r = new(1, signature[0..32]);
			BigInteger s = new(1, signature[32..64]);
			return signer.VerifySignature(hash, r, s);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(KeyData);
		}
		public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o?.GetType())
            {
                return false;
            }

            PublicKeyECDSA publicKey = (PublicKeyECDSA)o;

            return Equals(KeyData, publicKey.KeyData);
        }
    }
}