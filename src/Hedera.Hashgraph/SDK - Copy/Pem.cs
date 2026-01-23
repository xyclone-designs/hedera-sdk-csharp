// SPDX-License-Identifier: Apache-2.0
using Java.Io;
using Javax.Annotation;
using Javax.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Params;
using Org.BouncyCastle.Jce.Provider;
using Org.BouncyCastle.Openssl;
using Org.BouncyCastle.Openssl.Jcajce;
using Org.BouncyCastle.Operator;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Util.Io.Pem;
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
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class for handling PEM objects.
    /// <br>
    /// Privacy-Enhanced Mail (PEM) is a de facto file format for storing and
    /// sending cryptographic keys, certificates, and other data, based on a set of
    /// 1993 IETF standards defining "privacy-enhanced mail."
    /// </summary>
    sealed class Pem
    {
        private static readonly string TYPE_PRIVATE_KEY = "PRIVATE KEY";
        private static readonly string TYPE_ENCRYPTED_PRIVATE_KEY = "ENCRYPTED PRIVATE KEY";
        /// <summary>
        /// Constructor.
        /// </summary>
        private Pem()
        {
        }

        /// <summary>
        /// For some reason, this generates PEM encodings that we ourselves can import, but OpenSSL
        /// doesn't like. We decided to punt on generating encrypted PEMs for now but saving
        /// the code for when we get back to it and/or any demand arises.
        /// </summary>
        static void WriteEncryptedPrivateKey(PrivateKeyInfo pkInfo, Writer @out, string passphrase)
        {
            byte[] salt = Crypto.RandomBytes(Crypto.SALT_LEN);
            KeyParameter derivedKey = Crypto.DeriveKeySha256(passphrase, salt, Crypto.ITERATIONS, Crypto.CBC_DK_LEN);
            byte[] iv = Crypto.RandomBytes(Crypto.IV_LEN);
            Cipher cipher = Crypto.InitAesCbc128Encrypt(derivedKey, iv);
            byte[] encryptedKey = Crypto.RunCipher(cipher, pkInfo.GetEncoded());

            // I wanted to just do this with BC's PKCS8Generator and KcePKCSPBEOutputEncryptorBuilder
            // but it tries to init AES instance of `Cipher` with a `PBKDF2Key` and the former complains
            // So this is basically a reimplementation of that minus the excess OO
            PBES2Parameters parameters = new PBES2Parameters(new KeyDerivationFunc(PKCSObjectIdentifiers.id_PBKDF2, new PBKDF2Params(salt, Crypto.ITERATIONS, Crypto.CBC_DK_LEN, new AlgorithmIdentifier(PKCSObjectIdentifiers.id_hmacWithSHA256))), new EncryptionScheme(NISTObjectIdentifiers.id_aes128_CBC, ASN1Primitive.FromByteArray(cipher.GetParameters().GetEncoded())));
            EncryptedPrivateKeyInfo encryptedPrivateKeyInfo = new EncryptedPrivateKeyInfo(new AlgorithmIdentifier(PKCSObjectIdentifiers.id_PBES2, parameters), encryptedKey);
            PemWriter writer = new PemWriter(@out);
            writer.WriteObject(new PemObject(TYPE_ENCRYPTED_PRIVATE_KEY, encryptedPrivateKeyInfo.GetEncoded()));
            writer.Flush();
        }

        /// <summary>
        /// Create a private key info object from a reader.
        /// </summary>
        /// <param name="input">reader object</param>
        /// <param name="passphrase">passphrase</param>
        /// <returns>                         private key info object</returns>
        /// <exception cref="IOException">if IO operations fail</exception>
        static PrivateKeyInfo ReadPrivateKey(Reader input, string passphrase)
        {
            try
            {
                using (var parser = new PEMParser(input))
                {
                    object parsedObject = parser.ReadObject();
                    var password = (passphrase != null) ? passphrase.ToCharArray() : "".ToCharArray();
                    if (parsedObject == null)
                    {
                        throw new BadKeyException("PEM file did not contain a private key");
                    }
                    else if (parsedObject is PKCS8EncryptedPrivateKeyInfo)
                    {
                        var decryptProvider = new JceOpenSSLPKCS8DecryptorProviderBuilder().SetProvider(new BouncyCastleProvider()).Build(password);
                        var encryptedPrivateKeyInfo = (PKCS8EncryptedPrivateKeyInfo)parsedObject;
                        return encryptedPrivateKeyInfo.DecryptPrivateKeyInfo(decryptProvider);
                    }
                    else if (parsedObject is PrivateKeyInfo)
                    {
                        return (PrivateKeyInfo)parsedObject;
                    }
                    else if (parsedObject is PEMEncryptedKeyPair)
                    {
                        var decryptProvider = new JcePEMDecryptorProviderBuilder().SetProvider(new BouncyCastleProvider()).Build(password);
                        var encryptedKeyPair = (PEMEncryptedKeyPair)parsedObject;
                        return encryptedKeyPair.DecryptKeyPair(decryptProvider).GetPrivateKeyInfo();
                    }
                    else if (parsedObject is PEMKeyPair)
                    {
                        var keyPair = (PEMKeyPair)parsedObject;
                        return keyPair.GetPrivateKeyInfo();
                    }
                    else
                    {
                        throw new BadKeyException("PEM file contained something the SDK didn't know what to do with: " + parsedObject.GetType().GetName());
                    }
                }
            }
            catch (PKCSException e)
            {
                if (e.GetMessage().Contains("password empty"))
                {
                    throw new BadKeyException("PEM file contained an encrypted private key but no passphrase was given");
                }

                throw new Exception(e);
            }
            catch (OperatorCreationException e)
            {
                throw new Exception(e);
            }
        }
    }
}