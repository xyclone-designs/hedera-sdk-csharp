// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Exceptions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using System;
using System.IO;

namespace Hedera.Hashgraph.SDK
{
	/// <summary>
	/// Internal utility class for handling PEM objects.
	/// <br/>
	/// Privacy-Enhanced Mail (PEM) is a de facto file format for storing and
	/// sending cryptographic keys, certificates, and other data.
	/// </summary>
	internal sealed class Pem
	{
		private static readonly string TYPE_PRIVATE_KEY = "PRIVATE KEY";
		private static readonly string TYPE_ENCRYPTED_PRIVATE_KEY = "ENCRYPTED PRIVATE KEY";

		private Pem() { }

		/// <summary>
		/// Reads a private key from a PEM source.
		/// Supports:
		/// <list type="bullet">
		///   <item>Unencrypted PKCS#8 private keys</item>
		///   <item>Encrypted PKCS#8 private keys</item>
		///   <item>Traditional OpenSSL key pairs</item>
		/// </list>
		/// </summary>
		/// <param name="input">PEM text reader</param>
		/// <param name="passphrase">Optional passphrase</param>
		/// <returns>PrivateKeyInfo</returns>
		/// <exception cref="BadKeyException">If the key is invalid or unsupported</exception>
		public static PrivateKeyInfo ReadPrivateKey(TextReader input, string passphrase)
		{
			try
			{
				PemReader reader = new (input, string.IsNullOrEmpty(passphrase) ? null : new PasswordFinder(passphrase));

				if (reader.ReadObject() is not object obj)
					throw new BadKeyException("PEM file did not contain a private key");

				return obj switch
				{
					// PKCS#8 unencrypted
					PrivateKeyInfo pk => pk,

					// Traditional OpenSSL key pair
					AsymmetricCipherKeyPair kp => PrivateKeyInfoFactory.CreatePrivateKeyInfo(kp.Private),

					// Raw private key parameters
					AsymmetricKeyParameter keyParam when keyParam.IsPrivate => PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyParam),

					_ => throw new BadKeyException($"PEM file contained unsupported object: {obj.GetType().Name}")
				};
			}
			catch (InvalidCipherTextException)
			{
				throw new BadKeyException(
					"PEM file contained an encrypted private key but the passphrase was incorrect"
				);
			}
			catch (PkcsException e)
			{
				if (e.Message != null && e.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
					throw new BadKeyException("PEM file contained an encrypted private key but no passphrase was provided");

				throw new Exception("Failed to decode PKCS#8 private key", e);
			}
			catch (IOException e)
			{
				throw new Exception("Failed to read PEM file", e);
			}
		}
		static void WriteEncryptedPrivateKey(PrivateKeyInfo pkInfo, PemWriter @out, string passphrase)
		{
			byte[] salt = Crypto.RandomBytes(Crypto.SALT_LEN);
			KeyParameter derivedKey = Crypto.DeriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.CBC_DK_LEN);
			byte[] iv = Crypto.RandomBytes(Crypto.IV_LEN);
			BufferedBlockCipher cipher = Crypto.InitAesCbc128Encrypt(derivedKey, iv);
			byte[] encryptedKey = Crypto.RunCipher(cipher, pkInfo.GetEncoded());

			// I wanted to just do this with BC's PKCS8Generator and KcePKCSPBEOutputEncryptorBuilder
			// but it tries to init AES instance of `Cipher` with a `PBKDF2Key` and the former complains
			// So this is basically a reimplementation of that minus the excess OO

			Pbkdf2Params pbkdf2params = new (salt, Crypto.ITERATIONS, Crypto.CBC_DK_LEN, new AlgorithmIdentifier(PkcsObjectIdentifiers.IdHmacWithSha256));
			KeyDerivationFunc keyderivationfunc = new (PkcsObjectIdentifiers.IdPbkdf2, pbkdf2params);
			EncryptionScheme encryptionscheme = new (NistObjectIdentifiers.IdAes128Cbc, ASN1Primitive.FromByteArray(cipher.Parameters.GetEncoded()));
			PbeS2Parameters parameters = new(keyderivationfunc, encryptionscheme);
			AlgorithmIdentifier algorithmidentifier = new (PkcsObjectIdentifiers.IdPbeS2, parameters);
			EncryptedPrivateKeyInfo encryptedPrivateKeyInfo = new(algorithmidentifier, encryptedKey);

			@out.WriteObject(new PemObject(TYPE_ENCRYPTED_PRIVATE_KEY, encryptedPrivateKeyInfo.GetEncoded()));
		}

		/// <summary>
		/// Password finder used by PemReader for encrypted PEMs.
		/// </summary>
		private sealed class PasswordFinder : IPasswordFinder
		{
			private readonly char[] _password;

			public PasswordFinder(string password)
			{
				_password = password?.ToCharArray() ?? Array.Empty<char>();
			}

			public char[] GetPassword()
			{
				return _password;
			}
		}
	}
}
