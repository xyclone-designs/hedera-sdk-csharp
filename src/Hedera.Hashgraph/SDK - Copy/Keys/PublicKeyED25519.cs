// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;

using System;
using System.IO;
using System.Text;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// Encapsulate the ED25519 public key.
    /// </summary>
    class PublicKeyED25519 : PublicKey
    {
        private readonly byte[] KeyData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keyData">the byte array representing the key</param>
        private PublicKeyED25519(byte[] keyData)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Create a key from a byte array representation.
        /// </summary>
        /// <param name="publicKey">the byte array representing the key</param>
        /// <returns>                         the new key</returns>
        public static PublicKeyED25519 FromBytesInternal(byte[] publicKey)
        {
            if (publicKey.Length == Ed25519.PublicKeySize)
            {

                // Validate the key if it's not all zero public key, see HIP-540
                if (!Equals(publicKey, new byte[32]))
					// Will throw if the key is invalid
					new Ed25519PublicKeyParameters(publicKey, 0);


				// If this is a 32 byte string, assume an Ed25519 public key
				return new PublicKeyED25519(publicKey);
            }


            // Assume a DER-encoded public key descriptor
            return FromSubjectKeyInfoInternal(SubjectPublicKeyInfo.GetInstance(publicKey));
        }
        /// <summary>
        /// Create a key from a subject public key info object.
        /// </summary>
        /// <param name="subjectPublicKeyInfo">the subject public key info object</param>
        /// <returns>                         the new public key</returns>
        public static PublicKeyED25519 FromSubjectKeyInfoInternal(SubjectPublicKeyInfo subjectPublicKeyInfo)
        {
            return new PublicKeyED25519(subjectPublicKeyInfo.PublicKey.GetBytes());
        }

		public override ByteString ExtractSignatureFromProtobuf(Proto.SignaturePair pair)
		{
			return pair.Ed25519;
		}
		
		public override bool IsECDSA()
		{
			return false;
		}
		public override bool IsED25519()
		{
			return true;
		}
		public override byte[] ToBytes()
        {
            return ToBytesRaw();
        }
        public override byte[] ToBytesRaw()
        {
            return KeyData;
        }
		public override byte[] ToBytesDER()
		{
			try
			{
				return new SubjectPublicKeyInfo(new AlgorithmIdentifier(ID_ED25519), KeyData)
                    .GetEncoded("DER");
			}
			catch (IOException e)
			{
				throw new Exception(string.Empty, e);
			}
		}
		public override EvmAddress ToEvmAddress()
        {
            throw new InvalidOperationException("unsupported operation on Ed25519PublicKey");
        }
		public override Proto.Key ToProtobufKey()
		{
			return new Proto.Key
			{
				Ed25519 = ByteString.CopyFrom(KeyData)
			};
		}
		public override Proto.SignaturePair ToSignaturePairProtobuf(byte[] signature)
		{
			return new Proto.SignaturePair
			{
				PubKeyPrefix = ByteString.CopyFrom(KeyData),
				Ed25519 = ByteString.CopyFrom(signature),
			};
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

			if (o == null || GetType() != o.GetType())
			{
				return false;
			}

			PublicKeyED25519 publicKey = (PublicKeyED25519)o;

			return Equals(KeyData, publicKey.KeyData);
		}
		public override bool Verify(byte[] message, byte[] signature)
		{
			return Ed25519.Verify(signature, 0, KeyData, 0, message, 0, message.Length);
		}
	}
}