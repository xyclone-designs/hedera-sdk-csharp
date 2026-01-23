// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Io;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Transactions.Account;
using Java.Io;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Java.Util.Stream;
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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal utility class.
    /// </summary>
    class Network : BaseNetwork<Network, AccountId, Node>
    {
        private int maxNodesPerRequest;
        /// <summary>
        /// The protobuf address book converted into a map of node account IDs to NodeAddress
        /// 
        /// This variable is package private so tests can use it
        /// </summary>
        Dictionary<AccountId, NodeAddress> addressBook;
        private bool verifyCertificates = true;
        private Network(ExecutorService executor, Dictionary<string, AccountId> network) : base(executor)
        {
            try
            {
                SetNetwork(network);
            }
            catch (InterruptedException e)
            {
            }
            catch (TimeoutException e)
            {
            }
        }

        /// <summary>
        /// Create a network.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <param name="network">the network records</param>
        /// <returns>                         the new network</returns>
        static Network ForNetwork(ExecutorService executor, Dictionary<string, AccountId> network)
        {
            return new Network(executor, network);
        }

        /// <summary>
        /// Create a mainnet network.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>                         the new mainnet network</returns>
        static Network ForMainnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.MAINNET);
            HashMap<string, AccountId> network = AddressBookToNetwork(Objects.RequireNonNull(addressBook).Values());
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.MAINNET, addressBook);
        }

        /// <summary>
        /// Create a testnet network.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>                         the new testnet network</returns>
        static Network ForTestnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.TESTNET);
            HashMap<string, AccountId> network = AddressBookToNetwork(Objects.RequireNonNull(addressBook).Values());
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.TESTNET, addressBook);
        }

        /// <summary>
        /// Create a previewnet network.
        /// </summary>
        /// <param name="executor">the executor service</param>
        /// <returns>                         the new previewnet network</returns>
        static Network ForPreviewnet(ExecutorService executor)
        {
            var addressBook = GetAddressBookForLedger(LedgerId.PREVIEWNET);
            HashMap<string, AccountId> network = AddressBookToNetwork(Objects.RequireNonNull(addressBook).Values());
            return new Network(executor, network).SetLedgerIdInternal(LedgerId.PREVIEWNET, addressBook);
        }

        /// <summary>
        /// Are certificates being verified?
        /// </summary>
        /// <returns>                         are certificates being verified</returns>
        virtual bool IsVerifyCertificates()
        {
            return verifyCertificates;
        }

        /// <summary>
        /// Assign the desired verify certificate status.
        /// </summary>
        /// <param name="verifyCertificates">the desired status</param>
        /// <returns>{@code this}</returns>
        virtual Network SetVerifyCertificates(bool verifyCertificates)
        {
            lock (this)
            {
                verifyCertificates = verifyCertificates;
                foreach (var node in nodes)
                {
                    node.SetVerifyCertificates(verifyCertificates);
                }

                return this;
            }
        }

        override Network SetLedgerId(LedgerId ledgerId)
        {
            lock (this)
            {
                return SetLedgerIdInternal(ledgerId, GetAddressBookForLedger(ledgerId));
            }
        }

        private Network SetLedgerIdInternal(LedgerId ledgerId, Dictionary<AccountId, NodeAddress> addressBook)
        {
            base.SetLedgerId(ledgerId);
            addressBook = addressBook;
            foreach (var node in nodes)
            {
                node.SetAddressBookEntry(addressBook == null ? null : addressBook[node.GetAccountId()]);
            }

            return this;
        }

        virtual void SetAddressBook(NodeAddressBook addressBook)
        {
            Dictionary<AccountId, NodeAddress> newAddressBook = addressBook.GetNodeAddresses().Stream().Filter((nodeAddress) => Objects.NonNull(nodeAddress.GetAccountId())).Collect(Collectors.ToMap(NodeAddress.GetAccountId(), Function.Identity(), (a, b) => a));
            /*
             * Here we preserve the certificate hash in the case where one is previously defined and no new one is provided.
             *
             * Currently, this seems to be needed since the downloaded address book lacks the certificate hash. However,
             * it is expected the certificate hash will be provided in the future in which case this workaround will no
             * longer be necessary.
             * */
            if (null != addressBook)
            {
                foreach (Map.Entry<AccountId, NodeAddress> entry in newAddressBook.EntrySet())
                {
                    NodeAddress previous = addressBook[entry.GetKey()];
                    if (null != previous)
                    {
                        ByteString certHash = entry.GetValue().GetCertHash();
                        if (null == certHash || certHash.IsEmpty())
                        {
                            entry.GetValue().SetCertHash(previous.certHash);
                        }
                    }
                }
            }

            addressBook = newAddressBook;
            foreach (var node in nodes)
            {
                node.SetAddressBookEntry(addressBook[node.GetAccountId()]);
            }
        }

        private static Dictionary<AccountId, NodeAddress> GetAddressBookForLedger(LedgerId ledgerId)
        {
            return (ledgerId == null || !ledgerId.IsKnownNetwork()) ? null : ReadAddressBookResource("addressbook/" + ledgerId + ".pb");
        }

        static HashMap<string, AccountId> AddressBookToNetwork(Collection<NodeAddress> addressBook)
        {
            var network = new HashMap<string, AccountId>();
            foreach (var nodeAddress in addressBook)
            {
                foreach (var endpoint in nodeAddress.addresses)
                {
                    network.Put(endpoint.ToString(), nodeAddress.accountId);
                }
            }

            return network;
        }

        /// <summary>
        /// Import an address book.
        /// </summary>
        /// <param name="fileName">the file name</param>
        /// <returns>                         the list of address book records</returns>
        static Dictionary<AccountId, NodeAddress> ReadAddressBookResource(string fileName)
        {
            try
            {
                using (var inputStream = Objects.RequireNonNull(typeof(Network).GetResource("/" + fileName)).OpenStream())
                {
                    var contents = ByteStreams.ToByteArray(inputStream);
                    var nodeAddressBook = NodeAddressBook.FromBytes(ByteString.CopyFrom(contents));
                    var map = new HashMap<AccountId, NodeAddress>();
                    foreach (var nodeAddress in nodeAddressBook.nodeAddresses)
                    {
                        if (nodeAddress.accountId == null)
                        {
                            continue;
                        }

                        map.Put(nodeAddress.accountId, nodeAddress);
                    }

                    return map;
                }
            }
            catch (IOException e)
            {
                throw new Exception(e);
            }
        }

        /// <summary>
        /// Extract the of network records.
        /// </summary>
        /// <returns>                         list of network records</returns>
        virtual Dictionary<string, AccountId> GetNetwork()
        {
            lock (this)
            {
                Dictionary<string, AccountId> returnMap = new HashMap();
                foreach (var node in nodes)
                {
                    returnMap.Put(node.address.ToString(), node.GetAccountId());
                }

                return returnMap;
            }
        }

        protected override Node CreateNodeFromNetworkEntry(Map.Entry<String, AccountId> entry)
        {
            var addressBookEntry = addressBook != null ? addressBook[entry.GetValue()] : null;
            return new Node(entry.GetValue(), entry.GetKey(), executor).SetAddressBookEntry(addressBookEntry).SetVerifyCertificates(verifyCertificates);
        }

        /// <summary>
        /// Pick 1/3 of the nodes sorted by health and expected delay from the network.
        /// This is used by Query and Transaction for selecting node AccountId's.
        /// </summary>
        /// <returns>{@link java.util.List<AccountId>}</returns>
        virtual IList<AccountId> GetNodeAccountIdsForExecute()
        {
            lock (this)
            {
                var nodes = GetNumberOfMostHealthyNodes(GetNumberOfNodesForRequest());
                var nodeAccountIds = new List<AccountId>(nodes.Count);
                foreach (var node in nodes)
                {
                    nodeAccountIds.Add(node.GetAccountId());
                }

                return nodeAccountIds;
            }
        }

        /// <summary>
        /// Assign the maximum nodes to be returned for each request.
        /// </summary>
        /// <param name="maxNodesPerRequest">the desired number of nodes</param>
        /// <returns>{@code this}</returns>
        virtual Network SetMaxNodesPerRequest(int maxNodesPerRequest)
        {
            maxNodesPerRequest = maxNodesPerRequest;
            return this;
        }

        /// <summary>
        /// Extract the number of nodes for each request.
        /// </summary>
        /// <returns>                         the number of nodes for each request</returns>
        virtual int GetNumberOfNodesForRequest()
        {
            if (maxNodesPerRequest != null)
            {
                return Math.Min(maxNodesPerRequest, network.Count);
            }
            else
            {
                return (network.Count + 3 - 1) / 3;
            }
        }

        private IList<Node> GetNodesForKey(AccountId key)
        {
            if (network.ContainsKey(key))
            {
                return network[key];
            }
            else
            {
                var newList = new List<Node>();
                network.Put(key, newList);
                return newList;
            }
        }

        /// <summary>
        /// Enable or disable transport security (TLS).
        /// </summary>
        /// <param name="transportSecurity">should transport security be enabled</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="InterruptedException">when a thread is interrupted while it's waiting, sleeping, or otherwise occupied</exception>
        virtual Network SetTransportSecurity(bool transportSecurity)
        {
            lock (this)
            {
                if (transportSecurity != transportSecurity)
                {
                    network.Clear();
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        node.Dispose(closeTimeout);
                        node = transportSecurity ? node.ToSecure() : node.ToInsecure();
                        nodes[i] = node;
                        GetNodesForKey(node.GetKey()).Add(node);
                    }
                }

                healthyNodes = new List(nodes);
                transportSecurity = transportSecurity;
                return this;
            }
        }
    }
}