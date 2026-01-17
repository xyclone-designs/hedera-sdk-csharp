using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

using System;

namespace Hedera.Hashgraph.SDK
{
	/**
 * Utility class used internally by the sdk.
 */
	internal sealed class Crypto
	{
		internal static readonly int IV_LEN = 16;
		internal static readonly int ITERATIONS = 262144;
		internal static readonly int SALT_LEN = 32;
		internal static readonly int DK_LEN = 32;

		// OpenSSL doesn't like longer derived keys
		internal static readonly int CBC_DK_LEN = 16;

		internal static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
		internal static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (
			ECDSA_SECP256K1_CURVE.Curve,
			ECDSA_SECP256K1_CURVE.G,
			ECDSA_SECP256K1_CURVE.N,
			ECDSA_SECP256K1_CURVE.H);

		/**
		 * Constructor.
		 */
		private Crypto() { }

		/**
		 * Derive a sha 256 key.
		 *
		 * @param passphrase                the password will be converted into bytes
		 * @param salt                      the salt to be mixed in
		 * @param iterations                the iterations for mixing
		 * @param dkLenBytes                the key length in bytes
		 * @return                          the key parameter object
		 */
		internal static KeyParameter DeriveKeySha256(string passphrase, byte[] salt, int iterations, int dkLenBytes)
		{
			Pkcs5S2ParametersGenerator gen = new (new Sha256Digest());

			//gen.Init(passphrase.GetBytes(StandardCharsets.UTF_8), salt, iterations);
			gen.Init(passphrase.GetBytes(StandardCharsets.UTF_8), salt, iterations);

			return (KeyParameter)gen.GenerateDerivedParameters(dkLenBytes * 8);
		}

		/**
		 * Initialize an advanced encryption standard counter mode cipher.
		 *
		 * @param cipherKey                 the cipher key
		 * @param iv                        the initialization vector byte array
		 * @param forDecrypt                is this for decryption
		 * @return                          the aes ctr cipher
		 */
		internal static IBufferedCipher InitAesCtr128(KeyParameter cipherKey, byte[] iv, bool forDecrypt)
		{
			IBufferedCipher aesCipher;

			try 
			{
				aesCipher = CipherUtilities.GetCipher("AES/CTR/NoPadding"); 
			}
			catch (SecurityUtilityException e)
			{
				throw new Exception("Platform does not support AES-CTR ciphers", e);
			}

			return InitAesCipher(aesCipher, cipherKey, iv, forDecrypt);
		}
		/**
		* Initialize an advanced encryption standard cipher block chaining mode
		* cipher for encryption.
		*
		* @param cipherKey                 the cipher key
		* @param iv                        the initialization vector byte array
		* @return                          the aes cbc cipher
		*/
		internal static IBufferedCipher InitAesCbc128Encrypt(KeyParameter cipherKey, byte[] iv, bool forDecrypt)
		{
			IBufferedCipher aesCipher;

			try
			{
				aesCipher = CipherUtilities.GetCipher("AES/CBC/NoPadding");
			}
			catch (SecurityUtilityException e)
			{
				throw new Exception("platform does not support AES-CBC ciphers", e);
			}

			return InitAesCipher(aesCipher, cipherKey, iv, forDecrypt);
		}
		/**
		* Initialize an advanced encryption standard cipher block chaining mode
		* cipher for decryption.
		*
		* @param cipherKey                 the cipher key
		* @param parameters                the algorithm parameters
		* @return                          the aes cbc cipher
		*/
		internal static IBufferedCipher InitAesCbc128Decrypt(KeyParameter cipherKeyml)
		{
			IBufferedCipher aesCipher;

			try
			{
				aesCipher = CipherUtilities.GetCipher("AES/CBC/NoPadding");
			}
			catch (SecurityUtilityException e)
			{
				throw new Exception("platform does not support AES-CBC ciphers", e);
			}

			try
			{
				aesCipher.Init(true, new ParametersWithIV(new KeyParameter(cipherKeyml.GetKey(), 0, 16), parameters));
			}
			catch (InvalidKeyException e)
			{
				throw new Exception("platform does not support AES-128 ciphers", e);
			}
			catch (InvalidParameterException e)
			{
				throw new Exception(string.Empty, e);
			}

			return aesCipher;
		}
		/**
		 * Create a new aes cipher.
		 *
		 * @param aesCipher                 the aes cipher
		 * @param cipherKey                 the cipher key
		 * @param iv                        the initialization vector byte array
		 * @param forDecrypt                is this for decryption True or encryption False
		 * @return                          the new aes cipher
		 */
		private static IBufferedCipher InitAesCipher(IBufferedCipher aesCipher, KeyParameter cipherKey, byte[] iv, bool forDecrypt)
		{
			try
			{
				aesCipher.Init(forDecrypt, new ParametersWithIV(new KeyParameter(cipherKey.GetKey(), 0, 16), iv));
			}
			catch (InvalidKeyException e)
			{
				throw new Exception("platform does not support AES-128 ciphers", e);
			}
			catch (InvalidParameterException e)
			{
				throw new Exception(string.Empty, e);
			}

			return aesCipher;
		}

