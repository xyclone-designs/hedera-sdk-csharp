// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Crypto;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Io;
using Java.Math;
using Java.Util;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using Org.BouncyCastle.Math;
using System.IO;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// Encapsulate the ECDSA public key.
    /// </summary>
    class PublicKeyECDSA : PublicKey
    {
        // Compressed 33 byte form
        private byte[] keyData;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keyData">the byte array representing the key</param>
        private PublicKeyECDSA(byte[] keyData)
        {
            keyData = keyData;
        }

        /// <summary>
        /// Create a key from a byte array representation.
        /// </summary>
        /// <param name="publicKey">the byte array representing the key</param>
        /// <returns>                         the new key</returns>
        static PublicKeyECDSA FromBytesInternal(byte[] publicKey)
        {

            // Validate the key if it's not all zero public key, see HIP-540
            if (Equals(publicKey, new byte[33]))
            {
                return new PublicKeyECDSA(publicKey);
            }

            if (publicKey.Length == 33 || publicKey.Length == 65)
            {
                return new PublicKeyECDSA(Key.ECDSA_SECP256K1_CURVE.Curve().DecodePoint(publicKey).Encoded(true));
            }


            // Assume a DER-encoded public key descriptor
            return FromSubjectKeyInfoInternal(SubjectPublicKeyInfo.GetInstance(publicKey));
        }

        /// <summary>
        /// Create a key from a subject public key info object.
        /// </summary>
        /// <param name="subjectPublicKeyInfo">the subject public key info object</param>
        /// <returns>                         the new public key</returns>
        static PublicKeyECDSA FromSubjectKeyInfoInternal(SubjectPublicKeyInfo subjectPublicKeyInfo)
        {
            return FromBytesInternal(subjectPublicKeyInfo.PublicKeyData().Bytes());
        }

        override ByteString ExtractSignatureFromProtobuf(Proto.SignaturePair pair)
        {
            return pair.ECDSA384;
        }

        public override bool Verify(byte[] message, byte[] signature)
        {
            var hash = CalcKeccak256(message);
            ECDSASigner signer = new ECDSASigner();
            signer.Init(false, new ECPublicKeyParameters(Key.ECDSA_SECP256K1_CURVE.Curve().DecodePoint(keyData), Key.ECDSA_SECP256K1_DOMAIN));
            BigInteger r = new BigInteger(1, Array.CopyOf(signature, 32));
            BigInteger s = new BigInteger(1, Array.CopyOfRange(signature, 32, 64));
            return signer.VerifySignature(hash, r, s);
        }

        public override Proto.Key ToProtobufKey()
        {
            return new Proto.Key
            {
				ECDSASecp256K1 = ByteString.CopyFrom(keyData)
			};
        }

		public override Proto.SignaturePair ToSignaturePairProtobuf(byte[] signature)
        {
            return new Proto.SignaturePair
            {
				PubKeyPrefix = ByteString.CopyFrom(keyData),
				ECDSASecp256K1 = ByteString.CopyFrom(signature),
			};
        }

        public override byte[] ToBytesDER()
        {
            try
            {
                return new SubjectPublicKeyInfo(new AlgorithmIdentifier(ID_EC_PUBLIC_KEY, ID_ECDSA_SECP256K1), keyData).Encoded("DER");
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }
        }

        public override byte[] ToBytes()
        {
            return ToBytesDER();
        }

        public override byte[] ToBytesRaw()
        {
            return Array.CopyOf(keyData, keyData.Length);
        }

        public override EvmAddress ToEvmAddress()
        {

            // Calculate the Keccak-256 hash of the uncompressed key without "04" prefix
            byte[] uncompressed = Key.ECDSA_SECP256K1_CURVE.Curve().DecodePoint(ToBytesRaw()).Encoded(false);
            byte[] keccakBytes = CalcKeccak256(Array.CopyOfRange(uncompressed, 1, uncompressed.Length));

            // Return the last 20 bytes
            return EvmAddress.FromBytes(Array.CopyOfRange(keccakBytes, 12, 32));
        }

        public override bool Equals(object? o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            PublicKeyECDSA publicKey = (PublicKeyECDSA)o;
            return Equals(keyData, publicKey.keyData);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(keyData);
        }

        public override bool IsED25519()
        {
            return false;
        }

        public override bool IsECDSA()
        {
            return true;
        }
    }
}