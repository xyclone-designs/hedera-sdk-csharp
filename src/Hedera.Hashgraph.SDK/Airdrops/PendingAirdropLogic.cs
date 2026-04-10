// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Airdrops
{
    public abstract class PendingAirdropLogic<T> : Transaction<T> where T : PendingAirdropLogic<T>
    {
        protected PendingAirdropLogic() { }
		/// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:PendingAirdropLogic(Proto.TransactionBody)"]/*' />
		internal PendingAirdropLogic(Proto.TransactionBody txBody) : base(txBody) { }
		/// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:PendingAirdropLogic(DictionaryLinked{TransactionId,DictionaryLinked{AccountId,Proto.Transaction}})"]/*' />
		internal PendingAirdropLogic(DictionaryLinked<TransactionId, DictionaryLinked<AccountId, Proto.Transaction>> txs) : base(txs) { }

        /// <include file="PendingAirdropLogic.cs.xml" path='docs/member[@name="M:RequireNotFrozen"]/*' />
        public virtual IList<PendingAirdropId> PendingAirdropIds
        {
            get { RequireNotFrozen(); return field; }
            set { RequireNotFrozen(); field = value; }
        } = [];

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