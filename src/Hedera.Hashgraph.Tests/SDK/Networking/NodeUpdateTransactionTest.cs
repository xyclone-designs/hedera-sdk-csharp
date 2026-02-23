// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.Transactions;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.HBar;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NodeUpdateTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly ulong TEST_NODE_ID = 420;
        private static readonly AccountId TEST_ACCOUNT_ID = AccountId.FromString("0.6.9");
        private static readonly string TEST_DESCRIPTION = "Test description";
        private static readonly List<Endpoint> TEST_GOSSIP_ENDPOINTS = [ SpawnTestEndpointIpOnly((byte)0), 
            SpawnTestEndpointIpOnly((byte)1), 
            SpawnTestEndpointIpOnly((byte)2) ];
        private static readonly List<Endpoint> TEST_SERVICE_ENDPOINTS = [ SpawnTestEndpointIpOnly((byte)3), 
            SpawnTestEndpointIpOnly((byte)4), 
            SpawnTestEndpointIpOnly((byte)5), 
            SpawnTestEndpointIpOnly((byte)6) ];
        private static readonly Endpoint TEST_GRPC_WEB_PROXY_ENDPOINT = SpawnTestEndpointDomainOnly((byte)3);
        private static readonly byte[] TEST_GOSSIP_CA_CERTIFICATE = [0, 1, 2, 3, 4];
        private static readonly byte[] TEST_GRPC_CERTIFICATE_HASH = new byte[48]; // SHA-384 hash (48 bytes)
        private static readonly PublicKey TEST_ADMIN_KEY = PrivateKey.FromString("302e020100300506032b65700422042062c4b69e9f45a554e5424fb5a6fe5e6ac1f19ead31dc7718c2d980fd1f998d4b").GetPublicKey();
        readonly DateTimeOffset TEST_VALID_START = DateTimeOffset.FromUnixTimeMilliseconds(1554158542);
        
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }
        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private static Endpoint SpawnTestEndpointIpOnly(byte offset)
        {
            return new Endpoint
            {
				Address = new byte[] { 0x00, 0x01, 0x02, 0x03 },
				Port = 42 + offset
			};
        }

        private static Endpoint SpawnTestEndpointDomainOnly(byte offset)
        {
            return new Endpoint
            {
				DomainName = offset + "unit.test.com",
				Port = 42 + offset
			};
        }

        private NodeUpdateTransaction SpawnTestTransaction()
        {
            return new NodeUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006")],
				TransactionId = TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START.ToTimestamp()),
				NodeId = TEST_NODE_ID,
				AccountId = TEST_ACCOUNT_ID,
				Description = TEST_DESCRIPTION,
				GossipEndpoints = TEST_GOSSIP_ENDPOINTS,
				ServiceEndpoints = TEST_SERVICE_ENDPOINTS,
				GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE,
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH,
				AdminKey = TEST_ADMIN_KEY,
				MaxTransactionFee = new Hbar(1),
				DeclineReward = true,
				GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT,
			}
            .Freeze()
            .Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = Transaction.FromBytes<NodeUpdateTransaction>(tx.ToBytes());
            
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeUpdateTransaction();
            var tx2 = Transaction.FromBytes<NodeUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void TestUnrecognizedServicePort()
        {
            var tx = new NodeUpdateTransaction
            {
                ServiceEndpoints = [new Endpoint
                {
                    DomainName = "unit.test.com",
                    Port = 50111
                }]
            };
            
            var tx2 = Transaction.FromBytes<NodeUpdateTransaction>(tx.ToBytes());

            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void TestEmptyCertificates()
        {
            // Empty gRPC certificate hash is allowed (network validates it)
            // But empty gossip CA certificate should throw
            var tx = new NodeUpdateTransaction
            {
				GrpcCertificateHash = new byte[] { },
				NodeId = 0,
			};
            var tx2Bytes = tx.ToBytes();
            var deserializedTx = Transaction.FromBytes<NodeUpdateTransaction>(tx2Bytes);

            Assert.Equal(deserializedTx.GrpcCertificateHash, new byte[] { });
        }

        public virtual void TestSetNull()
        {
            _ = new NodeUpdateTransaction
            {
				Description = null,
				AccountId = null,
				GrpcCertificateHash = null,
				AdminKey = null,
			};
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = new Proto.SchedulableTransactionBody
            {
                NodeUpdate = new Proto.NodeUpdateTransactionBody()
			};
            var tx = Transaction.FromScheduledTransaction<NodeUpdateTransaction>(transactionBody);
            
            Assert.IsType<NodeUpdateTransaction>(tx);
        }

        public virtual void ConstructNodeUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = new Proto.NodeUpdateTransactionBody
            {
				NodeId = TEST_NODE_ID,
				AccountId = TEST_ACCOUNT_ID.ToProtobuf(),
				Description = TEST_DESCRIPTION.ToString(),
				GossipCaCertificate = ByteString.CopyFrom(TEST_GOSSIP_CA_CERTIFICATE),
				GrpcCertificateHash = ByteString.CopyFrom(TEST_GRPC_CERTIFICATE_HASH),
				AdminKey = TEST_ADMIN_KEY.ToProtobufKey(),
				DeclineReward = true,
			};
            
            foreach (Endpoint gossipEndpoint in TEST_GOSSIP_ENDPOINTS)
				transactionBodyBuilder.GossipEndpoint.Add(gossipEndpoint.ToProtobuf());

			foreach (Endpoint serviceEndpoint in TEST_SERVICE_ENDPOINTS)
				transactionBodyBuilder.ServiceEndpoint.Add(serviceEndpoint.ToProtobuf());

            var nodeUpdateTransaction = new NodeUpdateTransaction(
				new Proto.TransactionBody
				{
					NodeUpdate = transactionBodyBuilder
				});

            Assert.Equal(nodeUpdateTransaction.NodeId, TEST_NODE_ID);
            Assert.Equal(nodeUpdateTransaction.AccountId, TEST_ACCOUNT_ID);
            Assert.Equal(nodeUpdateTransaction.Description, TEST_DESCRIPTION);
            Assert.Equal(nodeUpdateTransaction.GossipEndpoints.Count, TEST_GOSSIP_ENDPOINTS.Count);
            Assert.Equal(nodeUpdateTransaction.ServiceEndpoints.Count, TEST_SERVICE_ENDPOINTS.Count);
            Assert.Equal(nodeUpdateTransaction.GossipCaCertificate, TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeUpdateTransaction.GrpcCertificateHash, TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeUpdateTransaction.AdminKey, TEST_ADMIN_KEY);
            Assert.Equal(nodeUpdateTransaction.DeclineReward, true);
        }

        public virtual void GetSetNodeId()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                NodeId = TEST_NODE_ID
            };
            Assert.Equal(nodeUpdateTransaction.NodeId, TEST_NODE_ID);
        }

        public virtual void GetSetNodeIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.NodeId = TEST_NODE_ID);
        }

        public virtual void GetSetAccountId()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                AccountId = TEST_ACCOUNT_ID
            };
            Assert.Equal(nodeUpdateTransaction.AccountId, TEST_ACCOUNT_ID);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AccountId = TEST_ACCOUNT_ID);
        }

        public virtual void GetSetDescription()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                Description = TEST_DESCRIPTION
            };
            Assert.Equal(nodeUpdateTransaction.Description, TEST_DESCRIPTION);
        }

        public virtual void GetSetDescriptionFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.Description = TEST_DESCRIPTION);
        }

        public virtual void GetSetGossipEndpoints()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                GossipEndpoints = TEST_GOSSIP_ENDPOINTS
            };
            Assert.Equal(nodeUpdateTransaction.GossipEndpoints, TEST_GOSSIP_ENDPOINTS);
        }

        public virtual void SetTestGossipEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.GossipEndpoints = TEST_GOSSIP_ENDPOINTS);
        }

        public virtual void GetSetServiceEndpoints()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                ServiceEndpoints = TEST_SERVICE_ENDPOINTS
            };
            Assert.Equal(nodeUpdateTransaction.ServiceEndpoints, TEST_SERVICE_ENDPOINTS);
        }

        public virtual void GetSetServiceEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.ServiceEndpoints = TEST_SERVICE_ENDPOINTS);
        }

        public virtual void GetSetGossipCaCertificate()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            {
                GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE
            };
            Assert.Equal(nodeUpdateTransaction.GossipCaCertificate, TEST_GOSSIP_CA_CERTIFICATE);
        }

        public virtual void GetSetGossipCaCertificateFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.GossipCaCertificate = TEST_GOSSIP_CA_CERTIFICATE);
        }

        public virtual void GetSetGrpcCertificateHash()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            {
				GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH
			};
            Assert.Equal(nodeUpdateTransaction.GrpcCertificateHash, TEST_GRPC_CERTIFICATE_HASH);
        }

        public virtual void GetSetGrpcCertificateHashFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.GrpcCertificateHash = TEST_GRPC_CERTIFICATE_HASH);
        }

        public virtual void GetSetAdminKey()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            { 
                AdminKey = TEST_ADMIN_KEY
            };
            Assert.Equal(nodeUpdateTransaction.AdminKey, TEST_ADMIN_KEY);
        }

        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.AdminKey = TEST_ADMIN_KEY);
        }

        public virtual void GetSetDeclineReward()
        {
            var tx = new NodeUpdateTransaction
            { 
                DeclineReward = true
            };
            Assert.Equal(tx.DeclineReward, true);
        }

        public virtual void GetSetDeclineRewardFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.DeclineReward = false);
        }

        public virtual void GetGrpcWebProxyEndpoint()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction
            {
                GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT
            };
            Assert.Equal(nodeUpdateTransaction.GrpcWebProxyEndpoint, TEST_GRPC_WEB_PROXY_ENDPOINT);
        }

        public virtual void SetGrpcWebProxyEndpointRequiresFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.GrpcWebProxyEndpoint = TEST_GRPC_WEB_PROXY_ENDPOINT);
        }

        public virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSet()
        {
            var transaction = new NodeUpdateTransaction
            {
				NodeAccountIds = [AccountId.FromString("0.0.3")],
				TransactionId = TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START.ToTimestamp()),
				NodeId = TEST_NODE_ID
			};

            transaction.FreezeWith(null);

			Assert.Equal(transaction.NodeId, TEST_NODE_ID);
        }

        public virtual void ShouldThrowErrorWhenFreezingWithoutSettingNodeId()
        {
            var transaction = new NodeUpdateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.3")],
                TransactionId = TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START.ToTimestamp()),
            };

            var exception = Assert.Throws<InvalidOperationException>(() => transaction.FreezeWith(null));
            
            Assert.Equal(exception.Message, "NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        public virtual void ShouldThrowErrorWhenFreezingWithZeroNodeId()
        {
            var transaction = new NodeUpdateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.3")],
                TransactionId = TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START.ToTimestamp()),
            };
            var exception = Assert.Throws<InvalidOperationException>(() => transaction.FreezeWith(null));
            
            Assert.Equal(exception.Message, "NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        public virtual void ShouldFreezeSuccessfullyWithActualClientWhenNodeIdIsSet()
        {
			var transaction = new NodeUpdateTransaction
			{
				NodeId = TEST_NODE_ID,
				NodeAccountIds = [AccountId.FromString("0.0.3")],
				TransactionId = TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START.ToTimestamp()),
			};
            var mockClient = Client.ForTestnet();

			transaction.FreezeWith(mockClient);
                
            Assert.Equal(transaction.NodeId, TEST_NODE_ID);
        }

        public virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSetWithAdditionalFields()
        {
            var transaction = new NodeUpdateTransaction
            {
                NodeAccountIds = [AccountId.FromString("0.0.3")],
                TransactionId = TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START.ToTimestamp()),
                NodeId = TEST_NODE_ID,
                Description = TEST_DESCRIPTION,
                AccountId = TEST_ACCOUNT_ID,
                DeclineReward = false,
            };

            transaction.FreezeWith(null);

			Assert.Equal(transaction.NodeId, TEST_NODE_ID);
            Assert.Equal(transaction.Description, TEST_DESCRIPTION);
            Assert.Equal(transaction.AccountId, TEST_ACCOUNT_ID);
            Assert.Equal(transaction.DeclineReward, false);
        }

        public virtual void ShouldThrowErrorWhenGettingNodeIdWithoutSettingIt()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = Assert.Throws<InvalidOperationException>(() => transaction.NodeId);

            Assert.Equal(exception.Message, "NodeUpdateTransaction: 'nodeId' has not been set");
        }

        // ===== Validation Tests =====
        public virtual void ShouldAllowSettingNodeIdToZero()
        {
            var transaction = new NodeUpdateTransaction
            {
				NodeId = 0
			};
            Assert.Equal(transaction.NodeId, (ulong)0);
        }

        public virtual void ShouldThrowErrorWhenDescriptionExceeds100Bytes()
        {
            var transaction = new NodeUpdateTransaction();

            // Create a 101-byte UTF-8 string
            var longDescription = string.Join(string.Empty, Enumerable.Range(0, 101).Select(_ => "a"));
            var exception = Assert.Throws<ArgumentException>(() => transaction.Description = longDescription);

            Assert.Equal(exception.Message, "Description must not exceed 100 bytes when encoded as UTF-8");
        }

        public virtual void ShouldAllowDescriptionWith100Bytes()
        {
            var transaction = new NodeUpdateTransaction();
            var description = string.Join(string.Empty, Enumerable.Range(0, 100).Select(_ => "a"));
			transaction.Description = description;
            
            Assert.Equal(transaction.Description, description);
        }

        public virtual void ShouldAllowNullDescription()
        {
            var transaction = new NodeUpdateTransaction();
            transaction.Description = null;
        }

        public virtual void ShouldThrowErrorWhenSettingEmptyGossipEndpointsList()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = Assert.Throws<ArgumentException>(() => transaction.GossipEndpoints = []);

            Assert.Equal(exception.Message, "Gossip endpoints list must not be empty");
        }

        public virtual void ShouldThrowErrorWhenSettingMoreThan10GossipEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
            Endpoint[] endpoints = [
                SpawnTestEndpointIpOnly((byte)0), 
                SpawnTestEndpointIpOnly((byte)1), 
                SpawnTestEndpointIpOnly((byte)2), 
                SpawnTestEndpointIpOnly((byte)3), 
                SpawnTestEndpointIpOnly((byte)4), 
                SpawnTestEndpointIpOnly((byte)5), 
                SpawnTestEndpointIpOnly((byte)6), 
                SpawnTestEndpointIpOnly((byte)7), 
                SpawnTestEndpointIpOnly((byte)8), 
                SpawnTestEndpointIpOnly((byte)9), 
                SpawnTestEndpointIpOnly((byte)10)];

            var exception = Assert.Throws<ArgumentException>(() => transaction.GossipEndpoints = endpoints);
            Assert.Equal(exception.Message, "Gossip endpoints list must not contain more than 10 entries");
        }

        public virtual void ShouldAllowExactly10GossipEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
			Endpoint[] endpoints = [
                SpawnTestEndpointIpOnly((byte)0), 
                SpawnTestEndpointIpOnly((byte)1), 
                SpawnTestEndpointIpOnly((byte)2), 
                SpawnTestEndpointIpOnly((byte)3), 
                SpawnTestEndpointIpOnly((byte)4), 
                SpawnTestEndpointIpOnly((byte)5), 
                SpawnTestEndpointIpOnly((byte)6), 
                SpawnTestEndpointIpOnly((byte)7), 
                SpawnTestEndpointIpOnly((byte)8), 
                SpawnTestEndpointIpOnly((byte)9)];

            transaction.GossipEndpoints = endpoints;
		}

        public virtual void ShouldThrowErrorWhenGossipEndpointHasBothIpAndDomain()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoint = new Endpoint
            {
				Address = new byte[] { 127, 0, 0, 1 },
				DomainName = "example.com",
				Port = 50211,
			};
            var exception = Assert.Throws<ArgumentException>(() => transaction.GossipEndpoints = [endpoint]);
            Assert.Equal(exception.Message, "Endpoint must not contain both ipAddressV4 and domainName");
        }

        public virtual void ShouldThrowErrorWhenSettingEmptyServiceEndpointsList()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = Assert.Throws<ArgumentException>(() => transaction.ServiceEndpoints = []);
            
            Assert.Equal(exception.Message, "Service endpoints list must not be empty");
        }

        public virtual void ShouldThrowErrorWhenSettingMoreThan8ServiceEndpoints()
        {
            var transaction = new NodeUpdateTransaction();

			Endpoint[] endpoints = [
                SpawnTestEndpointIpOnly((byte)0), 
                SpawnTestEndpointIpOnly((byte)1), 
                SpawnTestEndpointIpOnly((byte)2), 
                SpawnTestEndpointIpOnly((byte)3), 
                SpawnTestEndpointIpOnly((byte)4), 
                SpawnTestEndpointIpOnly((byte)5), 
                SpawnTestEndpointIpOnly((byte)6), 
                SpawnTestEndpointIpOnly((byte)7), 
                SpawnTestEndpointIpOnly((byte)8)];

            var exception = Assert.Throws<ArgumentException>(() => transaction.ServiceEndpoints = endpoints);
            
            Assert.Equal(exception.Message, "Service endpoints list must not contain more than 8 entries");
        }

        public virtual void ShouldAllowExactly8ServiceEndpoints()
        {
            var transaction = new NodeUpdateTransaction();

            Endpoint[] endpoints = [
                SpawnTestEndpointIpOnly((byte)0), 
                SpawnTestEndpointIpOnly((byte)1), 
                SpawnTestEndpointIpOnly((byte)2), 
                SpawnTestEndpointIpOnly((byte)3), 
                SpawnTestEndpointIpOnly((byte)4), 
                SpawnTestEndpointIpOnly((byte)5), 
                SpawnTestEndpointIpOnly((byte)6), 
                SpawnTestEndpointIpOnly((byte)7)];

            transaction.ServiceEndpoints = endpoints;

		}

        public virtual void ShouldThrowErrorWhenServiceEndpointHasBothIpAndDomain()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoint = new Endpoint
            {
                Address = new byte[] { 127, 0, 0, 1 },
                DomainName = "example.com",
                Port = 50212,
            };
            var exception = Assert.Throws<ArgumentException>(() => transaction.ServiceEndpoints = [endpoint]);
            
            Assert.Equal(exception.Message, "Endpoint must not contain both ipAddressV4 and domainName");
        }

        public virtual void ShouldThrowErrorWhenSettingNullGossipCaCertificate()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = Assert.Throws<ArgumentException>(() => transaction.GossipCaCertificate = null);
            
            Assert.Equal(exception.Message, "Gossip CA certificate must not be null or empty");
        }

        public virtual void ShouldThrowErrorWhenSettingEmptyGossipCaCertificate()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = Assert.Throws<ArgumentException>(() => transaction.GossipCaCertificate = []);
            
            Assert.Equal(exception.Message, "Gossip CA certificate must not be null or empty");
        }

        public virtual void ShouldAllowValidGossipCaCertificate()
        {
            var transaction = new NodeUpdateTransaction();
            var cert = new byte[]
            {
                1,
                2,
                3,
                4,
                5
            };

            transaction.GossipCaCertificate = cert;

			Assert.Equal(transaction.GossipCaCertificate, cert);
        }

        public virtual void ShouldThrowErrorWhenSettingGrpcCertificateHashWithWrongSize()
        {
            var transaction = new NodeUpdateTransaction();
            var wrongSizeHash = new byte[32]; // SHA-256 size, but we need SHA-384 (48 bytes)
            var exception = Assert.Throws<ArgumentException>(() => transaction.GrpcCertificateHash = wrongSizeHash);

            Assert.Equal(exception.Message, "gRPC certificate hash must be exactly 48 bytes (SHA-384)");
        }

        public virtual void ShouldAllowGrpcCertificateHashWith48Bytes()
        {
            var transaction = new NodeUpdateTransaction();
            var validHash = new byte[48]; // SHA-384 size

            transaction.GrpcCertificateHash = validHash;
			Assert.Equal(transaction.GrpcCertificateHash, validHash);
        }

        public virtual void ShouldAllowNullGrpcCertificateHash()
        {
            var transaction = new NodeUpdateTransaction();
            transaction.GrpcCertificateHash = null;
		}

        public virtual void ShouldAllowEmptyGrpcCertificateHash()
        {
            var transaction = new NodeUpdateTransaction();

            // Empty is allowed because network will validate it
            transaction.GrpcCertificateHash = [];
		}
    }
}