// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Queries;
using Hedera.Hashgraph.SDK;

using System.Threading;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class AccountIdPopulationIntegrationTest
    {
        [Fact]
        public virtual void CanPopulateAccountIdNumSync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction()
                    .AddHbarTransfer(evmAddressAccount, new Hbar(1))
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(-1))
                    .Execute(testEnv.Client);

                var receipt = new TransactionReceiptQuery
                { 
                    TransactionId = tx.TransactionId,
                    IncludeChildren = true

                }.Execute(testEnv.Client);

				var newAccountId = receipt.Children[0].AccountId;
				var idMirror = AccountId.FromEvmAddress(evmAddress, 0, 0);
                
                Thread.Sleep(5000);

                var accountId = idMirror.PopulateAccountNum(testEnv.Client);
                Assert.Equal(newAccountId.Num, accountId.Num);
            }
        }
        [Fact]
        public virtual void CanPopulateAccountIdNumAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction()
                    .AddHbarTransfer(evmAddressAccount, new Hbar(1))
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(-1))
                    .Execute(testEnv.Client);

                var receipt = new TransactionReceiptQuery
                { 
                    TransactionId = tx.TransactionId,
                    IncludeChildren = true
                }
                .Execute(testEnv.Client)
                .ValidateStatus(true);

				var newAccountId = receipt.Children[0].AccountId;
				var idMirror = AccountId.FromEvmAddress(evmAddress, 0, 0);
                
                Thread.Sleep(5000);

                var accountId = idMirror.PopulateAccountNumAsync(testEnv.Client).GetAwaiter().GetResult();
                Assert.Equal(newAccountId.Num, accountId.Num);
            }
        }
        [Fact]
        public virtual void CanPopulateAccountIdEvmAddressSync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction()
                    .AddHbarTransfer(evmAddressAccount, new Hbar(1))
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(-1))
                    .Execute(testEnv.Client);

                var receipt = new TransactionReceiptQuery
                { 
                    TransactionId = tx.TransactionId,
                    IncludeChildren = true

                }.Execute(testEnv.Client);

                var newAccountId = receipt.Children[0].AccountId;
                
                Thread.Sleep(5000);

                var accountId = newAccountId.PopulateAccountEvmAddress(testEnv.Client);
                Assert.Equal(evmAddressAccount.EvmAddress, accountId.EvmAddress);
            }
        }
        [Fact]
        public virtual void CanPopulateAccountIdEvmAddressAsync()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var privateKey = PrivateKey.GenerateECDSA();
                var publicKey = privateKey.GetPublicKey();
                var evmAddress = publicKey.ToEvmAddress();
                var evmAddressAccount = AccountId.FromEvmAddress(evmAddress, 0, 0);
                var tx = new TransferTransaction()
                    .AddHbarTransfer(evmAddressAccount, new Hbar(1))
                    .AddHbarTransfer(testEnv.OperatorId, new Hbar(-1))
                    .Execute(testEnv.Client);

                var receipt = new TransactionReceiptQuery
                {
                    TransactionId = tx.TransactionId,
                    IncludeChildren = true

                }.Execute(testEnv.Client);
				
                var newAccountId = receipt.Children[0].AccountId;
				
                Thread.Sleep(5000);

                var accountId = newAccountId.PopulateAccountEvmAddressAsync(testEnv.Client).GetAwaiter().GetResult();
                Assert.Equal(evmAddressAccount.EvmAddress, accountId.EvmAddress);
            }
        }
    }
}