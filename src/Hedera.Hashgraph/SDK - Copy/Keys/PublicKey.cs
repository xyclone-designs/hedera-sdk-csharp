// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Transactions.Account;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Utilities.Encoders;

using System;

namespace Hedera.Hashgraph.SDK.Keys
{
    /// <summary>
    /// A public key on the Hedera™ network.
    /// </summary>
    public abstract class PublicKey : Key
    {
        /// <summary>
        /// Create a public key from a byte array.
        /// </summary>
        /// <param name="publicKey">the byte array</param>
        /// <returns>                         the new public key</returns>
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

        /// <summary>
        /// Create a public key from a DER encoded byte array.
        /// </summary>
        /// <param name="publicKey">the DER encoded byte array</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromBytesDER(byte[] publicKey)
        {
            return PublicKey.FromSubjectKeyInfo(SubjectPublicKeyInfo.GetInstance(publicKey));
        }

        /// <summary>
        /// Create a public key from a byte array.
        /// </summary>
        /// <param name="publicKey">the byte array</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromBytesED25519(byte[] publicKey)
        {
            return PublicKeyED25519.FromBytesInternal(publicKey);
        }

        /// <summary>
        /// Create a public key from a byte array.
        /// </summary>
        /// <param name="publicKey">the byte array</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromBytesECDSA(byte[] publicKey)
        {
            return PublicKeyECDSA.FromBytesInternal(publicKey);
        }

        /// <summary>
        /// Create a public key from a string.
        /// </summary>
        /// <param name="publicKey">the string</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromString(string publicKey)
        {
            return PublicKey.FromBytes(Hex.Decode(publicKey));
        }

        /// <summary>
        /// Create a public key from a string.
        /// </summary>
        /// <param name="publicKey">the string</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromStringED25519(string publicKey)
        {
            return FromBytesED25519(Hex.Decode(publicKey));
        }

        /// <summary>
        /// Create a public key from a string.
        /// </summary>
        /// <param name="publicKey">the string</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromStringECDSA(string publicKey)
        {
            return FromBytesECDSA(Hex.Decode(publicKey));
        }

        /// <summary>
        /// Create a public key from a string.
        /// </summary>
        /// <param name="publicKey">the string</param>
        /// <returns>                         the new key</returns>
        public static PublicKey FromStringDER(string publicKey)
        {
            return FromBytesDER(Hex.Decode(publicKey));
        }

        /// <summary>
        /// Create a public key from a subject public key info object.
        /// </summary>
        /// <param name="subjectPublicKeyInfo">the subject public key info object</param>
        /// <returns>                         the new key</returns>
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

        /// <summary>
        /// The public key from an immutable byte string.
        /// </summary>
        /// <param name="aliasBytes">the immutable byte string</param>
        /// <returns>                         the key</returns>
        static PublicKey? FromAliasBytes(ByteString aliasBytes)
        {
            if (!aliasBytes.IsEmpty)
            {
                try
                {
                    return Key.FromProtobufKey(Proto.Key.Parser.ParseFrom(aliasBytes)) as PublicKey;
                }
                catch (InvalidProtocolBufferException) { }
            }

            return null;
        }

        /// <summary>
        /// Verify a signature on a message with this public key.
        /// </summary>
        /// <param name="message">The array of bytes representing the message</param>
        /// <param name="signature">The array of bytes representing the signature</param>
        /// <returns>bool</returns>
        public abstract bool Verify(byte[] message, byte[] signature);
        /// <summary>
        /// Get the signature from a signature pair protobuf.
        /// </summary>
        /// <param name="pair">the protobuf</param>
        /// <returns>                         the signature</returns>
        abstract ByteString ExtractSignatureFromProtobuf(Proto.SignaturePair pair);
        /// <summary>
        /// Is the given transaction valid?
        /// </summary>
        /// <param name="transaction">the transaction</param>
        /// <returns>                         is it valid</returns>
        public virtual bool VerifyTransaction<TWildcardTodo>(Transaction<TWildcardTodo> transaction) where TWildcardTodo : Transaction<TWildcardTodo>
		{
            if (!transaction.IsFrozen())
            {
                transaction.Freeze();
            }

            foreach (var publicKey in transaction.publicKeys)
            {
                if (publicKey.Equals(this))
                {
                    return true;
                }
            }

            foreach (var signedTransaction in transaction.innerSignedTransactions)
            {
                var found = false;
                foreach (var sigPair in signedTransaction.SigMap().SigPairList())
                {
                    if (sigPair.PubKeyPrefix().Equals(ByteString.CopyFrom(ToBytesRaw())))
                    {
                        found = true;
                        if (!Verify(signedTransaction.BodyBytes().ToByteArray(), ExtractSignatureFromProtobuf(sigPair).ToByteArray()))
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

        /// <summary>
        /// Serialize this key as a Proto.SignaturePair protobuf object
        /// </summary>
        abstract Proto.SignaturePair ToSignaturePairProtobuf(byte[] signature);
        public abstract override byte[] ToBytes();
        /// <summary>
        /// Extract the DER represented as a byte array.
        /// </summary>
        /// <returns>                         the DER represented as a byte array</returns>
        public abstract byte[] ToBytesDER();
        /// <summary>
        /// Extract the raw byte representation.
        /// </summary>
        /// <returns>                         the raw byte representation</returns>
        public abstract byte[] ToBytesRaw();
        public override string ToString()
        {
            return ToStringDER();
        }

        /// <summary>
        /// Extract the DER encoded string.
        /// </summary>
        /// <returns>                         the DER encoded string</returns>
        public virtual string ToStringDER()
        {
            return Hex.ToHexString(ToBytesDER());
        }

        /// <summary>
        /// Extract the raw string.
        /// </summary>
        /// <returns>                         the raw string</returns>
        public virtual string ToStringRaw()
        {
            return Hex.ToHexString(ToBytesRaw());
        }

        /// <summary>
        /// Create a new account id.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <returns>                         the new account id</returns>
        public virtual AccountId ToAccountId(long shard, long realm)
        {
            return new AccountId(shard, realm, 0, null, this, null);
        }

        /// <summary>
        /// Is this an ED25519 key?
        /// </summary>
        /// <returns>                         is this an ED25519 key</returns>
        public abstract bool IsED25519();
        /// <summary>
        /// Is this an ECDSA key?
        /// </summary>
        /// <returns>                         is this an ECDSA key</returns>
        public abstract bool IsECDSA();
        /// <summary>
        /// Converts the key to EVM address
        /// </summary>
        /// <returns>                         the EVM address</returns>
        public abstract EvmAddress ToEvmAddress();
        /// <summary>
        /// Returns an "unusable" public key.
        /// “Unusable” refers to a key such as an Ed25519 0x00000... public key,
        /// since it is (presumably) impossible to find the 32-byte string whose SHA-512 hash begins with 32 bytes of zeros.
        /// </summary>
        /// <returns>The "unusable" key</returns>
        public static PublicKey UnusableKey()
        {
            return PublicKey.FromStringED25519("0000000000000000000000000000000000000000000000000000000000000000");
        }
    }
}