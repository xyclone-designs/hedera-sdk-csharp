// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Token;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Cryptocurrency
{
    /// <include file="AccountBalance.cs.xml" path='docs/member[@name="T:AccountBalance"]/*' />
    public class AccountBalance
    {
        AccountBalance(Hbar hbars, Dictionary<TokenId, ulong> token, Dictionary<TokenId, uint> @decimal)
        {
            Hbars = hbars;
            Tokens = token;
            TokenDecimals = @decimal;
        }

		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="M:AccountBalance.FromBytes(System.Byte[])"]/*' />
		public static AccountBalance FromBytes(byte[] data)
		{
			return FromProtobuf(Proto.Services.CryptoGetAccountBalanceResponse.Parser.ParseFrom(data));
		}
		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="M:AccountBalance.FromProtobuf(Proto.Services.CryptoGetAccountBalanceResponse)"]/*' />
		public static AccountBalance FromProtobuf(Proto.Services.CryptoGetAccountBalanceResponse protobuf)
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

		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="P:AccountBalance.Hbars"]/*' />
		public Hbar Hbars { get; }
		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="M:AccountBalance.ToBytes"]/*' />
		public Dictionary<TokenId, ulong> Token { get; } = [];
		public Dictionary<TokenId, ulong> Tokens { get; }
		public Dictionary<TokenId, uint> TokenDecimals { get; }

		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="M:AccountBalance.ToBytes_2"]/*' />
		public virtual ByteString ToBytes()
		{
			return ToProtobuf().ToByteString();
		}
		/// <include file="AccountBalance.cs.xml" path='docs/member[@name="M:AccountBalance.ToProtobuf"]/*' />
		public virtual Proto.Services.CryptoGetAccountBalanceResponse ToProtobuf()
        {
            var protobuf = new Proto.Services.CryptoGetAccountBalanceResponse
            {
                Balance = (ulong)Hbars.ToTinybars() 
            };
            foreach (var entry in Tokens)
            {
                protobuf.TokenBalances.Add(new Proto.Services.TokenBalance
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
