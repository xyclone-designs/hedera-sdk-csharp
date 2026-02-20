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
        AccountBalance(Hbar hbars, Dictionary<TokenId, ulong> token, Dictionary<TokenId, uint> @decimal)
        {
            Hbars = hbars;
            Tokens = token;
            TokenDecimals = @decimal;
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
		/// The Hbar balance of the account
		/// </summary>
		public Hbar Hbars { get; }
		/// <summary>
		/// </summary>
		/// <remarks>@deprecated- Use `tokens` instead</remarks>
		public Dictionary<TokenId, ulong> Token { get; } = [];
		public Dictionary<TokenId, ulong> Tokens { get; }
		public Dictionary<TokenId, uint> TokenDecimals { get; }

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
            foreach (var entry in Tokens)
            {
                protobuf.TokenBalances.Add(new Proto.TokenBalance
                {
                    TokenId = entry.Key.ToProtobuf(),
                    Balance = entry.Value,
                    Decimals = TokenDecimals[entry.Key],
                });
            }

            return protobuf;
        }
    }
}