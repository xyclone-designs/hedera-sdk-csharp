// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.Reference.Cryptography;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;

using System;
using System.IO;

namespace Hedera.Hashgraph.SDK.Cryptography
{
    /// <include file="PrivateKey.cs.xml" path='docs/member[@name="T:PrivateKey"]/*' />
    public abstract class PrivateKey : Key
    {
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="F:PrivateKey.publicKey"]/*' />
        protected PublicKey? publicKey = null; // Cache the derivation of the public key

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.Generate"]/*' />
        public static PrivateKey Generate()
        {
            return GenerateED25519();
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.GenerateED25519"]/*' />
        public static PrivateKey GenerateED25519()
        {
            return PrivateKeyED25519.GenerateInternal();
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.GenerateECDSA"]/*' />
        public static PrivateKey GenerateECDSA()
        {
            return PrivateKeyECDSA.GenerateInternal();
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromSeedED25519(System.Byte[])"]/*' />
        public static PrivateKey FromSeedED25519(byte[] seed)
        {
            return PrivateKeyED25519.FromSeed(seed);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromSeedECDSAsecp256k1(System.Byte[])"]/*' />
        public static PrivateKey FromSeedECDSAsecp256k1(byte[] seed)
        {
            return PrivateKeyECDSA.FromSeed(seed);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromMnemonic(Mnemonic,System.String)"]/*' />
        public static PrivateKey FromMnemonic(Mnemonic mnemonic, string passphrase)
        {
            var seed = mnemonic.ToSeed(passphrase);
            PrivateKey derivedKey = FromSeedED25519(seed);

            // BIP-44 path with the Hedera Hbar coin-type (omitting key index)
            // we pre-derive most of the path as the mobile wallets don't expose more than the index
            // https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki
            // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
            foreach (int index in new int[]
            {
                44,
                3030,
                0,
                0
            }

            )
            {
                derivedKey = derivedKey.Derive(index);
            }

            return derivedKey;
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromMnemonic(Mnemonic)"]/*' />
        public static PrivateKey FromMnemonic(Mnemonic mnemonic)
        {
            return FromMnemonic(mnemonic, "");
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromString(System.String)"]/*' />
        public static PrivateKey FromString(string privateKey)
        {
            return FromBytes(Hex.Decode(privateKey.StartsWith("0x") ? privateKey.Substring(2) : privateKey));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromStringDER(System.String)"]/*' />
        public static PrivateKey FromStringDER(string privateKey)
        {
            return FromBytesDER(Hex.Decode(privateKey));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromStringED25519(System.String)"]/*' />
        public static PrivateKey FromStringED25519(string privateKey)
        {
            return FromBytesED25519(Hex.Decode(privateKey));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromStringECDSA(System.String)"]/*' />
        public static PrivateKey FromStringECDSA(string privateKey)
        {
            return FromBytesECDSA(Hex.Decode(privateKey));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromBytes(System.Byte[])"]/*' />
        public new static PrivateKey FromBytes(byte[] privateKey)
        {
            if ((privateKey.Length == Ed25519.SecretKeySize) || (privateKey.Length == Ed25519.SecretKeySize + Ed25519.PublicKeySize))
            {
                // If this is a 32 or 64 byte string, assume an Ed25519 private key
                return new PrivateKeyED25519(privateKey.CopyArray()[0 .. Ed25519.SecretKeySize], null);
            }

            // Assume a DER-encoded private key descriptor
            return FromBytesDER(privateKey);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromBytesED25519(System.Byte[])"]/*' />
        public static PrivateKey FromBytesED25519(byte[] privateKey)
        {
            return PrivateKeyED25519.FromBytesInternal(privateKey);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromBytesECDSA(System.Byte[])"]/*' />
        public static PrivateKey FromBytesECDSA(byte[] privateKey)
        {
            return PrivateKeyECDSA.FromBytesInternal(privateKey);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromBytesDER(System.Byte[])"]/*' />
        public static PrivateKey FromBytesDER(byte[] privateKey)
        {
            try
            {
                return FromPrivateKeyInfo(PrivateKeyInfo.GetInstance(privateKey));
            }
            catch (InvalidCastException)
            {
                return PrivateKeyECDSA.FromECPrivateKeyInternal(ECPrivateKeyStructure.GetInstance(privateKey));
            }
            catch (ArgumentException)
            {
                return PrivateKeyECDSA.FromECPrivateKeyInternal(ECPrivateKeyStructure.GetInstance(privateKey));
            }
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromPrivateKeyInfo(PrivateKeyInfo)"]/*' />
        private static PrivateKey FromPrivateKeyInfo(PrivateKeyInfo privateKeyInfo)
        {
            if (privateKeyInfo.PrivateKeyAlgorithm.Equals(new AlgorithmIdentifier(ID_ED25519)))
            {
                return PrivateKeyED25519.FromPrivateKeyInfoInternal(privateKeyInfo);
            }
            else
            {

                // assume ECDSA
                return PrivateKeyECDSA.FromPrivateKeyInfoInternal(privateKeyInfo);
            }
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ReadPem(PemReader)"]/*' />
        public static PrivateKey ReadPem(PemReader pemFile)
        {
            return ReadPem(pemFile, null);
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ReadPem(PemReader,System.String)"]/*' />
        public static PrivateKey ReadPem(PemReader pemFile, string? password)
        {
            return FromPrivateKeyInfo(Pem.ReadPrivateKey(pemFile.Reader, password));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromPem(System.String)"]/*' />
        public static PrivateKey FromPem(string pemEncoded)
        {
            return ReadPem(new PemReader(new StringReader(pemEncoded)));
        }
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.FromPem(System.String,System.String)"]/*' />
        public static PrivateKey FromPem(string encodedPem, string password)
        {
			return ReadPem(new PemReader(new StringReader(encodedPem)), password);
		}

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.LegacyDerive(System.Int32)"]/*' />
        public virtual PrivateKey LegacyDerive(int index)
        {
            return LegacyDerive((long)index);
        }

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.LegacyDerive(System.Int64)"]/*' />
        public abstract PrivateKey LegacyDerive(long index);
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.IsDerivable"]/*' />
        public abstract bool IsDerivable();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.Derive(System.Int32)"]/*' />
        public abstract PrivateKey Derive(int index);
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.GetPublicKey"]/*' />
        public abstract PublicKey GetPublicKey();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.Sign(System.Byte[])"]/*' />
        public abstract byte[] Sign(byte[] message);
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.SignTransaction``1(Transaction{``0})"]/*' />
        public virtual byte[] SignTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
		{
            transaction.RequireOneNodeAccountId();

            if (!transaction.IsFrozen())
            {
                transaction.Freeze();
            }

            var builder = transaction.InnerSignedTransactions[0];
            var signature = Sign(builder.BodyBytes.ToByteArray());
            transaction.AddSignature(GetPublicKey(), signature);
            return signature;
        }

        public abstract override byte[] ToBytes();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ToBytesDER"]/*' />
        public abstract byte[] ToBytesDER();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ToBytesRaw"]/*' />
        public abstract byte[] ToBytesRaw();
        public override string ToString()
        {
            return ToStringDER();
        }

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ToStringDER"]/*' />
        public virtual string ToStringDER()
        {
            return Hex.ToHexString(ToBytesDER());
        }

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ToStringRaw"]/*' />
        public virtual string ToStringRaw()
        {
            return Hex.ToHexString(ToBytesRaw());
        }

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.ToAccountId(System.Int64,System.Int64)"]/*' />
        public virtual AccountId ToAccountId(long shard, long realm)
        {
            return GetPublicKey().ToAccountId(shard, realm);
        }

        public override Proto.Services.Key ToProtobufKey()
        {
            // Forward to the corresponding public key.
            return GetPublicKey().ToProtobufKey();
        }

        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.IsED25519"]/*' />
        public abstract bool IsED25519();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.IsECDSA"]/*' />
        public abstract bool IsECDSA();
        /// <include file="PrivateKey.cs.xml" path='docs/member[@name="M:PrivateKey.GetChainCode"]/*' />
        public abstract KeyParameter GetChainCode();
    }
}
