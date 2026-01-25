// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Util;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Transactions.Account
{
    /// <summary>
    /// This class represents the account balance object
    /// </summary>
    public class AccountBalance
    {
        /// <summary>
        /// The Hbar balance of the account
        /// </summary>
        public readonly Hbar hbars;
        /// <summary>
        /// </summary>
        /// <remarks>@deprecated- Use `tokens` instead</remarks>
        public readonly Dictionary<TokenId, long> token = [];
        public readonly Dictionary<TokenId, long> tokens;
        public readonly Dictionary<TokenId, int> tokenDecimals;
        AccountBalance(Hbar hbars, Dictionary<TokenId, long> token, Dictionary<TokenId, int> @decimal)
        {
            hbars = hbars;
            tokens = token;
            tokenDecimals = @decimal;
        }

        /// <summary>
        /// Convert the protobuf object to an account balance object.
        /// </summary>
        /// <param name="protobuf">protobuf response object</param>
        /// <returns>                         the converted account balance object</returns>
        static AccountBalance FromProtobuf(CryptoGetAccountBalanceResponse protobuf)
        {
            var balanceList = protobuf.GetTokenBalancesList();
            Dictionary<TokenId, long> map = [];
            Dictionary<TokenId, int> decimalMap = [];
            for (int i = 0; i < protobuf.GetTokenBalancesCount(); i++)
            {
                map.Put(TokenId.FromProtobuf(balanceList[i].GetTokenId()), balanceList[i].GetBalance());
                decimalMap.Put(TokenId.FromProtobuf(balanceList[i].GetTokenId()), balanceList[i].GetDecimals());
            }

            return new AccountBalance(Hbar.FromTinybars(protobuf.GetBalance()), map, decimalMap);
        }

        /// <summary>
        /// Convert a byte array to an account balance object.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <returns>                         the converted account balance object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static AccountBalance FromBytes(byte[] data)
        {
            return FromProtobuf(CryptoGetAccountBalanceResponse.ParseFrom(data));
        }

        /// <summary>
        /// Convert an account balance object into a protobuf.
        /// </summary>
        /// <returns>                         the protobuf object</returns>
        virtual CryptoGetAccountBalanceResponse ToProtobuf()
        {
            var protobuf = CryptoGetAccountBalanceResponse.NewBuilder().SetBalance(hbars.ToTinybars());
            foreach (var entry in tokens.EntrySet())
            {
                protobuf.AddTokenBalances(TokenBalance.NewBuilder().SetTokenId(entry.GetKey().ToProtobuf()).SetBalance(entry.GetValue()).SetDecimals(Objects.RequireNonNull(tokenDecimals[entry.GetKey()])));
            }

            return protobuf.Build();
        }

        /// <summary>
        /// Convert the account balance object to a byte array.
        /// </summary>
        /// <returns>                         the converted account balance object</returns>
        public virtual ByteString ToBytes()
        {
            return ToProtobuf().ToByteString();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("hbars", hbars).Add("tokens", tokens).ToString();
        }
    }
}