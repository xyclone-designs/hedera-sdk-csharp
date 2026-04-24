// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <include file="Network.cs.xml" path='docs/member[@name="T:Network"]/*' />
	public class Network : BaseNetwork<Network, AccountId, Node>
    {
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.#ctor(ExecutorService,System.Collections.Generic.Dictionary{System.String,AccountId})"]/*' />
		private Dictionary<AccountId, NodeAddress>? AddressBook;

		public Network(ExecutorService executor, Dictionary<string, AccountId> network) : base(executor)
        {
			try
			{
				SetNetwork(network);
			}
			catch (ThreadInterruptedException) { }
			catch (TimeoutException) { }
        }

		internal static Dictionary<AccountId, NodeAddress> ReadAddressBookResource(string fileName)
		{
			try
			{
				using Stream stream = typeof(Network).Assembly.GetManifestResourceStream("Resources.AddressBook." + fileName) ?? throw new ArgumentNullException(null, "ReadAddressBookResource.[stream]");
				
				ByteString contents = ByteString.FromStream(stream);

				var nodeAddressBook = NodeAddressBook.FromBytes(contents);
				var map = new Dictionary<AccountId, NodeAddress>();
				foreach (var nodeAddress in nodeAddressBook.NodeAddresses)
				{
					if (nodeAddress.AccountId == null)
					{
						continue;
					}

					map.Add(nodeAddress.AccountId, nodeAddress);
				}

				return map;
			}
			catch (IOException e)
			{
				throw new Exception(string.Empty, e);
			}
		}
		internal static Dictionary<AccountId, NodeAddress>? GetAddressBookForLedger(LedgerId? ledgerId)
		{
			return (ledgerId == null || !ledgerId.IsKnownNetwork) ? null : ReadAddressBookResource("addressbook/" + ledgerId + ".pb");
		}
		internal static Dictionary<string, AccountId> AddressBookToNetwork(IEnumerable<NodeAddress> addressBook)
		{
			var network = new Dictionary<string, AccountId>();
			foreach (var nodeAddress in addressBook)
			{
				foreach (var endpoint in nodeAddress.Addresses)
					if (nodeAddress.AccountId is not null)
					{
						network.Add(endpoint.ToString(), nodeAddress.AccountId);
					}
			}

			return network;
		}

		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.ForMainnet(ExecutorService)"]/*' />
		internal static Network ForMainnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.MAINNET) ?? throw new Exception("AddressBookForLedger 'LedgerId.MAINNET' could not be found"); ;
            Dictionary<string, AccountId> network = AddressBookToNetwork(addressBook.Values);
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.MAINNET, addressBook);
        }
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.ForTestnet(ExecutorService)"]/*' />
		internal static Network ForTestnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.TESTNET) ?? throw new Exception("AddressBookForLedger 'LedgerId.TESTNET' could not be found"); ;
            Dictionary<string, AccountId> network = AddressBookToNetwork(addressBook.Values);
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.TESTNET, addressBook);
        }
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.ForPreviewnet(ExecutorService)"]/*' />
		internal static Network ForPreviewnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.PREVIEWNET) ?? throw new Exception("AddressBookForLedger 'LedgerId.PREVIEWNET' could not be found");
            Dictionary<string, AccountId> network = AddressBookToNetwork(addressBook.Values);
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.PREVIEWNET, addressBook);
        }
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.ForNetwork(ExecutorService,System.Collections.Generic.Dictionary{System.String,AccountId})"]/*' />
		internal static Network ForNetwork(ExecutorService executor, Dictionary<string, AccountId> network)
		{
			return new Network(executor, network);
		}

		public override LedgerId? LedgerId 
		{
			get => base.LedgerId;
			set { lock (this) SetLedgerIdInternal(value, GetAddressBookForLedger(value)); }
		}
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.lock(this)"]/*' />
		public override bool TransportSecurity 
		{
            get => base.TransportSecurity;
			set
			{
				lock (this)
				{
					if (base.TransportSecurity != value)
					{
						Network.Clear();
						for (int i = 0; i < Nodes.Count; i++)
						{
							var node = Nodes[i];
							node.Dispose(CloseTimeout);
							node = value ? node.ToSecure() : node.ToInsecure();
							Nodes[i] = node;
							GetNodesForKey(node.Key).Add(node);
						}
					}

					HealthyNodes = [.. Nodes];
					base.TransportSecurity = value;
				}
			}
		}
		/// <include file="Network.cs.xml" path='docs/member[@name="P:Network.MaxNodesPerRequest"]/*' />
		public virtual int? MaxNodesPerRequest { get; set; }
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.Min(MaxNodesPerRequest.,Network.)"]/*' />
		public virtual int NumberOfNodesForRequest
		{
			get => MaxNodesPerRequest is not null ? Math.Min(MaxNodesPerRequest.Value, Network.Count) : (Network.Count + 3 - 1) / 3;
		}
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.lock(this)_2"]/*' />
		public virtual bool VerifyCertificates 
        {
            get;
            set
            {
                lock (this)
                {
                    field = value;
                    foreach (var node in Nodes)
                    {
                        node.VerifyCertificates = value;
                    }
                }
            }
        } = true;

		private IList<Node> GetNodesForKey(AccountId key)
		{
			if (Network.TryGetValue(key, out IList<Node>? value))
			{
				return value;
			}
			else
			{
				var newList = new List<Node>();
				Network.Add(key, newList);
				return newList;
			}
		}
		private Network SetLedgerIdInternal(LedgerId? ledgerId, Dictionary<AccountId, NodeAddress>? addressBook)
        {
            base.LedgerId = ledgerId;

            AddressBook = addressBook;

            foreach (var node in Nodes)
				node.AddressBookEntry = addressBook[node.AccountId];

			return this;
        }
        
        protected override Node CreateNodeFromNetworkEntry(KeyValuePair<string, AccountId> entry)
        {
			return new Node(entry.Value, entry.Key, Executor)
			{
				AddressBookEntry = AddressBook[entry.Value],
				VerifyCertificates = VerifyCertificates,
			};
        }
        
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.GetNetwork"]/*' />
		public virtual Dictionary<string, AccountId> GetNetwork()
		{
			lock (this)
			{
				Dictionary<string, AccountId> returnMap = [];
				foreach (var node in Nodes)
				{
					returnMap.Add(node.Address.ToString(), node.AccountId);
				}

				return returnMap;
			}
		}
		/// <include file="Network.cs.xml" path='docs/member[@name="M:Network.GetNodeAccountIdsForExecute"]/*' />
		public virtual List<AccountId> GetNodeAccountIdsForExecute()
		{
			lock (this)
			{
				var nodes = GetNumberOfMostHealthyNodes(NumberOfNodesForRequest);
				var nodeAccountIds = new List<AccountId>(nodes.Count);
				foreach (var node in nodes)
				{
					nodeAccountIds.Add(node.AccountId);
				}

				return nodeAccountIds;
			}
		}
		public virtual void SetAddressBook(NodeAddressBook addressBook)
		{
			/*
            * Here we index by AccountId ignoring any subsequent entries with the same AccountId.
            *
            * Currently, this seems to be needed when reloading predefined address book for testnet which contains
            * multiple entries with the same AccountId.
            *
            * If it becomes necessary to better handle such cases, either the one-to-one mapping from AccountId to
            * single NodeAddress should be abandoned or NodeAddresses with the same AccountId may need to be merged.
            * */
			Dictionary<AccountId, NodeAddress> newAddressBook = addressBook.NodeAddresses
				.Where(_ => _?.AccountId is not null)
				.GroupBy(_ => _.AccountId!)
				.ToDictionary(_ => _.Key, _ => _.First());


			/*
             * Here we preserve the certificate hash in the case where one is previously defined and no new one is provided.
             *
             * Currently, this seems to be needed since the downloaded address book lacks the certificate hash. However,
             * it is expected the certificate hash will be provided in the future in which case this workaround will no
             * longer be necessary.
             * */
			if (null != addressBook)
			{
				foreach (KeyValuePair<AccountId, NodeAddress> entry in newAddressBook)
				{
					NodeAddress previous = AddressBook[entry.Key];

					if (null != previous)
						if (entry.Value.CertHash is null || entry.Value.CertHash.Length == 0)
							entry.Value.CertHash = previous.CertHash;
				}
			}

			AddressBook = newAddressBook;

			foreach (var node in Nodes)
			{
				node.AddressBookEntry = AddressBook[node.AccountId];
			}
		}

    }
}