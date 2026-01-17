using Google.Protobuf;
using Hedera.Hashgraph.Proto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Encapsulate the ECDSA public key.
 */
	public class PublicKeyECDSA : PublicKey
	{
		// Compressed 33 byte form
		private byte[] keyData;

		/**
		 * Constructor.
		 *
		 * @param keyData                   the byte array representing the key
		 */
		private PublicKeyECDSA(byte[] keyData)
		{
			this.keyData = keyData;
		}

		/**
		 * Create a key from a byte array representation.
		 *
		 * @param publicKey                 the byte array representing the key
		 * @return                          the new key
		 */
		public static PublicKeyECDSA FromBytesInternal(byte[] publicKey)
		{
			// Validate the key if it's not all zero public key, see HIP-540
			if (Equals(publicKey, new byte[33]))
			{
				return new PublicKeyECDSA(publicKey);
			}
			if (publicKey.Length == 33 || publicKey.Length == 65)
			{
				return new PublicKeyECDSA(
						// compress and validate the key
						Key.ECDSA_SECP256K1_CURVE.Curve().decodePoint(publicKey).getEncoded(true));
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
		public static PublicKeyECDSA FromSubjectKeyInfoInternal(SubjectPublicKeyInfo subjectPublicKeyInfo)
		{
			return fromBytesInternal(subjectPublicKeyInfo.getPublicKeyData().getBytes());
		}

		
		public override int GetHashCode()
		{
			return Arrays.hashCode(keyData);
		}
		public override bool Equals(object? obj)
		{
			if (this == o)
			{
				return true;
			}

			if (o == null || getClass() != o.getClass())
			{
				return false;
			}

			PublicKeyECDSA publicKey = (PublicKeyECDSA)obj;
			return Arrays.equals(keyData, publicKey.keyData);
		}
		public override bool IsED25519()
		{
			return false;
		}
		public override bool IsECDSA()
		{
			return true;
		}
		public override bool Verify(byte[] message, byte[] signature)
		{
			var hash = calcKeccak256(message);

			ECDSASigner signer = new ECDSASigner();
			signer.init(
					false,
					new ECPublicKeyParameters(
							Key.ECDSA_SECP256K1_CURVE.getCurve().decodePoint(keyData), Key.ECDSA_SECP256K1_DOMAIN));

			BigInteger r = new BigInteger(1, Arrays.copyOf(signature, 32));
			BigInteger s = new BigInteger(1, Arrays.copyOfRange(signature, 32, 64));

			return signer.verifySignature(hash, r, s);
		}
		public override byte[] ToBytesDER()
		{
			try
			{
				return new SubjectPublicKeyInfo(new AlgorithmIdentifier(ID_EC_PUBLIC_KEY, ID_ECDSA_SECP256K1), keyData)
						.getEncoded("DER");
			}
			catch (IOException e)
			{
				throw new RuntimeException(e);
			}
		}
		public override byte[] ToBytes()
		{
			return toBytesDER();
		}
		public override byte[] ToBytesRaw()
		{
			return Arrays.copyOf(keyData, keyData.Length);
		}
		public override EvmAddress ToEvmAddress()
		{
			// Calculate the Keccak-256 hash of the uncompressed key without "04" prefix
			byte[] uncompressed =
					Key.ECDSA_SECP256K1_CURVE.getCurve().decodePoint(toBytesRaw()).getEncoded(false);
			byte[] keccakBytes = calcKeccak256(Arrays.copyOfRange(uncompressed, 1, uncompressed.Length));

			// Return the last 20 bytes
			return EvmAddress.FromBytes(Arrays.copyOfRange(keccakBytes, 12, 32));
		}
		public override Proto.Key ToProtobufKey()
		{
			return new Proto.Key
			{
				ECDSASecp256K1 = ByteString.CopyFrom(keyData)
			};
		}
		public override SignaturePair ToSignaturePairProtobuf(byte[] signature)
		{
			return SignaturePair.newBuilder()
					.setPubKeyPrefix(ByteString.copyFrom(keyData))
					.setECDSASecp256K1(ByteString.copyFrom(signature))
					.build();
		}
		public override ByteString ExtractSignatureFromProtobuf(SignaturePair pair)
		{
			return pair.getECDSA384();
		}
	}
}