// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Token;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    class TokenDeleteIntegrationTest
    {
        public virtual void CanDeleteToken()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					Decimals = 3,
					InitialSupply = 1000000,
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,
					FreezeKey = testEnv.OperatorKey,
					WipeKey = testEnv.OperatorKey,
					KycKey = testEnv.OperatorKey,
					SupplyKey = testEnv.OperatorKey,
					FreezeDefault = false,

				}.Execute(testEnv.Client);
                
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;

                new TokenDeleteTransaction
                {
					TokenId = tokenId,

				}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanDeleteTokenWithOnlyAdminKeySet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = testEnv.OperatorKey,

				}.Execute(testEnv.Client);

                var _ = response.GetReceipt(testEnv.Client).TokenId;
            }
        }

        public virtual void CannotDeleteTokenWhenAdminKeyDoesNotSignTransaction()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                var key = PrivateKey.GenerateED25519();
                var response = new TokenCreateTransaction
                {
					TokenName = "ffff",
					TokenSymbol = "F",
					TreasuryAccountId = testEnv.OperatorId,
					AdminKey = key

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client);
                var tokenId = response.GetReceipt(testEnv.Client).TokenId;

                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    new TokenDeleteTransaction
                    {
						TokenId = tokenId,

					}.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }); Assert.Contains(ResponseStatus.InvalidSignature.ToString(), exception.Message);

                new TokenDeleteTransaction
                {
					TokenId = tokenId,

				}.FreezeWith(testEnv.Client).Sign(key).Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CannotDeleteTokenWhenTokenIDIsNotSet()
        {
            using (var testEnv = new IntegrationTestEnv(1).UseThrowawayAccount())
            {
                PrecheckStatusException exception = Assert.Throws<PrecheckStatusException>(() =>
                {
                    new TokenDeleteTransaction().Execute(testEnv.Client).GetReceipt(testEnv.Client);

                }); Assert.Contains(ResponseStatus.InvalidTokenId.ToString(), exception.Message);
            }
        }
    }
}