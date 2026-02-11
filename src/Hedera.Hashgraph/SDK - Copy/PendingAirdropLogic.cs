// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK
{
    public abstract class PendingAirdropLogic<T> : Transaction<T> where T : PendingAirdropLogic<T>
    {
        protected PendingAirdropLogic() { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txBody">protobuf TransactionBody</param>
		internal PendingAirdropLogic(Proto.TransactionBody txBody) : base(txBody) { }
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction) records</param>
		/// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
		internal PendingAirdropLogic(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs) { }

        /// <summary>
        /// Extract the pending airdrop ids
        /// </summary>
        /// <returns>the pending airdrop ids</returns>
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