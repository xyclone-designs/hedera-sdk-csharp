// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Account
{
    /// <summary>
    /// This class represents the account balance object
    /// </summary>
    public class AccountBalance
    {
        /// <summary>
        /// The Hbar balance of the account
        /// </summary>
        public readonly Hbar Hbars;
        /// <summary>
        /// </summary>
        /// <remarks>@deprecated- Use `tokens` instead</remarks>
        public readonly Dictionary<TokenId, ulong> token = [];
        public readonly Dictionary<TokenId, ulong> tokens;
        public readonly Dictionary<TokenId, uint> tokenDecimals;
        AccountBalance(Hbar hbars, Dictionary<TokenId, ulong> token, Dictionary<TokenId, uint> @decimal)
        {
            Hbars = hbars;
            tokens = token;
            tokenDecimals = @decimal;
        }

		/// <summary>
		/// Convert a byte array to an account balance object.
		/// </summary>
		/// <param name="data">the byte array</param>
		/// <returns>                         the converted account balance object</returns>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		public static AccountBalance FromBytes(byte[] data)
		{
			return FromProtobuf(Proto.CryptoGetAccountBalanceResponse.Parser.ParseFrom(data));
		}
		/// <summary>
		/// Convert the protobuf object to an account balance object.
		/// </summary>
		/// <param name="protobuf">protobuf response object</param>
		/// <returns>                         the converted account balance object</returns>
		public static AccountBalance FromProtobuf(Proto.CryptoGetAccountBalanceResponse protobuf)
        {
            var balanceList = protobuf.TokenBalances;

            Dictionary<TokenId, ulong> map = [];
            Dictionary<TokenId, uint> decimalMap = [];
            for (int i = 0; i < protobuf.TokenBalances.Count; i++)
            {
                map.Add(TokenId.FromProtobuf(balanceList[i].TokenId), balanceList[i].Balance);
                decimalMap.Add(TokenId.FromProtobuf(balanceList[i].TokenId), balanceList[i].Decimals);
            }

            return new AccountBalance(Hbar.FromTinybars(protobuf.Balance), map, decimalMap);
        }

		/// <summary>
		/// Convert the account balance object to a byte array.
		/// </summary>
		/// <returns>                         the converted account balance object</returns>
		public virtual ByteString ToBytes()
		{
			return ToProtobuf().ToByteString();
		}
		/// <summary>
		/// Convert an account balance object into a protobuf.
		/// </summary>
		/// <returns>                         the protobuf object</returns>
		public virtual Proto.CryptoGetAccountBalanceResponse ToProtobuf()
        {
            var protobuf = new Proto.CryptoGetAccountBalanceResponse
            {
                Balance = (ulong)Hbars.ToTinybars() 
            };
            foreach (var entry in tokens)
            {
                protobuf.TokenBalances.Add(new Proto.TokenBalance
                {
                    TokenId = entry.Key.ToProtobuf(),
                    Balance = entry.Value,
                    Decimals = tokenDecimals[entry.Key],
                });
            }

            return protobuf;
        }
    }
}