// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

using System;
using System.Text;

namespace Hedera.Hashgraph.SDK
{
	internal sealed class Crypto
	{
		internal static readonly int IV_LEN = 16;
		internal static readonly int ITERATIONS = 262144;
		internal static readonly int SALT_LEN = 32;
		internal static readonly int DK_LEN = 32;
		internal static readonly int CBC_DK_LEN = 16;

		internal static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
		internal static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (
			ECDSA_SECP256K1_CURVE.Curve, 
			ECDSA_SECP256K1_CURVE.G, 
			ECDSA_SECP256K1_CURVE.N, 
			ECDSA_SECP256K1_CURVE.H);

		private Crypto() { }

		/// <summary>
		/// Derive a sha 256 key.
		/// </summary>
		/// <param name="passphrase">the password will be converted into bytes</param>
		/// <param name="salt">the salt to be mixed in</param>
		/// <param name="iterations">the iterations for mixing</param>
		/// <param name="dkLenBytes">the key length in bytes</param>
		/// <returns>                         the key parameter object</returns>
		internal static KeyParameter DeriveKeySha256(string passphrase, byte[] salt, int iterations, int dkLenBytes)
		{
			Pkcs5S2ParametersGenerator gen = new (new Sha256Digest());
			gen.Init(Encoding.UTF8.GetBytes(passphrase), salt, iterations);

			return (KeyParameter)gen.GenerateDerivedParameters(dkLenBytes * 8);
		}
		/// <summary>
		/// Initialize an advanced encryption standard counter mode cipher.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="iv">the initialization vector byte array</param>
		/// <param name="forDecrypt">is this for decryption</param>
		/// <returns>                         the aes ctr cipher</returns>
		internal static BufferedBlockCipher InitAesCtr128(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new SicBlockCipher(new AesEngine()));
			cipher.Init(true, new ParametersWithIV(key, iv));
			return cipher;
		}
		/// <summary>
		/// Initialize an advanced encryption standard cipher block chaining mode
		/// cipher for encryption.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="iv">the initialization vector byte array</param>
		/// <returns>                         the aes cbc cipher</returns>
		internal static BufferedBlockCipher InitAesCbc128Encrypt(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new CbcBlockCipher(new AesEngine()));
			cipher.Init(true, new ParametersWithIV(key, iv));
			return cipher;
		}
		/// <summary>
		/// Initialize an advanced encryption standard cipher block chaining mode
		/// cipher for decryption.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="parameters">the algorithm parameters</param>
		/// <returns>                         the aes cbc cipher</returns>
		internal static BufferedBlockCipher InitAesCbc128Decrypt(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new CbcBlockCipher(new AesEngine()));
			cipher.Init(false, new ParametersWithIV(key, iv));
			return cipher;
		}

		/// <summary>
		/// Encrypt a byte array with the aes ctr cipher.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="iv">the initialization vector</param>
		/// <param name="input">the byte array to encrypt</param>
		/// <returns>                         the encrypted byte array</returns>
		internal static byte[] EncryptAesCtr128(KeyParameter key, byte[] iv, byte[] input)
		{
			return RunCipher(InitAesCtr128(key, iv), input);
		}
		/// <summary>
		/// Decrypt a byte array with the aes ctr cipher.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="iv">the initialization vector</param>
		/// <param name="input">the byte array to decrypt</param>
		/// <returns>                         the decrypted byte array</returns>
		internal static byte[] DecryptAesCtr128(KeyParameter key, byte[] iv, byte[] input)
		{
			return RunCipher(InitAesCtr128(key, iv), input);
		}
		/// <summary>
		/// Run the cipher on the given input.
		/// </summary>
		/// <param name="cipher">the cipher</param>
		/// <param name="input">the byte array</param>
		/// <returns>                         the output of running the cipher</returns>
		internal static byte[] RunCipher(BufferedBlockCipher cipher, byte[] input)
		{
			byte[] output = new byte[cipher.GetOutputSize(input.Length)];
			int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
			len += cipher.DoFinal(output, len);
			
			if (len == output.Length) return output;
			
			byte[] trimmed = new byte[len];
			Array.Copy(output, 0, trimmed, 0, len);

			return trimmed;
		}
		/// <summary>
		/// Calculate a hash message authentication code using the secure hash
		/// algorithm variant 384.
		/// </summary>
		/// <param name="cipherKey">the cipher key</param>
		/// <param name="iv">the initialization vector</param>
		/// <param name="input">the byte array</param>
		/// <returns>                         the hmac using sha 384</returns>
		internal static byte[] CalcHmacSha384(KeyParameter cipherKey, byte[]? iv, byte[] input)
		{
			HMac hmac = new (new Sha384Digest());
			byte[] output = new byte[hmac.GetMacSize()];
			
			hmac.Init(new KeyParameter(cipherKey.GetKey(), 16, 16));
			
			if (iv != null) hmac.BlockUpdate(iv, 0, iv.Length);
			
			hmac.BlockUpdate(input, 0, input.Length);
			hmac.DoFinal(output, 0);

			return output;
		}
		/// <summary>
		/// Calculate a keccak 256-bit hash.
		/// </summary>
		/// <param name="message">the message to be hashed</param>
		/// <returns>                         the hash</returns>
		internal static byte[] CalcKeccak256(byte[] input)
		{
			// Note: KeccakDigest(256) is NOT the same as Sha3Digest(256)

			KeccakDigest digest = new (256);
			byte[] output = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(input, 0, input.Length);
			digest.DoFinal(output, 0);
			return output;
		}
		/// <summary>
		/// Generate some randomness.
		/// </summary>
		/// <param name="len">the number of bytes requested</param>
		/// <returns>                         the byte array of randomness</returns>
		internal static byte[] RandomBytes(int len)
		{
			byte[] output = new byte[len];
			SecureRandom random = new ();
			random.NextBytes(output);
			return output;
		}
		/// <summary>
		/// Given the r and s components of a signature and the hash value of the message, recover and return the public key
		/// according to the algorithm in <a href="https://www.secg.org/sec1-v2.pdf">SEC1v2 section 4.1.6.</a>
		/// <p>
		/// Calculations and explanations in this method were taken and adapted from
		/// <a href="https://github.com/apache/incubator-tuweni/blob/0852e0b01ad126b47edae51b26e808cb73e294b1/crypto/src/main/java/org/apache/tuweni/crypto/SECP256K1.java#L199-L215">incubator-tuweni lib</a>
		/// </summary>
		/// <param name="recId">Which possible key to recover.</param>
		/// <param name="r">The R component of the signature.</param>
		/// <param name="s">The S component of the signature.</param>
		/// <param name="messageHash">Hash of the data that was signed.</param>
		/// <returns>A ECKey containing only the public part, or {@code null} if recovery wasn't possible.</returns>
		internal static byte[]? RecoverPublicKeyECDSAFromSignature(int recId, BigInteger r, BigInteger s, byte[] messageHash)
		{
			if (recId != 0 && recId != 1) throw new ArgumentException("Recovery Id must be 0 or 1 for secp256k1.");
			if (r.SignValue < 0 || s.SignValue < 0) throw new ArgumentException("'r' and 's' must be positive.");

			// 1.1 - 1.3 calculate point R
			ECPoint? R = DecompressKey(r, (recId & 1) == 1);

			// 1.4 nR should be a point at infinity
			if (R == null || !R.Multiply(ECDSA_SECP256K1_DOMAIN.N).IsInfinity) return null;

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

		private static ECPoint? DecompressKey(BigInteger xBN, bool yBit)
		{
			byte[] compEnc = X9IntegerConverter.IntegerToBytes(xBN, 1 + X9IntegerConverter.GetByteLength(ECDSA_SECP256K1_DOMAIN.Curve));
			compEnc[0] = (byte)(yBit ? 0x03 : 0x02);
			try 
			{
				return ECDSA_SECP256K1_DOMAIN.Curve.DecodePoint(compEnc);
			}
			catch (ArgumentException) { }

			return null;
		}
	}
}
