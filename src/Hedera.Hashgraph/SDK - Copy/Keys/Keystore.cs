// SPDX-License-Identifier: Apache-2.0
using Com.Google.Gson;
using Com.Google.Gson.Stream;
using Java.Io;
using Java.Nio.Charset;
using Java.Security;
using Java.Util;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// Internal utility class to serialize / deserialize between java object / json representation.
    /// </summary>
    sealed class Keystore
    {
        private static readonly Gson gson = new Gson();
        private static readonly JsonParser jsonParser = new JsonParser();
        private byte[] keyBytes;
        private Keystore(byte[] keyBytes)
        {
            keyBytes = keyBytes;
        }

        public Keystore(PrivateKey privateKey)
        {
            keyBytes = privateKey.ToBytes();
        }

        public static Keystore FromStream(InputStream stream, string passphrase)
        {
            try
            {
                JsonObject jsonObject = jsonParser.Parse(new InputStreamReader(stream, StandardCharsets.UTF_8)).AsJsonObject();
                return FromJson(jsonObject, passphrase);
            }
            catch (InvalidOperationException e)
            {
                throw new BadKeyException(Optional.OfNullable(e.Message()).OrElse("failed to parse Keystore"));
            }
            catch (JsonIOException e)
            {

                // RFC (@abonander): I'm all for keeping this as an unchecked exception
                // but I want consistency with export() so this may involve creating our own exception
                // because JsonIOException is kinda leaking implementation details.
                throw (IOException)Objects.RequireNonNull(e.Cause());
            }
            catch (JsonSyntaxException e)
            {
                throw new BadKeyException(e);
            }
        }

        private static Keystore FromJson(JsonObject @object, string passphrase)
        {
            int version = ExpectInt(@object, "version");

            //noinspection SwitchStatementWithTooFewBranches
            switch (version)
            {
                case 1:
                    return ParseKeystoreV1(ExpectObject(@object, "crypto"), passphrase);
                case 2:
                    return ParseKeystoreV2(ExpectObject(@object, "crypto"), passphrase);
                default:
                    throw new BadKeyException("unsupported keystore version: " + version);
                    break;
            }
        }

        private static Keystore ParseKeystoreV1(JsonObject crypto, string passphrase)
        {
            string ciphertext = ExpectString(crypto, "ciphertext");
            string ivString = ExpectString(ExpectObject(crypto, "cipherparams"), "iv");
            string cipher = ExpectString(crypto, "cipher");
            string kdf = ExpectString(crypto, "kdf");
            JsonObject kdfParams = ExpectObject(crypto, "kdfparams");
            string macString = ExpectString(crypto, "mac");
            if (!cipher.Equals("aes-128-ctr"))
            {
                throw new BadKeyException("unsupported keystore cipher: " + cipher);
            }

            if (!kdf.Equals("pbkdf2"))
            {
                throw new BadKeyException("unsuppported KDF: " + kdf);
            }

            int dkLen = ExpectInt(kdfParams, "dkLen");
            string saltStr = ExpectString(kdfParams, "salt");
            int count = ExpectInt(kdfParams, "c");
            string prf = ExpectString(kdfParams, "prf");
            if (!prf.Equals("hmac-sha256"))
            {
                throw new BadKeyException("unsupported KDF hash function: " + prf);
            }

            byte[] cipherBytes = Hex.Decode(ciphertext);
            byte[] iv = Hex.Decode(ivString);
            byte[] mac = Hex.Decode(macString);
            byte[] salt = Hex.Decode(saltStr);
            KeyParameter cipherKey = Crypto.DeriveKeySha256(passphrase, salt, count, dkLen);
            byte[] testHmac = Crypto.CalcHmacSha384(cipherKey, null, cipherBytes);
            if (!MessageDigest.IsEqual(mac, testHmac))
            {
                throw new BadKeyException("HMAC mismatch; passphrase is incorrect");
            }

            return new Keystore(Crypto.DecryptAesCtr128(cipherKey, iv, cipherBytes));
        }

        private static Keystore ParseKeystoreV2(JsonObject crypto, string passphrase)
        {
            string ciphertext = ExpectString(crypto, "ciphertext");
            string ivString = ExpectString(ExpectObject(crypto, "cipherparams"), "iv");
            string cipher = ExpectString(crypto, "cipher");
            string kdf = ExpectString(crypto, "kdf");
            JsonObject kdfParams = ExpectObject(crypto, "kdfparams");
            string macString = ExpectString(crypto, "mac");
            if (!cipher.Equals("aes-128-ctr"))
            {
                throw new BadKeyException("unsupported keystore cipher: " + cipher);
            }

            if (!kdf.Equals("pbkdf2"))
            {
                throw new BadKeyException("unsuppported KDF: " + kdf);
            }

            int dkLen = ExpectInt(kdfParams, "dkLen");
            string saltStr = ExpectString(kdfParams, "salt");
            int count = ExpectInt(kdfParams, "c");
            string prf = ExpectString(kdfParams, "prf");
            if (!prf.Equals("hmac-sha256"))
            {
                throw new BadKeyException("unsupported KDF hash function: " + prf);
            }

            byte[] cipherBytes = Hex.Decode(ciphertext);
            byte[] iv = Hex.Decode(ivString);
            byte[] mac = Hex.Decode(macString);
            byte[] salt = Hex.Decode(saltStr);
            KeyParameter cipherKey = Crypto.DeriveKeySha256(passphrase, salt, count, dkLen);
            byte[] testHmac = Crypto.CalcHmacSha384(cipherKey, iv, cipherBytes);
            if (!MessageDigest.IsEqual(mac, testHmac))
            {
                throw new BadKeyException("HMAC mismatch; passphrase is incorrect");
            }

            return new Keystore(Crypto.DecryptAesCtr128(cipherKey, iv, cipherBytes));
        }

        private static JsonObject ExpectObject(JsonObject @object, string key)
        {
            try
            {
                return @object[key].AsJsonObject();
            }
            catch (ClassCastException e)
            {
                throw new Exception("expected key '" + key + "' to be an object", e);
            }
            catch (NullReferenceException e)
            {
                throw new Exception("expected key '" + key + "' to be an object", e);
            }
        }

        private static int ExpectInt(JsonObject @object, string key)
        {
            try
            {
                return @object[key].AsInt();
            }
            catch (ClassCastException e)
            {
                throw new Exception("expected key '" + key + "' to be an integer", e);
            }
            catch (NullReferenceException e)
            {
                throw new Exception("expected key '" + key + "' to be an integer", e);
            }
        }

        private static string ExpectString(JsonObject @object, string key)
        {
            try
            {
                return @object[key].AsString();
            }
            catch (ClassCastException e)
            {
                throw new Exception("expected key '" + key + "' to be a string", e);
            }
            catch (NullReferenceException e)
            {
                throw new Exception("expected key '" + key + "' to be a string", e);
            }
        }

        /// <summary>
        /// Get the decoded key from this keystore as an {@link PrivateKey}.
        /// </summary>
        /// <exception cref="BadKeyException">if the key bytes are of an incorrect length for a raw
        ///                         private key or private key + public key, or do not represent a DER encoded Ed25519
        ///                         private key.</exception>
        public PrivateKey GetEd25519()
        {
            return PrivateKey.FromBytes(keyBytes);
        }

        public void Export(OutputStream outputStream, string passphrase)
        {
            JsonWriter writer = new JsonWriter(new OutputStreamWriter(outputStream, StandardCharsets.UTF_8));
            gson.ToJson(ExportJson(passphrase), writer);
            writer.Flush();
        }

        private JsonObject ExportJson(string passphrase)
        {
            JsonObject object = new JsonObject();
            @object.AddProperty("version", 2);
            JsonObject crypto = new JsonObject();
            crypto.AddProperty("cipher", "aes-128-ctr");
            crypto.AddProperty("kdf", "pbkdf2");
            byte[] salt = Crypto.RandomBytes(Crypto.SALT_LEN);
            KeyParameter cipherKey = Crypto.DeriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.DK_LEN);
            byte[] iv = Crypto.RandomBytes(Crypto.IV_LEN);
            byte[] cipherBytes = Crypto.EncryptAesCtr128(cipherKey, iv, keyBytes);
            byte[] mac = Crypto.CalcHmacSha384(cipherKey, iv, cipherBytes);
            JsonObject cipherParams = new JsonObject();
            cipherParams.AddProperty("iv", Hex.ToHexString(iv));
            JsonObject kdfParams = new JsonObject();
            kdfParams.AddProperty("dkLen", Crypto.DK_LEN);
            kdfParams.AddProperty("salt", Hex.ToHexString(salt));
            kdfParams.AddProperty("c", Crypto.ITERATIONS);
            kdfParams.AddProperty("prf", "hmac-sha256");
            crypto.Add("cipherparams", cipherParams);
            crypto.AddProperty("ciphertext", Hex.ToHexString(cipherBytes));
            crypto.Add("kdfparams", kdfParams);
            crypto.AddProperty("mac", Hex.ToHexString(mac));
            @object.Add("crypto", crypto);
            return @object;
        }
    }
}