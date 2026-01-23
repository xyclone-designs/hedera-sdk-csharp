// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Transactions.Account;
using Io.Grpc;
using Java.Util.Concurrent;
using Javax.Annotation;
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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    class Node : BaseNode<Node, AccountId>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="address">the address as a managed node address</param>
        /// <param name="executor">the executor service</param>
        Node(AccountId accountId, BaseNodeAddress address, ExecutorService executor) : base(address, executor)
        {
            accountId = accountId;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="address">the address as a string</param>
        /// <param name="executor">the executor service</param>
        Node(AccountId accountId, string address, ExecutorService executor) : this(accountId, BaseNodeAddress.FromString(address), executor)
        {
        }

        /// <summary>
        /// Constructor for a node that verifies certificates.
        /// </summary>
        /// <param name="node">the node</param>
        /// <param name="address">the address as a managed node address</param>
        Node(Node node, BaseNodeAddress address) : base(node, address)
        {
            AccountId = node.AccountId;
            VerifyCertificates = node.VerifyCertificates;
            AddressBookEntry = node.AddressBookEntry;
        }

		private readonly AccountId AccountId;
		// This kind of shadows the address field inherited from BaseNode.
		// This is only needed for the cert hash
		private NodeAddress AddressBookEntry;
		private bool VerifyCertificates;

		/// <summary>
		/// Create an insecure version of this node
		/// </summary>
		/// <returns>                         the insecure version of the node</returns>
		virtual Node ToInsecure()
        {
            return new Node(this, address.ToInsecure());
        }

        /// <summary>
        /// Create a secure version of this node
        /// </summary>
        /// <returns>                         the secure version of the node</returns>
        virtual Node ToSecure()
        {
            return new Node(this, address.ToSecure());
        }

        override AccountId GetKey()
        {
            return AccountId;
        }

        /// <summary>
        /// Extract the account id.
        /// </summary>
        /// <returns>                         the account id</returns>
        virtual AccountId GetAccountId()
        {
            return AccountId;
        }

        /// <summary>
        /// Extract the address book.
        /// </summary>
        /// <returns>                         the address book</returns>
        virtual NodeAddress GetAddressBookEntry()
        {
            return AddressBookEntry;
        }

        /// <summary>
        /// Assign the address book.
        /// </summary>
        /// <param name="addressBookEntry">the address book</param>
        /// <returns>{@code this}</returns>
        virtual Node SetAddressBookEntry(NodeAddress addressBookEntry)
        {
            addressBookEntry = addressBookEntry;
            return this;
        }

        /// <summary>
        /// Are the certificates being verified?
        /// </summary>
        /// <returns>                         are the certificates being verified</returns>
        virtual bool IsVerifyCertificates()
        {
            return VerifyCertificates;
        }

        /// <summary>
        /// Assign the certificate status.
        /// </summary>
        /// <param name="verifyCertificates">should certificates be verified</param>
        /// <returns>{@code this}</returns>
        virtual Node SetVerifyCertificates(bool verifyCertificates)
        {
            verifyCertificates = verifyCertificates;
            return this;
        }

        override ChannelCredentials GetChannelCredentials()
        {
            return TlsChannelCredentials.NewBuilder().TrustManager(new HederaTrustManager(AddressBookEntry == null ? null : AddressBookEntry.certHash, VerifyCertificates)).Build();
        }

        public override string ToString()
        {
            return address.ToString() + "->" + AccountId.ToString();
        }
    }
}