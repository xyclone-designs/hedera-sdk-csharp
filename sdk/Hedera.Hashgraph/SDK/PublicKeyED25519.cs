using Google.Protobuf;
using Hedera.Hashgraph.Proto;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.IO;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Encapsulate the ED25519 public key.
 */
	public class PublicKeyED25519 : PublicKey
	{

		private readonly byte[] keyData;

		/**
		 * Constructor.
		 *
		 * @param keyData                   the byte array representing the key
		 */
		private PublicKeyED25519(byte[] keyData)
		{
			this.keyData = keyData;
		}
		/**
		 * Create a key from a byte array representation.
		 *
		 * @param publicKey                 the byte array representing the key
		 * @return                          the new key
		 */
		public static PublicKeyED25519 FromBytesInternal(byte[] publicKey)
		{
			if (publicKey.Length == Ed25519.PUBLIC_KEY_SIZE)
			{
				// Validate the key if it's not all zero public key, see HIP-540
				if (!Arrays.equals(publicKey, new byte[32]))
				{
					// Will throw if the key is invalid
					new Ed25519PublicKeyParameters(publicKey, 0);
				}
				// If this is a 32 byte string, assume an Ed25519 public key
				return new PublicKeyED25519(publicKey);
			}

			// Assume a DER-encoded public key descriptor
			return fromSubjectKeyInfoInternal(SubjectPublicKeyInfo.getInstance(publicKey));
		}
		/**
		 * Create a key from a subject public key info object.
		 *
		 * @param subjectPublicKeyInfo      the subject public key info object
		 * @return                          the new public key
		 */
		public static PublicKeyED25519 FromSubjectKeyInfoInternal(SubjectPublicKeyInfo subjectPublicKeyInfo)
		{
			return new PublicKeyED25519(subjectPublicKeyInfo.getPublicKeyData().getBytes());
		}

		

		public override bool verify(byte[] message, byte[] signature)
		{
			return Ed25519.verify(signature, 0, keyData, 0, message, 0, message.Length);
		}

		public override Proto.Key ToProtobufKey() 
		{
			return new Proto.Key
			{
				Ed25519 = ByteString.CopyFrom(keyData)
			};
		}

		public override Proto.SignaturePair ToSignaturePairProtobuf(byte[] signature)
		{
			return new Proto.SignaturePair
			{
				PubKeyPrefix = ByteString.CopyFrom(keyData),
				Ed25519 = ByteString.CopyFrom(signature),
			};
		}
		public override ByteString ExtractSignatureFromProtobuf(SignaturePair pair)
		{
			return pair.getEd25519();
		}

		public override byte[] toBytesDER()
		{
			return new SubjectPublicKeyInfo(new AlgorithmIdentifier(ID_ED25519), keyData).getEncoded("DER");
		}
		public override byte[] ToBytes()
		{
			return ToBytesRaw();
		}
		public override byte[] ToBytesRaw()
		{
			return keyData;
		}
		public override EvmAddress ToEvmAddress()
		{
			throw new InvalidOperationException("unsupported operation on Ed25519PublicKey");
		}
		public override bool Equals(object? o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			PublicKeyED25519 publicKey = (PublicKeyED25519)o;

			return Equals(keyData, publicKey.keyData);
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(keyData);
		}
		public override bool IsED25519()
		{
			return true;
		}
		public override bool IsECDSA()
		{
			return false;
		}
	}
}