// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;

using System;
using System.Collections.Generic;
using System.Linq;

using VerifyXunit;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NodeCreateTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_ACCOUNT_ID = AccountId.FromString("0.6.9");
        private static readonly string TEST_DESCRIPTION = "Test description";
        private static readonly List<Endpoint> TEST_GOSSIP_ENDPOINTS = [ SpawnTestEndpoint((byte)0), SpawnTestEndpoint((byte)1), SpawnTestEndpoint((byte)2) ];
        private static readonly List<Endpoint> TEST_SERVICE_ENDPOINTS = [ SpawnTestEndpoint((byte)3), SpawnTestEndpoint((byte)4), SpawnTestEndpoint((byte)5), SpawnTestEndpoint((byte)6) ];
        private static readonly Endpoint TEST_GRPC_WEB_PROXY_ENDPOINT = SpawnTestEndpoint((byte)3);
        private static readonly byte[] TEST_GOSSIP_CA_CERTIFICATE = new byte[]
        {
            0,
            1,
            2,
            3,
            4
        };
        private static readonly byte[] TEST_GRPC_CERTIFICATE_HASH = new byte[]
        {
            5,
            6,
            7,
            8,
            9
        };
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
				AccountId = TEST_ACCOUNT_ID,
				Description = TEST_DESCRIPTION,
				GossipEndpoints = TEST_GOSSIP_ENDPOINTS,
				ServiceEndpoints = TEST_SERVICE_ENDPOINTS,
				GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE,
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH,

				AdminKey = TEST_ADMIN_KEY,
				MaxTransactionFee = new Hbar(1),
				DeclineReward = false,
				GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT,

				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START),
			}
            .Freeze()
            .Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<NodeCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
				NodeCreate = new Proto.NodeCreateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<NodeCreateTransaction>(transactionBody);

            Assert.IsType<NodeCreateTransaction>(tx);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeCreateTransaction();
            var tx2 = Transaction.FromBytes<NodeCreateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

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

        public virtual void SetDescriptionRejectsOver100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string tooLong = string.Join(string.Empty, Enumerable.Repeat("a", 101));
			Assert.Throws<ArgumentException>(() => tx.Description = tooLong);
        }

        public virtual void SetDescriptionAcceptsExactly100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string exact = string.Join(string.Empty, Enumerable.Repeat("a", 100));
            tx.Description = exact;

            Assert.Equal(tx.Description, exact);
        }

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

        public virtual void SetGossipCaCertificateRejectsEmpty()
        {
            var tx = new NodeCreateTransaction();

            Assert.Throws<ArgumentException>(() => tx.GossipCaCertificate = new byte[] { });
        }

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

			Endpoint gossipIpOnly = new Endpoint
			{
				Address = originalIp,
				Port = 50212
			};
			Endpoint serviceIpOnly = new Endpoint
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

        public virtual void BuildDoesNotRewriteWhenNoServiceIpAvailable()
        {
            Endpoint gossipFqdnOnly = new Endpoint
            {
                DomainName = "fqdn.example.com",
                Port = 50213 
            };
            Endpoint serviceFqdnOnly = new Endpoint
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

        public virtual void ConstructNodeCreateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = new Proto.NodeCreateTransactionBody
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



			var nodeCreateTransaction = new NodeCreateTransaction(new Proto.TransactionBody
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

		public virtual void GetSetAccountId()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				AccountId = TEST_ACCOUNT_ID
			};
			Assert.Equal(nodeCreateTransaction.AccountId, TEST_ACCOUNT_ID);
		}

		public virtual void GetSetAccountIdFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.AccountId = TEST_ACCOUNT_ID);
		}

		public virtual void GetSetDescription()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				Description = TEST_DESCRIPTION
			};
			Assert.Equal(nodeCreateTransaction.Description, TEST_DESCRIPTION);
		}

		public virtual void GetSetDescriptionFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.Description = TEST_DESCRIPTION);
		}

		public virtual void GetSetGossipEndpoints()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GossipEndpoints = TEST_GOSSIP_ENDPOINTS
			};
			Assert.Equal(nodeCreateTransaction.GossipEndpoints, TEST_GOSSIP_ENDPOINTS);
		}

		public virtual void SetTestGossipEndpointsFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GossipEndpoints = TEST_GOSSIP_ENDPOINTS);
		}

		public virtual void GetSetServiceEndpoints()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				ServiceEndpoints = TEST_SERVICE_ENDPOINTS
			};
			Assert.Equal(nodeCreateTransaction.ServiceEndpoints, TEST_SERVICE_ENDPOINTS);
		}

		public virtual void GetSetServiceEndpointsFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.ServiceEndpoints = TEST_SERVICE_ENDPOINTS);
		}

		public virtual void GetSetGossipCaCertificate()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE
			};
			Assert.Equal(nodeCreateTransaction.GossipCaCertificate, TEST_GOSSIP_CA_CERTIFICATE);
		}

		public virtual void GetSetGossipCaCertificateFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE);
		}

		public virtual void GetSetGrpcCertificateHash()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH
			};
			Assert.Equal(nodeCreateTransaction.GrpcCertificateHash, TEST_GRPC_CERTIFICATE_HASH);
		}

		public virtual void GetSetGrpcCertificateHashFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH);
		}

		public virtual void GetSetAdminKey()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				AdminKey = TEST_ADMIN_KEY
			};
			Assert.Equal(nodeCreateTransaction.AdminKey, TEST_ADMIN_KEY);
		}

		public virtual void GetSetAdminKeyFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.AdminKey = TEST_ADMIN_KEY);
		}

		public virtual void GetSetDeclineReward()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				DeclineReward = true
			};
			Assert.True(nodeCreateTransaction.DeclineReward);
		}

		public virtual void GetSetDeclineRewardFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.DeclineReward = false);
		}

		public virtual void GetGrpcWebProxyEndpoint()
		{
			var nodeCreateTransaction = new NodeCreateTransaction
			{
				GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT
			};
			Assert.Equal(nodeCreateTransaction.GrpcWebProxyEndpoint, TEST_GRPC_WEB_PROXY_ENDPOINT);
		}

		public virtual void SetGrpcWebProxyEndpointRequiresFrozen()
		{
			var tx = SpawnTestTransaction();
			Assert.Throws<InvalidOperationException>(() => tx.GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT);
		}
	}
}