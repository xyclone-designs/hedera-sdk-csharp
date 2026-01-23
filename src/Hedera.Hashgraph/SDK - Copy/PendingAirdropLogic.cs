// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Transactions;
using Hedera.Hashgraph.SDK.Transactions.Account;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
    abstract class PendingAirdropLogic<T> : Transaction<T> where T : PendingAirdropLogic<T>
    {
        protected IList<PendingAirdropId> PendingAirdropIds = [];
        protected PendingAirdropLogic() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        PendingAirdropLogic(LinkedHashMap<TransactionId, LinkedHashMap<AccountId, Proto.Transaction>> txs) : base(txs) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        PendingAirdropLogic(Proto.TransactionBody txBody) : base(txBody)
        {
        }

        /// <summary>
        /// Extract the pending airdrop ids
        /// </summary>
        /// <returns>the pending airdrop ids</returns>
        public virtual IList<PendingAirdropId> GetPendingAirdropIds()
        {
            return PendingAirdropIds;
        }

        /// <summary>
        /// Set the pending airdrop ids
        /// </summary>
        /// <param name="pendingAirdropIds"></param>
        /// <returns>{@code this}</returns>
        public virtual T SetPendingAirdropIds(IList<PendingAirdropId> pendingAirdropIds)
        {
            RequireNotFrozen();
            PendingAirdropIds = pendingAirdropIds;

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// clear the pending airdrop ids
        /// </summary>
        /// <returns>{@code this}</returns>
        public virtual T ClearPendingAirdropIds()
        {
            RequireNotFrozen();
            PendingAirdropIds = [];

            // noinspection unchecked
            return (T)this;
        }

        /// <summary>
        /// Add pendingAirdropId
        /// </summary>
        /// <param name="pendingAirdropId"></param>
        /// <returns>{@code this}</returns>
        public virtual T AddPendingAirdrop(PendingAirdropId pendingAirdropId)
        {
            RequireNotFrozen();
            PendingAirdropIds.Add(pendingAirdropId);

            // noinspection unchecked
            return (T)this;
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