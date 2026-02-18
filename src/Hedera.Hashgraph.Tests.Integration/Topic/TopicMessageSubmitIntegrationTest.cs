// SPDX-License-Identifier: Apache-2.0
using System;

using Hedera.Hashgraph.SDK.Topic;
using Hedera.Hashgraph.SDK.Transactions;

using Org.BouncyCastle.Utilities.Encoders;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicMessageSubmitIntegrationTest
    {
        public virtual void CanSubmitATopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction
                {
                    AdminKey = testEnv.OperatorKey,
                    TopicMemo = "[e2e::TopicCreateTransaction]"

                }.Execute(testEnv.Client);
                var topicId = response.GetReceipt(testEnv.Client).TopicId;
                var info = new TopicInfoQuery
                { 
                    TopicId = topicId
                
                }.Execute(testEnv.Client);
                Assert.Equal(info.TopicId, topicId);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.SequenceNumber, 0);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                new TopicMessageSubmitTransaction
                { 
                    TopicId = topicId
                
                }.SetMessage("Hello, from HCS!").Execute(testEnv.Client).GetReceipt(testEnv.Client);
                info = new TopicInfoQuery
                { 
                    TopicId = topicId
                
                }.Execute(testEnv.Client);
                Assert.Equal(info.TopicId, topicId);
                Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.SequenceNumber, 1);
                Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                new TopicDeleteTransaction
                { 
                    TopicId = topicId
                
                }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
            }
        }

        public virtual void CanSubmitALargeTopicMessage()
        {

            // Skip if using PreviewNet
            Assumptions.AssumeTrue(!System.GetProperty("HEDERA_NETWORK").Equals("previewnet"));
            AssertThatNoException(, () =>
            {
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    var response = new TopicCreateTransaction()AdminKey = testEnv.OperatorKey,.SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.Client);
                    var topicId = response.GetReceipt(testEnv.Client).TopicId);
                    Thread.Sleep(5000);
                    var info = new TopicInfoQuery
                { 
                        TopicId = topicId
                    
                    }.Execute(testEnv.Client);
                    Assert.Equal(info.TopicId, topicId);
                    Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                    Assert.Equal(info.SequenceNumber, 0);
                    Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                    var responses = new TopicMessageSubmitTransaction
                { 
                        TopicId = topicId
                    
                    }.SetMaxChunks(15).SetMessage(Contents.BIG_CONTENTS).ExecuteAll(testEnv.Client);
                    foreach (var resp in responses)
                    {
                        resp.GetReceipt(testEnv.Client);
                    }

                    info = new TopicInfoQuery
                { 
                        TopicId = topicId
                    
                    }.Execute(testEnv.Client);
                    Assert.Equal(info.TopicId, topicId);
                    Assert.Equal(info.TopicMemo, "[e2e::TopicCreateTransaction]");
                    Assert.Equal(info.SequenceNumber, 14);
                    Assert.Equal(info.AdminKey, testEnv.OperatorKey);
                    new TopicDeleteTransaction
                { 
                        TopicId = topicId
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }
            });
        }

        public virtual void CannotSubmitMessageWhenTopicIDIsNotSet()
        {
            // Skip if using PreviewNet
            Assumptions.AssumeTrue(!System.GetProperty("HEDERA_NETWORK").Equals("previewnet"));
            AssertThatNoException(, () =>
            {
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    var response = new TopicCreateTransaction()AdminKey = testEnv.OperatorKey,.SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.Client);
                    var topicId = response.GetReceipt(testEnv.Client).TopicId);
                    Assert.Throws(typeof(PrecheckStatusException), () =>
                    {
                        new TopicMessageSubmitTransaction().SetMessage(Contents.BIG_CONTENTS).SetMaxChunks(15).Execute(testEnv.Client).GetReceipt(testEnv.Client);
                    }).WithMessageContaining(Status.INVALID_TOPIC_ID.ToString());
                    new TopicDeleteTransaction
                { 
                        TopicId = topicId
                    
                    }.Execute(testEnv.Client).GetReceipt(testEnv.Client);
                }
            });
        }

        public virtual void DecodeHexRegressionTest()
        {
            string binaryHex = "2ac2010a580a130a0b08d38f8f880610a09be91512041899e11c120218041880c2d72f22020878da01330a0418a5a1201210303030303030313632373633373731351a190a130a0b08d38f8f880610a09be91512041899e11c1001180112660a640a20603edaec5d1c974c92cb5bee7b011310c3b84b13dc048424cd6ef146d6a0d4a41a40b6a08f310ee29923e5868aac074468b2bde05da95a806e2f4a4f452177f129ca0abae7831e595b5beaa1c947e2cb71201642bab33fece5184b04547afc40850a";
            byte[] transactionBytes = Hex.Decode(binaryHex);
            var transaction = Transaction.FromBytes(transactionBytes));
            string idString = transaction.TransactionId.ToString();
            string transactionString = transaction.ToString();
        }
    }
}