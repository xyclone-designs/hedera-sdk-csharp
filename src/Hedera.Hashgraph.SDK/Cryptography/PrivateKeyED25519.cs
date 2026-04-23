// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Utils;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;

namespace Hedera.Hashgraph.SDK.Cryptography
{
    /// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="T:PrivateKeyED25519"]/*' />
    class PrivateKeyED25519 : PrivateKey
    {
        private readonly byte[] KeyData;
        private readonly KeyParameter? ChainCode;
        /// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.#ctor(System.Byte[],KeyParameter)"]/*' />
        internal PrivateKeyED25519(byte[] keyData, KeyParameter? chainCode)
        {
            KeyData = keyData;
            ChainCode = chainCode;
        }

		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.FromSeed(System.Byte[])"]/*' />
		public static PrivateKey FromSeed(byte[] seed)
		{
			var hmacSha512 = new HMac(new Sha512Digest());
			hmacSha512.Init(new KeyParameter(Encoding.UTF8.GetBytes("ed25519 seed")));
			hmacSha512.BlockUpdate(seed, 0, seed.Length);
			var derivedState = new byte[hmacSha512.GetMacSize()];
			hmacSha512.DoFinal(derivedState, 0);
			return PrivateKeyED25519.DerivableKeyED25519(derivedState);
		}
		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.FromBytesInternal(System.Byte[])"]/*' />
		public static PrivateKey FromBytesInternal(byte[] privateKey)
		{
			if ((privateKey.Length == Ed25519.SecretKeySize) || (privateKey.Length == Ed25519.SecretKeySize + Ed25519.PublicKeySize))
			{
				// If this is a 32 or 64 byte string, assume an Ed25519 private key
				return new PrivateKeyED25519(privateKey[0..Ed25519.SecretKeySize], null);
			}

			// Assume a DER-encoded private key descriptor
			return FromPrivateKeyInfoInternal(PrivateKeyInfo.GetInstance(privateKey));
		}
		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.FromPrivateKeyInfoInternal(PrivateKeyInfo)"]/*' />
		public static PrivateKeyED25519 FromPrivateKeyInfoInternal(PrivateKeyInfo privateKeyInfo)
        {
            try
            {
                var privateKey = (Asn1OctetString)privateKeyInfo.ParsePrivateKey();
                return new PrivateKeyED25519(privateKey.GetOctets(), null);
            }
            catch (IOException e)
            {
                throw new BadKeyException(e);
            }
        }

		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.GenerateInternal"]/*' />
		public static PrivateKeyED25519 GenerateInternal()
		{

			// extra 32 bytes for chain code
			byte[] data = new byte[Ed25519.SecretKeySize + 32];
			ThreadLocalSecureRandom.Current().NextBytes(data);
			return DerivableKeyED25519(data);
		}
		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.DerivableKeyED25519(System.Byte[])"]/*' />
		public static PrivateKeyED25519 DerivableKeyED25519(byte[] deriveData)
		{
			var keyData = deriveData[0..32];
			var chainCode = new KeyParameter(deriveData, 32, 32);
			return new PrivateKeyED25519(keyData, chainCode);
		}

		/// <include file="PrivateKeyED25519.cs.xml" path='docs/member[@name="M:PrivateKeyED25519.LegacyDeriveChildKey(System.Byte[],System.Int64)"]/*' />
		public static byte[] LegacyDeriveChildKey(byte[] entropy, long index)
        {
            byte[] seed = new byte[entropy.Length + 8];
			Array.Fill(seed, (byte)0, seed.Length, 0);
			if (index == 0xffffffffff)
            {
                seed[entropy.Length + 3] = (byte)0xff;
				Array.Fill(seed, (byte)(index >>> 32), entropy.Length + 4, seed.Length - (entropy.Length + 4));
			}
			else
            {
                if (index < 0) Array.Fill(seed, (byte)255, entropy.Length, 4);
				else Array.Fill(seed, (byte)0, entropy.Length, 4);

				int startIndex = entropy.Length + 4;
				Array.Fill(seed, (byte)index, startIndex, seed.Length - startIndex);
			}

			Array.Copy(entropy, 0, seed, 0, entropy.Length);
            byte[] salt = [0xFF];
			Pkcs5S2ParametersGenerator pbkdf2 = new (new Sha512Digest());
            pbkdf2.Init(seed, salt, 2048);
            KeyParameter key = (KeyParameter)pbkdf2.GenerateDerivedParameters("AES256", 256);
            
			return key.GetKey();
        }

		public override bool IsDerivable()
		{
			return ChainCode != null;
		}
		public override PrivateKey Derive(int index)
		{
			if (ChainCode == null)
			{
				throw new InvalidOperationException("this private key does not support derivation");
			}

			if (Bip32Utils.IsHardenedIndex(index))
			{
				throw new ArgumentException("the index should not be pre-hardened");
			}


			// SLIP-10 child key derivation
			// https://github.com/satoshilabs/slips/blob/master/slip-0010.md#master-key-generation
			var hmacSha512 = new HMac(new Sha512Digest());
			hmacSha512.Init(ChainCode);
			hmacSha512.Update((byte)0);
			hmacSha512.BlockUpdate(KeyData, 0, Ed25519.SecretKeySize);

			// write the index in big-endian order, setting the 31st bit to mark it "hardened"
			var indexBytes = new byte[4];
			BinaryPrimitives.WriteInt32BigEndian(indexBytes, index);
			indexBytes[0] |= (byte)0b10000000;
			hmacSha512.BlockUpdate(indexBytes, 0, indexBytes.Length);
			var output = new byte[64];
			hmacSha512.DoFinal(output, 0);
			return DerivableKeyED25519(output);
		}
		public override PrivateKey LegacyDerive(long index)
        {
            var keyBytes = LegacyDeriveChildKey(KeyData, index);
            return FromBytesInternal(keyBytes);
        }
        public override PublicKey GetPublicKey()
        {
            if (publicKey != null)
            {
                return publicKey;
            }

            byte[] publicKeyData = new byte[Ed25519.PublicKeySize];
            Ed25519.GeneratePublicKey(KeyData, 0, publicKeyData, 0);
            publicKey = PublicKeyED25519.FromBytesInternal(publicKeyData);
            return publicKey;
        }
        public override KeyParameter GetChainCode()
        {
            return ChainCode;
        }

		public override bool IsECDSA()
		{
			return false;
		}
		public override bool IsED25519()
		{
			return true;
		}

		public override byte[] Sign(byte[] message)
        {
            byte[] signature = new byte[Ed25519.SignatureSize];
            Ed25519.Sign(KeyData, 0, message, 0, message.Length, signature, 0);
            return signature;
        }

        public override byte[] ToBytes()
        {
            return ToBytesRaw();
        }
        public override byte[] ToBytesRaw()
        {
            return KeyData;
        }
        public override byte[] ToBytesDER()
        {
            try
            {
                return new PrivateKeyInfo(new AlgorithmIdentifier(ID_ED25519), new DerOctetString(KeyData)).GetEncoded("DER");
            }
            catch (IOException e)
            {
                throw new Exception(string.Empty, e);
            }
        }
    }
}