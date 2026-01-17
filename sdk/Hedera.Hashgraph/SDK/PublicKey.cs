using Google.Protobuf;
using Hedera.Hashgraph.Proto;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK
{
	/**
 * A public key on the Hedera™ network.
 */
	public abstract class PublicKey : Key
	{
		/**
		 * Returns an "unusable" public key.
		 * “Unusable” refers to a key such as an Ed25519 0x00000... public key,
		 * since it is (presumably) impossible to find the 32-byte string whose SHA-512 hash begins with 32 bytes of zeros.
		 *
		 * @return The "unusable" key
		 */
		public static PublicKey UnusableKey()
		{
			return PublicKey.FromStringED25519("0000000000000000000000000000000000000000000000000000000000000000");
		}
		/**
		 * Create a public key from a byte array.
		 *
		 * @param publicKey                 the byte array
		 * @return                          the new public key
		 */
		public static PublicKey FromBytes(byte[] publicKey)
		{
			if (publicKey.Length == Ed25519.PublicKeySize)
			{
				// If this is a 32 byte string, assume an Ed25519 public key
				return PublicKeyED25519.FromBytesInternal(publicKey);
			}
			else if (publicKey.Length == 33)
			{
				// compressed 33 byte raw form
				return PublicKeyECDSA.FromBytesInternal(publicKey);
			}
			else if (publicKey.Length == 65)
			{
				// compress the 65 byte form
				return PublicKeyECDSA.FromBytesInternal(
						Key.ECDSA_SECP256K1_CURVE.getCurve().decodePoint(publicKey).getEncoded(true));
			}

			// Assume a DER-encoded private key descriptor
			return FromBytesDER(publicKey);
		}
		/**
		 * Create a public key from a DER encoded byte array.
		 *
		 * @param publicKey                 the DER encoded byte array
		 * @return                          the new key
		 */
		public static PublicKey FromBytesDER(byte[] publicKey)
		{
			return PublicKey.FromSubjectKeyInfo(SubjectPublicKeyInfo.getInstance(publicKey));
		}
		/**
		 * Create a public key from a byte array.
		 *
		 * @param publicKey                 the byte array
		 * @return                          the new key
		 */
		public static PublicKey FromBytesED25519(byte[] publicKey)
		{
			return PublicKeyED25519.FromBytes(publicKey);
		}
		/**
		 * Create a public key from a byte array.
		 *
		 * @param publicKey                 the byte array
		 * @return                          the new key
		 */
		public static PublicKey FromBytesECDSA(byte[] publicKey)
		{
			return PublicKeyECDSA.FromBytes(publicKey);
		}
		/**
		 * Create a public key from a string.
		 *
		 * @param publicKey                 the string
		 * @return                          the new key
		 */
		public static PublicKey FromString(string publicKey)
		{
			return PublicKey.FromBytes(Hex.Decode(publicKey));
		}
		/**
		 * Create a public key from a string.
		 *
		 * @param publicKey                 the string
		 * @return                          the new key
		 */
		public static PublicKey FromStringED25519(string publicKey)
		{
			return FromBytesED25519(Hex.Decode(publicKey));
		}
		/**
		 * Create a public key from a string.
		 *
		 * @param publicKey                 the string
		 * @return                          the new key
		 */
		public static PublicKey FromStringECDSA(string publicKey)
		{
			return FromBytesECDSA(Hex.Decode(publicKey));
		}
		/**
		 * Create a public key from a string.
		 *
		 * @param publicKey                 the string
		 * @return                          the new key
		 */
		public static PublicKey FromStringDER(string publicKey)
		{
			return FromBytesDER(Hex.Decode(publicKey));
		}
		/**
		 * The public key from an immutable byte string.
		 *
		 * @param aliasBytes                the immutable byte string
		 * @return                          the key
		 */
		public static PublicKey? FromAliasBytes(ByteString aliasBytes)
		{
			if (!aliasBytes.IsEmpty)
			{
				try
				{
					var key = Key.FromProtobufKey(Proto.Key.Parser.ParseFrom(aliasBytes));
					return (key instanceof PublicKey) ? ((PublicKey)key) : null;
				}
				catch (InvalidProtocolBufferException ignored)
				{
				}
			}
			return null;
		}
		/**
		 * Create a public key from a subject public key info object.
		 *
		 * @param subjectPublicKeyInfo      the subject public key info object
		 * @return                          the new key
		 */
		private static PublicKey FromSubjectKeyInfo(SubjectPublicKeyInfo subjectPublicKeyInfo)
		{
			if (subjectPublicKeyInfo.getAlgorithm().equals(new AlgorithmIdentifier(ID_ED25519)))
			{
				return PublicKeyED25519.FromSubjectKeyInfoInternal(subjectPublicKeyInfo);
			}
			else
			{
				// assume ECDSA
				return PublicKeyECDSA.FromSubjectKeyInfoInternal(subjectPublicKeyInfo);
			}
		}

		/**
		 * Extract the DER encoded string.
		 *
		 * @return                          the DER encoded string
		 */
		public string ToStringDER()
		{
			return Hex.ToHexString(ToBytesDER());
		}
		/**
		 * Extract the raw string.
		 *
		 * @return                          the raw string
		 */
		public string ToStringRaw()
		{
			return Hex.ToHexString(ToBytesRaw());
		}
		/**
		 * Is the given transaction valid?
		 *
		 * @param transaction               the transaction
		 * @return                          is it valid
		 */
		public bool VerifyTransaction(Transaction<?> transaction)
		{
			if (!transaction.isFrozen())
			{
				transaction.freeze();
			}

			for (var publicKey : transaction.publicKeys)
			{
				if (publicKey.equals(this))
				{
					return true;
				}
			}

			for (var signedTransaction : transaction.innerSignedTransactions)
			{
				var found = false;

				for (var sigPair : signedTransaction.getSigMap().getSigPairList())
				{
					if (sigPair.getPubKeyPrefix().equals(ByteString.copyFrom(toBytesRaw())))
					{
						found = true;

						if (!verify(
								signedTransaction.getBodyBytes().ToByteArray(),
								extractSignatureFromProtobuf(sigPair).ToByteArray()))
						{
							return false;
						}
					}
				}

				if (!found)
				{
					return false;
				}
			}

			return true;
		}
		/**
		 * Create a new account id.
		 *
		 * @param Shard                     the Shard part
		 * @param Realm                     the Realm part
		 * @return                          the new account id
		 */
		public AccountId ToAccountId(LongNN Shard, LongNN Realm)
		{
			return new AccountId(Shard, Realm, 0, null, this, null);
		}

		public override string ToString()
		{
			return ToStringDER();
		}

		/**
		 * Extract the DER represented as a byte array.
		 *
		 * @return                          the DER represented as a byte array
		 */
		public abstract byte[] ToBytesDER();

		/**
		 * Extract the DER represented as a byte array.
		 *
		 * @return                          the DER represented as a byte array
		 */
		public abstract byte[] ToBytesDER();
		/**
		 * Extract the raw byte representation.
		 *
		 * @return                          the raw byte representation
		 */
		public abstract byte[] ToBytesRaw();
		/**
		 * Is this an ED25519 key?
		 *
		 * @return                          is this an ED25519 key
		 */
		public abstract bool IsED25519();
		/**
		 * Is this an ECDSA key?
		 *
		 * @return                          is this an ECDSA key
		 */
		public abstract bool IsECDSA();
		/**
		 * Verify a signature on a message with this public key.
		 *
		 * @param message   The array of bytes representing the message
		 * @param signature The array of bytes representing the signature
		 * @return bool
		 */
		public abstract bool Verify(byte[] message, byte[] signature);
		/**
		 * Converts the key to EVM address
		 *
		 * @return                          the EVM address
		 */
		public abstract EvmAddress ToEvmAddress();
		/**
		 * Serialize this key as a SignaturePair protobuf object
		 */
		public abstract SignaturePair ToSignaturePairProtobuf(byte[] signature);
		/**
		 * Get the signature from a signature pair protobuf.
		 *
		 * @param pair                      the protobuf
		 * @return                          the signature
		 */
		public abstract ByteString ExtractSignatureFromProtobuf(SignaturePair pair);	
	}
}