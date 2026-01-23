// SPDX-License-Identifier: Apache-2.0
using Java.Math;
using Java.Nio.Charset;
using Java.Security;
using Javax.Annotation;
using Javax.Crypto;
using Javax.Crypto.Spec;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Jcajce.Provider.Digest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using Org.BouncyCastle.Math.EC;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Utility class used internally by the sdk.
    /// </summary>
    sealed class Crypto
    {
        static readonly int IV_LEN = 16;
        static readonly int ITERATIONS = 262144;
        static readonly int SALT_LEN = 32;
        static readonly int DK_LEN = 32;
        // OpenSSL doesn't like longer derived keys
        static readonly int CBC_DK_LEN = 16;
        static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
        static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (ECDSA_SECP256K1_CURVE.Curve, ECDSA_SECP256K1_CURVE.G, ECDSA_SECP256K1_CURVE.N, ECDSA_SECP256K1_CURVE.H);
        /// <summary>
        /// Constructor.
        /// </summary>
        private Crypto() { }

        /// <summary>
        /// Derive a sha 256 key.
        /// </summary>
        /// <param name="passphrase">the password will be converted into bytes</param>
        /// <param name="salt">the salt to be mixed in</param>
        /// <param name="iterations">the iterations for mixing</param>
        /// <param name="dkLenBytes">the key length in bytes</param>
        /// <returns>                         the key parameter object</returns>
        static KeyParameter DeriveKeySha256(string passphrase, byte[] salt, int iterations, int dkLenBytes)
        {
			Pkcs5S2ParametersGenerator gen = new (new Sha256Digest());
            gen.Init(passphrase.GetBytes(StandardCharsets.UTF_8), salt, iterations);

            return (KeyParameter)gen.GenerateDerivedParameters(dkLenBytes * 8);
        }

        /// <summary>
        /// Initialize an advanced encryption standard counter mode cipher.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector byte array</param>
        /// <param name="forDecrypt">is this for decryption</param>
        /// <returns>                         the aes ctr cipher</returns>
        static Cipher InitAesCtr128(KeyParameter cipherKey, byte[] iv, bool forDecrypt)
        {
            Cipher aesCipher;
            try
            {
                aesCipher = Cipher.GetInstance("AES/CTR/NOPADDING");
            }
            catch (NoSuchAlgorithmException e)
            {
                throw new Exception("platform does not support AES-CTR ciphers", e);
            }
            catch (NoSuchPaddingException e)
            {
                throw new Exception("platform does not support AES-CTR ciphers", e);
            }

            return InitAesCipher(aesCipher, cipherKey, iv, forDecrypt);
        }

        /// <summary>
        /// Initialize an advanced encryption standard cipher block chaining mode
        /// cipher for encryption.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector byte array</param>
        /// <returns>                         the aes cbc cipher</returns>
        static Cipher InitAesCbc128Encrypt(KeyParameter cipherKey, byte[] iv)
        {
            Cipher aesCipher;
            try
            {
                aesCipher = Cipher.GetInstance("AES/CBC/NoPadding");
            }
            catch (NoSuchAlgorithmException e)
            {
                throw new Exception("platform does not support AES-CBC ciphers", e);
            }
            catch (NoSuchPaddingException e)
            {
                throw new Exception("platform does not support AES-CBC ciphers", e);
            }

            return InitAesCipher(aesCipher, cipherKey, iv, false);
        }

        /// <summary>
        /// Initialize an advanced encryption standard cipher block chaining mode
        /// cipher for decryption.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="parameters">the algorithm parameters</param>
        /// <returns>                         the aes cbc cipher</returns>
        static Cipher InitAesCbc128Decrypt(KeyParameter cipherKey, AlgorithmParameters parameters)
        {
            Cipher aesCipher;
            try
            {
                aesCipher = Cipher.GetInstance("AES/CBC/NoPadding");
            }
            catch (NoSuchAlgorithmException e)
            {
                throw new Exception("platform does not support AES-CBC ciphers", e);
            }
            catch (NoSuchPaddingException e)
            {
                throw new Exception("platform does not support AES-CBC ciphers", e);
            }

            try
            {
                aesCipher.Init(Cipher.DECRYPT_MODE, new SecretKeySpec(cipherKey.GetKey(), 0, 16, "AES"), parameters);
            }
            catch (InvalidKeyException e)
            {
                throw new Exception("platform does not support AES-128 ciphers", e);
            }
            catch (InvalidAlgorithmParameterException e)
            {
                throw new Exception(e);
            }

            return aesCipher;
        }

        /// <summary>
        /// Create a new aes cipher.
        /// </summary>
        /// <param name="aesCipher">the aes cipher</param>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector byte array</param>
        /// <param name="forDecrypt">is this for decryption True or encryption False</param>
        /// <returns>                         the new aes cipher</returns>
        private static Cipher InitAesCipher(Cipher aesCipher, KeyParameter cipherKey, byte[] iv, bool forDecrypt)
        {
            int mode = forDecrypt ? Cipher.DECRYPT_MODE : Cipher.ENCRYPT_MODE;
            try
            {
                aesCipher.Init(mode, new SecretKeySpec(cipherKey.GetKey(), 0, 16, "AES"), new IvParameterSpec(iv));
            }
            catch (InvalidKeyException e)
            {
                throw new Exception("platform does not support AES-128 ciphers", e);
            }
            catch (InvalidAlgorithmParameterException e)
            {
                throw new Exception(e);
            }

            return aesCipher;
        }

        /// <summary>
        /// Encrypt a byte array with the aes ctr cipher.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector</param>
        /// <param name="input">the byte array to encrypt</param>
        /// <returns>                         the encrypted byte array</returns>
        static byte[] EncryptAesCtr128(KeyParameter cipherKey, byte[] iv, byte[] input)
        {
            Cipher aesCipher = InitAesCtr128(cipherKey, iv, false);
            return RunCipher(aesCipher, input);
        }

        /// <summary>
        /// Decrypt a byte array with the aes ctr cipher.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector</param>
        /// <param name="input">the byte array to decrypt</param>
        /// <returns>                         the decrypted byte array</returns>
        static byte[] DecryptAesCtr128(KeyParameter cipherKey, byte[] iv, byte[] input)
        {
            Cipher aesCipher = InitAesCtr128(cipherKey, iv, true);
            return RunCipher(aesCipher, input);
        }

        /// <summary>
        /// Run the cipher on the given input.
        /// </summary>
        /// <param name="cipher">the cipher</param>
        /// <param name="input">the byte array</param>
        /// <returns>                         the output of running the cipher</returns>
        static byte[] RunCipher(Cipher cipher, byte[] input)
        {
            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            try
            {
                cipher.DoFinal(input, 0, input.Length, output);
            }
            catch (ShortBufferException e)
            {
                throw new Exception(e);
            }
            catch (IllegalBlockSizeException e)
            {
                throw new Exception(e);
            }
            catch (BadPaddingException e)
            {
                throw new Exception(e);
            }

            return output;
        }

        /// <summary>
        /// Calculate a hash message authentication code using the secure hash
        /// algorithm variant 384.
        /// </summary>
        /// <param name="cipherKey">the cipher key</param>
        /// <param name="iv">the initialization vector</param>
        /// <param name="input">the byte array</param>
        /// <returns>                         the hmac using sha 384</returns>
        static byte[] CalcHmacSha384(KeyParameter cipherKey, byte[] iv, byte[] input)
        {
            HMac hmacSha384 = new (new Sha384Digest());
            byte[] output = new byte[hmacSha384.GetMacSize()];
            hmacSha384.Init(new KeyParameter(cipherKey.GetKey(), 16, 16));
            if (iv != null)
            {
                hmacSha384.BlockUpdate(iv, 0, iv.Length);
            }

            hmacSha384.BlockUpdate(input, 0, input.Length);
            hmacSha384.DoFinal(output, 0);
            return output;
        }

        /// <summary>
        /// Calculate a keccak 256-bit hash.
        /// </summary>
        /// <param name="message">the message to be hashed</param>
        /// <returns>                         the hash</returns>
        static byte[] CalcKeccak256(byte[] message)
        {
            var digest = new Digest256();
            digest.Update(message);
            return digest.Digest();
        }

        /// <summary>
        /// Generate some randomness.
        /// </summary>
        /// <param name="len">the number of bytes requested</param>
        /// <returns>                         the byte array of randomness</returns>
        static byte[] RandomBytes(int len)
        {
            byte[] _out = new byte[len];
            ThreadLocalSecureRandom.Current().NextBytes(_out);
            return _out;
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
        static byte[]? RecoverPublicKeyECDSAFromSignature(int recId, BigInteger r, BigInteger s, byte[] messageHash)
        {
            if (!(recId == 0 || recId == 1))
            {
                throw new ArgumentException("Recovery Id must be 0 or 1 for secp256k1.");
            }

            if (r.Signum() < 0 || s.Signum() < 0)
            {
                throw new ArgumentException("'r' and 's' shouldn't be negative.");
            }


            // 1.1 - 1.3 calculate point R
            ECPoint R = DecompressKey(r, (recId & 1) == 1);

            // 1.4 nR should be a point at infinity
            if (R == null || !R.Multiply(ECDSA_SECP256K1_DOMAIN.N).IsInfinity)
            {
                return null;
            }


            // 1.5 Compute e from M using Steps 2 and 3 of ECDSA signature verification.
            BigInteger e = new BigInteger(1, messageHash);

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
            BigInteger eInv = BigInteger.ZERO.Subtract(e).Mod(ECDSA_SECP256K1_DOMAIN.N);
            BigInteger rInv = r.ModInverse(ECDSA_SECP256K1_DOMAIN.N);
            BigInteger srInv = rInv.Multiply(s).Mod(ECDSA_SECP256K1_DOMAIN.N);
            BigInteger eInvrInv = rInv.Multiply(eInv).Mod(ECDSA_SECP256K1_DOMAIN.N);
            ECPoint q = ECAlgorithms.SumOfTwoMultiplies(ECDSA_SECP256K1_DOMAIN.G, eInvrInv, R, srInv);
            if (q.IsInfinity)
            {
                return null;
            }

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
            catch (ArgumentException e)
            {

                // the key was invalid
                return null;
            }
        }
    }
}