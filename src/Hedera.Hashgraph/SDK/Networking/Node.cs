// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// Internal utility class.
	/// </summary>
	public class Node : BaseNode<Node, AccountId>
    {
		/// <summary>
		/// Constructor for a node that verifies certificates.
		/// </summary>
		/// <param name="node">the node</param>
		/// <param name="address">the address as a managed node address</param>
		public Node(Node node, BaseNodeAddress address) : base(node, address)
		{
			AccountId = node.AccountId;
			VerifyCertificates = node.VerifyCertificates;
			AddressBookEntry = node.AddressBookEntry;
		}
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="accountId">the account id</param>
		/// <param name="address">the address as a managed node address</param>
		/// <param name="executor">the executor service</param>
		public Node(AccountId accountId, BaseNodeAddress address, ExecutorService executor) : base(address, executor)
        {
            AccountId = accountId;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="address">the address as a string</param>
        /// <param name="executor">the executor service</param>
        public Node(AccountId accountId, string address, ExecutorService executor) : this(accountId, BaseNodeAddress.FromString(address), executor) { }

		/// <summary>
		/// Create a secure version of this node
		/// </summary>
		/// <returns>                         the secure version of the node</returns>
		public virtual Node ToSecure()
		{
			return new Node(this, Address.ToSecure());
		}
		/// <summary>
		/// Create an insecure version of this node
		/// </summary>
		/// <returns>                         the insecure version of the node</returns>
		public virtual Node ToInsecure()
        {
            return new Node(this, Address.ToInsecure());
        }

        public override AccountId Key { get; }

		/// <summary>
		/// Extract the account id.
		/// </summary>
		/// <returns>                         the account id</returns>
		public virtual AccountId AccountId { get; }
		/// <summary>
		/// Extract the address book.
        /// This kind of shadows the address field inherited from BaseNode.
		/// This is only needed for the cert hash
		/// </summary>
		public virtual NodeAddress AddressBookEntry { get; set; }
        /// <summary>
        /// Are the certificates being verified?
        /// </summary>
        public virtual bool VerifyCertificates { get; set; }

        public override ChannelCredentials GetChannelCredentials()
        {
            return Proto. TlsChannelCredentials.NewBuilder().TrustManager(new HederaTrustManager(AddressBookEntry.CertHash, VerifyCertificates)).Build();
        }
    }
}