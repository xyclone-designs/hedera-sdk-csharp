// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Transactions;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.Token
{
    /// <summary>
    /// Token Airdrop
    /// An "airdrop" is a distribution of tokens from a funding account
    /// to one or more recipient accounts, ideally with no action required
    /// by the recipient account(s).
    /// </summary>
    public class TokenAirdropTransaction : AbstractTokenTransferTransaction<TokenAirdropTransaction>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TokenAirdropTransaction() : base()
        {
            DefaultMaxTransactionFee = new Hbar(1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txs">Compound list of transaction id's list of (AccountId, Transaction)
        ///            records</param>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        TokenAirdropTransaction(LinkedDictionary<TransactionId, LinkedDictionary<AccountId, Proto.Transaction>> txs) : base(txs)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="txBody">protobuf TransactionBody</param>
        TokenAirdropTransaction(Proto.TransactionBody txBody) : base(txBody)
        {
            InitFromTransactionBody();
        }

        /// <summary>
        /// Build the transaction body.
        /// </summary>
        /// <returns>{@link
        ///         Proto.TokenAirdropTransactionBody}</returns>
        public virtual Proto.TokenAirdropTransactionBody Build()
        {
            var transfers = SortTransfersAndBuild();
            var builder = new Proto.TokenAirdropTransactionBody();
            foreach (var transfer in transfers)
            {
                builder.TokenTransfers.Add(transfer.ToProtobuf());
            }

            return builder;
        }

        public override MethodDescriptor<Proto.Transaction, TransactionResponse> GetMethodDescriptor()
        {
            return TokenServiceGrpc.GetAirdropTokensMethod();
        }
        public override void OnFreeze(Proto.TransactionBody bodyBuilder)
        {
            bodyBuilder.TokenAirdrop = Build();
        }
        public override void OnScheduled(Proto.SchedulableTransactionBody scheduled)
        {
            scheduled.TokenAirdrop = Build();
        }

        /// <summary>
        /// Initialize from the transaction body.
        /// </summary>
        public virtual void InitFromTransactionBody()
        {
            var body = SourceTransactionBody.TokenAirdrop;

            foreach (var tokenTransferList in body.TokenTransfers)
            {
                var token = TokenId.FromProtobuf(tokenTransferList.Token);
                foreach (var transfer in tokenTransferList.Transfers)
                {
                    tokenTransfers.Add(new TokenTransfer(
                        token, 
                        AccountId.FromProtobuf(transfer.AccountID), 
                        transfer.Amount, 
                        tokenTransferList.ExpectedDecimals, 
                        transfer.IsApproval));
                }

                foreach (var transfer in tokenTransferList.NftTransfers)
                {
                    nftTransfers.Add(new TokenNftTransfer(
                        token, 
                        AccountId.FromProtobuf(transfer.SenderAccountID), 
                        AccountId.FromProtobuf(transfer.ReceiverAccountID), 
                        transfer.SerialNumber, 
                        transfer.IsApproval));
                }
            }
        }

        public override ResponseStatus MapResponseStatus(Proto.Response response)
        {
            throw new global::System.NotImplementedException();
        }
        public override TransactionResponse MapResponse(Proto.Response response, AccountId nodeId, Proto.Transaction request)
        {
            throw new global::System.NotImplementedException();
        }
    }
}