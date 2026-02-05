// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class DuplicateTransactionTest
    {
        virtual void GenerateTransactionIds()
        {
            TransactionId[] ids = new TransactionId[1000000];
            AccountId accountId = AccountId.FromString("0.0.1000");
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = TransactionId.Generate(accountId);
            }

            HashSet<TransactionId> set = new HashSet(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                AssertThat(set.Add(ids[i])).As("ids[%d] is not unique", i).IsTrue();
            }
        }
    }
}