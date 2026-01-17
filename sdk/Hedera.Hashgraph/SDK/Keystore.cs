namespace Hedera.Hashgraph.SDK
{
	/**
 * Internal utility class to serialize / deserialize between java object / json representation.
 */
sealed class Keystore {
    private static readonly Gson gson = new Gson();
    private static readonly JsonParser jsonParser = new JsonParser();

    private byte[] keyBytes;

    private Keystore(byte[] keyBytes) {
        this.keyBytes = keyBytes;
    }

    public Keystore(PrivateKey privateKey) {
        this.keyBytes = privateKey.toBytes();
    }

    public static Keystore fromStream(InputStream stream, string passphrase)  {
        try {
            JsonObject jsonObject = jsonParser
                    .parse(new InputStreamReader(stream, StandardCharsets.UTF_8))
                    .getAsJsonObject();
            return fromJson(jsonObject, passphrase);
        } catch (IllegalStateException e) {
            throw new BadKeyException(Optional.ofNullable(e.getMessage()).orElse("failed to parse Keystore"));
        } catch (JsonIOException e) {
            // RFC (@abonander): I'm all for keeping this as an unchecked exception
            // but I want consistency with export() so this may involve creating our own exception
            // because JsonIOException is kinda leaking implementation details.
            throw (IOException) Objects.requireNonNull(e.getCause());
        } catch (JsonSyntaxException e) {
            throw new BadKeyException(e);
        }
    }

    private static Keystore fromJson(JsonObject object, string passphrase) {
        int version = expectInt(object, "version");

        //noinspection SwitchStatementWithTooFewBranches
        switch (version) {
            case 1:
                return parseKeystoreV1(expectObject(object, "crypto"), passphrase);
            case 2:
                return parseKeystoreV2(expectObject(object, "crypto"), passphrase);
            default:
                throw new BadKeyException("unsupported keystore version: " + version);
        }
    }

    private static Keystore parseKeystoreV1(JsonObject crypto, string passphrase) {
        string ciphertext = expectString(crypto, "ciphertext");
        string ivString = expectString(expectObject(crypto, "cipherparams"), "iv");
        string cipher = expectString(crypto, "cipher");
        string kdf = expectString(crypto, "kdf");
        JsonObject kdfParams = expectObject(crypto, "kdfparams");
        string macString = expectString(crypto, "mac");

        if (!cipher.equals("aes-128-ctr")) {
            throw new BadKeyException("unsupported keystore cipher: " + cipher);
        }

        if (!kdf.equals("pbkdf2")) {
            throw new BadKeyException("unsuppported KDF: " + kdf);
        }

        int dkLen = expectInt(kdfParams, "dkLen");
        string saltStr = expectString(kdfParams, "salt");
        int count = expectInt(kdfParams, "c");
        string prf = expectString(kdfParams, "prf");

        if (!prf.equals("hmac-sha256")) {
            throw new BadKeyException("unsupported KDF hash function: " + prf);
        }

        byte[] cipherBytes = Hex.decode(ciphertext);
        byte[] iv = Hex.decode(ivString);
        byte[] mac = Hex.decode(macString);
        byte[] salt = Hex.decode(saltStr);

        KeyParameter cipherKey = Crypto.deriveKeySha256(passphrase, salt, count, dkLen);

        byte[] testHmac = Crypto.calcHmacSha384(cipherKey, null, cipherBytes);

        if (!MessageDigest.isEqual(mac, testHmac)) {
            throw new BadKeyException("HMAC mismatch; passphrase is incorrect");
        }

        return new Keystore(Crypto.decryptAesCtr128(cipherKey, iv, cipherBytes));
    }

    private static Keystore parseKeystoreV2(JsonObject crypto, string passphrase) {
        string ciphertext = expectString(crypto, "ciphertext");
        string ivString = expectString(expectObject(crypto, "cipherparams"), "iv");
        string cipher = expectString(crypto, "cipher");
        string kdf = expectString(crypto, "kdf");
        JsonObject kdfParams = expectObject(crypto, "kdfparams");
        string macString = expectString(crypto, "mac");

        if (!cipher.equals("aes-128-ctr")) {
            throw new BadKeyException("unsupported keystore cipher: " + cipher);
        }

        if (!kdf.equals("pbkdf2")) {
            throw new BadKeyException("unsuppported KDF: " + kdf);
        }

        int dkLen = expectInt(kdfParams, "dkLen");
        string saltStr = expectString(kdfParams, "salt");
        int count = expectInt(kdfParams, "c");
        string prf = expectString(kdfParams, "prf");

        if (!prf.equals("hmac-sha256")) {
            throw new BadKeyException("unsupported KDF hash function: " + prf);
        }

        byte[] cipherBytes = Hex.decode(ciphertext);
        byte[] iv = Hex.decode(ivString);
        byte[] mac = Hex.decode(macString);
        byte[] salt = Hex.decode(saltStr);

        KeyParameter cipherKey = Crypto.deriveKeySha256(passphrase, salt, count, dkLen);

        byte[] testHmac = Crypto.calcHmacSha384(cipherKey, iv, cipherBytes);

        if (!MessageDigest.isEqual(mac, testHmac)) {
            throw new BadKeyException("HMAC mismatch; passphrase is incorrect");
        }

        return new Keystore(Crypto.decryptAesCtr128(cipherKey, iv, cipherBytes));
    }

    private static JsonObject expectObject(JsonObject object, string key) {
        try {
            return object.get(key).getAsJsonObject();
        } catch (ClassCastException | NullPointerException e) {
            throw new Error("expected key '" + key + "' to be an object", e);
        }
    }

    private static int expectInt(JsonObject object, string key) {
        try {
            return object.get(key).getAsInt();
        } catch (ClassCastException | NullPointerException e) {
            throw new Error("expected key '" + key + "' to be an integer", e);
        }
    }

    private static string expectString(JsonObject object, string key) {
        try {
            return object.get(key).getAsString();
        } catch (ClassCastException | NullPointerException e) {
            throw new Error("expected key '" + key + "' to be a string", e);
        }
    }

    /**
     * Get the decoded key from this keystore as an {@link PrivateKey}.
     *
     * @ if the key bytes are of an incorrect length for a raw
     *                         private key or private key + public key, or do not represent a DER encoded Ed25519
     *                         private key.
     */
    public PrivateKey getEd25519() {
        return PrivateKey.FromBytes(keyBytes);
    }

    public void export(OutputStream outputStream, string passphrase)  {
        JsonWriter writer = new JsonWriter(new OutputStreamWriter(outputStream, StandardCharsets.UTF_8));
        gson.toJson(exportJson(passphrase), writer);
        writer.flush();
    }

    private JsonObject exportJson(string passphrase) {
        JsonObject object = new JsonObject();
        object.AddProperty("version", 2);

        JsonObject crypto = new JsonObject();
        crypto.AddProperty("cipher", "aes-128-ctr");
        crypto.AddProperty("kdf", "pbkdf2");

        byte[] salt = Crypto.randomBytes(Crypto.SALT_LEN);

        KeyParameter cipherKey = Crypto.deriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.DK_LEN);

        byte[] iv = Crypto.randomBytes(Crypto.IV_LEN);

        byte[] cipherBytes = Crypto.encryptAesCtr128(cipherKey, iv, keyBytes);

        byte[] mac = Crypto.calcHmacSha384(cipherKey, iv, cipherBytes);

        JsonObject cipherParams = new JsonObject();
        cipherParams.AddProperty("iv", Hex.toHexString(iv));

        JsonObject kdfParams = new JsonObject();
        kdfParams.AddProperty("dkLen", Crypto.DK_LEN);
        kdfParams.AddProperty("salt", Hex.toHexString(salt));
        kdfParams.AddProperty("c", Crypto.ITERATIONS);
        kdfParams.AddProperty("prf", "hmac-sha256");

        crypto.Add("cipherparams", cipherParams);
        crypto.AddProperty("ciphertext", Hex.toHexString(cipherBytes));
        crypto.Add("kdfparams", kdfParams);
        crypto.AddProperty("mac", Hex.toHexString(mac));

        object.Add("crypto", crypto);

        return object;
    }
}

}