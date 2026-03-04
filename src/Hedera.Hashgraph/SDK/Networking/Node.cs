// SPDX-License-Identifier: Apache-2.0
using Grpc.Core;

using Hedera.Hashgraph.SDK.Account;
using System.Security.Cryptography.X509Certificates;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="Node.cs.xml" path='docs/member[@name="T:Node"]/*' />
	public class Node : BaseNode<Node, AccountId>
    {
		/// <include file="Node.cs.xml" path='docs/member[@name="M:Node.#ctor(Node,BaseNodeAddress)"]/*' />
		public Node(Node node, BaseNodeAddress address) : base(node, address)
		{
			AccountId = node.AccountId;
			VerifyCertificates = node.VerifyCertificates;
			AddressBookEntry = node.AddressBookEntry;
		}
		/// <include file="Node.cs.xml" path='docs/member[@name="M:Node.#ctor(AccountId,BaseNodeAddress,ExecutorService)"]/*' />
		public Node(AccountId accountId, BaseNodeAddress address, ExecutorService executor) : base(address, executor)
        {
            AccountId = accountId;
        }
        /// <include file="Node.cs.xml" path='docs/member[@name="M:Node.#ctor(AccountId,System.String,ExecutorService)"]/*' />
        public Node(AccountId accountId, string address, ExecutorService executor) : this(accountId, BaseNodeAddress.FromString(address), executor) { }

		/// <include file="Node.cs.xml" path='docs/member[@name="M:Node.ToSecure"]/*' />
		public virtual Node ToSecure()
		{
			return new Node(this, Address.ToSecure());
		}
		/// <include file="Node.cs.xml" path='docs/member[@name="M:Node.ToInsecure"]/*' />
		public virtual Node ToInsecure()
        {
            return new Node(this, Address.ToInsecure());
        }

        public override AccountId Key { get; }

		/// <include file="Node.cs.xml" path='docs/member[@name="P:Node.AccountId"]/*' />
		public virtual AccountId AccountId { get; }
		/// <include file="Node.cs.xml" path='docs/member[@name="P:Node.AddressBookEntry"]/*' />
		public virtual NodeAddress AddressBookEntry { get; set; }
        /// <include file="Node.cs.xml" path='docs/member[@name="P:Node.VerifyCertificates"]/*' />
        public virtual bool VerifyCertificates { get; set; }

        public override ChannelCredentials GetChannelCredentials()
        {
			return ChannelCredentials.SecureSsl;
			// TODO
			//return Proto.TlsChannelCredentials.NewBuilder().TrustManager(new HederaTrustManager(AddressBookEntry.CertHash, VerifyCertificates)).Build();
		}
    }
}