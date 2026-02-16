// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Org.Junit.Jupiter.Api.Assertions;
using Com.Google.Protobuf;
using Proto;
using Io.Github.JsonSnapshot;
using Java.Time;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Networking
{
    public class NodeCreateTransactionTest
    {
        private static readonly PrivateKey TEST_PRIVATE_KEY = PrivateKey.FromString("302e020100300506032b657004220420db484b828e64b2d8f12ce3c0a0e93a0b8cce7af1bb8f39c97732394482538e10");
        private static readonly AccountId TEST_ACCOUNT_ID = AccountId.FromString("0.6.9");
        private static readonly string TEST_DESCRIPTION = "Test description";
        private static readonly IList<Endpoint> TEST_GOSSIP_ENDPOINTS = List.Of(SpawnTestEndpoint((byte)0), SpawnTestEndpoint((byte)1), SpawnTestEndpoint((byte)2));
        private static readonly IList<Endpoint> TEST_SERVICE_ENDPOINTS = List.Of(SpawnTestEndpoint((byte)3), SpawnTestEndpoint((byte)4), SpawnTestEndpoint((byte)5), SpawnTestEndpoint((byte)6));
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

        private static Endpoint SpawnTestEndpoint(byte offset)
        {

            // Valid endpoint: use domainName only to comply with SDK validation
            return new Endpoint().SetDomainName(offset + "unit.test.com").SetPort(42 + offset);
        }

        private NodeCreateTransaction SpawnTestTransaction()
        {
            return new NodeCreateTransaction().SetNodeAccountIds(Arrays.AsList(AccountId.FromString("0.0.5005"), AccountId.FromString("0.0.5006"))).SetTransactionId(TransactionId.WithValidStart(AccountId.FromString("0.0.5006"), TEST_VALID_START)).SetAccountId(TEST_ACCOUNT_ID).SetDescription(TEST_DESCRIPTION).SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS).SetServiceEndpoints(TEST_SERVICE_ENDPOINTS).SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE).SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH).SetAdminKey(TEST_ADMIN_KEY).SetMaxTransactionFee(new Hbar(1)).SetDeclineReward(false).SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT).Freeze().Sign(TEST_PRIVATE_KEY);
        }

        public virtual void ShouldBytes()
        {
            var tx = SpawnTestTransaction();
            var tx2 = NodeCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void FromScheduledTransaction()
        {
            var transactionBody = SchedulableTransactionBody.NewBuilder().SetNodeCreate(NodeCreateTransactionBody.NewBuilder().Build()).Build();
            var tx = Transaction.FromScheduledTransaction(transactionBody);
            Assert.IsType<NodeCreateTransaction>(tx);
        }

        public virtual void ShouldBytesNoSetters()
        {
            var tx = new NodeCreateTransaction();
            var tx2 = Transaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void TestUnrecognizedServicePort()
        {
            var tx = new NodeCreateTransaction().SetServiceEndpoints(List.Of(new Endpoint().SetDomainName("unit.test.com").SetPort(50111)));
            var tx2 = NodeCreateTransaction.FromBytes(tx.ToBytes());
            Assert.Equal(tx2.ToString(), tx.ToString());
        }

        public virtual void TestSetNull()
        {
            new NodeCreateTransaction().SetDescription(null).SetAccountId(null).SetGossipCaCertificate(null).SetGrpcCertificateHash(null).SetAdminKey(null);
        }

        public virtual void SetDescriptionRejectsOver100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string tooLong = "a".Repeat(101);
            Assert.Throws<ArgumentException>(() => tx.SetDescription(tooLong));
        }

        public virtual void SetDescriptionAcceptsExactly100Utf8Bytes()
        {
            var tx = new NodeCreateTransaction();
            string exact = "a".Repeat(100);
            tx.SetDescription(exact);
            Assert.Equal(tx.GetDescription(), exact);
        }

        public virtual void SetGossipEndpointsRejectsMoreThan10()
        {
            var tx = new NodeCreateTransaction();
            var endpoints = new List<Endpoint>();
            for (int i = 0; i < 11; i++)
            {
                endpoints.Add(new Endpoint().SetDomainName("gossip" + i + ".test").SetPort(5000 + i));
            }

            Assert.Throws<ArgumentException>(() => tx.SetGossipEndpoints(endpoints));
        }

        public virtual void AddGossipEndpointRejectsMoreThan10()
        {
            var tx = new NodeCreateTransaction();
            for (int i = 0; i < 10; i++)
            {
                tx.AddGossipEndpoint(new Endpoint().SetDomainName("gossip" + i + ".test").SetPort(5000 + i));
            }

            Assert.Throws<ArgumentException>(() => tx.AddGossipEndpoint(new Endpoint().SetDomainName("gossipX.test").SetPort(6000)));
        }

        public virtual void SetGossipEndpointsRejectsIpAndDomainTogether()
        {
            var tx = new NodeCreateTransaction();
            var invalid = new Endpoint().SetAddress(new byte[] { 1, 2, 3, 4 }).SetDomainName("both.test").SetPort(5000);
            Assert.Throws<ArgumentException>(() => tx.SetGossipEndpoints(List.Of(invalid)));
        }

        public virtual void SetServiceEndpointsRejectsMoreThan8()
        {
            var tx = new NodeCreateTransaction();
            var endpoints = new List<Endpoint>();
            for (int i = 0; i < 9; i++)
            {
                endpoints.Add(new Endpoint().SetDomainName("svc" + i + ".test").SetPort(6000 + i));
            }

            Assert.Throws<ArgumentException>(() => tx.SetServiceEndpoints(endpoints));
        }

        public virtual void AddServiceEndpointRejectsMoreThan8()
        {
            var tx = new NodeCreateTransaction();
            for (int i = 0; i < 8; i++)
            {
                tx.AddServiceEndpoint(new Endpoint().SetDomainName("svc" + i + ".test").SetPort(7000 + i));
            }

            Assert.Throws<ArgumentException>(() => tx.AddServiceEndpoint(new Endpoint().SetDomainName("svcX.test").SetPort(8000)));
        }

        public virtual void SetServiceEndpointsRejectsIpAndDomainTogether()
        {
            var tx = new NodeCreateTransaction();
            var invalid = new Endpoint().SetAddress(new byte[] { 5, 6, 7, 8 }).SetDomainName("both.test").SetPort(6000);
            Assert.Throws<ArgumentException>(() => tx.SetServiceEndpoints(List.Of(invalid)));
        }

        public virtual void SetGossipCaCertificateRejectsEmpty()
        {
            var tx = new NodeCreateTransaction();
            Assert.Throws<ArgumentException>(() => tx.SetGossipCaCertificate(new byte[] { }));
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
            Endpoint gossipFqdnOnly = new Endpoint().SetDomainName("fqdn.example.com").SetPort(50211);
            Endpoint serviceIpOnly = new Endpoint().SetAddress(serviceIp).SetPort(50211);
            var tx = new NodeCreateTransaction().SetGossipEndpoints(List.Of(gossipFqdnOnly)).SetServiceEndpoints(List.Of(serviceIpOnly));
            var body = tx.Build().Build();
            var rewritten = body.GetGossipEndpoint(0);

            // gossip endpoint should now carry IP and no domain
            org.assertj.core.api.Assertions.Assert.Equal(rewritten.GetIpAddressV4().ToByteArray(), serviceIp);
            org.assertj.core.api.Assertions.Assert.Empty(rewritten.GetDomainName());
            org.assertj.core.api.Assertions.Assert.Equal(rewritten.GetPort(), 50211);
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
            Endpoint gossipIpOnly = new Endpoint().SetAddress(originalIp).SetPort(50212);
            byte[] serviceIp = new byte[]
            {
                10,
                0,
                0,
                2
            };
            Endpoint serviceIpOnly = new Endpoint().SetAddress(serviceIp).SetPort(50211);
            var tx = new NodeCreateTransaction().SetGossipEndpoints(List.Of(gossipIpOnly)).SetServiceEndpoints(List.Of(serviceIpOnly));
            var body = tx.Build().Build();
            var ge = body.GetGossipEndpoint(0);
            org.assertj.core.api.Assertions.Assert.Equal(ge.GetIpAddressV4().ToByteArray(), originalIp);
            org.assertj.core.api.Assertions.Assert.Equal(ge.GetPort(), 50212);
        }

        public virtual void BuildDoesNotRewriteWhenNoServiceIpAvailable()
        {
            Endpoint gossipFqdnOnly = new Endpoint().SetDomainName("fqdn.example.com").SetPort(50213);
            Endpoint serviceFqdnOnly = new Endpoint().SetDomainName("svc.example.com").SetPort(50211);
            var tx = new NodeCreateTransaction().SetGossipEndpoints(List.Of(gossipFqdnOnly)).SetServiceEndpoints(List.Of(serviceFqdnOnly));
            var body = tx.Build().Build();
            var ge = body.GetGossipEndpoint(0);
            org.assertj.core.api.Assertions.AssertThat(ge.GetIpAddressV4().IsEmpty()).IsTrue();
            org.assertj.core.api.Assertions.Assert.Equal(ge.GetDomainName(), "fqdn.example.com");
            org.assertj.core.api.Assertions.Assert.Equal(ge.GetPort(), 50213);
        }

        public virtual void ConstructNodeCreateTransactionFromTransactionBodyProtobuf()
        {
            var transactionBodyBuilder = NodeCreateTransactionBody.NewBuilder();
            transactionBodyBuilder.SetAccountId(TEST_ACCOUNT_ID.ToProtobuf());
            transactionBodyBuilder.SetDescription(TEST_DESCRIPTION);
            foreach (Endpoint gossipEndpoint in TEST_GOSSIP_ENDPOINTS)
            {
                transactionBodyBuilder.AddGossipEndpoint(gossipEndpoint.ToProtobuf());
            }

            foreach (Endpoint serviceEndpoint in TEST_SERVICE_ENDPOINTS)
            {
                transactionBodyBuilder.AddServiceEndpoint(serviceEndpoint.ToProtobuf());
            }

            transactionBodyBuilder.SetGossipCaCertificate(ByteString.CopyFrom(TEST_GOSSIP_CA_CERTIFICATE));
            transactionBodyBuilder.SetGrpcCertificateHash(ByteString.CopyFrom(TEST_GRPC_CERTIFICATE_HASH));
            transactionBodyBuilder.SetAdminKey(TEST_ADMIN_KEY.ToProtobufKey());
            transactionBodyBuilder.SetDeclineReward(true);
            var tx = TransactionBody.NewBuilder().SetNodeCreate(transactionBodyBuilder.Build()).Build();
            var nodeCreateTransaction = new NodeCreateTransaction(tx);
            Assert.Equal(nodeCreateTransaction.GetAccountId(), TEST_ACCOUNT_ID);
            Assert.Equal(nodeCreateTransaction.GetDescription(), TEST_DESCRIPTION);
            AssertThat(nodeCreateTransaction.GetGossipEndpoints()).HasSize(TEST_GOSSIP_ENDPOINTS.Count);
            AssertThat(nodeCreateTransaction.GetServiceEndpoints()).HasSize(TEST_SERVICE_ENDPOINTS.Count);
            Assert.Equal(nodeCreateTransaction.GetGossipCaCertificate(), TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeCreateTransaction.GetGrpcCertificateHash(), TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeCreateTransaction.GetAdminKey(), TEST_ADMIN_KEY);
        }

        public virtual void GetSetAccountId()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetAccountId(TEST_ACCOUNT_ID);
            Assert.Equal(nodeCreateTransaction.GetAccountId(), TEST_ACCOUNT_ID);
        }

        public virtual void GetSetAccountIdFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAccountId(TEST_ACCOUNT_ID));
        }

        public virtual void GetSetDescription()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetDescription(TEST_DESCRIPTION);
            Assert.Equal(nodeCreateTransaction.GetDescription(), TEST_DESCRIPTION);
        }

        public virtual void GetSetDescriptionFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetDescription(TEST_DESCRIPTION));
        }

        public virtual void GetSetGossipEndpoints()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS);
            Assert.Equal(nodeCreateTransaction.GetGossipEndpoints(), TEST_GOSSIP_ENDPOINTS);
        }

        public virtual void SetTestGossipEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetGossipEndpoints(TEST_GOSSIP_ENDPOINTS));
        }

        public virtual void GetSetServiceEndpoints()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetServiceEndpoints(TEST_SERVICE_ENDPOINTS);
            Assert.Equal(nodeCreateTransaction.GetServiceEndpoints(), TEST_SERVICE_ENDPOINTS);
        }

        public virtual void GetSetServiceEndpointsFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetServiceEndpoints(TEST_SERVICE_ENDPOINTS));
        }

        public virtual void GetSetGossipCaCertificate()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE);
            Assert.Equal(nodeCreateTransaction.GetGossipCaCertificate(), TEST_GOSSIP_CA_CERTIFICATE);
        }

        public virtual void GetSetGossipCaCertificateFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetGossipCaCertificate(TEST_GOSSIP_CA_CERTIFICATE));
        }

        public virtual void GetSetGrpcCertificateHash()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH);
            Assert.Equal(nodeCreateTransaction.GetGrpcCertificateHash(), TEST_GRPC_CERTIFICATE_HASH);
        }

        public virtual void GetSetGrpcCertificateHashFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetGrpcCertificateHash(TEST_GRPC_CERTIFICATE_HASH));
        }

        public virtual void GetSetAdminKey()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetAdminKey(TEST_ADMIN_KEY);
            Assert.Equal(nodeCreateTransaction.GetAdminKey(), TEST_ADMIN_KEY);
        }

        public virtual void GetSetAdminKeyFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetAdminKey(TEST_ADMIN_KEY));
        }

        public virtual void GetSetDeclineReward()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetDeclineReward(true);
            AssertThat(nodeCreateTransaction.GetDeclineReward()).IsTrue();
        }

        public virtual void GetSetDeclineRewardFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetDeclineReward(false));
        }

        public virtual void GetGrpcWebProxyEndpoint()
        {
            var nodeCreateTransaction = new NodeCreateTransaction().SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT);
            Assert.Equal(nodeCreateTransaction.GetGrpcWebProxyEndpoint(), TEST_GRPC_WEB_PROXY_ENDPOINT);
        }

        public virtual void SetGrpcWebProxyEndpointRequiresFrozen()
        {
            var tx = SpawnTestTransaction();
            Assert.Throws<InvalidOperationException>(() => tx.SetGrpcWebProxyEndpoint(TEST_GRPC_WEB_PROXY_ENDPOINT));
        }
    }
}