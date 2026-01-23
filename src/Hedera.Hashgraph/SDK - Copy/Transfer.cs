// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Hedera.Hashgraph.SDK.Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using static Hedera.Hashgraph.SDK.NetworkName;
using static Hedera.Hashgraph.SDK.NftHookType;
using static Hedera.Hashgraph.SDK.RequestType;
using static Hedera.Hashgraph.SDK.Status;
using static Hedera.Hashgraph.SDK.TokenKeyValidation;
using static Hedera.Hashgraph.SDK.TokenSupplyType;
using static Hedera.Hashgraph.SDK.TokenType;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// A transfer of Hbar that occurred within a transaction.
    /// <p>
    /// Returned with a {@link TransactionRecord}.
    /// </summary>
    public sealed class Transfer
    {
        /// <summary>
        /// The Account ID that sends or receives crypto-currency.
        /// </summary>
        public readonly AccountId accountId;
        /// <summary>
        /// The amount that the account sends (negative) or receives (positive).
        /// </summary>
        public readonly Hbar amount;
        Transfer(AccountId accountId, Hbar amount)
        {
            this.accountId = accountId;
            this.amount = amount;
        }

        /// <summary>
        /// Create a transfer from a protobuf.
        /// </summary>
        /// <param name="accountAmount">the protobuf</param>
        /// <returns>                         the new transfer</returns>
        static Transfer FromProtobuf(AccountAmount accountAmount)
        {
            return new Transfer(AccountId.FromProtobuf(accountAmount.GetAccountID()), Hbar.FromTinybars(accountAmount.GetAmount()));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        AccountAmount ToProtobuf()
        {
            return AccountAmount.NewBuilder().SetAccountID(accountId.ToProtobuf()).SetAmount(amount.ToTinybars()).Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("accountId", accountId).Add("amount", amount).ToString();
        }
    }
}