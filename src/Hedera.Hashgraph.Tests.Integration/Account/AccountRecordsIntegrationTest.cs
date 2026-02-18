// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class AccountRecordsIntegrationTest
    {
        public virtual void CanQueryAccountRecords()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var key = PrivateKey.GenerateED25519();
                var response = new AccountCreateTransaction
                {
					InitialBalance = new Hbar(1)

				}.SetKeyWithoutAlias(key).Execute(testEnv.Client);
                var accountId = response.GetReceipt(testEnv.Client).AccountId;
                new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, new Hbar(1).Negated()).AddHbarTransfer(accountId, new Hbar(1)).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                new TransferTransaction().AddHbarTransfer(testEnv.OperatorId, new Hbar(1)).AddHbarTransfer(accountId, new Hbar(1).Negated()).FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                var records = new AccountRecordsQuery
                {
					QueryPayment = new Hbar(10),
					AccountId = testEnv.OperatorId,

				}.Execute(testEnv.Client);

                Assert.False(records.Count == 0);
            }
        }
    }
}