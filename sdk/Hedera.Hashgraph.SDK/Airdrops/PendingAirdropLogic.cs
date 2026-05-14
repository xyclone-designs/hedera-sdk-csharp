// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Airdrops
{
    public abstract class PendingAirdropLogic<T> : Transaction<T> where T : PendingAirdropLogic<T>
    {
        protected PendingAirdropLogic() { }
		/// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:PendingAirdropLogic(Proto.Services.TransactionBody)"]' />
		internal PendingAirdropLogic(Proto.Services.TransactionBody txBody) : base(txBody) { }
		/// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:PendingAirdropLogic(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Services.Transaction}})"]' />
		internal PendingAirdropLogic(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Services.Transaction>> txs) : base(txs) { }

        /// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]' />
        public virtual ListGuarded<PendingAirdropId> PendingAirdropIds
        {
            init; get => field ??= new ListGuarded<PendingAirdropId>
            {
                OnRequireNotFrozen = RequireNotFrozen
            };
        }

        public override void ValidateChecksums(Client client)
        {
            foreach (var pendingAirdropId in PendingAirdropIds)
            {
                pendingAirdropId.TokenId?.ValidateChecksum(client);
                pendingAirdropId.Receiver?.ValidateChecksum(client);
                pendingAirdropId.Sender?.ValidateChecksum(client);
				pendingAirdropId.NftId?.TokenId.ValidateChecksum(client);
			}
        }
    }
}