		private static ECPoint? DecompressKey(BigInteger xBN, bool yBit)
		{
			byte[] compEnc = X9IntegerConverter.IntegerToBytes(xBN, 1 + X9IntegerConverter.GetByteLength(ECDSA_SECP256K1_DOMAIN.Curve));
			compEnc[0] = (byte)(yBit ? 0x03 : 0x02);
			try
			{
				return ECDSA_SECP256K1_DOMAIN.Curve.DecodePoint(compEnc);
			}
			catch (ArgumentException e)
			{
				// the key was invalid
				return null;
			}
		}

		/**
		 * Encrypt a byte array with the aes ctr cipher.
		 *
		 * @param cipherKey                 the cipher key
		 * @param iv                        the initialization vector
		 * @param input                     the byte array to encrypt
		 * @return                          the encrypted byte array
		 */
		internal static byte[] EncryptAesCtr128(KeyParameter cipherKey, byte[] iv, byte[] input)
		{
			IBufferedCipher aesCipher = InitAesCtr128(cipherKey, iv, false);
			
			return RunCipher(aesCipher, input);
		}
		/**
		 * Decrypt a byte array with the aes ctr cipher.
		 *
		 * @param cipherKey                 the cipher key
		 * @param iv                        the initialization vector
		 * @param input                     the byte array to decrypt
		 * @return                          the decrypted byte array
		 */
		internal static byte[] DecryptAesCtr128(KeyParameter cipherKey, byte[] iv, byte[] input)
		{
			IBufferedCipher aesCipher = InitAesCtr128(cipherKey, iv, true);
			
			return RunCipher(aesCipher, input);
		}
		/**
		 * Run the cipher on the given input.
		 *
		 * @param cipher                    the cipher
		 * @param input                     the byte array
		 * @return                          the output of running the cipher
		 */
		internal static byte[] RunCipher(IBufferedCipher cipher, byte[] input)
		{
			try
			{
				return cipher.DoFinal(input);
			}
			catch (CryptoException e)
			{
				throw new Exception("Cryptographic operation failed", e);
			}
		}
		/**
		 * Calculate a hash message authentication code using the secure hash
		 * algorithm variant 384.
		 *
		 * @param cipherKey                 the cipher key
		 * @param iv                        the initialization vector
		 * @param input                     the byte array
		 * @return                          the hmac using sha 384
		 */
		internal static byte[] CalcHmacSha384(KeyParameter cipherKey, byte[]? iv, byte[] input)
		{
			HMac hmacSha384 = new (new Sha384Digest());
			byte[] output = new byte[hmacSha384.GetMacSize()];

			hmacSha384.Init(new KeyParameter(cipherKey.GetKey(), 16, 16));

			if (iv != null) hmacSha384.BlockUpdate(iv, 0, iv.Length);
			hmacSha384.BlockUpdate(input, 0, input.Length);
			hmacSha384.DoFinal(output, 0);

			return output;
		}
		/**
			* Calculate a keccak 256-bit hash.
			*
			* @param message                   the message to be hashed
			* @return                          the hash
			*/
		internal static byte[] CalcKeccak256(byte[] message)
		{
			var digest = new Keccak.Digest256();
			digest.update(message);
			return digest.digest();
		}
		/**
			* Generate some randomness.
			*
			* @param len                       the number of bytes requested
			* @return                          the byte array of randomness
			*/
		internal static byte[] RandomBytes(int len)
		{
			byte[] _out = new byte[len];
			ThreadLocalSecureRandom.current().nextBytes(_out);

			return _out;
		}
		/**
		* Given the r and s components of a signature and the hash value of the message, recover and return the public key
		* according to the algorithm in <a href="https://www.secg.org/sec1-v2.pdf">SEC1v2 section 4.1.6.</a>
		* <p>
		* Calculations and explanations in this method were taken and adapted from
		* <a href="https://github.com/apache/incubator-tuweni/blob/0852e0b01ad126b47edae51b26e808cb73e294b1/crypto/src/main/java/org/apache/tuweni/crypto/SECP256K1.java#L199-L215">incubator-tuweni lib</a>
		*
		* @param recId Which possible key to recover.
		* @param r The R component of the signature.
		* @param s The S component of the signature.
		* @param messageHash Hash of the data that was signed.
		* @return A ECKey containing only the public part, or {@code null} if recovery wasn't possible.
		*/
		internal static byte[]? RecoverPublicKeyECDSAFromSignature(int recId, BigInteger r, BigInteger s, byte[] messageHash)
		{
			if (!(recId == 0 || recId == 1)) throw new ArgumentException("Recovery Id must be 0 or 1 for secp256k1.");
			if (r.SignValue < 0 || s.SignValue < 0) throw new ArgumentException("'r' and 's' shouldn't be negative.");
			// 1.1 - 1.3 calculate point R
			ECPoint R = DecompressKey(r, (recId & 1) == 1);
			// 1.4 nR should be a point at infinity
			if (R.Multiply(ECDSA_SECP256K1_DOMAIN.N).IsInfinity is false) return null;
			// 1.5 Compute e from M using Steps 2 and 3 of ECDSA signature verification.
			BigInteger e = new (1, messageHash);

			// 1.6.1 Compute a candidate public key as:
			//   Q = mi(r) * (sR - eG)
			// Where mi(x) is the modular multiplicative inverse. We transform this into the following:
			//   Q = (mi(r) * s ** R) + (mi(r) * -e ** G)
			// Where -e is the modular additive inverse of e, that is z such that z + e = 0 (mod n).
			// In the above equation ** is point multiplication and + is point addition (the EC group
			// operator).
			//
			// We can find the additive inverse by subtracting e from zero then taking the mod. For example the additive
			// inverse of 3 modulo 11 is 8 because 3 + 8 mod 11 = 0, and -3 mod 11 = 8.
			BigInteger eInv = BigInteger.Zero.Subtract(e).Mod(ECDSA_SECP256K1_DOMAIN.N);
			BigInteger rInv = r.ModInverse(ECDSA_SECP256K1_DOMAIN.N);
			BigInteger srInv = rInv.Multiply(s).Mod(ECDSA_SECP256K1_DOMAIN.N);
			BigInteger eInvrInv = rInv.Multiply(eInv).Mod(ECDSA_SECP256K1_DOMAIN.N);
			ECPoint q = ECAlgorithms.SumOfTwoMultiplies(ECDSA_SECP256K1_DOMAIN.G, eInvrInv, R, srInv);

			if (q.IsInfinity) return null;

			return q.GetEncoded(true);
		}
	}

}