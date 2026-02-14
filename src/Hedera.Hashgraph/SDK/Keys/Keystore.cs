// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Exceptions;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Hedera.Hashgraph.SDK.Keys
{
	/// <summary>
	/// Internal utility class to serialize / deserialize between java object / json representation.
	/// </summary>

	sealed class Keystore
	{
		private readonly byte[] KeyBytes;

		private Keystore(byte[] keyBytes)
		{
			KeyBytes = keyBytes;
		}
		public Keystore(PrivateKey privateKey)
		{
			KeyBytes = privateKey.ToBytes();
		}

		public static Keystore FromStream(Stream stream, string passphrase)
		{
			try
			{
				using var document = JsonDocument.Parse(stream);
				return FromJson(document.RootElement, passphrase);
			}
			catch (JsonException e)
			{
				throw new BadKeyException(e);
			}
			catch (IOException)
			{
				throw;
			}
		}
		private static Keystore FromJson(JsonElement root, string passphrase)
		{
			int version = ExpectInt(root, "version");

			return version switch
			{
				1 => ParseKeystoreV1(ExpectObject(root, "crypto"), passphrase),
				2 => ParseKeystoreV2(ExpectObject(root, "crypto"), passphrase),
				_ => throw new BadKeyException("unsupported keystore version: " + version)
			};
		}
		private static Keystore ParseKeystoreV1(JsonElement crypto, string passphrase)
		{
			string ciphertext = ExpectString(crypto, "ciphertext");
			string ivString = ExpectString(ExpectObject(crypto, "cipherparams"), "iv");
			string cipher = ExpectString(crypto, "cipher");
			string kdf = ExpectString(crypto, "kdf");
			JsonElement kdfParams = ExpectObject(crypto, "kdfparams");
			string macString = ExpectString(crypto, "mac");

			if (cipher != "aes-128-ctr")
				throw new BadKeyException("unsupported keystore cipher: " + cipher);

			if (kdf != "pbkdf2")
				throw new BadKeyException("unsupported KDF: " + kdf);

			int dkLen = ExpectInt(kdfParams, "dkLen");
			string saltStr = ExpectString(kdfParams, "salt");
			int count = ExpectInt(kdfParams, "c");
			string prf = ExpectString(kdfParams, "prf");

			if (prf != "hmac-sha256")
				throw new BadKeyException("unsupported KDF hash function: " + prf);

			byte[] cipherBytes = Hex.Decode(ciphertext);
			byte[] iv = Hex.Decode(ivString);
			byte[] mac = Hex.Decode(macString);
			byte[] salt = Hex.Decode(saltStr);

			KeyParameter cipherKey =
				Crypto.DeriveKeySha256(passphrase, salt, count, dkLen);

			byte[] testHmac = Crypto.CalcHmacSha384(cipherKey, null, cipherBytes);

			if (!CryptographicOperations.FixedTimeEquals(mac, testHmac))
				throw new BadKeyException("HMAC mismatch; passphrase is incorrect");

			return new Keystore(Crypto.DecryptAesCtr128(cipherKey, iv, cipherBytes));
		}
		private static Keystore ParseKeystoreV2(JsonElement crypto, string passphrase)
		{
			string ciphertext = ExpectString(crypto, "ciphertext");
			string ivString = ExpectString(ExpectObject(crypto, "cipherparams"), "iv");
			string cipher = ExpectString(crypto, "cipher");
			string kdf = ExpectString(crypto, "kdf");
			JsonElement kdfParams = ExpectObject(crypto, "kdfparams");
			string macString = ExpectString(crypto, "mac");

			if (cipher != "aes-128-ctr")
				throw new BadKeyException("unsupported keystore cipher: " + cipher);

			if (kdf != "pbkdf2")
				throw new BadKeyException("unsupported KDF: " + kdf);

			int dkLen = ExpectInt(kdfParams, "dkLen");
			string saltStr = ExpectString(kdfParams, "salt");
			int count = ExpectInt(kdfParams, "c");
			string prf = ExpectString(kdfParams, "prf");

			if (prf != "hmac-sha256")
				throw new BadKeyException("unsupported KDF hash function: " + prf);

			byte[] cipherBytes = Hex.Decode(ciphertext);
			byte[] iv = Hex.Decode(ivString);
			byte[] mac = Hex.Decode(macString);
			byte[] salt = Hex.Decode(saltStr);

			KeyParameter cipherKey =
				Crypto.DeriveKeySha256(passphrase, salt, count, dkLen);

			byte[] testHmac =
				Crypto.CalcHmacSha384(cipherKey, iv, cipherBytes);

			if (!CryptographicOperations.FixedTimeEquals(mac, testHmac))
				throw new BadKeyException("HMAC mismatch; passphrase is incorrect");

			return new Keystore(
				Crypto.DecryptAesCtr128(cipherKey, iv, cipherBytes)
			);
		}

		private static int ExpectInt(JsonElement obj, string key)
		{
			if (!obj.TryGetProperty(key, out JsonElement value) ||
				value.ValueKind != JsonValueKind.Number ||
				!value.TryGetInt32(out int result))
			{
				throw new Exception($"expected key '{key}' to be an integer");
			}

			return result;
		}
		private static string ExpectString(JsonElement obj, string key)
		{
			if (!obj.TryGetProperty(key, out JsonElement value) ||
				value.ValueKind != JsonValueKind.String)
			{
				throw new Exception($"expected key '{key}' to be a string");
			}

			return value.GetString()!;
		}

		private static JsonElement ExpectObject(JsonElement obj, string key)
		{
			if (!obj.TryGetProperty(key, out JsonElement value) ||
				value.ValueKind != JsonValueKind.Object)
			{
				throw new Exception($"expected key '{key}' to be an object");
			}

			return value;
		}

		public PrivateKey GetEd25519()
		{
			return PrivateKey.FromBytes(KeyBytes);
		}

		public void Export(Stream outputStream, string passphrase)
		{
			using var writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions
			{
				Indented = false
			});

			WriteExportJson(writer, passphrase);
			writer.Flush();
		}
		private void WriteExportJson(Utf8JsonWriter writer, string passphrase)
		{
			writer.WriteStartObject();
			writer.WriteNumber("version", 2);

			writer.WritePropertyName("crypto");
			writer.WriteStartObject();

			writer.WriteString("cipher", "aes-128-ctr");
			writer.WriteString("kdf", "pbkdf2");

			byte[] salt = Crypto.RandomBytes(Crypto.SALT_LEN);
			KeyParameter cipherKey =
				Crypto.DeriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.DK_LEN);

			byte[] iv = Crypto.RandomBytes(Crypto.IV_LEN);
			byte[] cipherBytes = Crypto.EncryptAesCtr128(cipherKey, iv, KeyBytes);
			byte[] mac = Crypto.CalcHmacSha384(cipherKey, iv, cipherBytes);

			writer.WritePropertyName("cipherparams");
			writer.WriteStartObject();
			writer.WriteString("iv", Hex.ToHexString(iv));
			writer.WriteEndObject();

			writer.WriteString("ciphertext", Hex.ToHexString(cipherBytes));

			writer.WritePropertyName("kdfparams");
			writer.WriteStartObject();
			writer.WriteNumber("dkLen", Crypto.DK_LEN);
			writer.WriteString("salt", Hex.ToHexString(salt));
			writer.WriteNumber("c", Crypto.ITERATIONS);
			writer.WriteString("prf", "hmac-sha256");
			writer.WriteEndObject();

			writer.WriteString("mac", Hex.ToHexString(mac));

			writer.WriteEndObject(); // crypto
			writer.WriteEndObject(); // root
		}
	}

}