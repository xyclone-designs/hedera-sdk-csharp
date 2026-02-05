// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Assertj.Core.Api.AssertionsForClassTypes;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    public class NodeUpdateTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly long TEST_NODE_ID = 420;
        private static readonly AccountId TEST_ACCOUNT_ID = AccountId.FromString("0.6.9");
        private static readonly string TEST_DESCRIPTION = "Test description";
        private static readonly IList<Endpoint> TEST_GOSSIP_ENDPOINTS = List.Of(SpawnTestEndpointIpOnly((byte)0), SpawnTestEndpointIpOnly((byte)1), SpawnTestEndpointIpOnly((byte)2));
        private static readonly IList<Endpoint> TEST_SERVICE_ENDPOINTS = List.Of(SpawnTestEndpointIpOnly((byte)3), SpawnTestEndpointIpOnly((byte)4), SpawnTestEndpointIpOnly((byte)5), SpawnTestEndpointIpOnly((byte)6));
        private static readonly Endpoint TEST_GRPC_WEB_PROXY_ENDPOINT = SpawnTestEndpointDomainOnly((byte)3);
        private static readonly byte[] TEST_GOSSIP_CA_CERTIFICATE = new byte[]
        {
            0,
            1,
            2,
            3,
            4
        };
        private static readonly byte[] TEST_GRPC_CERTIFICATE_HASH = new byte[48]; // SHA-384 hash (48 bytes)
        private static readonly PublicKey TEST_ADMIN_KEY = PrivateKey.FromString("302e020100300506032b65700422042062c4b69e9f45a554e5424fb5a6fe5e6ac1f19ead31dc7718c2d980fd1f998d4b").GetPublicKey();
        readonly Instant TEST_VALID_START = Instant.OfEpochSecond(1554158542);
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        virtual void ShouldSerialize()
        {
            SnapshotMatcher.Expect(SpawnTestTransaction().ToString()).ToMatchSnapshot();
        }

        private static Endpoint SpawnTestEndpointIpOnly(byte offset)
        {
            return new Endpoint().SetAddress(new byte[] { 0x00, 0x01, 0x02, 0x03 }).SetPort(42 + offset);
        }

        private static Endpoint SpawnTestEndpointDomainOnly(byte offset)
        {
            return new Endpoint().SetDomainName(offset + "unit.test.com").SetPort(42 + offset);
        }

        private NodeUpdateTransaction SpawnTestTransaction()
        {
            return new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetNodeId(TEST_NODE_ID).SetAccountId(TEST_ACCOUNT_ID).SetDescription(TEST_DESCRIPTION).SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS).SetServiceEndpoints(TEST_SERVICE_ENDPOINTS).SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE).SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH).SetAdminKey(TEST_ADMIN_KEY).SetMaxTransactionFee(new Hbar(1)).SetDeclineReward(true).SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = NodeUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeUpdateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void TestUnrecognizedServicePort()
        {
            var tx = new NodeUpdateTransaction().SetServiceEndpoints(List.Of(new Endpoint().SetDomainName("unit.test.com").SetPort(50111)));
            var tx2 = NodeUpdateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        virtual void TestEmptyCertificates()
        {

            // Empty gRPC certificate hash is allowed (network validates it)
            // But empty gossip CA certificate should throw
            var tx = new NodeUpdateTransaction().SetGrpcCertificateHash(new byte[] { }).SetNodeId(0);
            var tx2Bytes = tx.ToBytes();
            NodeUpdateTransaction deserializedTx = (NodeUpdateTransaction)Transaction.FromBytes(tx2Bytes);
            Assert.Equal(deserializedTx.GetGrpcCertificateHash(), new byte[] { });
        }

        virtual void TestSetNull()
        {
            new NodeUpdateTransaction().SetDescription(null).SetAccountId(null).SetGrpcCertificateHash(null).SetAdminKey(null);
        }

        virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetNodeUpdate(NodeUpdateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<NodeUpdateTransaction>(tx);
        }

        virtual void ConstructNodeUpdateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = NodeUpdateTransactionBody.NewBuilder();
            transactionBodyBuilder.SetNodeId(TEST_NODE_ID);
            transactionBodyBuilder.SetAccountId(TEST_ACCOUNT_ID.ToProtobuf());
            transactionBodyBuilder.SetDescription(StringValue.Of(TEST_DESCRIPTION));
            foreach (Endpoint gossipEndpoint in TEST_GOSSIP_ENDPOINTS)
            {
                transactionBodyBuilder.AddGossipEndpoint(gossipEndpoint.ToProtobuf());
            }

            foreach (Endpoint serviceEndpoint in TEST_SERVICE_ENDPOINTS)
            {
                transactionBodyBuilder.AddServiceEndpoint(serviceEndpoint.ToProtobuf());
            }

            transactionBodyBuilder.SetGossipCaCertificate(BytesValue.Of(ByteString.CopyFrom(TEST_GOSSIP_CA_CERTIFICATE)));
            transactionBodyBuilder.SetGrpcCertificateHash(BytesValue.Of(ByteString.CopyFrom(TEST_GRPC_CERTIFICATE_HASH)));
            transactionBodyBuilder.SetAdminKey(TEST_ADMIN_KEY.ToProtobufKey());
            transactionBodyBuilder.SetDeclineReward(BoolValue.Of(true));
            var tx = TransactionBody.NewBuilder().SetNodeUpdate(transactionBodyBuilder.Build()).Build();
            var nodeUpdateTransaction = new NodeUpdateTransaction(tx);
            Assert.Equal(nodeUpdateTransaction.GetNodeId(), TEST_NODE_ID);
            Assert.Equal(nodeUpdateTransaction.GetAccountId(), TEST_ACCOUNT_ID);
            Assert.Equal(nodeUpdateTransaction.GetDescription(), TEST_DESCRIPTION);
            AssertThat(nodeUpdateTransaction.GetGossipEndpoints()).HasSize(TEST_GOSSIP_ENDPOINTS.Count);
            AssertThat(nodeUpdateTransaction.GetServiceEndpoints()).HasSize(TEST_SERVICE_ENDPOINTS.Count);
            Assert.Equal(nodeUpdateTransaction.GetGossipCaCertificate(), TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeUpdateTransaction.GetGrpcCertificateHash(), TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeUpdateTransaction.GetAdminKey(), TEST_ADMIN_KEY);
            Assert.Equal(nodeUpdateTransaction.GetDeclineReward(), true);
        }

        virtual void GetSetNodeId()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetNodeId(TEST_NODE_ID);
            Assert.Equal(nodeUpdateTransaction.GetNodeId(), TEST_NODE_ID);
        }

        virtual void GetSetNodeIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetNodeId(TEST_NODE_ID));
        }

        virtual void GetSetAccountId()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetAccountId(TEST_ACCOUNT_ID);
            Assert.Equal(nodeUpdateTransaction.GetAccountId(), TEST_ACCOUNT_ID);
        }

        virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAccountId(TEST_ACCOUNT_ID));
        }

        virtual void GetSetDescription()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetDescription(TEST_DESCRIPTION);
            Assert.Equal(nodeUpdateTransaction.GetDescription(), TEST_DESCRIPTION);
        }

        virtual void GetSetDescriptionFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetDescription(TEST_DESCRIPTION));
        }

        virtual void GetSetGossipEndpoints()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS);
            Assert.Equal(nodeUpdateTransaction.GetGossipEndpoints(), TEST_GOSSIP_ENDPOINTS);
        }

        virtual void SetTestGossipEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS));
        }

        virtual void GetSetServiceEndpoints()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetServiceEndpoints(TEST_SERVICE_ENDPOINTS);
            Assert.Equal(nodeUpdateTransaction.GetServiceEndpoints(), TEST_SERVICE_ENDPOINTS);
        }

        virtual void GetSetServiceEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetServiceEndpoints(TEST_SERVICE_ENDPOINTS));
        }

        virtual void GetSetGossipCaCertificate()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeUpdateTransaction.GetGossipCaCertificate(), TEST_GOSSIP_CA_CERTIFICATE);
        }

        virtual void GetSetGossipCaCertificateFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE));
        }

        virtual void GetSetGrpcCertificateHash()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeUpdateTransaction.GetGrpcCertificateHash(), TEST_GRPC_CERTIFICATE_HASH);
        }

        virtual void GetSetGrpcCertificateHashFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH));
        }

        virtual void GetSetAdminKey()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetAdminKey(TEST_ADMIN_KEY);
            Assert.Equal(nodeUpdateTransaction.GetAdminKey(), TEST_ADMIN_KEY);
        }

        virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetAdminKey(TEST_ADMIN_KEY));
        }

        virtual void GetSetDeclineReward()
        {
            var tx = new NodeUpdateTransaction().SetDeclineReward(true);
            Assert.Equal(tx.GetDeclineReward(), true);
        }

        virtual void GetSetDeclineRewardFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetDeclineReward(false));
        }

        virtual void GetGrpcWebProxyEndpoint()
        {
            var nodeUpdateTransaction = new NodeUpdateTransaction().SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT);
            Assert.Equal(nodeUpdateTransaction.GetGrpcWebProxyEndpoint(), TEST_GRPC_WEB_PROXY_ENDPOINT);
        }

        virtual void SetGrpcWebProxyEndpointRequiresFrozen()
        {
            var tx = SpawnTestTransaction();
            await Assert.ThrowsAsync<InvalidOperationException>(() => tx.SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT));
        }

        virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSet()
        {
            var transaction = new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START)).SetNodeId(TEST_NODE_ID);
            AssertThatCode(() => transaction.FreezeWith(null)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetNodeId(), TEST_NODE_ID);
        }

        virtual void ShouldThrowErrorWhenFreezingWithoutSettingNodeId()
        {
            var transaction = new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.GetMessage(), "NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        virtual void ShouldThrowErrorWhenFreezingWithZeroNodeId()
        {
            var transaction = new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.FreezeWith(null));
            Assert.Equal(exception.GetMessage(), "NodeUpdateTransaction: 'nodeId' must be explicitly set before calling freeze().");
        }

        virtual void ShouldFreezeSuccessfullyWithActualClientWhenNodeIdIsSet()
        {
            var transaction = new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START)).SetNodeId(TEST_NODE_ID);
            var mockClient = Client.ForTestnet();
            AssertThatCode(() => transaction.FreezeWith(mockClient)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetNodeId(), TEST_NODE_ID);
        }

        virtual void ShouldFreezeSuccessfullyWhenNodeIdIsSetWithAdditionalFields()
        {
            var transaction = new NodeUpdateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.3"))).SetTransactionId(TransactionId.WithValidStart(TEST_ACCOUNT_ID, TEST_VALID_START)).SetNodeId(TEST_NODE_ID).SetDescription(TEST_DESCRIPTION).SetAccountId(TEST_ACCOUNT_ID).SetDeclineReward(false);
            AssertThatCode(() => transaction.FreezeWith(null)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetNodeId(), TEST_NODE_ID);
            Assert.Equal(transaction.GetDescription(), TEST_DESCRIPTION);
            Assert.Equal(transaction.GetAccountId(), TEST_ACCOUNT_ID);
            Assert.Equal(transaction.GetDeclineReward(), false);
        }

        virtual void ShouldThrowErrorWhenGettingNodeIdWithoutSettingIt()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.GetNodeId());
            Assert.Equal(exception.GetMessage(), "NodeUpdateTransaction: 'nodeId' has not been set");
        }

        // ===== Validation Tests =====
        virtual void ShouldThrowErrorWhenSettingNegativeNodeId()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetNodeId(-1));
            Assert.Equal(exception.GetMessage(), "nodeId must be non-negative");
        }

        virtual void ShouldAllowSettingNodeIdToZero()
        {
            var transaction = new NodeUpdateTransaction().SetNodeId(0);
            Assert.Equal(transaction.GetNodeId(), 0);
        }

        virtual void ShouldThrowErrorWhenDescriptionExceeds100Bytes()
        {
            var transaction = new NodeUpdateTransaction();

            // Create a 101-byte UTF-8 string
            var longDescription = "a".Repeat(101);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetDescription(longDescription));
            Assert.Equal(exception.GetMessage(), "Description must not exceed 100 bytes when encoded as UTF-8");
        }

        virtual void ShouldAllowDescriptionWith100Bytes()
        {
            var transaction = new NodeUpdateTransaction();
            var description = "a".Repeat(100);
            AssertThatCode(() => transaction.SetDescription(description)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetDescription(), description);
        }

        virtual void ShouldAllowNullDescription()
        {
            var transaction = new NodeUpdateTransaction();
            AssertThatCode(() => transaction.SetDescription(null)).DoesNotThrowAnyException();
        }

        virtual void ShouldThrowErrorWhenSettingEmptyGossipEndpointsList()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGossipEndpoints(List.Of()));
            Assert.Equal(exception.GetMessage(), "Gossip endpoints list must not be empty");
        }

        virtual void ShouldThrowErrorWhenSettingMoreThan10GossipEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoints = List.Of(SpawnTestEndpointIpOnly((byte)0), SpawnTestEndpointIpOnly((byte)1), SpawnTestEndpointIpOnly((byte)2), SpawnTestEndpointIpOnly((byte)3), SpawnTestEndpointIpOnly((byte)4), SpawnTestEndpointIpOnly((byte)5), SpawnTestEndpointIpOnly((byte)6), SpawnTestEndpointIpOnly((byte)7), SpawnTestEndpointIpOnly((byte)8), SpawnTestEndpointIpOnly((byte)9), SpawnTestEndpointIpOnly((byte)10));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGossipEndpoints(endpoints));
            Assert.Equal(exception.GetMessage(), "Gossip endpoints list must not contain more than 10 entries");
        }

        virtual void ShouldAllowExactly10GossipEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoints = List.Of(SpawnTestEndpointIpOnly((byte)0), SpawnTestEndpointIpOnly((byte)1), SpawnTestEndpointIpOnly((byte)2), SpawnTestEndpointIpOnly((byte)3), SpawnTestEndpointIpOnly((byte)4), SpawnTestEndpointIpOnly((byte)5), SpawnTestEndpointIpOnly((byte)6), SpawnTestEndpointIpOnly((byte)7), SpawnTestEndpointIpOnly((byte)8), SpawnTestEndpointIpOnly((byte)9));
            AssertThatCode(() => transaction.SetGossipEndpoints(endpoints)).DoesNotThrowAnyException();
        }

        virtual void ShouldThrowErrorWhenGossipEndpointHasBothIpAndDomain()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoint = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetDomainName("example.com").SetPort(50211);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGossipEndpoints(List.Of(endpoint)));
            Assert.Equal(exception.GetMessage(), "Endpoint must not contain both ipAddressV4 and domainName");
        }

        virtual void ShouldThrowErrorWhenSettingEmptyServiceEndpointsList()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetServiceEndpoints(List.Of()));
            Assert.Equal(exception.GetMessage(), "Service endpoints list must not be empty");
        }

        virtual void ShouldThrowErrorWhenSettingMoreThan8ServiceEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoints = List.Of(SpawnTestEndpointIpOnly((byte)0), SpawnTestEndpointIpOnly((byte)1), SpawnTestEndpointIpOnly((byte)2), SpawnTestEndpointIpOnly((byte)3), SpawnTestEndpointIpOnly((byte)4), SpawnTestEndpointIpOnly((byte)5), SpawnTestEndpointIpOnly((byte)6), SpawnTestEndpointIpOnly((byte)7), SpawnTestEndpointIpOnly((byte)8));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetServiceEndpoints(endpoints));
            Assert.Equal(exception.GetMessage(), "Service endpoints list must not contain more than 8 entries");
        }

        virtual void ShouldAllowExactly8ServiceEndpoints()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoints = List.Of(SpawnTestEndpointIpOnly((byte)0), SpawnTestEndpointIpOnly((byte)1), SpawnTestEndpointIpOnly((byte)2), SpawnTestEndpointIpOnly((byte)3), SpawnTestEndpointIpOnly((byte)4), SpawnTestEndpointIpOnly((byte)5), SpawnTestEndpointIpOnly((byte)6), SpawnTestEndpointIpOnly((byte)7));
            AssertThatCode(() => transaction.SetServiceEndpoints(endpoints)).DoesNotThrowAnyException();
        }

        virtual void ShouldThrowErrorWhenServiceEndpointHasBothIpAndDomain()
        {
            var transaction = new NodeUpdateTransaction();
            var endpoint = new Endpoint().SetAddress(new byte[] { 127, 0, 0, 1 }).SetDomainName("example.com").SetPort(50212);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetServiceEndpoints(List.Of(endpoint)));
            Assert.Equal(exception.GetMessage(), "Endpoint must not contain both ipAddressV4 and domainName");
        }

        virtual void ShouldThrowErrorWhenSettingNullGossipCaCertificate()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGossipCaCertificate(null));
            Assert.Equal(exception.GetMessage(), "Gossip CA certificate must not be null or empty");
        }

        virtual void ShouldThrowErrorWhenSettingEmptyGossipCaCertificate()
        {
            var transaction = new NodeUpdateTransaction();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGossipCaCertificate(new byte[] { }));
            Assert.Equal(exception.GetMessage(), "Gossip CA certificate must not be null or empty");
        }

        virtual void ShouldAllowValidGossipCaCertificate()
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
            AssertThatCode(() => transaction.SetGossipCaCertificate(cert)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetGossipCaCertificate(), cert);
        }

        virtual void ShouldThrowErrorWhenSettingGrpcCertificateHashWithWrongSize()
        {
            var transaction = new NodeUpdateTransaction();
            var wrongSizeHash = new byte[32]; // SHA-256 size, but we need SHA-384 (48 bytes)
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => transaction.SetGrpcCertificateHash(wrongSizeHash));
            Assert.Equal(exception.GetMessage(), "gRPC certificate hash must be exactly 48 bytes (SHA-384)");
        }

        virtual void ShouldAllowGrpcCertificateHashWith48Bytes()
        {
            var transaction = new NodeUpdateTransaction();
            var validHash = new byte[48]; // SHA-384 size
            AssertThatCode(() => transaction.SetGrpcCertificateHash(validHash)).DoesNotThrowAnyException();
            Assert.Equal(transaction.GetGrpcCertificateHash(), validHash);
        }

        virtual void ShouldAllowNullGrpcCertificateHash()
        {
            var transaction = new NodeUpdateTransaction();
            AssertThatCode(() => transaction.SetGrpcCertificateHash(null)).DoesNotThrowAnyException();
        }

        virtual void ShouldAllowEmptyGrpcCertificateHash()
        {
            var transaction = new NodeUpdateTransaction();

            // Empty is allowed because network will validate it
            AssertThatCode(() => transaction.SetGrpcCertificateHash(new byte[] { })).DoesNotThrowAnyException();
        }
    }
}