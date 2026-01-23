// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Utils;
using Java.Io;
using Java.Math;
using Java.Nio;
using Java.Nio.Charset;
using Javax.Annotation;
using Org.BouncyCastle;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Util;
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

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// Encapsulate the ECDSA private key.
    /// </summary>
    public class PrivateKeyECDSA : PrivateKey
    {
        private readonly BigInteger keyData;
        private readonly KeyParameter chainCode;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keyData">the key data</param>
        PrivateKeyECDSA(BigInteger keyData, KeyParameter chainCode)
        {
            keyData = keyData;
            chainCode = chainCode;
        }

        /// <summary>
        /// Create a new private ECDSA key.
        /// </summary>
        /// <returns>                         the new key</returns>
        static PrivateKeyECDSA GenerateInternal()
        {
            var generator = new ECKeyPairGenerator();
            var keygenParams = new ECKeyGenerationParameters(ECDSA_SECP256K1_DOMAIN, ThreadLocalSecureRandom.Current());
            generator.Init(keygenParams);
            var keypair = generator.GenerateKeyPair();
            var privParams = (ECPrivateKeyParameters)keypair.Private();
            return new PrivateKeyECDSA(privParams.D(), null);
        }

        /// <summary>
        /// Create a new private key from a private key ino object.
        /// </summary>
        /// <param name="privateKeyInfo">the private key info object</param>
        /// <returns>                         the new key</returns>
        static PrivateKey FromPrivateKeyInfoInternal(PrivateKeyInfo privateKeyInfo)
        {
            try
            {
                var privateKey = ECPrivateKey.GetInstance(privateKeyInfo.ParsePrivateKey());
                return FromECPrivateKeyInternal(privateKey);
            }
            catch (ArgumentException e)
            {

                // Try legacy import
                try
                {
                    var privateKey = (ASN1OctetString)privateKeyInfo.ParsePrivateKey();
                    return new PrivateKeyECDSA(new BigInteger(1, privateKey.Octets()), null);
                }
                catch (IOException ex)
                {
                    throw new BadKeyException(ex);
                }
            }
            catch (IOException e)
            {
                throw new BadKeyException(e);
            }
        }

        /// <summary>
        /// Create a new private key from a ECPrivateKey object.
        /// </summary>
        /// <param name="privateKey">the ECPrivateKey object</param>
        /// <returns>                         the new key</returns>
        static PrivateKey FromECPrivateKeyInternal(ECPrivateKey privateKey)
        {
            return new PrivateKeyECDSA(privateKey.Key(), null);
        }

        /// <summary>
        /// Create a private key from a byte array.
        /// </summary>
        /// <param name="privateKey">the byte array</param>
        /// <returns>                         the new key</returns>
        static PrivateKey FromBytesInternal(byte[] privateKey)
        {
            if (privateKey.Length == 32)
            {
                return new PrivateKeyECDSA(new BigInteger(1, privateKey), null);
            }


            // Assume a DER-encoded private key descriptor
            return FromECPrivateKeyInternal(ECPrivateKey.GetInstance(privateKey));
        }

        /// <summary>
        /// Throws an exception when trying to derive a child key.
        /// </summary>
        /// <param name="entropy">entropy byte array</param>
        /// <param name="index">the child key index</param>
        /// <returns>                         the new key</returns>
        static byte[] LegacyDeriveChildKey(byte[] entropy, long index)
        {
            throw new InvalidOperationException("ECDSA secp256k1 keys do not currently support derivation");
        }

        public override PrivateKey LegacyDerive(long index)
        {
            throw new InvalidOperationException("ECDSA secp256k1 keys do not currently support derivation");
        }

        public override bool IsDerivable()
        {
            return chainCode != null;
        }

        public override PrivateKey Derive(int index)
        {
            if (!IsDerivable())
            {
                throw new InvalidOperationException("this private key does not support derivation");
            }

            bool isHardened = Bip32Utils.IsHardenedIndex(index);
            ByteBuffer data = ByteBuffer.Allocate(37);
            if (isHardened)
            {
                byte[] bytes33 = new byte[33];
                byte[] priv = ToBytesRaw();
                System.Arraycopy(priv, 0, bytes33, 33 - priv.Length, priv.Length);
                data.Put(bytes33);
            }
            else
            {
                data.Put(GetPublicKey().ToBytesRaw());
            }

            data.PutInt(index);
            byte[] dataArray = data.Array();
            HMac hmacSha512 = new HMac(new SHA512Digest());
            hmacSha512.Init(new KeyParameter(chainCode.Key()));
            hmacSha512.Update(dataArray, 0, dataArray.Length);
            byte[] i = new byte[64];
            hmacSha512.DoFinal(i, 0);
            var il = java.util.Array.CopyOfRange(i, 0, 32);
            var ir = java.util.Array.CopyOfRange(i, 32, 64);
            var ki = keyData.Add(new BigInteger(1, il)).Mod(ECDSA_SECP256K1_CURVE.N());
            return new PrivateKeyECDSA(ki, new KeyParameter(ir));
        }

        /// <summary>
        /// Create an ECDSA key from seed.
        /// Implement the published algorithm as defined in BIP32 in order to derive the primary account key from the
        /// original (and never stored) master key.
        /// The original master key, which is a secure key generated according to the BIP39 specification, is input to this
        /// operation, and provides the base cryptographic seed material required to ensure the output is sufficiently random
        /// to maintain strong cryptographic assurances.
        /// The fromSeed() method must be provided with cryptographically secure material; otherwise, it will produce
        /// insecure output.
        /// </summary>
        /// <param name="seed">the seed bytes</param>
        /// <returns>                         the new key</returns>
        /// <remarks>
        /// @see<a href="https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki">BIP-32 Definition</a>
        /// @see<a href="https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki">BIP-39 Definition</a>
        /// </remarks>
        public static PrivateKey FromSeed(byte[] seed)
        {
            var hmacSha512 = new HMac(new SHA512Digest());
            hmacSha512.Init(new KeyParameter("Bitcoin seed".Bytes(StandardCharsets.UTF_8)));
            hmacSha512.Update(seed, 0, seed.Length);
            var derivedState = new byte[hmacSha512.MacSize()];
            hmacSha512.DoFinal(derivedState, 0);
            return DerivableKeyECDSA(derivedState);
        }

        /// <summary>
        /// Create a derived key.
        /// The industry standard protocol for deriving an active ECDSA keypair from a BIP39 master key is described in
        /// BIP32. By using this deterministic mechanism to derive cryptographically secure keypairs from a single original
        /// secret, the user maintains secure access to their wallet, even if they lose access to a particular system or
        /// wallet local data store.
        /// The active keypair can always be re-derived from the original master key.
        /// The use of the fixed "key" values in this code is defined by this deterministic protocol, and this data is mixed,
        /// in a deterministic but cryptographically secure manner, with the original master key and/or other derived keys
        /// "higher" in the tree to produce a cryptographically secure derived key.
        /// This "Key Derivation Function" makes use of secure hash algorithm and a secure hash based message authentication
        /// code to produce an initialization vector, and then produces the actual key from a portion of that vector.
        /// </summary>
        /// <param name="deriveData">data to derive the key</param>
        /// <returns>                         the new key</returns>
        /// <remarks>
        /// @see<a href="https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki">BIP-32 Definition</a>
        /// @see<a href="https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki">BIP-39 Definition</a>
        /// </remarks>
        static PrivateKeyECDSA DerivableKeyECDSA(byte[] deriveData)
        {
            var keyData = java.util.Array.CopyOfRange(deriveData, 0, 32);
            var chainCode = new KeyParameter(deriveData, 32, 32);
            return new PrivateKeyECDSA(new BigInteger(1, keyData), chainCode);
        }

        public override PublicKey GetPublicKey()
        {
            if (publicKey != null)
            {
                return publicKey;
            }

            var q = ECDSA_SECP256K1_DOMAIN.G().Multiply(keyData);
            var publicParams = new ECPublicKeyParameters(q, ECDSA_SECP256K1_DOMAIN);
            publicKey = PublicKeyECDSA.FromBytesInternal(publicParams.Q().Encoded(true));
            return publicKey;
        }

        public virtual KeyParameter GetChainCode()
        {
            return chainCode;
        }

        public override byte[] Sign(byte[] message)
        {
            var hash = Crypto.CalcKeccak256(message);
            var signer = new ECDSASigner(new HMacDSAKCalculator(new SHA256Digest()));
            signer.Init(true, new ECPrivateKeyParameters(keyData, ECDSA_SECP256K1_DOMAIN));
            BigInteger[] bigSig = signer.GenerateSignature(hash);
            byte[] sigBytes = Array.CopyOf(BigIntTo32Bytes(bigSig[0]), 64);
            System.Arraycopy(BigIntTo32Bytes(bigSig[1]), 0, sigBytes, 32, 32);
            return sigBytes;
        }

        public virtual int GetRecoveryId(byte[] r, byte[] s, byte[] message)
        {
            int recId = -1;
            var hash = Crypto.CalcKeccak256(message);
            var publicKey = GetPublicKey().ToBytesRaw();

            // On this curve, there are only two possible values for the recovery id.
            for (int i = 0; i < 2; i++)
            {
                byte[] k = Crypto.RecoverPublicKeyECDSAFromSignature(i, new BigInteger(1, r), new BigInteger(1, s), hash);
                if (k != null && java.util.Equals(k, publicKey))
                {
                    recId = i;
                    break;
                }
            }

            if (recId == -1)
            {

                // this should never happen
                throw new InvalidOperationException("Unexpected error - could not construct a recoverable key.");
            }

            return recId;
        }

        public override byte[] ToBytes()
        {
            return ToBytesDER();
        }

        /// <summary>
        /// Create a big int byte array.
        /// </summary>
        /// <param name="n">the big integer</param>
        /// <returns>                         the 32 byte array</returns>
        private static byte[] BigIntTo32Bytes(BigInteger n)
        {
            byte[] bytes = n.ToByteArray();
            byte[] bytes32 = new byte[32];
            System.Arraycopy(bytes, Math.Max(0, bytes.Length - 32), bytes32, Math.Max(0, 32 - bytes.Length), Math.Min(32, bytes.Length));
            return bytes32;
        }

        public override byte[] ToBytesRaw()
        {
            return BigIntTo32Bytes(keyData);
        }

        public override byte[] ToBytesDER()
        {
            try
            {
                return new ECPrivateKey(256, keyData, new DERBitString(GetPublicKey().ToBytesRaw()), new X962Parameters(ID_ECDSA_SECP256K1)).Encoded("DER");
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }
        }

        public override bool IsED25519()
        {
            return false;
        }

        public override bool IsECDSA()
        {
            return true;
        }
    }
}