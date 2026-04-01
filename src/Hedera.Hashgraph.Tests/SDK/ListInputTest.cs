// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Fees;

namespace Hedera.Hashgraph.Tests.SDK
{
    // A number of transactions take List<>s as inputs.
    // If the list parameter is used directly/naively, it can break encapsulation.
    // That is, if you call foo.setBar(bazList), later calling bazList.add(-1) will alter the list
    // that would be returned by foo.getBar().
    public class ListInputTest
    {
        public virtual void TokenAssociateListTest()
        {
            var tx = new TokenAssociateTransaction();
            
            var list = new List<TokenId>();
            list.Add(TokenId.FromString("1.2.3"));
            tx.TokenIds = list;
            
            var v1 = new List<TokenId>(tx.TokenIds);
            list.Add(TokenId.FromString("4.5.6"));
            
            var v2 = new List<TokenId>(tx.TokenIds);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.TokenIds;
            list2.Add(TokenId.FromString("7.8.9"));
            
            var v3 = tx.TokenIds;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void NodeAccountIdsListTest()
        {
            var tx = new TokenAssociateTransaction();
            
            var list = new List<AccountId>();
            list.Add(AccountId.FromString("1.2.3"));
            tx.NodeAccountIds = [.. list];
            
            var v1 = new List<AccountId>(tx.NodeAccountIds.Read);
            list.Add(AccountId.FromString("4.5.6"));
            
            var v2 = new List<AccountId>(tx.NodeAccountIds.Read);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.NodeAccountIds;
            list2.Add(AccountId.FromString("7.8.9"));
            
            var v3 = tx.NodeAccountIds;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenBurnListTest()
        {
            var tx = new TokenBurnTransaction
			{
				Serials = [0]
			};

            var list = new List<int>();
            var v1 = new List<long>(tx.Serials);
            list.Add(1);
            
            var v2 = new List<long>(tx.Serials);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.Serials;
            list2.Add(2);
            
            var v3 = tx.Serials;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenWipeListTest()
        {
            var tx = new TokenWipeTransaction
            {
                Serials = [0]
            };

            var list = new List<int>();
            var v1 = new List<long>(tx.Serials);
            list.Add(1);
            
            var v2 = new List<long>(tx.Serials);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.Serials;
            list2.Add(2);
            
            var v3 = tx.Serials;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenMintListTest()
        {
            var tx = new TokenMintTransaction();
            
            var list = new List<byte[]>();
            list.Add(new byte[] { 0 });
            tx.Metadata = list;
            
            List<byte[]> v1 = [ .. tx.Metadata ];
            list.Add(new byte[] { 1 });
            
            List<byte[]> v2 = [ .. tx.Metadata ];
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.Metadata;
            list2.Add(new byte[] { 2 });
            
            var v3 = tx.Metadata;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenDissociateListTest()
        {
            var tx = new TokenDissociateTransaction();
            
            var list = new List<TokenId>();
            list.Add(TokenId.FromString("1.2.3"));
            tx.TokenIds = list;
            
            var v1 = new List<TokenId>(tx.TokenIds);
            list.Add(TokenId.FromString("4.5.6"));
            
            var v2 = new List<TokenId>(tx.TokenIds);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.TokenIds;
            list2.Add(TokenId.FromString("7.8.9"));
            
            var v3 = tx.TokenIds;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenCreateListTest()
        {
            var tx = new TokenCreateTransaction();
            var list = new List<CustomFee>
            {
                new CustomFixedFee { Amount = 1 }
            };
            tx.CustomFees = list;
            
            var v1 = new List<CustomFee>(tx.CustomFees);
            list.Add(new CustomFixedFee { Amount = 2 });
            
            var v2 = new List<CustomFee>(tx.CustomFees);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.CustomFees;
            list2.Add(new CustomFixedFee { Amount = 3 });
            
            var v3 = tx.CustomFees;
            Assert.Equal(v1.ToString(), v3.ToString());
        }

        public virtual void TokenFeeScheduleUpdateListTest()
        {
            var tx = new TokenFeeScheduleUpdateTransaction();
            var list = new List<CustomFee>
            {
                new CustomFixedFee { Amount = 1 }
            };
            tx.CustomFees = list;
            
            var v1 = new List<CustomFee>(tx.CustomFees);
            list.Add(new CustomFixedFee { Amount = 2 });
            
            var v2 = new List<CustomFee>(tx.CustomFees);
            Assert.Equal(v1.ToString(), v2.ToString());
            
            var list2 = tx.CustomFees;
            list2.Add(new CustomFixedFee { Amount = 3 });
            
            var v3 = tx.CustomFees;
            Assert.Equal(v1.ToString(), v3.ToString());
        }
    }
}