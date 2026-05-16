// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Core;
using Hedera.Hashgraph.SDK.Cryptocurrency;
using Hedera.Hashgraph.SDK.Cryptography;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Node
{
    /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="T:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest"]' />
    public class NodeCreateTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_ACCOUNT_ID = AccountId.FromString("0.6.9");
        private static readonly string TEST_DESCRIPTION = "Test description";
        private static readonly List<Endpoint> TEST_GOSSIP_ENDPOINTS = [ SpawnTestEndpoint(0), SpawnTestEndpoint(1), SpawnTestEndpoint(2) ];
        private static readonly List<Endpoint> TEST_SERVICE_ENDPOINTS = [ SpawnTestEndpoint(3), SpawnTestEndpoint(4), SpawnTestEndpoint(5), SpawnTestEndpoint(6) ];
        private static readonly Endpoint TEST_GRPC_WEB_PROXY_ENDPOINT = SpawnTestEndpoint(3);
        private static readonly byte[] TEST_GOSSIP_CA_CERTIFICATE = [ 0, 1, 2, 3, 4 ];
        private static readonly byte[] TEST_GRPC_CERTIFICATE_HASH = [ 5, 6, 7, 8, 9 ];
        private static readonly PublicKey TEST_ADMIN_KEY = PrivateKey.FromString("302e020100300506032b65700422042062c4b69e9f45a554e5424fb5a6fe5e6ac1f19ead31dc7718c2d980fd1f998d4b").GetPublicKey();
        readonly DateTimeOffset TEST_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);

        public virtual void ShouldSerialize()
        {
            Verifier.Verify(SpawnTestTransaction().ToString());
        }

        private static Endpoint SpawnTestEndpoint(byte offset)
        {
            // Valid endpoint: use domainName only to comply with SDK validation
            return new Endpoint
            {
				DomainName = offset + "unit.test.com",
				Port = 42 + offset,
			};
        }

        private NodeCreateTransaction SpawnTestTransaction()
        {
            return new NodeCreateTransaction
            {
                AdminKey = TEST_ADMIN_KEY,
                AccountId = TEST_ACCOUNT_ID,
				Description = TEST_DESCRIPTION,
				GossipEndpoints = TEST_GOSSIP_ENDPOINTS,
				ServiceEndpoints = TEST_SERVICE_ENDPOINTS,
				GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE,
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH,
                GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT,

				MaxTransactionFee = new Hbar(1),
				DeclineReward = false,

				NodeAccountIds = [ AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006") ],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START),
			}
            .Freeze()
            .Sign(TEST_PRIVATE_KEY);
        }
        [Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.ShouldBytes"]' />
        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<NodeCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.FromScheduledTransaction"]' />
        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.Services.SchedulableTransactionBody
            {
				NodeCreate = new Proto.Services.NodeCreateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<NodeCreateTransaction>(transactionBody);

            Assert.IsType<NodeCreateTransaction>(tx);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.ShouldBytesNoSetters"]' />
        public virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeCreateTransaction();
            var tx2 = Transaction.FromBytes<NodeCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.TestUnrecognizedServicePort"]' />
        public virtual void TestUnrecognizedServicePort()
        {
            var tx = new NodeCreateTransaction
            {
				ServiceEndpoints = [ new Endpoint
				{
					DomainName = "unit.test.com",
					Port = 50111,
				} ]
			};
            var tx2 = Transaction.FromBytes<NodeCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.TestSetNull"]' />
        public virtual void TestSetNull()
        {
            new NodeCreateTransaction
            {
                Description = null,
                AccountId = null,
                GossipCaCertificate = null,
                GrpcCertificateHash = null,
                AdminKey = null,
            };
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetDescriptionRejectsOver100Utf8Bytes"]' />
        public virtual void SetDescriptionRejectsOver100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string tooLong = string.Join(string.Empty, Enumerable.Repeat("a", 101));
			Assert.Throws<ArgumentException>(() => tx.Description = tooLong);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetDescriptionAcceptsExactly100Utf8Bytes"]' />
        public virtual void SetDescriptionAcceptsExactly100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string exact = string.Join(string.Empty, Enumerable.Repeat("a", 100));
            tx.Description = exact;

            Assert.Equal(tx.Description, exact);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetGossipEndpointsRejectsMoreThan10"]' />
        public virtual void SetGossipEndpointsRejectsMoreThan10()
        {
            var tx = new NodeCreateTransaction();
            var endpoints = new List<Endpoint>();
            for (int i = 0; i < 11; i++)
				endpoints.Add(new Endpoint
				{
					DomainName = "gossip" + i + ".test",
					Port = 5000 + i,
				});


            Assert.Throws<ArgumentException>(() => tx.GossipEndpoints = endpoints);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.AddGossipEndpointRejectsMoreThan10"]' />
        public virtual void AddGossipEndpointRejectsMoreThan10()
        {
            var tx = new NodeCreateTransaction();
            
			for (int i = 0; i < 10; i++)
				tx.GossipEndpoints.Add(new Endpoint
				{
					DomainName = "gossip" + i + ".test",
					Port = 5000 + i
				});

			Assert.Throws<ArgumentException>(() => tx.GossipEndpoints.Add(new Endpoint
            {
				DomainName = "gossipX.test",
				Port = 6000
			}));
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetGossipEndpointsRejectsIpAndDomainTogether"]' />
        public virtual void SetGossipEndpointsRejectsIpAndDomainTogether()
        {
            var tx = new NodeCreateTransaction();
            var invalid = new Endpoint
            {
				Address = new byte[] { 1, 2, 3, 4 },
				DomainName = "both.test",
				Port = 5000,
			};

            Assert.Throws<ArgumentException>(() => tx.GossipEndpoints = [invalid]);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetServiceEndpointsRejectsMoreThan8"]' />
        public virtual void SetServiceEndpointsRejectsMoreThan8()
        {
            var tx = new NodeCreateTransaction();
            var endpoints = new List<Endpoint>();
            for (int i = 0; i < 9; i++)
				endpoints.Add(new Endpoint
				{
					DomainName = "svc" + i + ".test",
					Port = 6000 + i,
				});

			Assert.Throws<ArgumentException>(() => tx.ServiceEndpoints = endpoints);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.AddServiceEndpointRejectsMoreThan8"]' />
        public virtual void AddServiceEndpointRejectsMoreThan8()
        {
            var tx = new NodeCreateTransaction();
            for (int i = 0; i < 8; i++)
				tx.ServiceEndpoints.Add(new Endpoint
				{
					DomainName = "svc" + i + ".test",
					Port = 7000 + i,
				});

			Assert.Throws<ArgumentException>(() => tx.ServiceEndpoints.Add(new Endpoint
			{
				DomainName = "svcX.test",
				Port = 8000,
			}));
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetServiceEndpointsRejectsIpAndDomainTogether"]' />
        public virtual void SetServiceEndpointsRejectsIpAndDomainTogether()
        {
            var tx = new NodeCreateTransaction();
            var invalid = new Endpoint
			{
				Address = new byte[] { 5, 6, 7, 8 },
				DomainName = "both.test",
				Port = 6000
			};

            Assert.Throws<ArgumentException>(() => tx.ServiceEndpoints = [invalid]);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetGossipCaCertificateRejectsEmpty"]' />
        public virtual void SetGossipCaCertificateRejectsEmpty()
        {
            var tx = new NodeCreateTransaction();

            Assert.Throws<ArgumentException>(() => tx.GossipCaCertificate = new byte[] { });
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.BuildRewritesGossipFqdnWithServiceIpFallback"]' />
        public virtual void BuildRewritesGossipFqdnWithServiceIpFallback()
        {
            byte[] serviceIp = new byte[]
            {
                10,
                0,
                0,
                1
            };
            
			Endpoint gossipFqdnOnly = new Endpoint
			{
				DomainName = "fqdn.example.com",
				Port = 50211
			};
			Endpoint serviceFqdnOnly = new Endpoint
			{
				Address = serviceIp,
				Port = 50211
			};
			var tx = new NodeCreateTransaction
			{
				GossipEndpoints = [gossipFqdnOnly],
				ServiceEndpoints = [serviceFqdnOnly],
			};
			var rewritten = tx.GossipEndpoints[0];

			// gossip endpoint should now carry IP and no domain
			Assert.Equal(rewritten.Address, serviceIp);
            Assert.Empty(rewritten.DomainName);
            Assert.Equal(rewritten.Port, 50211);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.BuildDoesNotRewriteGossipWhenIpPresent"]' />
        public virtual void BuildDoesNotRewriteGossipWhenIpPresent()
        {
            byte[] originalIp = new byte[]
            {
                127,
                0,
                0,
                1
            };
            byte[] serviceIp = new byte[]
            {
                10,
                0,
                0,
                2
            };

			Endpoint gossipIpOnly = new ()
			{
				Address = originalIp,
				Port = 50212
			};
			Endpoint serviceIpOnly = new ()
			{
				Address = serviceIp,
				Port = 50211
			};
			var tx = new NodeCreateTransaction
			{
				GossipEndpoints = [gossipIpOnly],
				ServiceEndpoints = [serviceIpOnly],
			};
			var ge = tx.GossipEndpoints[0];

			Assert.Equal(ge.Address, originalIp);
            Assert.Equal(ge.Port, 50212);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.BuildDoesNotRewriteWhenNoServiceIpAvailable"]' />
        public virtual void BuildDoesNotRewriteWhenNoServiceIpAvailable()
        {
            Endpoint gossipFqdnOnly = new ()
            {
                DomainName = "fqdn.example.com",
                Port = 50213 
            };
            Endpoint serviceFqdnOnly = new ()
            {
                DomainName = "svc.example.com",
                Port = 50211 
            };
            var tx = new NodeCreateTransaction
            {
				GossipEndpoints = [gossipFqdnOnly],
				ServiceEndpoints = [serviceFqdnOnly],
			};
            var ge = tx.GossipEndpoints[0];

            Assert.True(ge.Address.Length == 0);
            Assert.Equal(ge.DomainName, "fqdn.example.com");
            Assert.Equal(ge.Port, 50213);
        }
		[Fact]
        /// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.ConstructNodeCreateTransactionFromTransactionBodyProtobuf"]' />
        public virtual void ConstructNodeCreateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = new Proto.Services.NodeCreateTransactionBody
            {
				GossipCaCertificate = ByteString.CopyFrom(TEST_GOSSIP_CA_CERTIFICATE),
				GrpcCertificateHash = ByteString.CopyFrom(TEST_GRPC_CERTIFICATE_HASH),
				AdminKey = TEST_ADMIN_KEY.ToProtobufKey(),
				DeclineReward = true,
				AccountId = TEST_ACCOUNT_ID.ToProtobuf(),
				Description = TEST_DESCRIPTION,
			};

            foreach (Endpoint gossipEndpoint in TEST_GOSSIP_ENDPOINTS)
				transactionBodyBuilder.GossipEndpoint.Add(gossipEndpoint.ToProtobuf());

			foreach (Endpoint serviceEndpoint in TEST_SERVICE_ENDPOINTS)
				transactionBodyBuilder.ServiceEndpoint.Add(serviceEndpoint.ToProtobuf());



			var nodeCreateTransaction = new NodeCreateTransaction(new Proto.Services.TransactionBody
			{
				NodeCreate = transactionBodyBuilder
			});
            Assert.Equal(nodeCreateTransaction.AccountId, TEST_ACCOUNT_ID);
            Assert.Equal(nodeCreateTransaction.Description, TEST_DESCRIPTION);
            Assert.Equal(nodeCreateTransaction.GossipEndpoints.Count, TEST_GOSSIP_ENDPOINTS.Count);
            Assert.Equal(nodeCreateTransaction.ServiceEndpoints.Count, TEST_SERVICE_ENDPOINTS.Count);
            Assert.Equal(nodeCreateTransaction.GossipCaCertificate, TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeCreateTransaction.GrpcCertificateHash, TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeCreateTransaction.AdminKey, TEST_ADMIN_KEY);
        }
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetAccountId"]' />
		public virtual void GetSetAccountId()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				AccountId = TEST_ACCOUNT_ID
			};
			Assert.Equal(nodeCreateTransaction.AccountId, TEST_ACCOUNT_ID);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetAccountIdFrozen"]' />
		public virtual void GetSetAccountIdFrozen()
		{
			var tx = SpawnTestTransaction();

			Assert.Throws<InvalidOperationException>(() => tx.AccountId = TEST_ACCOUNT_ID);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetDescription"]' />
		public virtual void GetSetDescription()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				Description = TEST_DESCRIPTION
			};
			Assert.Equal(nodeCreateTransaction.Description, TEST_DESCRIPTION);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetDescriptionFrozen"]' />
		public virtual void GetSetDescriptionFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.Description = TEST_DESCRIPTION);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetGossipEndpoints"]' />
		public virtual void GetSetGossipEndpoints()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GossipEndpoints = TEST_GOSSIP_ENDPOINTS
			};
			Assert.Equal(nodeCreateTransaction.GossipEndpoints, TEST_GOSSIP_ENDPOINTS);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetTestGossipEndpointsFrozen"]' />
		public virtual void SetTestGossipEndpointsFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GossipEndpoints = TEST_GOSSIP_ENDPOINTS);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetServiceEndpoints"]' />
		public virtual void GetSetServiceEndpoints()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				ServiceEndpoints = TEST_SERVICE_ENDPOINTS
			};
			Assert.Equal(nodeCreateTransaction.ServiceEndpoints, TEST_SERVICE_ENDPOINTS);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetServiceEndpointsFrozen"]' />
		public virtual void GetSetServiceEndpointsFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.ServiceEndpoints = TEST_SERVICE_ENDPOINTS);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetGossipCaCertificate"]' />
		public virtual void GetSetGossipCaCertificate()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE
			};
			Assert.Equal(nodeCreateTransaction.GossipCaCertificate, TEST_GOSSIP_CA_CERTIFICATE);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetGossipCaCertificateFrozen"]' />
		public virtual void GetSetGossipCaCertificateFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetGrpcCertificateHash"]' />
		public virtual void GetSetGrpcCertificateHash()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH
			};
			Assert.Equal(nodeCreateTransaction.GrpcCertificateHash, TEST_GRPC_CERTIFICATE_HASH);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetGrpcCertificateHashFrozen"]' />
		public virtual void GetSetGrpcCertificateHashFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetAdminKey"]' />
		public virtual void GetSetAdminKey()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				AdminKey = TEST_ADMIN_KEY
			};
			Assert.Equal(nodeCreateTransaction.AdminKey, TEST_ADMIN_KEY);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetAdminKeyFrozen"]' />
		public virtual void GetSetAdminKeyFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.AdminKey = TEST_ADMIN_KEY);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetDeclineReward"]' />
		public virtual void GetSetDeclineReward()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				DeclineReward = true
			};
			Assert.True(nodeCreateTransaction.DeclineReward);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetSetDeclineRewardFrozen"]' />
		public virtual void GetSetDeclineRewardFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.DeclineReward = false);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.GetGrpcWebProxyEndpoint"]' />
		public virtual void GetGrpcWebProxyEndpoint()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT
			};
			Assert.Equal(nodeCreateTransaction.GrpcWebProxyEndpoint, TEST_GRPC_WEB_PROXY_ENDPOINT);
		}
		[Fact]
		/// <include file="test-node-create-transaction.ts.cs.xml" path='docs/member[@name="M:Hedera.Hashgraph.Tests.SDK.Node.NodeCreateTransactionTest.SetGrpcWebProxyEndpointRequiresFrozen"]' />
		public virtual void SetGrpcWebProxyEndpointRequiresFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT);
		}
	}
}
