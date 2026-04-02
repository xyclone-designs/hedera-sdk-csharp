// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Threading;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Networking;
using Hedera.Hashgraph.SDK.HBar;
using System.Linq;
using Hedera.Hashgraph.SDK.Exceptions;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NodeUpdateTransactionIntegrationTest
    {
        [Fact]
        public virtual void CanExecuteNodeUpdateTransaction()
        {

            // Set the network
            var network = new Dictionary<string, AccountId>
            {
                { "localhost:50211", new AccountId(0, 0, 3) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{
                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Set up grpcWebProxyEndpoint address
                var grpcWebProxyEndpoint = new Endpoint
                {
					DomainName = "testWebUpdated.com",
					Port = 123456,
				};
                var response = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					DeclineReward = true,
					GrpcWebProxyEndpoint = grpcWebProxyEndpoint,
				
                }.Execute(client);
                response.GetReceipt(client);
            }
        }
        [Fact]
        public virtual void CanDeleteGrpcWebProxyEndpoint()
        {

            // Set the network
            var network = new Dictionary<string, AccountId>
			{
				{ "localhost:50211", new AccountId(0, 0, 3) }
			};
			using (var client = Client.ForNetwork(network, client =>
			{
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{
                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);
                var response = new NodeUpdateTransaction 
                {
					NodeId = 0
				
                }.DeleteGrpcWebProxyEndpoint().Execute(client);
                response.GetReceipt(client);
            }
        }

        [Fact]// ================== hip-1299 changing node account ID (dab) tests ==================
        public virtual void ShouldSucceedWhenUpdatingNodeAccountIdWithProperSignatures()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
            using (var client = Client.ForNetwork(network, client =>
            {
                client.TransportSecurity = false;
                client.MirrorNetwork_.Network = ["localhost:5600"];
            }))
            {

                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: A node with an existing account ID (0.0.3)
                // First, create a new account that will be used as the new node account ID
                var newAccountKey = PrivateKey.GenerateED25519();
                var newAccountId = CreateAccount(client, newAccountKey.GetPublicKey(), Hbar.From(10));

                // Create a node admin key
                var nodeAdminKey = PrivateKey.GenerateED25519();

                // When: A NodeUpdateTransaction is submitted to change the account ID
                var nodeUpdateTransaction = new NodeUpdateTransaction
                {
					NodeId = 0,
					AccountId = newAccountId,
					AdminKey = nodeAdminKey.GetPublicKey(),
					MaxTransactionFee = Hbar.From(10),
					TransactionMemo = "Update node account ID for DAB testing",
				};

                // Sign with both the node admin key and the new account key
                nodeUpdateTransaction.FreezeWith(client).Sign(nodeAdminKey).Sign(newAccountKey);

				// Then: The transaction should succeed
				var response = nodeUpdateTransaction.Execute(client);
				Assert.NotNull(response);

				// Verify the transaction was successful by checking the receipt
				var receipt = response.GetReceipt(client);
				Assert.Equal(receipt.Status, ResponseStatus.Success);
			}
        }
        [Fact]
        public virtual void TestNodeUpdateTransactionCanChangeToSameAccount()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
            using (var client = Client.ForNetwork(network, client =>
            {
                client.TransportSecurity = false;
                client.MirrorNetwork_.Network = ["localhost:5600"];
            }))
            {

                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: A node with an existing account ID (0.0.3)
                // When: A NodeUpdateTransaction is submitted to change the account ID to the same account (0.0.3)
                var resp = new NodeUpdateTransaction
                {
                    NodeId = 0,
                    Description = "testUpdated",
                    AccountId = new AccountId(0, 0, 3),
                    NodeAccountIds = [new AccountId(0, 0, 3)],

                }.Execute(client);

                // Then: The transaction should succeed
                resp.ValidateStatus = true;
                var receipt = resp.GetReceipt(client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);
            }
        }
        [Fact]
        public virtual void TestNodeUpdateTransactionCanChangeNodeAccountUpdateAddressbookAndRetry()
        {

            // Set the network
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.TransportSecurity = false;
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{

                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Create the account that will be the node account id
                var newNodeAccountID = CreateAccount(client, originalOperatorKey.GetPublicKey(), Hbar.From(1));
                Assert.NotNull(newNodeAccountID);

                // Update node account id (0.0.3 -> newNodeAccountID)
                var resp = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = newNodeAccountID,

				}.Execute(client);

                Console.WriteLine("Transaction node: " + resp.NodeId);
                Console.WriteLine("Receipt query nodes: " + resp.GetReceiptQuery().NodeAccountIds);
                Console.WriteLine("Client network: " + client.Network_);
                
                resp.ValidateStatus = true;
                var receipt = resp.GetReceipt(client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);

                // Wait for mirror node to import data
                Thread.Sleep(10000);

                // Submit to node 3 and node 4
                // Node 3 will fail with INVALID_NODE_ACCOUNT (because it now uses newNodeAccountID)
                // The SDK should automatically:
                // 1. Detect INVALID_NODE_ACCOUNT error
                // 2. Advance to next node
                // 3. Update the address book asynchronously
                // 4. Mark node 3 as unhealthy
                // 5. Retry with node 4 which should succeed
                ExecuteAccountCreate(client, [new AccountId(0, 0, 3), new AccountId(0, 0, 4)]);

                // This transaction should succeed using the updated node account ID
                ExecuteAccountCreate(client, [ newNodeAccountID ]);

                // Revert the node account id (newNodeAccountID -> 0.0.3)
                resp = new NodeUpdateTransaction
                {
					NodeId = 0,
					NodeAccountIds = [newNodeAccountID],
					Description = "testUpdated",
					AccountId = new AccountId(0, 0, 3),
				
                }.Execute(client);
                
                resp.ValidateStatus = true;
                receipt = resp.GetReceipt(client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);
            }
        }
        [Fact]
        public virtual void TestNodeUpdateTransactionFailsWithInvalidSignatureWhenMissingNodeAdminSignature()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.TransportSecurity = false;
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{

                // Set the operator to be account 0.0.2 initially to create a new account
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: Create a new operator account without node admin privileges
                var newOperatorKey = PrivateKey.GenerateED25519();
                var newOperatorAccountId = CreateAccount(client, newOperatorKey.GetPublicKey(), Hbar.From(2));

                // Switch to the new operator (who doesn't have node admin privileges)
                client.OperatorSet(newOperatorAccountId, newOperatorKey);

                // When: A NodeUpdateTransaction is submitted without node admin signature
                // (only has the new operator's signature, which is not sufficient)
                var nodeUpdateTransaction = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = new AccountId(0, 0, 3),
					NodeAccountIds = [new AccountId(0, 0, 3)],
				};

				// Then: The transaction should fail with INVALID_SIGNATURE
				ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
				{
					var response = nodeUpdateTransaction.Execute(client);
					response.GetReceipt(client);
				});

				Assert.Contains(exception.Message, "INVALID_SIGNATURE");
				Assert.Equal(exception.Receipt.Status, ResponseStatus.InvalidSignature);
            }
        }
        [Fact]
        public virtual void TestNodeUpdateTransactionFailsWithInvalidSignatureWhenMissingAccountIdSignature()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.TransportSecurity = false;
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{

                // Set the operator to be account 0.0.2 (has node admin privileges)
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: Create a new account that will be used as the new node account ID
                var newAccountKey = PrivateKey.GenerateED25519();
                var newAccountId = CreateAccount(client, newAccountKey.GetPublicKey(), Hbar.From(2));

                // When: A NodeUpdateTransaction is submitted with node admin signature
                // but WITHOUT the new account ID's signature
                var nodeUpdateTransaction = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = newAccountId,
					NodeAccountIds = [new AccountId(0, 0, 3)]
				};

                // Note: The operator (0.0.2) has node admin privileges, so the transaction
                // is automatically signed with the operator's key (node admin signature).
                // However, we're NOT signing with newAccountKey, which is required.
                // Then: The transaction should fail with INVALID_SIGNATURE
                ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
                {
                    var response = nodeUpdateTransaction.Execute(client);
                    response.GetReceipt(client);
                });

				Assert.Contains(exception.Message, "INVALID_SIGNATURE");
				Assert.Equal(exception.Receipt.Status, ResponseStatus.InvalidSignature);
            }
        }

        [Fact]// TODO - currently the test fails because returned status is ResponseStatus.InvalidSignature
        public virtual void TestNodeUpdateTransactionFailsWithInvalidAccountIdForNonExistentAccount()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.TransportSecurity = false;
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{

                // Set the operator to be account 0.0.2 (has admin privileges)
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: A node with an existing account ID (0.0.3)
                // When: A NodeUpdateTransaction is submitted to change to a non-existent account (0.0.9999999)
                var nodeUpdateTransaction = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = new AccountId(0, 0, 9999999),
					NodeAccountIds = [new AccountId(0, 0, 3)]
				};

				// Then: The transaction should fail with INVALID_NODE_ACCOUNT_ID
				ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
				{
					var response = nodeUpdateTransaction.Execute(client);
					response.GetReceipt(client);
				});

				// Assert.Contains(exception.Message, "INVALID_ACCOUNT_ID");
				// Assert.Contains(exception.Message, "INVALID_NODE_ACCOUNT_ID");
				Assert.Equal(exception.Receipt.Status, ResponseStatus.InvalidAccountId | ResponseStatus.InvalidNodeAccountId);
            }
        }
        [Fact]
        public virtual void TestNodeUpdateTransactionFailsWithAccountDeletedForDeletedAccount()
        {

            // Set up the local network with 2 nodes
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
            using (var client = Client.ForNetwork(network, client =>
            {
                client.TransportSecurity = false;
                client.MirrorNetwork_.Network = ["localhost:5600"];
            }))
            {

                // Set the operator to be account 0.0.2 (has admin privileges)
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: Create a new account that will be deleted
                var newAccountKey = PrivateKey.GenerateED25519();
                var newAccountId = CreateAccount(client, newAccountKey.GetPublicKey(), Hbar.From(2));

                // Delete the account (transfer balance to operator account)
                var deleteResponse = new AccountDeleteTransaction
                {
					AccountId = newAccountId,
					TransferAccountId = client.OperatorAccountId,
				}
                .FreezeWith(client)
                .Sign(newAccountKey)
                .Execute(client);
                var deleteReceipt = deleteResponse.GetReceipt(client);
                Assert.Equal(deleteReceipt.Status, ResponseStatus.Success);

                // When: A NodeUpdateTransaction is submitted to change to the deleted account
                var nodeUpdateTransaction = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = newAccountId,
					NodeAccountIds = [new AccountId(0, 0, 3)],
				}
                .FreezeWith(client)
                .Sign(newAccountKey);

				// Then: The transaction should fail with ACCOUNT_DELETED
				ReceiptStatusException exception = Assert.Throws<ReceiptStatusException>(() =>
				{
					var response = nodeUpdateTransaction.Execute(client);
					response.GetReceipt(client);
				});

				Assert.Contains(exception.Message, "ACCOUNT_DELETED");
				Assert.Equal(exception.Receipt.Status, ResponseStatus.AccountDeleted);
            }
        }
        [Fact]
        public virtual void TestSubsequentTransactionWithNewNodeAccountIdSucceeds()
        {

            // Set the network
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
            using (var client = Client.ForNetwork(network, client => 
            {
                client.TransportSecurity = false;
                client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
            {

                // Set the operator to be account 0.0.2
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);

                // Given: Create a new account that will be the node account id
                var newNodeAccountID = CreateAccount(client, originalOperatorKey.GetPublicKey(), Hbar.From(1));
                Assert.NotNull(newNodeAccountID);

                // Update node 0's account id (0.0.3 -> newNodeAccountID)
                var resp = new NodeUpdateTransaction
                {
					NodeId = 0,
					Description = "testUpdated",
					AccountId = newNodeAccountID,
				
                }.Execute(client);
                resp.ValidateStatus = true;
                var receipt = resp.GetReceipt(client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);

                // Wait for mirror node to import data
                Thread.Sleep(10000);

                // Given: Successfully handled transaction with outdated node account ID
                // This transaction targets old node account ID (0.0.3) and new node account ID (0.0.4)
                // Node 0.0.3 will fail with INVALID_NODE_ACCOUNT and SDK will retry with 0.0.4
                ExecuteAccountCreate(client, [new AccountId(0, 0, 3), new AccountId(0, 0, 4)]);

                // When: Subsequent transaction targets the NEW node account ID directly
                ExecuteAccountCreate(client, [ newNodeAccountID ]);

                // Cleanup: Revert the node account id (newNodeAccountID -> 0.0.3)
                resp = new NodeUpdateTransaction
                {
					NodeId = 0,
					NodeAccountIds = [newNodeAccountID],
					Description = "testUpdated",
					AccountId = new AccountId(0, 0, 3),
				
                }.Execute(client);
                resp.ValidateStatus = true;
                receipt = resp.GetReceipt(client);

                Assert.Equal(receipt.Status, ResponseStatus.Success);
            }
        }
        [Fact]
        public virtual void TestSdkUpdatesNetworkConfigurationOnInvalidNodeAccount()
        {
            var network = new Dictionary<string, AccountId>()
            {
                { "localhost:50211", new AccountId(0, 0, 3) },
                { "localhost:51211", new AccountId(0, 0, 4) }
            };
			using (var client = Client.ForNetwork(network, client =>
			{
				client.TransportSecurity = false;
				client.MirrorNetwork_.Network = ["localhost:5600"];
			}))
			{
                var originalOperatorKey = PrivateKey.FromString("302e020100300506032b65700422042091132178e72057a1d7528025956fe39b0b847f200ab59b2fdd367017f3087137");
                client.OperatorSet(new AccountId(0, 0, 2), originalOperatorKey);
                var newNodeAccountID = CreateAccount(client, originalOperatorKey.GetPublicKey(), Hbar.From(1));
                Assert.NotNull(newNodeAccountID);
                UpdateNodeAccountId(client, 0, newNodeAccountID, null);
                Thread.Sleep(10000);

                // Trigger INVALID_NODE_ACCOUNT error and retry
                ExecuteAccountCreate(client, [new AccountId(0, 0, 3), new AccountId(0, 0, 4)]);

                // Verify subsequent transaction with new node account ID
                ExecuteAccountCreate(client, [ newNodeAccountID ]);

                // Verify the network configuration now includes the new account ID
                var finalNetwork = client.Network_;
                var hasNewAccountId = finalNetwork.Network_Read.Values.Any((accountId) => accountId.Equals(newNodeAccountID));

                Assert.True(hasNewAccountId, "Client network should contain the new node account ID after address book update");

                // Cleanup
                UpdateNodeAccountId(client, 0, new AccountId(0, 0, 3), [ newNodeAccountID ]);
            }
        }

        private AccountId CreateAccount(Client client, Key key, Hbar initialBalance)
        {
            var resp = new AccountCreateTransaction
            {
				Key = key,
				InitialBalance = initialBalance,
			
            }.Execute(client);

            resp.ValidateStatus = true;

            return resp.GetReceipt(client).AccountId;
        }

        public void UpdateNodeAccountId(Client client, long nodeId, AccountId newAccountId, IList<AccountId> nodeAccountIds)
        {
            var transaction = new NodeUpdateTransaction
            {
                NodeAccountIds = [.. nodeAccountIds],
                NodeId = (ulong)nodeId,
                Description = "testUpdated",
                AccountId = newAccountId,
            };

            var resp = transaction.Execute(client);
            resp.ValidateStatus = true;
            var receipt = resp.GetReceipt(client);

            Assert.Equal(receipt.Status, ResponseStatus.Success);
        }
        public void ExecuteAccountCreate(Client client, IList<AccountId> nodeAccountIds)
        {
            var newAccountKey = PrivateKey.GenerateED25519();
            var resp = new AccountCreateTransaction
            {
				Key = newAccountKey.GetPublicKey(),
				NodeAccountIds = [..nodeAccountIds],
			
            }.Execute(client);
            
            Assert.NotNull(resp);
            resp.ValidateStatus = true;
            var receipt = resp.GetReceipt(client);

            Assert.Equal(receipt.Status, ResponseStatus.Success);
        }
    }
}