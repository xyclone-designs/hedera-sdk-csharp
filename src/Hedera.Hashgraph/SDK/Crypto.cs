// SPDX-License-Identifier: Apache-2.0
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
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

		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:DeriveKeySha256(System.String,System.Byte[],System.Int32,System.Int32)"]/*' />
		internal static KeyParameter DeriveKeySha256(string passphrase, byte[] salt, int iterations, int dkLenBytes)
		{
			Pkcs5S2ParametersGenerator gen = new (new Sha256Digest());
			gen.Init(Encoding.UTF8.GetBytes(passphrase), salt, iterations);

			return (KeyParameter)gen.GenerateDerivedParameters("AES256", dkLenBytes * 8);
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:InitAesCtr128(KeyParameter,System.Byte[])"]/*' />
		internal static BufferedBlockCipher InitAesCtr128(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new SicBlockCipher(new AesEngine()));
			cipher.Init(true, new ParametersWithIV(key, iv));
			return cipher;
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:InitAesCbc128Encrypt(KeyParameter,System.Byte[])"]/*' />
		internal static BufferedBlockCipher InitAesCbc128Encrypt(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new CbcBlockCipher(new AesEngine()));
			cipher.Init(true, new ParametersWithIV(key, iv));
			return cipher;
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:InitAesCbc128Decrypt(KeyParameter,System.Byte[])"]/*' />
		internal static BufferedBlockCipher InitAesCbc128Decrypt(KeyParameter key, byte[] iv)
		{
			BufferedBlockCipher cipher = new (new CbcBlockCipher(new AesEngine()));
			cipher.Init(false, new ParametersWithIV(key, iv));
			return cipher;
		}

		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:EncryptAesCtr128(KeyParameter,System.Byte[],System.Byte[])"]/*' />
		internal static byte[] EncryptAesCtr128(KeyParameter key, byte[] iv, byte[] input)
		{
			return RunCipher(InitAesCtr128(key, iv), input);
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:DecryptAesCtr128(KeyParameter,System.Byte[],System.Byte[])"]/*' />
		internal static byte[] DecryptAesCtr128(KeyParameter key, byte[] iv, byte[] input)
		{
			return RunCipher(InitAesCtr128(key, iv), input);
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:RunCipher(BufferedBlockCipher,System.Byte[])"]/*' />
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
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:CalcHmacSha384(KeyParameter,System.Byte[],System.Byte[])"]/*' />
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
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:CalcKeccak256(System.Byte[])"]/*' />
		internal static byte[] CalcKeccak256(byte[] input)
		{
			// Note: KeccakDigest(256) is NOT the same as Sha3Digest(256)

			KeccakDigest digest = new (256);
			byte[] output = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(input, 0, input.Length);
			digest.DoFinal(output, 0);
			return output;
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:RandomBytes(System.Int32)"]/*' />
		internal static byte[] RandomBytes(int len)
		{
			byte[] output = new byte[len];
			SecureRandom random = new ();
			random.NextBytes(output);
			return output;
		}
		/// <include file="Crypto.cs.xml" path='docs/member[@name="M:RecoverPublicKeyECDSAFromSignature(System.Int32,BigInteger,BigInteger,System.Byte[])"]/*' />
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