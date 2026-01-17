using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.IO;

namespace Hedera.Hashgraph.SDK
{
	/**
	 * Internal utility class for handling PEM objects.
	 * <br>
	 * Privacy-Enhanced Mail (PEM) is a de facto file format for storing and
	 * sending cryptographic keys, certificates, and other data, based on a set of
	 * 1993 IETF standards defining "privacy-enhanced mail."
	 */
	sealed class Pem
	{
		private static readonly string TYPE_PRIVATE_KEY = "PRIVATE KEY";
		private static readonly string TYPE_ENCRYPTED_PRIVATE_KEY = "ENCRYPTED PRIVATE KEY";

		/**
		 * Constructor.
		 */
		private Pem() { }

			/**
			 * For some reason, this generates PEM encodings that we ourselves can import, but OpenSSL
			 * doesn't like. We decided to punt on generating encrypted PEMs for now but saving
			 * the code for when we get back to it and/or any demand arises.
			 */
	
		internal static void WriteEncryptedPrivateKey(PrivateKeyInfo pkInfo, TextWriter _out, string passphrase) 
		{

			byte[] salt = Crypto.RandomBytes(Crypto.SALT_LEN);

			KeyParameter derivedKey = Crypto.DeriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.CBC_DK_LEN);

			byte[] iv = Crypto.RandomBytes(Crypto.IV_LEN);

			IBufferedCipher cipher = Crypto.InitAesCbc128Encrypt(derivedKey, iv, false);

			byte[] encryptedKey = Crypto.RunCipher(cipher, pkInfo.GetEncoded());

			// I wanted to just do this with BC's PKCS8Generator and KcePKCSPBEOutputEncryptorBuilder
			// but it tries to init AES instance of `Cipher` with a `PBKDF2Key` and the former complains

			// So this is basically a reimplementation of that minus the excess OO
			PbeS2Parameters parameters = new (
				new KeyDerivationFunc(
					PkcsObjectIdentifiers.IdPbkdf2,
					new Pbkdf2Params(
						salt,
						Crypto.ITERATIONS,
						Crypto.CBC_DK_LEN,
						new AlgorithmIdentifier(PkcsObjectIdentifiers.IdHmacWithSha256))),
				new EncryptionScheme(
					NistObjectIdentifiers.IdAes128Cbc,
					ASN1Primitive.FromByteArray(cipher.getParameters().getEncoded())));

			EncryptedPrivateKeyInfo encryptedPrivateKeyInfo = new (new AlgorithmIdentifier(PkcsObjectIdentifiers.IdPbeS2, parameters), encryptedKey);

			PemWriter writer = new (_out);
			writer.WriteObject(new PemObject(TYPE_ENCRYPTED_PRIVATE_KEY, encryptedPrivateKeyInfo.GetEncoded()));
			writer.Dispose();
		}

    /**
     * Create a private key info object from a reader.
     *
     * @param input                     reader object
     * @param passphrase                passphrase
     * @return                          private key info object
     * @              if IO operations fail
     */
    static PrivateKeyInfo readPrivateKey(Reader input, @Nullable string passphrase) 
		{
        try (readonly var parser = new PEMParser(input)) {
					Object parsedObject = parser.readObject();
				var password = (passphrase != null) ? passphrase.toCharArray() : "".toCharArray();
					if (parsedObject == null) {
						throw new BadKeyException("PEM file did not contain a private key");
		} else if (parsedObject instanceof PKCS8EncryptedPrivateKeyInfo) {
			var decryptProvider = new JceOpenSSLPKCS8DecryptorProviderBuilder()
					.setProvider(new BouncyCastleProvider())
					.build(password);
			var encryptedPrivateKeyInfo = (PKCS8EncryptedPrivateKeyInfo)parsedObject;
			return encryptedPrivateKeyInfo.decryptPrivateKeyInfo(decryptProvider);
		} else if (parsedObject instanceof PrivateKeyInfo) {
			return (PrivateKeyInfo)parsedObject;
		} else if (parsedObject instanceof PEMEncryptedKeyPair) {
			var decryptProvider = new JcePEMDecryptorProviderBuilder()
					.setProvider(new BouncyCastleProvider())
					.build(password);
			var encryptedKeyPair = (PEMEncryptedKeyPair)parsedObject;
			return encryptedKeyPair.decryptKeyPair(decryptProvider).getPrivateKeyInfo();
		} else if (parsedObject instanceof PEMKeyPair) {
			var keyPair = (PEMKeyPair)parsedObject;
			return keyPair.getPrivateKeyInfo();
		} else
		{
			throw new BadKeyException("PEM file contained something the SDK didn't know what to do with: "
					+ parsedObject.getClass().getName());
		}
        } catch (PKCSException e) {
            if (e.getMessage().contains("password empty")) {
                throw new BadKeyException("PEM file contained an encrypted private key but no passphrase was given");
            }
            throw new RuntimeException(e);
        } catch (OperatorCreationException e) {
            throw new RuntimeException(e);
        }
    }
}

}