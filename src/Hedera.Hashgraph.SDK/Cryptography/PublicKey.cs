// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK.Cryptography
{
    /// <include file="PublicKey.cs.xml" path='docs/member[@name="T:PublicKey"]/*' />
    public abstract class PublicKey : Key
    {
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromBytes(System.Byte[])"]/*' />
        public new static PublicKey FromBytes(byte[] publicKey)
        {
            if (publicKey.Length == Ed25519.PublicKeySize)
            {
                // If this is a 32 byte string, assume an Ed25519 public key
                return PublicKeyED25519.FromBytesInternal(publicKey);
            }
            else if (publicKey.Length == 33)
            {
                // compressed 33 byte raw form
                return PublicKeyECDSA.FromBytesInternal(publicKey);
            }
            else if (publicKey.Length == 65)
            {
                // compress the 65 byte form
                return PublicKeyECDSA.FromBytesInternal(ECDSA_SECP256K1_CURVE.Curve.DecodePoint(publicKey).GetEncoded(true));
            }


            // Assume a DER-encoded private key descriptor
            return FromBytesDER(publicKey);
        }

        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromBytesDER(System.Byte[])"]/*' />
        public static PublicKey FromBytesDER(byte[] publicKey)
        {
            return PublicKey.FromSubjectKeyInfo(SubjectPublicKeyInfo.GetInstance(publicKey));
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromBytesED25519(System.Byte[])"]/*' />
        public static PublicKey FromBytesED25519(byte[] publicKey)
        {
            return PublicKeyED25519.FromBytesInternal(publicKey);
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromBytesECDSA(System.Byte[])"]/*' />
        public static PublicKey FromBytesECDSA(byte[] publicKey)
        {
            return PublicKeyECDSA.FromBytesInternal(publicKey);
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromString(System.String)"]/*' />
        public static PublicKey FromString(string publicKey)
        {
            return PublicKey.FromBytes(Hex.Decode(publicKey));
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromStringED25519(System.String)"]/*' />
        public static PublicKey FromStringED25519(string publicKey)
        {
            return FromBytesED25519(Hex.Decode(publicKey));
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromStringECDSA(System.String)"]/*' />
        public static PublicKey FromStringECDSA(string publicKey)
        {
            return FromBytesECDSA(Hex.Decode(publicKey));
        }
        /// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromStringDER(System.String)"]/*' />
        public static PublicKey FromStringDER(string publicKey)
        {
            return FromBytesDER(Hex.Decode(publicKey));
        }
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromAliasBytes(ByteString)"]/*' />
		public static PublicKey? FromAliasBytes(ByteString aliasBytes)
		{
			if (aliasBytes.Length != 0)
			{
				try
				{
					return Key.FromProtobufKey(Proto.Services.Key.Parser.ParseFrom(aliasBytes)) as PublicKey;
				}
				catch (InvalidProtocolBufferException) { }
			}

			return null;
		}
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.UnusableKey"]/*' />
		public static PublicKey UnusableKey()
		{
			return PublicKey.FromStringED25519("0000000000000000000000000000000000000000000000000000000000000000");
		}

		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.FromSubjectKeyInfo(SubjectPublicKeyInfo)"]/*' />
		private static PublicKey FromSubjectKeyInfo(SubjectPublicKeyInfo subjectPublicKeyInfo)
        {
            if (subjectPublicKeyInfo.Algorithm.Equals(new AlgorithmIdentifier(ID_ED25519)))
            {
                return PublicKeyED25519.FromSubjectKeyInfoInternal(subjectPublicKeyInfo);
            }
            else
            {

                // assume ECDSA
                return PublicKeyECDSA.FromSubjectKeyInfoInternal(subjectPublicKeyInfo);
            }
        }

		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.IsECDSA"]/*' />
		public abstract bool IsECDSA();
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.IsED25519"]/*' />
		public abstract bool IsED25519();
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToBytesDER"]/*' />
		public abstract byte[] ToBytesDER();
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToBytesRaw"]/*' />
		public abstract byte[] ToBytesRaw();
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToEvmAddress"]/*' />
		public abstract EvmAddress ToEvmAddress();
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.Verify(System.Byte[],System.Byte[])"]/*' />
		public abstract bool Verify(byte[] message, byte[] signature);
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToSignaturePairProtobuf(System.Byte[])"]/*' />
		public abstract Proto.Services.SignaturePair ToSignaturePairProtobuf(byte[] signature);
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ExtractSignatureFromProtobuf(Proto.Services.SignaturePair)"]/*' />
		public abstract ByteString ExtractSignatureFromProtobuf(Proto.Services.SignaturePair pair);

		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToStringDER"]/*' />
		public virtual string ToStringDER()
		{
			return Hex.ToHexString(ToBytesDER());
		}
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToStringRaw"]/*' />
		public virtual string ToStringRaw()
		{
			return Hex.ToHexString(ToBytesRaw());
		}
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.ToAccountId(System.Int64,System.Int64)"]/*' />
		public virtual AccountId ToAccountId(long shard, long realm)
		{
			return new AccountId(shard, realm, 0, null, this, null);
		}
		/// <include file="PublicKey.cs.xml" path='docs/member[@name="M:PublicKey.VerifyTransaction``1(Transaction{``0})"]/*' />
		public virtual bool VerifyTransaction<T>(Transaction<T> transaction) where T : Transaction<T>
		{
            if (!transaction.IsFrozen())
				transaction.Freeze();

			foreach (var publicKey in transaction.PublicKeys)
            {
                if (publicKey.Equals(this))
                {
                    return true;
                }
            }

            foreach (var signedTransaction in transaction.InnerSignedTransactions)
            {
                var found = false;
                foreach (var sigPair in signedTransaction.SigMap.SigPair)
                {
                    if (sigPair.PubKeyPrefix.Equals(ByteString.CopyFrom(ToBytesRaw())))
                    {
                        found = true;
                        if (!Verify(signedTransaction.BodyBytes.ToByteArray(), ExtractSignatureFromProtobuf(sigPair).ToByteArray()))
                        {
                            return false;
                        }
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

		public override string ToString()
        {
            return ToStringDER();
        }        
    }
}
