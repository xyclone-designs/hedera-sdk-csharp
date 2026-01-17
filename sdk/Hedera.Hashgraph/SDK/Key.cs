using Org.BouncyCastle.Asn1;


using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace Hedera.Hashgraph.SDK
{
	/**
    * A common base for the signing authority or key that entities in Hedera may have.
    *
    * See <a href="https://docs.hedera.com/guides/docs/hedera-api/basic-types/key">Hedera Documentation</a>
    * @see KeyList
    * @see PublicKey
    */
    public abstract class Key 
    {
		//public static readonly ASN1ObjectIdentifier ID_ED25519 = new ASN1ObjectIdentifier("1.3.101.112");
		public static readonly DerObjectIdentifier ID_ED25519 = new ("1.3.101.112");
        public static readonly DerObjectIdentifier ID_ECDSA_SECP256K1 = new ASN1ObjectIdentifier("1.3.132.0.10");
        public static readonly DerObjectIdentifier ID_EC_PUBLIC_KEY = new ASN1ObjectIdentifier("1.2.840.10045.2.1"); 
        public static readonly X9ECParameters ECDSA_SECP256K1_CURVE = SecNamedCurves.GetByName("secp256k1");
        public static readonly ECDomainParameters ECDSA_SECP256K1_DOMAIN = new (
                ECDSA_SECP256K1_CURVE.Curve,
                ECDSA_SECP256K1_CURVE.G,
                ECDSA_SECP256K1_CURVE.N,
                ECDSA_SECP256K1_CURVE.H);

        /**
         * Create a specific key type from the protobuf.
         *
         * @param key                       the protobuf key of unknown type
         * @return                          the differentiated key
         */
        public static Key FromProtobufKey(Proto.Key key)
        {
            return key.KeyCase switch
            {
				Proto.Key.KeyOneofCase.Ed25519 => PublicKeyED25519.FromBytes(key.Ed25519.ToByteArray()),
				Proto.Key.KeyOneofCase.ECDSASecp256K1 => ,
				Proto.Key.KeyOneofCase.KeyList => KeyList.FromProtobuf(key.KeyList, null),
				Proto.Key.KeyOneofCase.ThresholdKey => KeyList.FromProtobuf(key.ThresholdKey.Keys, key.ThresholdKey.Threshold),
				Proto.Key.KeyOneofCase.ContractID => ContractId.FromProtobuf(key.ContractID),
				Proto.Key.KeyOneofCase.DelegatableContractId => DelegateContractId.FromProtobuf(key.DelegatableContractId),
				Proto.Key.KeyOneofCase.None => ,

				_ => throw new InvalidOperationException("Key#FromProtobuf: unhandled key case: " + key.KeyCase)
            };


			switch (key.KeyCase) {
                case Proto.Key.KeyOneofCase.Ed25519 -> {
                    return PublicKeyED25519.FromBytes(key.Ed25519.ToByteArray());
                }
                case ECDSA_SECP256K1 -> {
                    if (key.getECDSASecp256K1().size() == 20) {
                        return new EvmAddress(key.getECDSASecp256K1().ToByteArray());
                    } else {
                        return PublicKeyECDSA.FromBytesInternal(
                                key.getECDSASecp256K1().ToByteArray());
                    }
                }
                case KEYLIST -> {
                    return ;
                }
                case THRESHOLDKEY -> {
                    return ;
                }
                case CONTRACTID -> {
                    return 
                }
                case DELEGATABLE_CONTRACT_ID -> {
                    return ;
                }
                case KEY_NOT_SET -> {
                    return null;
                }
                default -> throw new IllegalStateException("Key#FromProtobuf: unhandled key case: " + key.getKeyCase());
            }
        }

        /**
         * Serialize this key as a protobuf object
         */
        public abstract Proto.Key ToProtobufKey();

        /**
         * Create the byte array.
         *
         * @return                          the byte array representation
         */
        public byte[] ToBytes() 
        {
            return ToProtobufKey().ToByteArray();
        }

        /**
         * Create Key from proto.Key byte array
         *
         * @param bytes
         * @return Key representation
         * @
         */
        public static Key FromBytes(byte[] bytes) 
        {
            return FromProtobufKey(Proto.Key.Parser.ParseFrom(bytes));
        }
    }

}