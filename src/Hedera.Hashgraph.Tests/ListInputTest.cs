// SPDX-License-Identifier: Apache-2.0
using Org.Junit.Jupiter.Api.Assertions;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    // A number of transactions take List<>s as inputs.
    // If the list parameter is used directly/naively, it can break encapsulation.
    // That is, if you call foo.setBar(bazList), later calling bazList.add(-1) will alter the list
    // that would be returned by foo.getBar().
    public class ListInputTest
    {
        virtual void TokenAssociateListTest()
        {
            var tx = new TokenAssociateTransaction();
            var list = new List<TokenId>();
            list.Add(TokenId.FromString("1.2.3"));
            tx.SetTokenIds(list);
            var v1 = new List(tx.GetTokenIds());
            list.Add(TokenId.FromString("4.5.6"));
            var v2 = new List(tx.GetTokenIds());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetTokenIds();
            list2.Add(TokenId.FromString("7.8.9"));
            var v3 = tx.GetTokenIds();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void NodeAccountIdsListTest()
        {
            var tx = new TokenAssociateTransaction();
            var list = new List<AccountId>();
            list.Add(AccountId.FromString("1.2.3"));
            tx.SetNodeAccountIds(list);
            var v1 = new List(Objects.RequireNonNull(tx.GetNodeAccountIds()));
            list.Add(AccountId.FromString("4.5.6"));
            var v2 = new List(tx.GetNodeAccountIds());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetNodeAccountIds();
            list2.Add(AccountId.FromString("7.8.9"));
            var v3 = tx.GetNodeAccountIds();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenBurnListTest()
        {
            var tx = new TokenBurnTransaction();
            var list = new List<long>();
            list.Add(0);
            tx.SetSerials(list);
            var v1 = new List(tx.GetSerials());
            list.Add(1);
            var v2 = new List(tx.GetSerials());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetSerials();
            list2.Add(2);
            var v3 = tx.GetSerials();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenWipeListTest()
        {
            var tx = new TokenWipeTransaction();
            var list = new List<long>();
            list.Add(0);
            tx.SetSerials(list);
            var v1 = new List(tx.GetSerials());
            list.Add(1);
            var v2 = new List(tx.GetSerials());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetSerials();
            list2.Add(2);
            var v3 = tx.GetSerials();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenMintListTest()
        {
            var tx = new TokenMintTransaction();
            var list = new List<byte[]>();
            list.Add(new byte[] { 0 });
            tx.SetMetadata(list);
            var v1 = new List(tx.GetMetadata());
            list.Add(new byte[] { 1 });
            var v2 = new List(tx.GetMetadata());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetMetadata();
            list2.Add(new byte[] { 2 });
            var v3 = tx.GetMetadata();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenDissociateListTest()
        {
            var tx = new TokenDissociateTransaction();
            var list = new List<TokenId>();
            list.Add(TokenId.FromString("1.2.3"));
            tx.SetTokenIds(list);
            var v1 = new List(tx.GetTokenIds());
            list.Add(TokenId.FromString("4.5.6"));
            var v2 = new List(tx.GetTokenIds());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetTokenIds();
            list2.Add(TokenId.FromString("7.8.9"));
            var v3 = tx.GetTokenIds();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenCreateListTest()
        {
            var tx = new TokenCreateTransaction();
            var list = new List<CustomFee>();
            list.Add(new CustomFixedFee().SetAmount(1));
            tx.SetCustomFees(list);
            var v1 = new List(Objects.RequireNonNull(tx.GetCustomFees()));
            list.Add(new CustomFixedFee().SetAmount(2));
            var v2 = new List(tx.GetCustomFees());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetCustomFees();
            list2.Add(new CustomFixedFee().SetAmount(3));
            var v3 = tx.GetCustomFees();
            AssertEquals(v1.ToString(), v3.ToString());
        }

        virtual void TokenFeeScheduleUpdateListTest()
        {
            var tx = new TokenFeeScheduleUpdateTransaction();
            var list = new List<CustomFee>();
            list.Add(new CustomFixedFee().SetAmount(1));
            tx.SetCustomFees(list);
            var v1 = new List(tx.GetCustomFees());
            list.Add(new CustomFixedFee().SetAmount(2));
            var v2 = new List(tx.GetCustomFees());
            AssertEquals(v1.ToString(), v2.ToString());
            var list2 = tx.GetCustomFees();
            list2.Add(new CustomFixedFee().SetAmount(3));
            var v3 = tx.GetCustomFees();
            AssertEquals(v1.ToString(), v3.ToString());
        }
    }
}