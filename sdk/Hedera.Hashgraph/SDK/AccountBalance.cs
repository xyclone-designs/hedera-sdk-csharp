using Google.Protobuf;
using Hedera.Hashgraph.Proto;
using System;
using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
	/**
 * This class represents the account balance object
 */
    public class AccountBalance 
    {
        /**
         * The Hbar balance of the account
         */
        public Hbar Hbars;

        /**
         * @deprecated - Use `tokens` instead
         */
        [Obsolete]
        public readonly Dictionary<TokenId, long> Token = [];
        public readonly Dictionary<TokenId, long> Tokens = [];
        public readonly Dictionary<TokenId, int> TokenDecimals = [];

        AccountBalance(Hbar hbars, Dictionary<TokenId, long> token, Dictionary<TokenId, int> decimals) {
            Hbars = hbars;
            Tokens = token;
            TokenDecimals = decimals;
        }

        /**
         * Convert the protobuf object to an account balance object.
         *
         * @param protobuf                  protobuf response object
         * @return                          the converted account balance object
         */
        public static AccountBalance FromProtobuf(CryptoGetAccountBalanceResponse protobuf) 
        {
            var balanceList = protobuf.TokenBalances;
            Dictionary<TokenId, long> map = new HashMap<>();
            Dictionary<TokenId, Integer> decimalMap = new HashMap<>();
            for (int i = 0; i < protobuf.getTokenBalancesCount(); i++) {
                map.put(
                        TokenId.FromProtobuf(balanceList.get(i).getTokenId()),
                        balanceList.get(i).getBalance());
                decimalMap.put(
                        TokenId.FromProtobuf(balanceList.get(i).getTokenId()),
                        balanceList.get(i).getDecimals());
            }

            return new AccountBalance(Hbar.FromTinybars(protobuf.getBalance()), map, decimalMap);
        }

        /**
         * Convert a byte array to an account balance object.
         *
         * @param data                      the byte array
         * @return                          the converted account balance object
         * @       when there is an issue with the protobuf
         */
        public static AccountBalance FromBytes(byte[] data)  {
            return FromProtobuf(CryptoGetAccountBalanceResponse.Parser.ParseFrom(data));
        }

        /**
         * Convert an account balance object into a protobuf.
         *
         * @return                          the protobuf object
         */
        public Proto.CryptoGetAccountBalanceResponse ToProtobuf() 
        {
            new CryptoGetAccountBalanceResponse
            {
                TokenBalances = new TokenBalance
            };


			var protobuf = CryptoGetAccountBalanceResponse.NewBuilder().setBalance(hbars.toTinybars());

            for (var entry : tokens.entrySet()) {
                protobuf.AddTokenBalances(TokenBalance.newBuilder()
                        .setTokenId(entry.getKey().ToProtobuf())
                        .setBalance(entry.getValue())
                        .setDecimals(Objects.requireNonNull(tokenDecimals.get(entry.getKey()))));
            }

            return protobuf.build();
        }

        /**
         * Convert the account balance object to a byte array.
         *
         * @return                          the converted account balance object
         */
        public ByteString ToBytes() 
        {
            return ToProtobuf().ToByteString();
        }
    }
}